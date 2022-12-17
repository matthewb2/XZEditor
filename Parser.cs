using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XZ.Edit.Actions;
using XZ.Edit.Entity;
using XZ.Edit.Interfaces;

namespace XZ.Edit {
    public class Parser {

        
        private IEdit pIEdit;
        private Draw pDraw;
        private LanguageMode pLanguageMode;
        private CharFontStyle pCharFontStyle;
        private CursorAndIME pCursor;
        public Parser(IEdit iedit, CursorAndIME cursor) {
            this.pIEdit = iedit;
            this.PPucker = new Pucker(this);
            this.pDraw = new Draw(this);
            this.pCharFontStyle = new CharFontStyle(iedit);
            this.pCursor = cursor;
            this.PRedo = new List<BaseAction>();
            this.PUndo = new List<BaseAction>();

        }

        
        public Pucker PPucker { get; set; }

        //public LineColsProperty PLCProperty { get; set; }

        public IEdit PIEdit {
            get { return this.pIEdit; }
        }

        public CharFontStyle PCharFontStyle {
            get { return this.pCharFontStyle; }
        }

        public LanguageMode PLanguageMode {
            get { return this.pLanguageMode; }
        }

        public CursorAndIME PCursor {
            get { return this.pCursor; }
        }

        /// <summary>
        /// 获取最大宽度
        /// </summary>
        public int GetMaxWidth {
            get {
                return this.pDraw.MaxWidth;
            }
        }

        public int GetLeftSpace {
            get {
                return this.pDraw.GetLeftSpace;
            }
        }

        /// <summary>
        /// 获取选择部分开始和结束坐标
        /// </summary>
        public CPoint[] GetSelectPartPoint {
            get {
                return this.pDraw.GetSelectPartPoint;
            }
        }
        /// <summary>
        /// 撤消记录
        /// </summary>
        public List<BaseAction> PRedo { get; set; }

        /// <summary>
        /// 重做
        /// </summary>
        public List<BaseAction> PUndo { get; set; }

        /// <summary>
        /// 键盘山下键是所在行默认索引值
        /// </summary>
        public int UpDownLineStringDefaultIndex { get; set; }


        public int PPuckerWidth {
            get {
                return this.pDraw.PPuckerWidth;
            }
        }

        /// <summary>
        /// 隐藏部分宽度
        /// </summary>
        public int GetHidePartWidth {
            get {
                return this.pDraw.GetHidePartWidth; ;
            }
        }


        
        public string GetText() {
            var sbStr = new StringBuilder();
            foreach (var s in this.PLineString) {
                if (s.IsFurl()) {
                    sbStr.AppendLine(s.Text + this.PPucker.PDictPuckerLeavings[s.ID].Item1);
                    this.SetHideText(sbStr, s);
                } else
                    sbStr.AppendLine(s.Text);

            }

            return sbStr.ToString();
        }

        private void SetHideText(StringBuilder sbStr, LineString ls) {
            LineString[] outValue;
            if (this.PPucker.PDictPuckerList.TryGetValue(ls.ID, out outValue)) {
                foreach (var s in outValue) {
                    if (s.IsFurl()) {
                        sbStr.AppendLine(s.Text + this.PPucker.PDictPuckerLeavings[s.ID].Item1);
                        this.SetHideText(sbStr, s);
                    } else
                        sbStr.AppendLine(s.Text);
                }
            }
        }

        /// <summary>
        /// 添加字符串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public void AddText(string value) {
            if (this.pLanguageMode == null)
                throw new Exception("error");

            this.PLineString = this.pCharFontStyle.GetPLineStrings(value);
        }

        /// <summary>
        /// 插入字符串
        /// </summary>
        /// <param name="c"></param>
        public void InsertChar(char c) {
            switch (c) {
                case CharCommand.Char_Enter:
                    var enter = new EnterAction(this);
                    enter.Execute();
                    this.AddAction(enter);
                    break;
                case CharCommand.Char_Tab:
                    this.AddTab();
                    break;
                case CharCommand.Char_BackSpace:
                    var backSpace = new BackSpaceAction(this);
                    backSpace.PIsUndoOrRedo = false;
                    backSpace.Execute();
                    this.AddAction(backSpace);
                    break;
                default:
                    this.AddChar(c);
                    break;
            }
        }

        /// <summary>
        /// 插入Tab 字符
        /// </summary>
        public void AddTab() {
            if (this.GetSelectPartPoint != null && this.GetSelectPartPoint[0].Y != this.GetSelectPartPoint[1].Y) {
                var rectraction = new RetractAction(this);
                rectraction.PRetractType = RetractType.Add;
                rectraction.Execute();
                AddAction(rectraction);
                return;
                
            }
            this.AddChar(CharCommand.Char_Tab);
        }

        /// <summary>
        /// 添加字符
        /// </summary>
        /// <param name="c"></param>
        private void AddChar(char c) {
            var insertAction = new InsertAction(this);
            insertAction.AddChar(c);
            if (this.PUndo.Count > 0 && !this.pLanguageMode.CompartChars.Contains(c)) {
                var lastAction = this.PUndo.Last();
                if (lastAction is InsertAction) {
                    var undoInsert = lastAction as InsertAction;
                    if (!this.pLanguageMode.CompartChars.Contains(c) && c != CharCommand.Char_Tab) {
                        var operationAciton = lastAction.PActionOperation as DeleteLineStringAction;
                        operationAciton.DeleteString += insertAction.PInsertString;
                        operationAciton.DeleteStringWidth += insertAction.PCharWidth;
                        return;
                    }
                }
            }
            AddAction(insertAction);
        }

        /// <summary>
        /// 添加Action
        /// </summary>
        /// <param name="action"></param>
        public void AddAction(BaseAction action) {
            if (PUndo.Count > this.pIEdit.GetRepealCount)
                PUndo.RemoveAt(0);
            if (this.PRedo.Count > 0 && this.PUndo.Count == 0)
                this.PRedo.Clear();
            this.PUndo.Add(action);
        }

        
        /// <summary>
        /// 获取行个数
        /// </summary>
        public int GetCount() {
            if (this.PLineString != null)
                return this.PLineString.Count;
            else
                return 0;
        }

        public List<LineString> PLineString { get; set; }

        /// <summary>
        /// 获取当前行
        /// </summary>
        public LineString GetLineString {
            get {
                return this.PLineString[this.pCursor.CousorPointForWord.Y];
            }
        }

        /// <summary>
        /// 获取LineString 宽度
        /// </summary>
        /// <param name="ls"></param>
        /// <returns></returns>
        public int GetLsWidth(LineString ls) {
            int width = GetPuckerLeavingsWidth(ls);
            if (ls.Width == 0 && string.IsNullOrEmpty(ls.Text))
                return ls.Width;

            foreach (var w in ls.PWord) {
                if (w.Width == 0 && !string.IsNullOrEmpty(w.Text))
                    w.Width = CharCommand.GetWrodWidth(w, this.PIEdit.GetGraphics, this.PLanguageMode.TabSpaceCount);

                width += w.Width;
            }

            return width;
        }

        private int GetPuckerLeavingsWidth(LineString ls) {
            if (!ls.IsFurl())
                return 0;
            Tuple<string, Word[]> outVaule;
            int width = 0;
            if (this.PPucker.PDictPuckerLeavings.TryGetValue(ls.ID, out outVaule)) {
                foreach (var w in outVaule.Item2)
                    width += w.Width;
            }
            return width;
        }

        #region 方法

        /// <summary>
        /// 清除选择
        /// </summary>
        public void ClearSelect() {
            this.pDraw.SetBgStartPoint();
            this.ClearCouple();
        }

        public void SetLangeMode(string language) {
            pLanguageMode = InitLanguageMode.GetLanguageMode(language);
            this.pCharFontStyle.SetLanguageMode(pLanguageMode);
        }

        /// <summary>
        /// 绘制  
        /// </summary>
        /// <param name="g"></param>
        public void DrawContents(Graphics g) {
            this.pDraw.DrawContents(g);
        }

        public void SetBgStartPoint(CPoint point) {
            if (point != null)
                this.pDraw.SetBgStartPoint(point);
        }

        public void SetBgEndPoint(CPoint point) {
            if (point != null)
                this.pDraw.SetBgEndPoint(point);
        }
        
        public void ClearFindDatas() {
            this.pDraw.FindDatas.Clear();
        }

        public void AddFindData(FindTextLocation fLocation) {
            FindTextLocation outValue;
            if (this.pDraw.FindDatas.TryGetValue(fLocation.ID, out outValue)) {
                if (outValue.NextFindTextLocations == null)
                    outValue.NextFindTextLocations = new List<FindTextLocation>();
                outValue.NextFindTextLocations.Add(fLocation);
            } else
                this.pDraw.FindDatas.Add(fLocation.ID, fLocation);
        }

        #endregion

        public void DoubleSelectCrusorWord() {
            var sWord = this.pCursor.PSursorSelectWord;
            if (sWord.PWord == null)
                return;

            int y = this.pCursor.CousorPointForWord.Y * FontContainer.FontHeight;
            var line = this.GetLineString;
            CPoint startPoint = new CPoint() { Y = y, LineWidth = line.Width };
            CPoint endPoint = new CPoint() { Y = y, LineWidth = line.Width };

            if (sWord.LineIndex == line.Length - 1)
                this.DoubleSelectLast(line, startPoint, endPoint);
            else if (sWord.LineIndex == -1)
                DoubleSelectStart(line, startPoint, endPoint);
            else
                DoubleSelectMiddle(line, startPoint, endPoint);

            this.SetBgStartPoint(startPoint);
            this.SetBgEndPoint(endPoint);
            this.pCursor.SetPosition(endPoint.X, -1, this.GetLeftSpace);
            this.pCursor.SetPosition();
        }

        /// <summary>
        /// 双击选择最后一个字符
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        private void DoubleSelectLast(LineString ls, CPoint startPoint, CPoint endPoint) {
            endPoint.LineStringIndex = ls.Length - 1;
            endPoint.X = ls.Width + this.GetLeftSpace;
            int length = 0;
            int width = 0;
            for (var i = ls.PWord.Count - 1; i > -1; i--) {
                var w = ls.PWord[i];
                if (w.PEWordType == EWordType.Word || w.PEWordType == EWordType.Compart) {
                    startPoint.LineStringIndex = ls.Length - length - w.Length - 1;
                    startPoint.X = ls.Width + this.GetLeftSpace - w.Width - width;
                    return;
                }
                length += w.Length;
                width += w.Width;
            }

        }

        /// <summary>
        /// 双击选择开始
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        private void DoubleSelectStart(LineString ls, CPoint startPoint, CPoint endPoint) {
            startPoint.LineStringIndex = -1;
            startPoint.X = this.GetLeftSpace;
            int length = 0;
            int width = 0;
            var w = ls.PWord[0];

            if (ls.PWord.Count == 1) {
                endPoint.LineStringIndex = w.Length - 1;
                endPoint.X = w.Width + this.GetLeftSpace;
                return;
            }
            length += w.Length;
            width += w.Width;
            var type = w.PEWordType;
            if (type == EWordType.Tab)
                type = EWordType.Space;
            for (var i = 1; i < ls.PWord.Count; i++) {
                w = ls.PWord[i];
                if ((w.PEWordType == EWordType.Tab ? EWordType.Space : w.PEWordType) != type) {
                    endPoint.LineStringIndex = length - 1;
                    endPoint.X = width + this.GetLeftSpace;
                    return;
                }
                length += w.Length;
                width += w.Width;
            }
            endPoint.LineStringIndex = length - 1;
            endPoint.X = width + this.GetLeftSpace;
        }

        /// <summary>
        /// 双击选择中间
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        private void DoubleSelectMiddle(LineString ls, CPoint startPoint, CPoint endPoint) {
            var m = this.pCursor.PSursorSelectWord.PWord;
            var i = 0;

            int length = 0;
            int width = 0;
            for (; i < m.LineIndex; i++) {
                length += ls.PWord[i].Length;
                width += ls.PWord[i].Width;
            }

            if (m.PEWordType == EWordType.Word) {
                startPoint.LineStringIndex = length - 1;
                startPoint.X = width + this.GetLeftSpace;
                endPoint.LineStringIndex = length - 1 + m.Length;
                endPoint.X = width + this.GetLeftSpace + m.Width;
            } else {
                var type = m.PEWordType;
                if (type == EWordType.Tab)
                    type = EWordType.Space;

                startPoint.LineStringIndex = length - 1;
                startPoint.X = width + this.GetLeftSpace;

                for (i = m.LineIndex - 1; i > -1; i--) {
                    var ml = ls.PWord[i];
                    if ((ml.PEWordType == EWordType.Tab ? EWordType.Space : ml.PEWordType) != type) {
                        break;
                    }
                    startPoint.LineStringIndex -= m.Length;
                    startPoint.X -= ml.Width;
                }
                endPoint.LineStringIndex = length - 1 + m.Length;
                endPoint.X = width + this.GetLeftSpace + m.Width;
                for (i = m.LineIndex + 1; i < ls.PWord.Count; i++) {
                    var ml = ls.PWord[i];
                    if ((ml.PEWordType == EWordType.Tab ? EWordType.Space : ml.PEWordType) != type) {
                        break;
                    }
                    endPoint.LineStringIndex += m.Length;
                    endPoint.X += ml.Width;
                }
            }
        }


        public int GetSelectLineIndex {
            get {
                if (this.pCursor.IsShowCurosr && this.pCursor.CousorPointForEdit.Y > 0 && this.PIEdit.SelectLineStyle != ESelectLineStyle.None)
                    return this.pCursor.CousorPointForEdit.Y;
                else
                    return 0;
            }
        }

        /// <summary>
        /// 重新设置光标的位置
        /// </summary>
        /// <returns></returns>
        public void SetCurosrPosition() {
            this.pCursor.CousorPointForEdit.X = this.pCursor.XForLeft + this.pDraw.GetLeftSpace;
            this.pCursor.SetPosition();
        }

        public CPoint SetCurosrPosition(int x, int y, bool isClearSelectBg = true) {
            y = Math.Max(0, y);
            x = Math.Max(0, x) + this.pIEdit.GetHorizontalScrollValue;
            this.UpDownLineStringDefaultIndex = -1;
            y += this.pIEdit.GetVerticalScrollValue;
            int leftX = x - (this.pDraw.PNumRowWidth + Draw.LeftSpace);
            this.pCursor.CousorPointForWord.Y = y / FontContainer.FontHeight;
            this.pCursor.CousorPointForWord.Y = Math.Min(this.pCursor.CousorPointForWord.Y, this.PLineString.Count - 1);
            var ssWord = this.GetLineStringIndex(this.GetLineString, x);
            this.pCursor.CousorPointForWord.X = ssWord.LineIndex;
        
            this.pCursor.PSursorSelectWord = ssWord;
            this.pCursor.CousorPointForEdit.X = ssWord.LeftWidth + this.pDraw.GetLeftSpace;
            if (isClearSelectBg)
                this.ClearSelect();
            if (ssWord.LeftWidth < this.pIEdit.GetHorizontalScrollValue) {
                this.pIEdit.SetHorizontalScrollValue(0, -1, true);
                
            } else
                this.pCursor.CousorPointForEdit.X -= this.pIEdit.GetHorizontalScrollValue;

            this.PCursor.SetPosition(this.pCursor.CousorPointForEdit.X, this.pCursor.CousorPointForWord.Y * FontContainer.FontHeight, this.pDraw.GetLeftSpace);
            this.pCursor.SetPosition();
            return new CPoint(this.pCursor.CousorPointForEdit.X + this.pIEdit.GetHorizontalScrollValue, this.pCursor.CousorPointForEdit.Y, this.GetLineString.Width, ssWord.LineIndex);
        }

        /// <summary>
        /// 获取X所在的行位置
        /// </summary>
        /// <param name="ls">选择的行数据</param>
        /// <param name="xwidth">光标x轴距离</param>
        /// <returns></returns>
        public SursorSelectWord GetLineStringIndex(LineString ls, int xwidth) {
            var surWord = new SursorSelectWord();
            if (ls.Text.Length == 0 || xwidth < this.pDraw.GetLeftSpace)
                return surWord;
            xwidth = xwidth - this.pDraw.GetLeftSpace;
            int maxWidth = 0;
            int index = -1;
            Word word = null;
            foreach (var w in ls.PWord) {
                maxWidth += w.Width;
                if (maxWidth > xwidth) {
                    maxWidth -= w.Width;
                    word = w;
                    break;
                } else if (maxWidth == xwidth) {
                    surWord.PWord = w;
                    surWord.PWordIndex = w.Length;
                    surWord.LeftWidth = maxWidth;
                    surWord.LeftWidthForWord = maxWidth;
                    surWord.LineIndex = index + w.Length;
                    surWord.End = true;
                    return surWord;
                }
                surWord.WordLeftWidth += w.Width;
                word = null;
                index += w.Length;
            }
            if (word != null) {
                int leftWordWidth = 0;
                for (var i = 0; i < word.Length; i++) {
                    var lwidth = 0;
                    switch (word.Text[i]) {
                        case CharCommand.Char_Space:
                            lwidth = FontContainer.GetSpaceWidth(this.pIEdit.GetGraphics);
                            break;
                        case CharCommand.Char_Tab:
                            lwidth = FontContainer.GetSpaceWidth(this.pIEdit.GetGraphics) * this.pLanguageMode.TabSpaceCount;
                            break;
                        default:
                            lwidth = CharCommand.GetCharWidth(this.pIEdit.GetGraphics, word.Text[i].ToString(), CharCommand.GetFont(word));
                            break;
                    }
                    leftWordWidth += lwidth;
                    maxWidth += lwidth;
                    var _i = i;
                    var _wordIndex = Math.Max(0, i - 1);
                    if (maxWidth >= xwidth) {
                        if (maxWidth == xwidth || maxWidth - xwidth < lwidth * 4 / 7) {
                            _i++;
                            _wordIndex = i;
                            lwidth = 0;
                        }
                        surWord.PWord = word;
                        surWord.PWordIndex = _wordIndex;
                        surWord.LeftWidth = maxWidth - lwidth;
                        surWord.LineIndex = index + _i;
                        surWord.LeftWidthForWord = leftWordWidth - lwidth;
                        return surWord;
                    }
                }
            }

            surWord.PWord = ls.PWord.Last();
            surWord.WordLeftWidth -= surWord.PWord.Width;
            surWord.PWordIndex = surWord.PWord.Length;
            surWord.LeftWidth = maxWidth;
            surWord.LeftWidthForWord = surWord.PWord.Width;
            surWord.LineIndex = index;
            return surWord;
        }

        public ToolTipMessageEventArgs GetToolTipMessage(Point point, out Rectangle rect) {
            rect = Rectangle.Empty;
            if (this.GetSelectPartPoint != null)
                return null;
            var y = (point.Y + this.pIEdit.GetVerticalScrollValue) / FontContainer.FontHeight;
            if (y >= this.PLineString.Count || point.X < this.GetLeftSpace)
                return null;
            var ls = this.PLineString[y];
            var x = Math.Max(0, point.X) + this.pIEdit.GetHorizontalScrollValue;
            if (x > ls.Width + this.GetLeftSpace)
                return null;
            var ssWord = this.GetLineStringIndex(ls, x);
            if (ssWord == null || ssWord.PWord == null)
                return null;
            var tipMessage = new ToolTipMessageEventArgs();
            if (ssWord.LineIndex < 0)
                return null;
            
            tipMessage.LineString = ls;
            tipMessage.LineStringIndex = y;
            tipMessage.PChar = ls.Text[ssWord.LineIndex];
            tipMessage.TextIndex = ssWord.LineIndex;
            tipMessage.Word = ssWord.PWord.Text;
            tipMessage.WordIndex = ssWord.PWord.LineIndex;
            tipMessage.WordStartIndex = ssWord.PWordIndex;
            tipMessage.WordWidth = ssWord.PWord.Width;

            rect.X = ssWord.WordLeftWidth + this.GetLeftSpace - this.pIEdit.GetHorizontalScrollValue;
            rect.Y = y * FontContainer.FontHeight - this.pIEdit.GetVerticalScrollValue;
            rect.Width = ssWord.PWord.Width;
            rect.Height = FontContainer.FontHeight;

            return tipMessage;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetLineStringIndexWidth(LineString ls, int index) {
            if (ls.Text.Length == 0 || index == -1)
                return 0;

            int maxWidth = 0;
            int _index = -1;
            int fi = 0;
            Word word = null;
            foreach (var w in ls.PWord) {
                fi++;
                _index += w.Length;
                if (w.Width == 0 && w.Length > 0) {
                    w.Width = CharCommand.GetWrodWidth(w, this.pIEdit.GetGraphics, this.pLanguageMode.TabSpaceCount);
                    ls.Width += w.Width;
                }
                maxWidth += w.Width;
                if (_index > index) {
                    _index -= w.Length;
                    maxWidth -= w.Width;
                    word = w;
                    SetPWordWord(ls, fi);
                    break;
                } else if (_index == index)
                    return maxWidth;
                word = null;

            }
            if (word != null) {
                for (var i = 0; i < word.Length; i++) {
                    var lwidth = 0;
                    switch (word.Text[i]) {
                        case CharCommand.Char_Space:
                            lwidth = FontContainer.GetSpaceWidth(this.pIEdit.GetGraphics);
                            break;
                        case CharCommand.Char_Tab:
                            lwidth = FontContainer.GetSpaceWidth(this.pIEdit.GetGraphics) * this.pLanguageMode.TabSpaceCount;
                            break;
                        default:
                            lwidth = CharCommand.GetCharWidth(this.pIEdit.GetGraphics, word.Text[i].ToString(), CharCommand.GetFont(word));
                            break;
                    }
                    maxWidth += lwidth;
                    if (_index + i + 1 >= index) {
                        return maxWidth;
                    }
                }
            }

            return ls.Width;

        }

        private void SetPWordWord(LineString ls, int i) {
            for (; i < ls.PWord.Count; i++) {
                var w = ls.PWord[i];
                if (w.Width == 0 && w.Length > 0) {
                    w.Width = CharCommand.GetWrodWidth(w, this.pIEdit.GetGraphics, this.pLanguageMode.TabSpaceCount);
                    ls.Width += w.Width;
                }
            }
        }



        /// <summary>
        /// 重置行中的LNP。同时删除Pucker相关数据
        /// </summary>
        /// <param name="formerlyLS">原来行</param>
        /// <param name="nowLS">当前行</param>
        public void ResetLineLNPAndClearPucker(LineString formerlyLS, LineString nowLS) {
            this.ResetLineLNPAndClearPucker(formerlyLS.PLNProperty, formerlyLS.ID, nowLS);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lnp"></param>
        /// <param name="id"></param>
        /// <param name="nowLs"></param>
        public void ResetLineLNPAndClearPucker(LineNodeProperty lnp, int id, LineString nowLS) {
            if (nowLS.PLNProperty != null && lnp != null) {
                if (lnp.IsStartRange && nowLS.PLNProperty.IsStartRange
                    || lnp.IsEndRange && nowLS.PLNProperty.IsEndRange) {
                    nowLS.ID = id;
                    return;
                }
            }
            this.DeletePuckerCache(id);

        }

        /// <summary>
        /// 删除折叠缓存
        /// </summary>
        /// <param name="id"></param>
        public void DeletePuckerCache(int id) {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tuple"></param>
        /// <param name="nowLS"></param>
        public void ResetLineLNPAndClearPucker(Tuple<LineNodeProperty, int> tuple, LineString nowLS) {
            this.ResetLineLNPAndClearPucker(tuple.Item1, tuple.Item2, nowLS);
        }

        /// <summary>
        /// 复制折叠隐藏数据
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="sbLine"></param>
        /// <param name="copyHideLine"></param>
        public void CopyLinePuckerHide(LineString ls, StringBuilder sbLine, out int copyHideLine) {
            copyHideLine = 0;
        }


        /// <summary>
        /// 鼠标移动选择
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void MouseMoveSelect(int x, int y, CPoint points) {
            var nPoints = this.SetCurosrPosition(x, y);
            MouseMoveSelect(nPoints, points);
        }

        public void MouseMoveSelect(CPoint point1, CPoint point2) {
            if (point1.CompareTo(point2) == -1) {
                this.pDraw.SetBgStartPoint(point1);
                this.pDraw.SetBgEndPoint(point2);
            } else {
                this.pDraw.SetBgStartPoint(point2);
                this.pDraw.SetBgEndPoint(point1);
            }
        }

        public void MouseLeftClickSelect(CPoint point, Point upCur) {
            var gbs = this.GetSelectPartPoint;
            CPoint startPoint;
            CPoint endPoint;
            if (upCur.Y == gbs[0].Y && upCur.X + this.pIEdit.GetHorizontalScrollValue == gbs[0].X) {
                startPoint = point;
                endPoint = gbs[1].Create();
            } else {
                startPoint = gbs[0].Create();
                endPoint = point;
            }
            MouseMoveSelect(startPoint, endPoint);

        }

        public bool IsFindCouple { get; set; }

        /// <summary>
        /// 清楚已经选择的配对
        /// </summary>
        public void ClearCouple() {
            this.pDraw.PFindDrawRects.Clear();
        }

        /// <summary>
        /// 配备字符
        /// </summary>
        public void CoupleChar() {
            //return;
            ClearCouple();
            IsFindCouple = false;
            if (this.GetLineString.PLNProperty != null && this.GetLineString.PLNProperty.Couple != null && !this.GetLineString.PLNProperty.IsFurl) {
                var findCharIndex = this.pCursor.CousorPointForWord.X;
                int sGoCount = 0;
            Start:
                CoupleProperty outValue;
                if (!this.GetLineString.PLNProperty.Couple.TryGetValue(findCharIndex, out outValue)) {
                    if (sGoCount == 0) {
                        findCharIndex++;
                        sGoCount = 1;
                        goto Start;
                    }
                    return;
                }
                var findY = this.pCursor.CousorPointForWord.Y;
                var anotherCouple = this.GetCoupleItem(outValue, ref findY);
                if (anotherCouple == null)
                    return;

                Parallel.Invoke(
                    () => {
                        int charWidth;
                        var startX = GetCoupleX(outValue, this.pCursor.CousorPointForWord.Y, out charWidth);
                        var startRect = new DrawRect() {
                            StartX = startX - charWidth,
                            StartY = this.pCursor.CousorPointForWord.Y,
                            EndX = startX,
                            EndY = this.pCursor.CousorPointForWord.Y,
                            PSolidBrush = this.pLanguageMode.CoupleBackGround
                        };
                        this.pDraw.PFindDrawRects.Add(startRect);
                    },
                    () => {
                        int endCharWidth;
                        var endX = GetCoupleX(anotherCouple, findY, out endCharWidth);
                        var endRect = new DrawRect() {
                            StartX = endX - endCharWidth,
                            StartY = findY,
                            EndX = endX,
                            EndY = findY,
                            PSolidBrush = this.pLanguageMode.CoupleBackGround
                        };
                        this.pDraw.PFindDrawRects.Add(endRect);
                    }
                    );
            }
        }

        public int GetCoupleX(CoupleProperty value, int y, out int endWidth) {
            var ls = this.PLineString[y];
            endWidth = ls.PWord[value.WordIndex].Width;
            if (endWidth == 0)
                endWidth = CharCommand.GetWrodWidth(ls.PWord[value.WordIndex], this.pIEdit.GetGraphics, this.pLanguageMode.TabSpaceCount);
            var width = 0;
            for (var i = 0; i <= value.WordIndex; i++) {
                var w = ls.PWord[i];
                if (!string.IsNullOrEmpty(w.Text) && w.Width == 0)
                    w.Width = CharCommand.GetWrodWidth(w, this.pIEdit.GetGraphics, this.pLanguageMode.TabSpaceCount);
                width += w.Width;
            }

            return width + this.GetLeftSpace;
        }

        /// <summary>
        /// 查找匹配项目
        /// </summary>
        /// <param name="couple"></param>
        /// <returns></returns>
        public CoupleProperty GetCoupleItem(CoupleProperty couple, ref int y) {
            int showSelfCount = 0;
            char findChar = couple.FindChar;
            char nChar = couple.PChar;
            var direction = couple.PECouplePropertyDirection;
        Start:
            var next = couple.GetNextItem(direction);
            while (next != null) {

                if (next.PChar == nChar)
                    showSelfCount++;
                else if (next.PChar == findChar) {
                    if (showSelfCount == 0)
                        return next;
                    else
                        showSelfCount--;
                }
                next = next.GetNextItem(direction);
            }
            LineNodeProperty lnp = null;
            if (direction == ECouplePropertyDirection.Ahead)
                lnp = this.PPucker.GetUpNode(ref y);
            else
                lnp = this.PPucker.GetNextNode(ref y);

            var findCouple = GetCoupleItem(lnp, direction);
            if (findCouple == null)
                return null;
            else if (findCouple.PChar == findChar) {
                if (showSelfCount == 0)
                    return findCouple;
                else
                    showSelfCount--;
            } else if (findCouple.PChar == nChar)
                showSelfCount++;

            couple = findCouple;
            goto Start;
        }


        private CoupleProperty GetCoupleItem(LineNodeProperty lnp, ECouplePropertyDirection direction) {
            if (lnp == null || lnp.Couple == null || lnp.Couple.Count == 0)
                return null;
            if (direction == ECouplePropertyDirection.Ahead)
                return lnp.Couple.Last().Value;
            else
                return lnp.Couple.First().Value;
        }

    }
}
