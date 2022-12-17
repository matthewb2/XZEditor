using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using XZ.Edit.Entity;
using XZ.Edit.Interfaces;

namespace XZ.Edit {
    public class LineColsProperty {


        private List<MoreLineProperty> PMoreStyle = new List<MoreLineProperty>();

        public void ClearPMoreStyle(int y) {
            foreach (var start in PMoreStyle)
                if (start.Y == y) {
                    PMoreStyle.Remove(start);
                    break;
                }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <param name="value"></param>
        public void AddMoreStyle(int y, MoreLineProperty value) {
            //var f = PMoreStyle.FirstOrDefault(F => F.StartY == y);
            //if (f != null)
            //    PMoreStyle.Remove(f);
            //for (var i = 0; i < PMoreStyle.Count; i++) { 
            //    PMoreStyle
            //}
            if (PMoreStyle.Count == 0) {
                PMoreStyle.Add(value);
                return;
            }
            var i = 0;
            while (i < PMoreStyle.Count) {
                var info = PMoreStyle[i];
                if (info.Y == y) {
                    PMoreStyle.RemoveAt(i);
                    info = PMoreStyle[i++];
                }
                if (y < info.Y) {
                    PMoreStyle.Insert(i, value);
                    return;
                }
                i++;
            }
            PMoreStyle.Add(value);

        }

        public MoreLineProperty FindMoreStyle(int y) {
            int i = 0;
            while (i < PMoreStyle.Count) {
                var info = PMoreStyle[i];
                if (info.Y <= y) {
                    var result = info.Create();
                    i++;
                    while (i < PMoreStyle.Count) {
                        if (PMoreStyle[i].TagString == result.TagString) {
                            result.EndY = PMoreStyle[i].Y;
                            result.EndPWordIndex = PMoreStyle[i].EndPWordIndex;
                            break;
                        }
                        i++;
                    }
                    return result;
                }
                i++;
            }
            return null;
        }

        #region 无效
        //private Dictionary<int, MoreLineProperty> PMoreStartStyle = new Dictionary<int, MoreLineProperty>();
        //private Dictionary<int, MoreLineProperty> PMoreEndStyle = new Dictionary<int, MoreLineProperty>();
        //private List<MoreLineProperty> PListMoreStyle = new List<MoreLineProperty>();
        //public void ClearPMoreStyle(int y) {
        //    //if (PMoreStartStyle.ContainsKey(y))
        //    //    PMoreStartStyle.Remove(y);
        //    MoreLineProperty outStartValue;
        //    if (PMoreStartStyle.TryGetValue(y, out outStartValue)) {
        //        PMoreStartStyle.Remove(y);
        //        PListMoreStyle.Remove(outStartValue);
        //    }
        //    MoreLineProperty outEndValue;
        //    if (PMoreEndStyle.TryGetValue(y, out outEndValue)) {
        //        PMoreEndStyle.Remove(y);
        //        if (PMoreStartStyle.ContainsKey(outEndValue.StartY))
        //            PMoreStartStyle.Remove(outEndValue.StartY);
        //    }
        //}


        //public MoreLineProperty GetMoreStyle(int y) {
        //    MoreLineProperty outValue;
        //    PMoreStartStyle.TryGetValue(y, out outValue);
        //    return outValue;
        //}

        //public void AddMoreStartStyle(int y, MoreLineProperty value) {
        //    MoreLineProperty outStartValue;
        //    if (PMoreStartStyle.TryGetValue(y, out outStartValue)) {
        //        //PMoreStartStyle.Remove(y);
        //        //PListMoreStyle.Remove(outStartValue);
        //        outStartValue.PWFontColor = value.PWFontColor;
        //        outStartValue.StartPWordIndex = value.StartPWordIndex;
        //        outStartValue.StartY = value.StartY;
        //        //outStartValue.Y = value.Y;
        //        outStartValue.EndPWordIndex = value.EndPWordIndex;
        //        outStartValue.EndY = value.EndY;
        //    } else {
        //        PMoreStartStyle.Add(y, value);
        //        PListMoreStyle.Add(value);
        //    }

        //    if (PListMoreStyle.Count > 1)
        //        PListMoreStyle = PListMoreStyle.OrderByDescending(o => o.EndY).ToList();
        //    //if (PMoreStartStyle.ContainsKey(y)) {

        //    //} else
        //    //    PMoreStartStyle.Add(y, value);
        //}

        //public void AddMoreEndStyle(int y, MoreLineProperty value) {
        //    if (PMoreEndStyle.ContainsKey(y))
        //        PMoreEndStyle[y] = value;
        //    else
        //        PMoreEndStyle.Add(y, value);
        //}


        //public MoreLineProperty FindMoreStyle(int y) {
        //    foreach (var m in this.PListMoreStyle) {
        //        if (m.StartY <= y)
        //            return m;
        //    }
        //    return null;
        //}

        #endregion
        //private Dictionary<int, ColsProperty> pColsProperty = new Dictionary<int, ColsProperty>();


        //public void Set(int x, int y) { 

        //}

        //public ColsProperty Set(int y) {

        //    return null;
        //}

    }

    public class MoreLineProperty : IProperty {
        public MoreLineProperty() {
            EndY = -1;
        }
        //public int PStartX { get; set; }
        public string TagString { get; set; }
        public int StartY { get; set; }
        public WFontColor PWFontColor { get; set; }
        public int EndY { get; set; }
        public int StartPWordIndex { get; set; }
        public int EndPWordIndex { get; set; }
        public EColsProperty PEColsProperty { get; set; }
        public int Y {
            get {
                if (PEColsProperty == EColsProperty.Start)
                    return StartY;
                else
                    return EndY;
            }
        }

        public MoreLineProperty Create() {
            return new MoreLineProperty() {
                TagString = this.TagString,
                StartY = this.StartY,
                PWFontColor = this.PWFontColor,
                StartPWordIndex = this.StartPWordIndex,
                PEColsProperty = this.PEColsProperty
            };
        }
    }

    public class FindProperty {
        public IProperty LeftNode { get; set; }
        //public 
    }
    public class ColsProperty {

        public EColsProperty PEColsProperty { get; set; }
        public int X { get; set; }
        public ColsProperty Up { get; set; }
        public ColsProperty Last { get; set; }
    }

    public enum EColsProperty {
        Start,
        End
    }
}
