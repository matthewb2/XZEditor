using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using XZ.Edit.Entity;

namespace XZ.Edit {
    public class Draw {

        public const int LeftSpace = 3;

        private Parser pParser;

        /// <summary>
        /// 绘制
        /// </summary>
        /// <param name="parser"></param>
        public Draw(Parser parser) {
            this.pParser = parser;
            this.PFindDrawRects = new List<DrawRect>();
            this.PRangeStartEndY = new Tuple<int, int>(-1, -1);
        }
        
        

        /// <summary>
        /// 背景开始坐标
        /// Y：到顶部的距离 + 垂直滚动条高度
        /// </summary>
        private CPoint pBGStartPoint = new CPoint(-1, -1, 0);
        /// <summary>
        /// 背景结束坐标
        /// </summary>
        private CPoint pBgEndPoint = new CPoint();

        /// <summary>
        /// 设置绘制背景开始坐标
        /// </summary>
        /// <param name="p"></param>
        public void SetBgStartPoint(int x, int y) {
            pBGStartPoint.X = x;
            pBGStartPoint.Y = y;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="lwidth"></param>
        /// <param name="lsIndex"></param>
        public void SetBgStartPoint(int x, int y, int lwidth, int lsIndex) {
            pBGStartPoint.X = x;
            pBGStartPoint.Y = y;
            pBGStartPoint.LineWidth = lwidth;
            pBGStartPoint.LineStringIndex = lsIndex;
        }

        public void SetBgStartPoint(CPoint point) {
            pBGStartPoint.X = point.X;
            pBGStartPoint.Y = point.Y;
            pBGStartPoint.LineWidth = point.LineWidth;
            pBGStartPoint.LineStringIndex = point.LineStringIndex;
        }


        /// <summary>
        /// 设置绘制背景结束坐标
        /// </summary>
        /// <param name="p"></param>
        public void SetBgEndPoint(int x, int y) {
            pBgEndPoint.X = x;
            pBgEndPoint.Y = y;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="lwidth"></param>
        /// <param name="lsIndex"></param>
        public void SetBgEndPoint(int x, int y, int lwidth, int lsIndex) {
            pBgEndPoint.X = x;
            pBgEndPoint.Y = y;
            pBgEndPoint.LineWidth = lwidth;
            pBgEndPoint.LineStringIndex = lsIndex;
        }

        public void SetBgEndPoint(CPoint point) {
            pBgEndPoint.X = point.X;
            pBgEndPoint.Y = point.Y;
            pBgEndPoint.LineWidth = point.LineWidth;
            pBgEndPoint.LineStringIndex = point.LineStringIndex;
        }
        /// <summary>
        /// 清除开始坐标
        /// </summary>
        public void SetBgStartPoint() {
            this.pBGStartPoint.X = -1;
        }

        /// <summary>
        /// 是否选择了部分
        /// </summary>
        private bool IsSelectPart {
            get {
                return this.pBGStartPoint.X > -1;
            }
        }

        /// <summary>
        /// 获取选择部分开始和结束坐标
        /// </summary>
        public CPoint[] GetSelectPartPoint {
            get {
                if (IsSelectPart)
                    return new CPoint[] { 
                        pBGStartPoint , pBgEndPoint
                };
                else
                    return null;
            }
        }



        /// <summary>
        /// 获取右侧空格
        /// </summary>
        public int GetLeftSpace {
            get {
                return this.PNumRowWidth + Draw.LeftSpace + this.PPuckerWidth;
            }
        }

        public int PPuckerWidth {
            get {
                return pNumWidth;
            }
        }

        public int MaxWidth { get; set; }


        /// <summary>
        /// 找到的需要绘制的背景
        /// </summary>
        public List<DrawRect> PFindDrawRects { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Tuple<int, int> PRangeStartEndY { get; set; }
        private List<int> pDrawNum = new List<int>();
        
        /// <summary>
        /// 绘制内容
        /// </summary>
        /// <param name="g"></param>
        public void DrawContents(Graphics g) {
            //Console.WriteLine("drawcontents called");
            this.pDrawNum.Clear();
            this.pParser.PPucker.PuckerMarker.Clear();
            int i = this.pParser.PIEdit.GetVerticalScrollValue / FontContainer.FontHeight;
            int numStart = i;
            int startNum = this.GetStartNum(i);
            int count = Math.Min(this.pParser.GetCount(), i + (int)Math.Ceiling(this.pParser.PIEdit.GetHeight * 1d / FontContainer.FontHeight));
            int top;
            int startValue = i * FontContainer.FontHeight;
            int heightValue = count * FontContainer.FontHeight;
            this.GetLeftNumWidth(g);
            this.DrawSelectLine(g);
            this.DrawFindDrawRects(i, count, g);
            this.DrawSelectBackGround(startValue, heightValue, g);
            this.ClearDrawMoreStyle(i);
            FindTextLocation findTextLocation;
            while (i < count) {
                var ls = this.pParser.PLineString[i];
                int allWidth = this.GetLeftSpace - this.pParser.PIEdit.GetHorizontalScrollValue;
                int leftLength = 0;
                top = FontContainer.FontHeight * i - this.pParser.PIEdit.GetVerticalScrollValue;
                ls.Width = 0;
                var qMStyles = DrawMoreStyle(ls, i);
                var forIndex = 0;
                findTextLocation = this.GetFindTextLocation(ls);
                foreach (var w in ls.PWord) {
                    int width = 0;
                    switch (w.PEWordType) {
                        case EWordType.Compart:
                        case EWordType.Word:
                            WFontColor forWfc = GetWordStyle(qMStyles, forIndex, i);
                            DrawFind(findTextLocation, i, leftLength, allWidth, w, CharCommand.GetFont(w, forWfc), g);
                            width = DrawWrod(g, w, forWfc, allWidth, top);
                            break;
                        default:
                            DrawFind(findTextLocation, i, leftLength, allWidth, w, null, g);
                            width = DrawTabSpace(g, w, allWidth, top);
                            break;
                    }
                    leftLength += w.Length;
                    forIndex++;
                    allWidth += width;
                    ls.Width += width;
                }
                if (ls.Width > this.MaxWidth) {
                    this.MaxWidth = ls.Width;
                    this.pParser.PIEdit.SetMaxScollMaxWidth(ls.Width + this.GetLeftSpace);
                }
                
                this.SetLineIndexYAndDrawNum(ls, i - numStart, ref startNum, g);
                i++;
            }
            //draw line number
            this.DranNumBg(g);
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private int GetStartNum(int i) {
            if (i == 0)
                return 0;
            var ls = this.pParser.PLineString[i - 1];
            if (ls.IsFurl())
                return ls.IndexY + this.pParser.PPucker.PDictPuckerList[ls.ID].Length;

            return ls.IndexY;
        }

        /// <summary>
        /// 获取Word样式
        /// </summary>
        /// <param name="qMStyles"></param>
        /// <param name="forIndex"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private WFontColor GetWordStyle(Queue<MoreLineStyle> qMStyles, int forIndex, int i) {
            WFontColor forWfc = null;
            if (qMStyles != null && qMStyles.Count > 0) {
                var mStyle = qMStyles.Peek();
                if (qMStyles.Count == 1) {
                    if (mStyle.IndexY != i || (mStyle.IsStart && mStyle.WordsIndex <= forIndex) || (!mStyle.IsStart && mStyle.WordsIndex >= forIndex))
                        forWfc = mStyle.PFontColor;
                } else {
                    if ((mStyle.IsStart && mStyle.WordsIndex <= forIndex) || (!mStyle.IsStart && mStyle.WordsIndex >= forIndex)) {
                        forWfc = mStyle.PFontColor;
                        if (mStyle.WordsIndex == forIndex)
                            qMStyles.Dequeue();
                    }
                }
            }

            return forWfc;
        }
        /// <summary>
        /// 隐藏的多行样式
        /// </summary>
        private Stack<MoreLineStyle> pHideMoreLineStyle = new Stack<MoreLineStyle>();

        /// <summary>
        /// 多行样式
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="isStart"></param>
        private Queue<MoreLineStyle> DrawMoreStyle(LineString ls, int i) {
            var last = pHideMoreLineStyle.GetPeek();
            if (last != null && !last.IsStart)
                last = null;

            if (ls.PMoreLineStyles.IsNull())
                return last.ToQueue();

            if (ls.PMoreLineStyles.Count == 1) {
                var mLS = ls.PMoreLineStyles[0];
                pHideMoreLineStyle.Push(mLS);
                mLS.IndexY = i;
                if (mLS.IsStart) {
                    if (last != null && last.IsStart)
                        return last.ToQueue();
                    return mLS.ToQueue();
                } else {
                    if (last != null)
                        mLS.PFontColor = last.PFontColor;
                    return mLS.ToQueue();
                }
            } else {
                var q = new Queue<MoreLineStyle>();
                var isStart = last != null && last.IsStart;
                foreach (var mls in ls.PMoreLineStyles) {
                    if (isStart && mls.IsStart)
                        continue;
                    if (isStart && !mls.IsStart)
                        isStart = false;
                    if (mls.IsStart)
                        isStart = true;
                    pHideMoreLineStyle.Push(mls);
                    mls.IndexY = i;
                    q.Enqueue(mls);
                }
                if (q.Count > 0)
                    return q;
            }
            return last.ToQueue();
        }

        private void ClearDrawMoreStyle(int startI) {
        Start:
            var last = pHideMoreLineStyle.GetPeek();
            if (last != null && last.IndexY >= startI) {
                pHideMoreLineStyle.Pop();
                goto Start;
            }
        }

        private int _dianWidth = 0;
        /// <summary>
        /// 隐藏部分宽度
        /// </summary>
        public int GetHidePartWidth {
            get {
                return _dianWidth * 3;
            }
        }
        private SolidBrush findBackGoundBrush;
        private SolidBrush findSelectBackGroundBrush;
        /// <summary>
        /// 查找到的文本位置
        /// </summary>
        public Dictionary<int, FindTextLocation> FindDatas = new Dictionary<int, FindTextLocation>();

        private FindTextLocation GetFindTextLocation(LineString ls) {
            if (this.FindDatas.Count > 0) {
                FindTextLocation dRect;
                if (this.FindDatas.TryGetValue(ls.ID, out dRect)) {
                    if (this.findBackGoundBrush == null) {
                        this.findBackGoundBrush = new SolidBrush(this.pParser.PIEdit.FindBackGroundColor);
                        this.findSelectBackGroundBrush = new SolidBrush(this.pParser.PIEdit.FindSelectBackGroundColor);
                    }
                    dRect.NextFindIndex = 0;
                    return dRect;
                }
            }
            return null;
        }

        private void DrawFind(FindTextLocation ftl, int i, int leftIndex, int leftWidth, Word word, Font font, Graphics g) {
            if (ftl == null)
                return;



            FindTextLocation fineTextLocation = null;
            if (ftl.NextFindIndex == 0)
                fineTextLocation = ftl;
            else {
                if (ftl.NextFindIndex - 1 >= ftl.NextFindTextLocations.Count)
                    return;

                fineTextLocation = ftl.NextFindTextLocations[ftl.NextFindIndex - 1];
            }

            int x = leftWidth;
            int width = 0;
            if (leftIndex + word.Length > fineTextLocation.IndexX && leftIndex < fineTextLocation.IndexX + fineTextLocation.FineString.Length) {
            
                if (leftIndex <= fineTextLocation.IndexX ) { 
                    if (leftIndex + word.Length >= fineTextLocation.FineString.Length + fineTextLocation.IndexX && ftl.NextFindTextLocations.IsNotNull())
                        ftl.NextFindIndex++; 

                    if (leftIndex < fineTextLocation.IndexX)
                        x += CharCommand.GetCharWidth(g, word.Text.Substring(0, fineTextLocation.IndexX - leftIndex), font);

                    if (word.PEWordType == EWordType.Space || word.PEWordType == EWordType.Tab)
                        width = CharCommand.GetWrodWidth(word, g, this.pParser.PLanguageMode.TabSpaceCount);
                    else
                        width = CharCommand.GetCharWidth(g, word.Text.Substring(fineTextLocation.IndexX - leftIndex,
                             Math.Min(fineTextLocation.FineString.Length, word.Text.Length - (fineTextLocation.IndexX - leftIndex)))
                            , font);

                } else if (fineTextLocation.IndexX + fineTextLocation.FineString.Length < leftIndex + word.Length) { //最后一个Word部分内容
                    if (ftl.NextFindTextLocations.IsNotNull())
                        ftl.NextFindIndex++;

                    width = CharCommand.GetCharWidth(g, word.Text.Substring(0, fineTextLocation.IndexX + fineTextLocation.FineString.Length - leftIndex), font);

                } else {
                    if (word.Width > 0)
                        width = word.Width;
                    else {
                        word.Width = CharCommand.GetWrodWidth(word, g, font, this.pParser.PLanguageMode.TabSpaceCount);
                        width = word.Width;
                    }
                }
            }
            if (width == 0)
                return;

            var findBrush = this.findBackGoundBrush;
            var rect = new Rectangle(
                                 x,
                                 i * FontContainer.FontHeight - this.pParser.PIEdit.GetVerticalScrollValue,
                                 width,
                                 FontContainer.FontHeight
                                );
            if (this.IsSelectPart) {
            }

            g.FillRectangle(findBrush, rect);

        }

        
        /// <summary>
        /// 设置空格和制表符背景画刷
        /// </summary>
        private static SolidBrush _tapSpaceBackGoundColorBrush = new SolidBrush(Color.Transparent);

        private int DrawWrod(Graphics g, Word w, WFontColor defFont, int allWidth, int y) {
            var f = CharCommand.GetFont(w, defFont);
            var c = CharCommand.GetColor(w, defFont);
            TextRenderer.DrawText(g, w.Text.Trim(), f, new Point(allWidth, y), c, CharCommand.CTextFormatFlags);
            if (w.Width == 0)
                w.Width = CharCommand.GetCharWidth(g, w.Text, f);
            return w.Width;
        }

        /// <summary>
        /// 绘制制表符和空格
        /// </summary>
        /// <param name="g"></param>
        /// <param name="w"></param>
        /// <param name="allWidth"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private int DrawTabSpace(Graphics g, Word w, int allWidth, float y) {
            int width = CharCommand.GetWrodWidth(w, g, this.pParser.PLanguageMode.TabSpaceCount);
            w.Width = width;
            return width;
        }

        
        public int PNumRowWidth { get { return pNumRowWidth; } }

        private SolidBrush _leftNumBackGroundColorBrush;
        private Pen _leftNumSeparatorColorPen;
        private SolidBrush _leftNumColorBrush;
        private int pNumRowWidth;
        private int pLsCount = -1;
        private int pNumWidth;
        private int pAllNumWidth;
        /// <summary>
        /// 获取右侧行号宽度
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        private int GetLeftNumWidth(Graphics g) {
            int maxCount = this.pParser.GetCount() + this.pParser.PPucker.PuckerCount;
            if (!(pLsCount == maxCount && pNumRowWidth > 0)) {
                pAllNumWidth = maxCount.ToString().Length;
                pNumWidth = CharCommand.GetCharWidth(g, "4", FontContainer.DefaultFont);
                pNumRowWidth = pNumWidth * pAllNumWidth + pNumWidth;
                pLsCount = this.pParser.GetCount();
            }
            return pNumRowWidth;
        }



        /// <summary>
        /// 绘制右侧行背景
        /// </summary>
        /// <param name="top"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="g"></param>
        private void DranNumBg(Graphics g) {
            int width = this.GetLeftNumWidth(g);
            if (_leftNumBackGroundColorBrush == null) {
                _leftNumBackGroundColorBrush = new SolidBrush(this.pParser.PIEdit.LeftNumBackGroundColor);
                _leftNumColorBrush = new SolidBrush(this.pParser.PIEdit.LeftNumColor);
            }

            g.FillRectangle(_leftNumBackGroundColorBrush, new Rectangle(0, 0,
            width,
            this.pParser.PIEdit.GetHeight));

            if (this._leftNumSeparatorColorPen == null)
                this._leftNumSeparatorColorPen = new Pen(new SolidBrush(this.pParser.PIEdit.LeftNumSeparatorColor));

            g.DrawLine(this._leftNumSeparatorColorPen, width, 0, width, this.pParser.PIEdit.GetHeight);

            var index = 0;
            foreach (var startNum in pDrawNum) {
                var showNum = startNum.ToString();
                int leftX = (pAllNumWidth - showNum.Length) * pNumWidth;
                g.DrawString(showNum, FontContainer.DefaultFont, this._leftNumColorBrush, new PointF(leftX, index * FontContainer.FontHeight));
                index++;
            }


        }

        /// <summary>
        /// 设置Line行属性，绘制行号码
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="startNum"></param>
        private void SetLineIndexYAndDrawNum(LineString ls, int index, ref int startNum, Graphics g) {


            startNum++;
            ls.IndexY = startNum;
            pDrawNum.Add(startNum);
            if (ls.IsFurl()) {
                GetHidePuckerCount(ls.ID, ref startNum);
            }
        }

        private void GetHidePuckerCount(int id, ref int count) {
            LineString[] outArray;
            if (this.pParser.PPucker.PDictPuckerList.TryGetValue(id, out outArray)) {
                count += outArray.Length;
                foreach (var l in outArray) {
                    if (l.IsFurl())
                        GetHidePuckerCount(l.ID, ref count);

                }
            }
        }

        private SolidBrush _selectBackGoundBrush;
        private void DrawSelectBackGround(int yIndex, int count, Graphics g) {
            if (this._selectBackGoundBrush == null)
                this._selectBackGoundBrush = new SolidBrush(this.pParser.PIEdit.SelectBackGroundColor);
            if (!this.IsSelectPart)
                return;

            int startY = Math.Max(yIndex, this.pBGStartPoint.Y);
            int endY = Math.Min(this.pBgEndPoint.Y, yIndex + count);
            if (startY >= yIndex && endY <= count + yIndex) {
                if (startY == endY) {
                    var rect = new Rectangle(this.pBGStartPoint.X - this.pParser.PIEdit.GetHorizontalScrollValue,
                        startY - this.pParser.PIEdit.GetVerticalScrollValue,
                        this.pBgEndPoint.X - this.pBGStartPoint.X,
                        FontContainer.FontHeight);
                    g.FillRectangle(this._selectBackGoundBrush, rect);
                } else {
                    var i = startY;
                    while (i <= endY) {
                        if (i == this.pBGStartPoint.Y) {
                            g.FillRectangle(this._selectBackGoundBrush, new Rectangle(
                                this.pBGStartPoint.X - this.pParser.PIEdit.GetHorizontalScrollValue,
                                startY - this.pParser.PIEdit.GetVerticalScrollValue,
                                Math.Max(this.pBGStartPoint.LineWidth - this.pBGStartPoint.X + this.GetLeftSpace, FontContainer.GetSpaceWidth(g)),
                                FontContainer.FontHeight
                                ));
                        } else if (i == pBgEndPoint.Y) {
                            g.FillRectangle(this._selectBackGoundBrush, new Rectangle(
                                this.GetLeftSpace,
                                this.pBgEndPoint.Y - this.pParser.PIEdit.GetVerticalScrollValue,
                                Math.Max(this.pBgEndPoint.X - this.pParser.PIEdit.GetHorizontalScrollValue - this.GetLeftSpace, FontContainer.GetSpaceWidth(g)),
                                FontContainer.FontHeight
                                ));
                        } else {
                            var w = Math.Max(CharCommand.GetLineStringWidth(this.pParser.PLineString[i / FontContainer.FontHeight], g, this.pParser.PLanguageMode.TabSpaceCount), FontContainer.GetSpaceWidth(g)) - this.pParser.PIEdit.GetHorizontalScrollValue;
                            g.FillRectangle(this._selectBackGoundBrush, new Rectangle(
                                 this.GetLeftSpace,
                                 i - this.pParser.PIEdit.GetVerticalScrollValue,
                                 w,
                                 FontContainer.FontHeight
                                ));
                        }
                        i += FontContainer.FontHeight;
                    }
                }
            }
        }


        /// <summary>
        /// 绘制找到的区域
        /// </summary>
        /// <param name="yIndex"></param>
        /// <param name="count"></param>
        /// <param name="g"></param>
        private void DrawFindDrawRects(int yIndex, int count, Graphics g) {
            if (this.PFindDrawRects.Count == 0)
                return;

            foreach (var rect in this.PFindDrawRects) {
                if (rect == null)
                    continue;
                if (rect.StartY == rect.EndY) {
                    if (!(rect.StartY >= yIndex && rect.StartY <= count + yIndex))
                        continue;
                    var brush = this._selectBackGoundBrush;
                    if (rect.PSolidBrush != null)
                        brush = rect.PSolidBrush;

                    g.FillRectangle(brush, new Rectangle(rect.StartX - this.pParser.PIEdit.GetHorizontalScrollValue,
                       rect.StartY * FontContainer.FontHeight - this.pParser.PIEdit.GetVerticalScrollValue,
                       rect.EndX - rect.StartX,
                       FontContainer.FontHeight));
                }
            }

        }


        private SolidBrush _drawSelectLineBrush;
        private Pen _drawSelectLinePen;
        /// <summary>
        /// 绘制选择的行
        /// </summary>
        public void DrawSelectLine(Graphics g) {
            if (this.pParser.GetSelectLineIndex < 0)
                return;

            if (this._drawSelectLineBrush == null)
                this._drawSelectLineBrush = new SolidBrush(this.pParser.PIEdit.SelectLineColor);
            var rect = new Rectangle(this.GetLeftSpace,
               this.pParser.GetSelectLineIndex - this.pParser.PIEdit.GetVerticalScrollValue,
               this.pParser.PIEdit.GetWidth - this.GetLeftSpace,
               FontContainer.FontHeight);
            switch (this.pParser.PIEdit.SelectLineStyle) {
                case ESelectLineStyle.Broder:
                    if (this._drawSelectLinePen == null)
                        this._drawSelectLinePen = new Pen(this._drawSelectLineBrush, 2);
                    g.DrawRectangle(this._drawSelectLinePen, rect);
                    break;
                case ESelectLineStyle.Fill:
                    g.FillRectangle(this._drawSelectLineBrush, rect);
                    break;
            }
        }
        

        


        /// <summary>
        /// 设置标记
        /// </summary>
        /// <param name="ls"></param>
        private void SetPuckerMarker(LineString ls, int y) {
            if (this.pParser.PLanguageMode.Range == null)
                return;
            if (ls.IsRange()) {
                this.pParser.PPucker.PuckerMarker.Add(new PuckerMarker() {
                    PositionY = y * FontContainer.FontHeight - this.pParser.PIEdit.GetVerticalScrollValue,
                    IsStart = ls.IsStartRange(),
                    ID = ls.ID,
                    IsFurl = ls.IsFurl(),
                    IndexY = y
                }
                  );
            }
        }

        
    }
}


