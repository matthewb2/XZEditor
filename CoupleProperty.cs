using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XZ.Edit {
    /// <summary>
    /// 成对属性
    /// </summary>
    public class CoupleProperty {

        /// <summary>
        /// 上一个位置
        /// </summary>
        public CoupleProperty UpNode { get; set; }

        /// <summary>
        /// 下一个位置
        /// </summary>
        public CoupleProperty NextNode { get; set; }

        /// <summary>
        /// 当前字符
        /// </summary>
        public char PChar { get; set; }

        /// <summary>
        /// 要查找的字符
        /// </summary>
        public char FindChar { get; set; }
        
        /// <summary>
        /// Word 所在的行索引位置
        /// </summary>
        public int WordIndex { get; set; }

        /// <summary>
        /// 行中所在的位置
        /// </summary>
        public int LineIndex { get; set; }

        //public int Y { get; set; }

        public LineNodeProperty LNProperty { get; set; }

        /// <summary>
        /// 操作的方向
        /// </summary>
        public ECouplePropertyDirection PECouplePropertyDirection { get; set; }

        public CoupleProperty GetNextItem(ECouplePropertyDirection direction) {
            if (direction == ECouplePropertyDirection.Ahead)
                return this.UpNode;
            else
                return this.NextNode;
        }

    }

    public enum ECouplePropertyDirection {
        /// <summary>
        /// 前 
        /// </summary>
        Ahead,
        /// <summary>
        /// 后
        /// </summary>
        Rear
    }
}
