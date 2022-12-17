using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XZ.Edit {
    public class LineNodeProperty {

        public LineNodeProperty() {
            this.GUID = Guid.NewGuid().ToString();
        }


        public string GUID { get;  set; }

        /// <summary>
        /// 上一个节点
        /// </summary>
        //public LineNodeProperty UpNode { get; set; }

        /// <summary>
        /// 下一个节点
        /// </summary>
        //public LineNodeProperty NextNode { get; set; }

        /// <summary>
        /// 块开始
        /// </summary>
        public bool IsStartRange { get; set; }

        /// <summary>
        ///  块结束
        /// </summary>
        public bool IsEndRange { get; set; }

        /// <summary>
        /// 是否已经收起
        /// </summary>
        public bool IsFurl { get; set; }

        /// <summary>
        /// 行索引
        /// </summary>
        [Obsolete("取消了行，该属性应该可以删除")]
        public int IndexForLineStrings { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int IndexForLineWords { get; set; }

        public int IndexForLineString { get; set; }

        //public int 

        /// <summary>
        /// 成对字符
        /// </summary>
        public Dictionary<int, CoupleProperty> Couple { get; set; }

        //public void AddLineIndex(int value, bool isSelfAdd = true) {
        //    if (isSelfAdd)
        //        this.LineIndex += value;
        //    if (this.NextNode != null)
        //        this.NextNode.AddLineIndex(value);
        //}
    }
}
