using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using XZ.Edit.Entity;
using XZ.Edit.Interfaces;

namespace XZ.Edit {
    /// <summary>
    /// 设置字符样式
    /// </summary>
    public class CharFontStyle {

        private int lineStringIndex = 0;
        /// <summary>
        /// 设置文本的宽度
        /// </summary>
        /// <param name="ls"></param>
        //public delegate void SetWordWidth(LineString ls);

        private LanguageMode pLanguageMode;
        //private SetWordWidth pSetWordWidth;

        private IEdit pIEdit;

        //public LineColsProperty PLCProperty { get; set; }
        //public LineNodeProperty PNowLNProperty { get; set; }
        //private LineNodeProperty pLineNodeProperty;
        private CoupleProperty pCoupleProperty;
        //private LineNode pRoot;
        //public LineNode PRootNode;
        //private bool IsStart = true;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parser"></param>
        public CharFontStyle(IEdit iedit/*, LineColsProperty lcPro*/) {
            this.pIEdit = iedit;
            //this.PLCProperty = lcPro;
            //this.pSetWordWidth = new SetWordWidth(this.SetWordWidthEvent);
        }

        public void SetLanguageMode(LanguageMode mode) {
            this.pLanguageMode = mode;
        }

        /// <summary>
        /// 获取行字符串
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public List<LineString> GetPLineStrings(string text) {
            if (string.IsNullOrEmpty(text))
                return new List<LineString>();

            //this.pRoot = new LineNode(null);
            //this.PRootNode = this.pRoot;
            var ls = new List<LineString>();
            var array = text.Split(CharCommand.Char_Newline);
            for (var i = 0; i < array.Length; i++)
                ls.Add(this.GetLineString(array[i]/*, i*/));
            //nums.Add(i);


            this.pWordIncluedStyle = null;
            //this.IsStart = false;
            return ls;
        }

        /// <summary>
        /// 清除属性
        /// </summary>
        public void ClearProperty() {
            //this.pLineNodeProperty = null;
            this.pCoupleProperty = null;
        }

        public LineString GetLineString() {
            return new LineString(lineStringIndex++);
        }

        private WordIncluedStyle pWordIncluedStyle;
        //private int pWIStyleY;

        /// <summary>
        /// 获取行字符串
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public LineString GetLineString(string line/*, int y*/) {
            this.pCoupleProperty = null;
            var pUseWord = new List<Tuple<Word, UseWordFont>>();
            line = line.TrimEnd(CharCommand.Char_Enter);
            int length = 0;
            int index = 0;
            Word w;
            WFontColor wfc;
            WordIncluedStyle wiStyle = null;
            LineNodeProperty lnProperty = null;
            var lineString = GetLineString();
            lineString.PWord = new List<Word>();
            lineString.Text = string.Empty;
            while (length < line.Length) {
                bool isSetWFC = true;
                char c = line[length];
                w = new Word();
                switch (c) {
                    case CharCommand.Char_Tab:
                        //for (var i = 0; i < this.pLanguageMode.TabSpaceCount - 1; i++) {
                        //    var wSpace = new Word();
                        //    wSpace.Text = " ";
                        //    wSpace.PEWordType = EWordType.Space;
                        //    lineString.Text += wSpace.Text;
                        //    wSpace.LineIndex = index++;
                        //    lineString.PWord.Add(wSpace);
                        //}
                        //w = new Word();
                        w.PEWordType = EWordType.Space;
                        w.Text = " ";
                        line = line.Substring(0, length) + " ".PadLeft(this.pLanguageMode.TabSpaceCount) + line.Substring(length + 1);
                        break;
                    case CharCommand.Char_Space:
                        //w = new Word();
                        w.PEWordType = EWordType.Space;
                        w.Text = c.ToString();
                        break;
                    default:
                        if (pLanguageMode.CompartChars.Contains(c)) {
                            w.PEWordType = EWordType.Compart;
                            w.Text = c.ToString();
                            if (pLanguageMode.CompartCharFont.TryGetValue(c, out wfc))
                                w.PFont = wfc;
                            #region 包含不换行颜色
                            if (wiStyle == null) {
                                wiStyle = GetIncludeFont(c, wiStyle, line, ref length);
                                if (wiStyle != null) {
                                    w.Text = wiStyle.Text;
                                    wiStyle = SetMoreLineStartStyle(wiStyle, lineString, index);
                                }
                            } else if (wiStyle.IsEndStr) {
                                if (EndIncludeFont(c, wiStyle, w, line, ref length)) {
                                    w.PIncluedFont = wiStyle.PFontColor;
                                    wiStyle = null;
                                    isSetWFC = false;
                                }
                            }
                            SetMoreLineEndStyle(lineString, c, w, line, index, ref length);
                            #endregion
                            #region 是否行属性
                            if (wiStyle == null)
                                lnProperty = GetLineNodeProperty(c, length, index, /*y,*/ lnProperty);
                            #endregion
                        } else {
                            //w = new Word();
                            w.PEWordType = EWordType.Word;
                            w.Text = c.ToString() + GetTag(line, ref length);
                            if (pLanguageMode.WordFonts.TryGetValue(w.Text, out wfc))
                                w.PFont = wfc;

                            if (w.PEWordType == EWordType.Word && this.pIEdit.SetWordStyleEvent != null && w.PFont == null) {
                                w.PFont = this.pIEdit.SetWordStyleEvent(w.Text);
                            }
                        }
                        #region  自定义样式
                        if (pLanguageMode.UseBefore != null) {
                            if (pLanguageMode.UseBefore.ContainsKey(w.Text))
                                pUseWord.Add(Tuple.Create(w, pLanguageMode.UseBefore[w.Text]));
                        }
                        if (pLanguageMode.UseAfter != null) {
                            if (pLanguageMode.UseAfter.ContainsKey(w.Text))
                                pUseWord.Add(Tuple.Create(w, pLanguageMode.UseAfter[w.Text]));
                        }
                        #endregion

                        break;
                }
                lineString.Text += w.Text;
                w.LineIndex = index++;
                if (wiStyle != null && isSetWFC)
                    w.PIncluedFont = wiStyle.PFontColor;
                lineString.PWord.Add(w);
                length++;
            }

            lineString.PLNProperty = lnProperty;
            this.SetUseWordFont(pUseWord, lineString.PWord);
            return lineString;
        }

        private void SetUseWordFont(List<Tuple<Word, UseWordFont>> pUseWord, List<Word> words) {
            if (pUseWord.Count == 0)
                return;

            foreach (var uw in pUseWord) {
                var nextWord = words.GetNetOrUpWord(uw.Item1, uw.Item2.PType == UseWordFontType.After ? -1 : 1);
                if (nextWord == null || nextWord.PFont != null)
                    continue;
                if (uw.Item2.After != null) {
                    var lastWord = words.GetNetOrUpWord(nextWord, 1);
                    var key = lastWord == null ? "#___NULL___#" : lastWord.Text;
                    UseWordFont outUWF;
                    if (uw.Item2.After.TryGetValue(key, out outUWF)) {
                        if (outUWF.PFont != null) {
                            nextWord.PFont = outUWF.PFont;
                            continue;
                        }
                    }
                }
                if (nextWord.PEWordType == EWordType.Word)
                    nextWord.PFont = uw.Item2.PFont;
            }
        }

        /// <summary>
        /// 设置当前行的属性值
        /// </summary>
        /// <param name="c">当前字符</param>
        /// <param name="length">当前字符出现在字符串中的位置</param>
        /// <param name="index">当前字符出现在字符串中WORDS中的位置</param>
        /// <param name="y">当前行</param>
        /// <param name="lnp">当前行属性</param>
        /// <returns></returns>
        private LineNodeProperty GetLineNodeProperty(char c, int length, int index,/* int y,*/ LineNodeProperty lnp) {
            #region 块
            if (this.pLanguageMode.Range != null) {
                if (c == this.pLanguageMode.Range.Item1) {
                    SetLNProperty(/*y,*/ ref lnp);
                    if (lnp.IsEndRange)
                        lnp.IsEndRange = false;
                    else
                        lnp.IsStartRange = true;
                    lnp.IndexForLineWords = index;
                    lnp.IndexForLineString = length;
                } else if (c == this.pLanguageMode.Range.Item2) {
                    SetLNProperty(/*y,*/ ref lnp);
                    if (lnp.IsStartRange)
                        lnp.IsStartRange = false;
                    else
                        lnp.IsEndRange = true;
                    lnp.IndexForLineWords = index;
                    lnp.IndexForLineString = length;
                }
            }
            #endregion
            #region 成对字符
            if (this.pLanguageMode.CoupleStart != null) {
                if (this.pLanguageMode.CoupleStart.ContainsKey(c)) {
                    SetLNProperty(/*y, */ref lnp);
                    SetCouple(lnp, c, this.pLanguageMode.CoupleStart[c], index, length, ECouplePropertyDirection.Rear);
                }
            }
            if (this.pLanguageMode.CoupleEnd != null) {
                if (this.pLanguageMode.CoupleEnd.ContainsKey(c)) {
                    SetLNProperty(/*y, */ref lnp);
                    SetCouple(lnp, c, this.pLanguageMode.CoupleEnd[c], index, length, ECouplePropertyDirection.Ahead);
                }
            }

            #endregion
            return lnp;
        }

        /// <summary>
        /// 设置成对出现字符串
        /// </summary>
        /// <param name="lnpro"></param>
        /// <param name="startC"></param>
        /// <param name="endC"></param>
        /// <param name="index"></param>
        /// <param name="lineIndex"></param>
        private void SetCouple(LineNodeProperty lnpro, char startC, char endC, int index, int lineIndex, ECouplePropertyDirection epd) {
            var coupleProperty = new CoupleProperty() {
                FindChar = endC,
                PChar = startC,
                WordIndex = index,
                LineIndex = lineIndex,
                LNProperty = lnpro,
                PECouplePropertyDirection = epd
            };


            if (lnpro.Couple == null)
                lnpro.Couple = new Dictionary<int, CoupleProperty>();
            lnpro.Couple.Add(lineIndex, coupleProperty);
            if (this.pCoupleProperty == null)
                this.pCoupleProperty = coupleProperty;
            else {
                var last = pCoupleProperty;
                last.NextNode = coupleProperty;
                coupleProperty.UpNode = last;
                this.pCoupleProperty = coupleProperty;
            }
        }

        private void SetLNProperty(/*int y, */ref LineNodeProperty lnPro) {
            if (lnPro == null) {
                lnPro = new LineNodeProperty();
            }
        }

        private WordIncluedStyle SetMoreLineStartStyle(WordIncluedStyle wiStyle, LineString ls, int index) {
            if (wiStyle.IsMoreLine) {
                this.pWordIncluedStyle = wiStyle;
                if (ls.PMoreLineStyles == null)
                    ls.PMoreLineStyles = new List<MoreLineStyle>();
                ls.PMoreLineStyles.Add(new MoreLineStyle() {
                    Tag = wiStyle.Text,
                    WordsIndex = index,
                    PFontColor = wiStyle.PFontColor,
                    IsStart = true
                });
                //pWIStyleY = y;
                //this.PLCProperty.AddMoreStyle(y, new MoreLineProperty() {
                //    StartPWordIndex = index,
                //    PWFontColor = wiStyle.PFontColor,
                //    StartY = y,
                //    TagString = wiStyle.Text,
                //    PEColsProperty = EColsProperty.Start

                //});
                return null;
            }
            return wiStyle;
        }
        private void SetMoreLineEndStyle(LineString ls, char c, Word word, string line, int index, ref int length) {
            if (this.pWordIncluedStyle != null) {
                if (EndIncludeFont(c, this.pWordIncluedStyle, word, line, ref length)) {
                    //this.PLCProperty.AddMoreStyle(y, new MoreLineProperty() {
                    //    PEColsProperty = EColsProperty.End,
                    //    TagString = this.pWordIncluedStyle.Text,
                    //    EndPWordIndex = index,
                    //    EndY = y

                    //});
                    if (ls.PMoreLineStyles == null)
                        ls.PMoreLineStyles = new List<MoreLineStyle>();
                    ls.PMoreLineStyles.Add(new MoreLineStyle() {
                        Tag = this.pWordIncluedStyle.Text,
                        WordsIndex = index,
                        PFontColor = this.pWordIncluedStyle.PFontColor
                    });
                    this.pWordIncluedStyle = null;
                }
            } else if (this.pLanguageMode.PEndWordStyle != null) {
                var endStyle = this.pLanguageMode.PEndWordStyle.Get(c);
                if (endStyle != null) {
                Loop:
                    length++;
                    if (length >= line.Length)
                        return;
                    c = line[length];
                    var nextStyle = endStyle.Get(c);
                    if (nextStyle == null) {
                        length--;
                        return;
                    } else if (nextStyle.IsEnd) {
                        word.Text += c;
                        //this.PLCProperty.AddMoreStyle(y, new MoreLineProperty() {
                        //    PEColsProperty = EColsProperty.End,
                        //    TagString = nextStyle.StartString,
                        //    EndPWordIndex = index,
                        //    EndY = y
                        //});
                        if (ls.PMoreLineStyles == null)
                            ls.PMoreLineStyles = new List<MoreLineStyle>();
                        ls.PMoreLineStyles.Add(new MoreLineStyle() {
                            Tag = nextStyle.StartString,
                            WordsIndex = index
                        });
                    } else
                        goto Loop;

                }
            }
        }

        /// <summary>
        /// 获取包含颜色
        /// </summary>
        /// <param name="c"></param>
        /// <param name="wiStyle"></param>
        /// <param name="line"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private WordIncluedStyle GetIncludeFont(char c, WordIncluedStyle wiStyle, string line, ref int length) {
            if (this.pLanguageMode.PStartWordStyle != null) {
                var ew = this.pLanguageMode.PStartWordStyle.Get(c);
                if (ew != null) {
                    var _ewNext = ew;
                    string w = c.ToString();
                Start:
                    length++;
                    if (length >= line.Length)
                        return new WordIncluedStyle(_ewNext, w);
                    c = line[length];
                    var ewNext = _ewNext.Get(c);
                    if (ewNext != null) {
                        _ewNext = ewNext;
                        w += c.ToString();
                        goto Start;
                    } else if (!_ewNext.IsComplete) {
                        length--;
                        return wiStyle;
                    }

                    length--;
                    return new WordIncluedStyle(_ewNext, w);
                }
            }
            return wiStyle;
        }

        /// <summary>
        /// 保护颜色是否结束
        /// </summary>
        /// <param name="c"></param>
        /// <param name="wiStyle"></param>
        /// <param name="line"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private bool EndIncludeFont(char c, WordIncluedStyle wiStyle, Word word, string line, ref int length) {
            if (wiStyle.EndFirst == c) {
                if (length > 0 && wiStyle.BeforeNoChar != CharCommand.Char_Empty && line[length - 1] == wiStyle.BeforeNoChar)
                    return false;

                if (wiStyle.EndString.Length > 1) {
                    string endChar = c.ToString();
                Start:
                    if (length >= line.Length - 1)
                        return false;

                    endChar += line[++length];
                    if (endChar == wiStyle.EndString) {
                        word.Text = endChar;
                        return true;
                    } else if (endChar.Length < wiStyle.EndString.Length)
                        goto Start;

                    length--;
                    return false;
                }
                return true;

            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private string GetTag(string line, ref int length) {
            length++;
            string tag = string.Empty;
            while (length < line.Length) {
                char c = line[length];
                if (!(pLanguageMode.CompartChars.Contains(c) || c == CharCommand.Char_Tab))
                    tag += c;
                else {
                    length--;
                    return tag;
                }
                length++;
            }
            return tag;
        }

        /// <summary>
        /// 异步给文本设置宽度
        /// </summary>
        /// <param name="ls"></param>
        private void SetWordWidthEvent(LineString ls) {
            if (ls.PWord == null)
                return;

            foreach (var w in ls.PWord) {
                w.Width = CharCommand.GetWrodWidth(w, this.pIEdit.GetGraphics, this.pLanguageMode.TabSpaceCount);
                ls.Width += w.Width;
            }
        }





    }
}
