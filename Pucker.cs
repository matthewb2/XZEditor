using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XZ.Edit.Actions;
using XZ.Edit.Entity;
using XZ.Edit.Interfaces;

namespace XZ.Edit {
    /// <summary>
    /// 折叠
    /// </summary>
    public class Pucker {

        private Parser pParser;
        private IEdit pIEdit;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parser"></param>
        public Pucker(Parser parser) {
            this.pParser = parser;
            this.pIEdit = parser.PIEdit;
            this.InitSelectPuckerStartEndY();

        }

        /// <summary>
        /// 折叠标记
        /// </summary>
        public List<PuckerMarker> PuckerMarker = new List<PuckerMarker>();

        /// <summary>
        /// 折叠个数
        /// </summary>
        public int PuckerCount { get; set; }

        /// <summary>
        /// 折叠的内容
        /// </summary>
        public Dictionary<int, LineString[]> PDictPuckerList = new Dictionary<int, LineString[]>();

        public Dictionary<int, Tuple<string, Word[]>> PDictPuckerLeavings = new Dictionary<int, Tuple<string, Word[]>>();

        public Dictionary<int, CoupleProperty> PCoupleProperty = new Dictionary<int, CoupleProperty>();

        /// <summary>
        /// 获取下一个节点
        /// </summary>
        /// <param name="lnp"></param>
        /// <returns></returns>
        public LineNodeProperty GetNextNode(ref int y) {
            y++;
        Start:
            if (y >= this.pParser.GetCount())
                return null;
    
            var nextLNP = this.pParser.PLineString[y].PLNProperty;
            if (nextLNP == null) {
                y++;
                goto Start;
            }
            return nextLNP;
        }

        /// <summary>
        /// 获取上一个节点
        /// </summary>
        /// <param name="lnp"></param>
        /// <returns></returns>
        public LineNodeProperty GetUpNode(ref int y) {
            y--;
        Start:
            if (y < 0)
                return null;
            var upLNP = this.pParser.PLineString[y].PLNProperty;
            if (upLNP == null) {
                y--;
                goto Start;
            }
            return upLNP;
        }


        public Tuple<string, LineString> GetPuckerLsText(LineString ls, List<PuckerLineStringAndID> list, int y, ref int puckerCount) {
            if (!ls.IsFurl())
                return null;
            list.Add(new PuckerLineStringAndID(true, puckerCount + y));

            StringBuilder sbText = new StringBuilder();
            LineString[] puckerArray;
            LineString lastCHilds = null;
            if (PDictPuckerList.TryGetValue(ls.ID, out puckerArray)) {
                lastCHilds = puckerArray.Last();
                foreach (var pa in puckerArray) {
                    puckerCount++;
                    sbText.AppendLine(pa.Text);
                    this.GetPuckerLsText(pa, sbText, list, y, ref puckerCount);
                }
            }
            return Tuple.Create(sbText.ToString(), lastCHilds);
        }

        public void GetPuckerLsText(LineString ls, StringBuilder sbText) {
            int puckerCount = 0;
            GetPuckerLsText(ls, sbText, null, 0, ref puckerCount);
        }

        /// <summary>
        /// 获取折叠部分内容
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="sbText"></param>
        /// <param name="list"></param>
        /// <param name="y"></param>
        /// <param name="puckerCount"></param>
        public void GetPuckerLsText(LineString ls, StringBuilder sbText, List<PuckerLineStringAndID> list, int y, ref int puckerCount) {
            if (!ls.IsFurl())
                return;
            if (list != null)
                list.Add(new PuckerLineStringAndID(true, puckerCount + y));
            LineString[] puckerArray;
            if (PDictPuckerList.TryGetValue(ls.ID, out puckerArray)) {
                foreach (var pa in puckerArray) {
                    puckerCount++;
                    sbText.AppendLine(pa.Text + this.PDictPuckerLeavings.GetText(pa.ID));
                    this.GetPuckerLsText(pa, sbText, list, y, ref puckerCount);
                }
            }
        }

        /// <summary>
        /// 是否点中了折叠部分
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void ClickPucker(int x, int y) {
            if (this.pParser.PLanguageMode.Range == null)
                return;
            this.InitSelectPuckerStartEndY();
            y = (y + this.pIEdit.GetVerticalScrollValue) / FontContainer.FontHeight;
            var ls = this.pParser.PLineString[y];
            if (x > this.pParser.GetLeftSpace - this.pParser.PPuckerWidth && ls.PLNProperty != null && ls.PLNProperty.IsStartRange) {
                ClickPucker(ls, y);
                this.pIEdit.Invalidate();
            }
        }

        private int GetFurlLength(LineString ls, int y) {
            int showMarker = 0;
            bool findLs = false;
            int length = 0;
            int lastY = 0;

            if (this.PuckerMarker.Count == 0 || this.PuckerMarker.First().IndexY > y) {
                length = y;
                lastY = y;
            } else {
                for (var i = 0; i < this.PuckerMarker.Count; i++) {
                    var pm = this.PuckerMarker[i];
                    if (pm.IsFurl)
                        continue;
                    if (pm.ID == ls.ID) {
                        findLs = true;
                        length = pm.IndexY;
                        if (i == this.PuckerMarker.Count - 1)
                            lastY = y;
                        continue;
                    }
                    if (findLs) {
                        lastY = pm.IndexY;
                        if (pm.IsStart)
                            showMarker++;
                        else if (showMarker == 0)
                            return pm.IndexY - length;
                        else
                            showMarker--;
                    }
                }
            }
            lastY++;
            for (; lastY < this.pParser.PLineString.Count; lastY++) {
                if (this.pParser.PLineString[lastY].PLNProperty == null || this.pParser.PLineString[lastY].IsFurl())
                    continue;
                if (this.pParser.PLineString[lastY].IsStartRange())
                    showMarker++;
                else if (this.pParser.PLineString[lastY].IsEndRange()) {
                    if (showMarker == 0)
                        return lastY - length;
                    else
                        showMarker--;
                }
            }

            return this.pParser.PLineString.Count - y - 1;
        }

        /// <summary>
        /// 点击展开或收起折叠部分
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="y"></param>
        public void ClickPucker(LineString ls, int y) {
            this.ClickPucker(ls, y, true, false);
        }

        /// <summary>
        /// 点击展开或收起折叠部分
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="y"></param>
        /// <param name="isAddAction"></param>
        public void ClickPucker(LineString ls, int y, bool isAddAction) {
            this.ClickPucker(ls, y, isAddAction, false);
        }

        /// <summary>
        /// 点击折叠展开
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="y"></param>
        public void ClickPuckerUnfurl(LineString ls, int y, bool changePuckerMarker = false) {
            var isFurl = false;
            if (!ls.IsCommentPucker) {
                ls.PLNProperty.IsFurl = !ls.PLNProperty.IsFurl;
                isFurl = ls.PLNProperty.IsFurl;
            }
            if (isFurl) {
                var length = GetFurlLength(ls, y);
                ChangeSelectBackgroundHide(y, length);
                if (changePuckerMarker)
                    ChangePuckerMarker(y, length);
                if (length > 0) {
                    this.pParser.ClearCouple();
                    this.RemoveLineString(ls, y, length);
                    this.HideCurosr(y, length, ls);
                }
            } else {
                var length = PDictPuckerList[ls.ID].Length;
                this.ChangeSelectBackgroundShow(y, length);
                this.pParser.ClearCouple();
                this.AddLineString(ls, y);
                ShowCurosr(y, length);
            }
            this.pParser.PIEdit.ChangeScollSize();
        }

        private CPoint pHideEndPoint;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <param name="length"></param>
        private void ChangeSelectBackgroundHide(int yIndex, int length) {
            this.pHideEndPoint = null;
            if (this.pParser.GetSelectPartPoint != null) {
                var startPoint = this.pParser.GetSelectPartPoint[0];
                var endPoint = this.pParser.GetSelectPartPoint[1];
                int y = yIndex * FontContainer.FontHeight;
                if (endPoint.Y > y && startPoint.Y <= y) {
                    pHideEndPoint = endPoint.Create();
                    var ls = this.pParser.PLineString[yIndex];
                    if (endPoint.Y > y + length * FontContainer.FontHeight) {
                        endPoint.Y -= length * FontContainer.FontHeight;
                    } else {
                        int hLength = 0, hWidth = 0;
                        hLength = GetLineStringPuckerHideLength(ls, out hWidth);
                        endPoint.Y = y;
                        endPoint.X = this.pParser.GetLeftSpace + ls.Width - hWidth;
                        endPoint.LineWidth = ls.Width - hWidth;
                        endPoint.LineStringIndex = ls.Length - 1 - hLength;
                    }
                }
            }
        }

        private void ChangeSelectBackgroundShow(int yIndex, int length) {
            if (this.pParser.GetSelectPartPoint == null)
                return;

            var startPoint = this.pParser.GetSelectPartPoint[0];
            var endPoint = this.pParser.GetSelectPartPoint[1];
            int y = yIndex * FontContainer.FontHeight;
            if (endPoint.Y >= y && startPoint.Y < y) {
                var ls = this.pParser.PLineString[yIndex];
                if (endPoint.Y == y) {
                    if (endPoint.LineStringIndex == ls.Length - 1 && this.pHideEndPoint != null)
                        this.pParser.SetBgEndPoint(this.pHideEndPoint);
                } else
                    endPoint.Y += length * FontContainer.FontHeight;
            }
        }

        /// <summary>
        /// 获取Line 折叠之后隐藏的宽度和长度
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="width">宽度</param>
        /// <returns>长度</returns>
        public int GetLineStringPuckerHideLength(LineString ls, out int width) {
            width = 0;
            if (ls.IsStartRange()) {
                int length = 0;
                var array = new Word[ls.PWord.Count - ls.PLNProperty.IndexForLineWords];
                ls.PWord.CopyTo(ls.PLNProperty.IndexForLineWords, array, 0, array.Length);
                foreach (var a in array) {
                    length += a.Length;
                    width += a.Width;
                }
                return length;
            }
            return 0;
        }

        /// <summary>
        /// 点击展开或收起折叠部分
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="y"></param>
        /// <param name="isAddAction"></param>
        public void ClickPucker(LineString ls, int y, bool isAddAction, bool changePuckerMarker) {
            var puckerAction = new PuckerAction(this.pParser);
            puckerAction.Pucker(y);
            this.pParser.AddAction(puckerAction);

        }

        private void ChangePuckerMarker(int y, int length) {
            foreach (var pm in this.PuckerMarker) {
                if (pm.IndexY > y)
                    pm.IndexY -= length;
            }
        }

        /// <summary>
        /// 判断是否要隐藏光标
        /// </summary>
        /// <param name="y"></param>
        /// <param name="length"></param>
        private void HideCurosr(int y, int length, LineString ls) {
            if (this.pParser.PCursor.CousorPointForWord.Y > y && this.pParser.PCursor.CousorPointForWord.Y <= y + length) {
                var width = this.pParser.GetLsWidth(ls);
                this.pParser.PCursor.SetPosition(width + this.pParser.GetLeftSpace, y * FontContainer.FontHeight, this.pParser.GetLeftSpace);
                this.pParser.PCursor.CousorPointForWord.Y = y;
                this.pParser.PCursor.CousorPointForWord.X = ls.Length - 1;
                this.pParser.PCursor.SetPosition();
            } else if (this.pParser.PCursor.CousorPointForWord.Y > y + length) {
                this.pParser.PCursor.CousorPointForWord.Y -= length;
                this.pParser.PCursor.CousorPointForEdit.Y -= length * FontContainer.FontHeight;
                this.pParser.PCursor.SetPosition();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <param name="length"></param>
        private void ShowCurosr(int y, int length) {
            if (this.pParser.PCursor.CousorPointForWord.Y > y) {
                this.pParser.PCursor.CousorPointForWord.Y += length;
                length = length * FontContainer.FontHeight;
                this.pParser.PCursor.CousorPointForEdit.Y += length;

                this.pParser.PCursor.SetPosition();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <param name="length"></param>
        private void RemoveLineString(LineString ls, int y, int length) {
            var id = ls.ID;
            RmovePuckerLeavings(ls);
            var copyArray = new LineString[length];
            this.PuckerCount = this.PuckerCount + length;
            this.pParser.PLineString.CopyTo(y + 1, copyArray, 0, copyArray.Length);
            this.pParser.PLineString.RemoveRange(y + 1, copyArray.Length);
            this.PDictPuckerList[id] = copyArray;

        }

        /// <summary>
        /// 将行剩余的部分添加到缓存中，同时删除行中的部分
        /// </summary>
        /// <param name="ls"></param>
        public void RmovePuckerLeavings(LineString ls) {
            HideCoupe(ls);
            var array = new Word[ls.PWord.Count - ls.PLNProperty.IndexForLineWords];
            ls.PWord.CopyTo(ls.PLNProperty.IndexForLineWords, array, 0, array.Length);
            ls.PWord.RemoveRange(ls.PLNProperty.IndexForLineWords, array.Length);
            string text = string.Empty;
            foreach (var a in array)
                text += a.Text;
            ls.Text = ls.Text.Remove(ls.Text.Length - text.Length);
            PDictPuckerLeavings[ls.ID] = Tuple.Create(text, array);
        }

        private void HideCoupe(LineString ls) {
            var index = ls.PLNProperty.IndexForLineString;
            CoupleProperty outCP;
            if (ls.PLNProperty.Couple.TryGetValue(index, out outCP)) {
                ls.PLNProperty.Couple.Remove(index);
                if (outCP.UpNode != null)
                    outCP.UpNode.NextNode = null;

                //return outCP;
                PCoupleProperty[ls.ID] = outCP;
            }
            //return null;
        }

        private void ShowCoupe(LineString ls) {
            CoupleProperty outCP;
            if (PCoupleProperty.TryGetValue(ls.ID, out outCP)) {
                ls.PLNProperty.Couple[ls.PLNProperty.IndexForLineString] = outCP;
                if (outCP.UpNode != null)
                    outCP.UpNode.NextNode = outCP;
                PCoupleProperty.Remove(ls.ID);
            }
        }

        public void RmovePuckerLeavingOnly(LineString ls) {           
            int length = ls.PWord.Count - ls.PLNProperty.IndexForLineWords;
            ls.Text = ls.Text.Remove(ls.PLNProperty.IndexForLineString);
            ls.PWord.RemoveRange(ls.PLNProperty.IndexForLineWords, length);
        }

        /// <summary>
        /// 重缓存中获取行剩余部分，添加到当前行中。并且重缓存中删除当前行
        /// </summary>
        /// <param name="ls"></param>
        private void AddPuckerLeavings(LineString ls) {
            ShowCoupe(ls);
            if (ls.IsCommentPucker) {
                PDictPuckerLeavings.Remove(ls.ID);
                return;
            }
            Tuple<string, Word[]> outValue;
            if (PDictPuckerLeavings.TryGetValue(ls.ID, out outValue)) {
                PDictPuckerLeavings.Remove(ls.ID);
                ls.PWord.AddRange(outValue.Item2);
                ls.Text += outValue.Item1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        private void AddLineString(LineString ls, int y) {
            var id = ls.ID;
            AddPuckerLeavings(ls);
            LineString[] lines;
            if (PDictPuckerList.TryGetValue(id, out lines)) {
                this.PuckerCount = this.PuckerCount - lines.Length;
                this.pParser.PLineString.InsertRange(y + 1, lines);
                PDictPuckerList.Remove(id);
            }
        }

        /// <summary>
        /// 或者折叠部分的内容
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public LineString[] GetHidePucker(Point point, out Rectangle rect) {
            rect = Rectangle.Empty;
            var y = (point.Y + this.pIEdit.GetVerticalScrollValue) / FontContainer.FontHeight;
            
            if (y >= this.pParser.PLineString.Count || y < 0)
               return null;
            
            var ls = this.pParser.PLineString[y];
            if (ls.IsFurl()) {
                int x = ls.Width + this.pParser.GetLeftSpace;
                if (point.X > x && point.X < x + this.pParser.GetHidePartWidth) {
                    rect.X = x;
                    rect.Y = y * FontContainer.FontHeight - this.pIEdit.GetVerticalScrollValue;
                    rect.Width = this.pParser.GetHidePartWidth;
                    rect.Height = FontContainer.FontHeight;
                    return this.PDictPuckerList[ls.ID];
                }
            }

            return null;
        }

        #region 选择
        public int SelectPuckerStartY { get; set; }
        public int SelectPuckerEndY { get; set; }

        public void InitSelectPuckerStartEndY() {
            this.SelectPuckerStartY = -1;
            this.SelectPuckerEndY = -1;
        }


        public void SelectPuckerChange(int y, int num) {
            foreach (var pm in PuckerMarker) {
                if (pm.IndexY > y) {
                    pm.IndexY += num;
                    pm.PositionY += num * FontContainer.FontHeight;
                }
            }
        }

        public void SelectPuckerForLineIndex(int y) {
            if (this.pParser.PLanguageMode.Range == null)
                return;
            this.InitSelectPuckerStartEndY();

            int i = 0;
            for (i = this.PuckerMarker.Count - 1; i > -1; i--) {
                if (this.PuckerMarker[i].IsFurl)
                    continue;
                if (this.PuckerMarker[i].IndexY <= y) {
                    if (this.PuckerMarker[i].IsStart) {
                        this.SelectPuckerStartY = this.PuckerMarker[i].IndexY;
                        SelectFindEnd(i + 1, this.PuckerMarker[i].IndexY);
                    } else {
                        int showCount = 1;
                        if (this.PuckerMarker[i].IndexY == y) {
                            this.SelectPuckerEndY = this.PuckerMarker[i].IndexY;
                            showCount = 0;
                        } else {
                            this.SelectFindEnd(i + 1, this.PuckerMarker[i].IndexY);
                        }
                        this.SelectFindStart(i - 1, this.PuckerMarker[i].IndexY, showCount);
                    }
                    return;
                }
            }

            if (this.PuckerMarker.Count > 0) {
                var first = this.PuckerMarker[0];
                this.SelectFindStart(-1, first.IndexY, 0);
                if (first.IsStart)
                    this.SelectFindEnd(0, first.PositionY);
                else
                    this.SelectPuckerEndY = first.IndexY;

                return;
            }
            i = y;
            i--;
            if (!this.pParser.PLineString.ValidListIndex(i))
                return;
            for (; i > -1; i--) {
                if (this.pParser.PLineString[i].IsStartRange()) {
                    this.SelectPuckerStartY = i;
                    i = y + this.pIEdit.GetHeight / FontContainer.FontHeight;
                    for (; i < this.pParser.PLineString.Count; i++) {
                        if (this.pParser.PLineString[i].IsEndRange()) {
                            this.SelectPuckerEndY = i;
                            break;
                        }
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// 选择块
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="y"></param>
        public void SelectPucker(int y) {
            y = (y + this.pIEdit.GetVerticalScrollValue) / FontContainer.FontHeight;
            SelectPuckerForLineIndex(y);
        }

        /// <summary>
        /// 查找结束标志
        /// </summary>
        /// <param name="y"></param>
        private void SelectFindEnd(int i, int y) {
            int showCount = 0;
            for (; i < this.PuckerMarker.Count; i++) {
                if (this.PuckerMarker[i].IsFurl)
                    continue;
                y = this.PuckerMarker[i].IndexY;
                if (this.PuckerMarker[i].IsStart)
                    showCount++;
                else {
                    if (showCount == 0) {
                        SelectPuckerEndY = y;
                        return;
                    } else
                        showCount--;
                }
            }

            y++;
            for (; y < this.pParser.PLineString.Count; y++) {
                if (this.pParser.PLineString[y].IsFurl())
                    continue;
                if (this.pParser.PLineString[y].IsEndRange()) {
                    if (showCount == 0) {
                        SelectPuckerEndY = y;
                        return;
                    } else
                        showCount--;
                } else if (this.pParser.PLineString[y].IsStartRange())
                    showCount++;
            }

            SelectPuckerEndY = this.pParser.PLineString.Count - 1;
        }

        /// <summary>
        /// 找到开始位置
        /// </summary>
        /// <param name="i"></param>
        private void SelectFindStart(int i, int y, int showCount) {
            for (; i > -1; i--) {
                if (this.PuckerMarker[i].IsFurl)
                    continue;
                y = this.PuckerMarker[i].IndexY;
                if (!this.PuckerMarker[i].IsStart)
                    showCount++;
                else {
                    if (showCount == 0) {
                        SelectPuckerStartY = y;
                        return;
                    } else
                        showCount--;
                }
            }
            y--;
            for (; y > -1; y--) {
                if (this.pParser.PLineString[y].IsFurl())
                    continue;
                if (this.pParser.PLineString[y].IsStartRange()) {
                    if (showCount == 0) {
                        SelectPuckerStartY = y;
                        return;
                    } else
                        showCount--;
                } else if (this.pParser.PLineString[y].IsEndRange())
                    showCount++;
            }


            this.SelectPuckerStartY = -1;
        }

        #endregion
    }
}
