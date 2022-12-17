using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using XZ.Edit.Entity;

namespace XZ.Edit {
    public static class ToObject {
        public const char NullChar = '\0';

        /// <summary>
        /// 获取第一个字符串
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static char GetFirst(this string text) {
            if (string.IsNullOrEmpty(text))
                return NullChar;
            else
                return text[0];
        }

        /// <summary>
        /// 获取最后一个字符
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static char GetLast(this string text) {
            if (string.IsNullOrEmpty(text))
                return NullChar;
            else
                return text.Last();
        }

        /// <summary>
        /// 创建一个新的对象
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Point Create(this Point point) {
            return new Point(point.X, point.Y);
        }

        /// <summary>
        /// 创建一个新的对象
        /// </summary>
        /// <param name="wfcolor"></param>
        /// <returns></returns>
        public static WFontColor Create(this WFontColor wfcolor) {
            return new WFontColor(wfcolor.PFont.FontFamily.Name, wfcolor.PFont.Size, wfcolor.PFont.Style, wfcolor.PColor);
        }

        public static bool IsNotNull<T>(this List<T> list) {
            return list != null && list.Count > 0;
        }

        public static bool IsNull<T>(this List<T> list) {
            return !list.IsNotNull();
        }

        /// <summary>
        /// 返回位于 System.Collections.Generic.Stack<T> 顶部的对象但不将其移除。
        /// </summary>
        /// <param name="T"></typeparam>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static T GetPeek<T>(this Stack<T> stack) {
            if (stack != null && stack.Count > 0)
                return stack.Peek();
            return default(T);
        }

        /// <summary>
        /// 转化为字符串类型，如果该对象为NULL对象，则返回默认字符串
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static string ToNullString(this object value, string defValue = "") {
            if (value == null)
                return defValue;
            else
                return value.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ls"></param>
        /// <returns></returns>
        public static Tuple<LineNodeProperty, int, bool> GetLnpAndId(this LineString ls) {
            return Tuple.Create(ls.PLNProperty, ls.ID, ls.IsCommentPucker);
        }


        public static void WriteLog(this List<LineString> ls) {
            var _index = 0;
            foreach (var l in ls) {
                if (l.PLNProperty != null)
                    System.Diagnostics.Debug.WriteLine(_index + " , " + l.PLNProperty.IndexForLineStrings);

                _index++;
            }
        }

        //public static void InsertAfter(this LineNode node, LineString insertLs) {
        //    node.Insert(insertLs);
        //}

        ///// <summary>
        ///// 往后插入一条数据
        ///// </summary>
        ///// <param name="ls"></param>
        ///// <param name="insertLs"></param>
        //public static void InsertAfter(this LineString ls, LineString insertLs) {
        //    ls.PNode.Father.Add(ls, insertLs);
        //}

        ///// <summary>
        ///// 移除后面一项数据
        ///// </summary>
        ///// <param name="ls"></param>
        ///// <param name="removeLs"></param>
        //public static void RemoveAfter(this LineString ls, LineString removeLs, bool upLsIsEndRange) {
        //    if (removeLs.IsStartRange() && upLsIsEndRange)
        //        ls.PNode.Father.Father.Remove(removeLs);
        //    else
        //        ls.PNode.Father.Remove(removeLs);
        //}

        /// <summary>
        /// 是否是开始块
        /// </summary>
        /// <param name="ls"></param>
        /// <returns></returns>
        public static bool IsStartRange(this LineString ls) {
            return ls.PLNProperty != null && ls.PLNProperty.IsStartRange;
        }

        /// <summary>
        /// 是否是结束块
        /// </summary>
        /// <param name="ls"></param>
        /// <returns></returns>
        public static bool IsEndRange(this LineString ls) {
            return ls.PLNProperty != null && ls.PLNProperty.IsEndRange;
        }

        public static bool IsRange(this LineNodeProperty lnp) {
            return lnp != null && (lnp.IsEndRange || lnp.IsStartRange);
        }

        /// <summary>
        /// 是否是块
        /// </summary>
        /// <param name="ls"></param>
        /// <returns></returns>
        public static bool IsRange(this LineString ls) {
            return !(ls.PLNProperty == null || (ls.PLNProperty != null && !ls.PLNProperty.IsStartRange && !ls.PLNProperty.IsEndRange));
        }

        /// <summary>
        /// 是否已经收起
        /// </summary>
        /// <param name="ls"></param>
        /// <returns></returns>
        public static bool IsFurl(this LineString ls) {
            return ls.PLNProperty != null && ls.PLNProperty.IsFurl;
        }


        public static string GetText(this Dictionary<int, Tuple<string, Word[]>> dict, int id) {
            Tuple<string, Word[]> outValue;
            if (dict.TryGetValue(id, out outValue))
                return outValue.Item1;

            return string.Empty;
        }

        public static ResultPuckerListData AppendForPucker(this StringBuilder sbText, LineString ls, string text, int length, Pucker pucker, int y) {
            sbText.Append(text);
            if (ls.IsFurl() && ls.Length <= length + 1) {
                ResultPuckerListData result = new ResultPuckerListData();
                Tuple<string, Word[]> outValue;
                if (pucker.PDictPuckerLeavings.TryGetValue(ls.ID, out outValue)) {
                    sbText.AppendLine(outValue.Item1);
                    result.LineLeavingLength = outValue.Item1.Length;
                    foreach (var w in outValue.Item2)
                        result.LineLeavingWidth += w.Width;
                }
                result.PuckerLineStringAndY = new List<PuckerLineStringAndID>();
                int puckerCount = 0;
                var tupleResult = pucker.GetPuckerLsText(ls, result.PuckerLineStringAndY, y, ref puckerCount);
                if (tupleResult != null) {
                    sbText.Append(tupleResult.Item1.TrimEnd());
                    result.LastChildLS = tupleResult.Item2;
                }
                result.PuckerCount = puckerCount;
                return result;
            }
            return null;
        }

        public static void AppendForPucker(this StringBuilder sbText, LineString ls, Pucker pucker) {
            if (ls.IsFurl()) {
                sbText.AppendLine(ls.Text + pucker.PDictPuckerLeavings.GetText(ls.ID));
            } else
                sbText.AppendLine(ls.Text);
        }

        public static Tuple<int, int> AppendForPucker(this StringBuilder sbText, LineString ls, string text, Pucker pucker) {
            if (ls.IsFurl()) {
                Tuple<string, Word[]> outValue;
                if (pucker.PDictPuckerLeavings.TryGetValue(ls.ID, out outValue)) {
                    sbText.AppendLine(text + outValue.Item1);
                    int width = 0;
                    foreach (var w in outValue.Item2)
                        width += w.Width;

                    return Tuple.Create(outValue.Item1.Length, width);
                }
            } else
                sbText.AppendLine(text);

            return null;
        }

        public static Word GetFristWord(this LineString ls) {
            for (var i = 0; i < ls.PWord.Count; i++) {
                var w = ls.PWord[i];
                if (w.PEWordType == EWordType.Word)
                    return w;
            }
            return null;
        }


        public static Queue<T> ToQueue<T>(this T t) {
            if (t == null)
                return null;
            else {
                var q = new Queue<T>();
                q.Enqueue(t);
                return q;
            }
        }

        public static Word GetNetOrUpWord(this List<Word> list, Word startWord, int direction) {
            int y = startWord.LineIndex + direction;
            while (y > -1 && y < list.Count) {
                var w = list[y];
                if (w.PEWordType == EWordType.Tab || w.PEWordType == EWordType.Space)
                    y = y + direction;
                else
                    return w;
            }
            return null;
        }

        public static bool ValidListIndex<T>(this List<T> list, int index) {
            return index > -1 && index < list.Count;
        }
    }
}
