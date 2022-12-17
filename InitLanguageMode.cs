using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using XZ.Edit.Entity;
using System.Xml.Linq;

namespace XZ.Edit {
    public class InitLanguageMode {

        private static Dictionary<string, LanguageMode> pLanguageMode = new Dictionary<string, LanguageMode>();

        /// <summary>
        /// 初始化语言作色配置
        /// </summary>
        /// <param name="language"></param>
        /// <param name="defFont"></param>
        public static void Init(string language, Font defFont) {
            LanguageMode mode;
            if (pLanguageMode.TryGetValue(language.ToLower(), out mode))
                return;

            var strem = Assembly.GetExecutingAssembly().GetManifestResourceStream("XZ.Edit.Res." + language + ".xml");
            mode = new LanguageMode();
            pLanguageMode.Add(language.ToLower(), mode);
            if (strem == null) {
                mode.CompartChars = new HashSet<char>();
                mode.WordFonts = new Dictionary<string, WFontColor>();
                return;
            }
                //throw new Exception("未加载" + language + "语言文件");

            
            LoadXML(strem, defFont, mode);
        }

        /// <summary>
        /// 加载XML
        /// </summary>
        /// <param name="sm"></param>
        /// <param name="font"></param>
        /// <param name="mode"></param>
        private static void LoadXML(Stream sm, Font font, LanguageMode mode) {
            var doc = XDocument.Load(sm);
            var root = doc.Element("root");
            if (root == null)
                return;

            #region 制表符多少个空格
            var tabSpaceCount = root.Element("Tab");
            if (tabSpaceCount != null)
                mode.TabSpaceCount = int.Parse(tabSpaceCount.Value);
            #endregion

            #region 分割字符
            var compartChars = root.Element("CompartChars");
            if (compartChars != null) {
                var comChars = compartChars.Value;
                if (!string.IsNullOrEmpty(comChars)) {
                    mode.CompartChars = new HashSet<char>();
                    foreach (char c in comChars)
                        mode.CompartChars.Add(c);
                }
            }
            #endregion

            #region 分割字符颜色
            var compartCharFontEle = root.Elements("CompartCharFont");
            if (compartCharFontEle != null) {
                mode.CompartCharFont = new Dictionary<char, WFontColor>();
                foreach (var cCharFe in compartCharFontEle) {
                    var wfColor = GetWFontColor(cCharFe, font);
                    var value = cCharFe.Value;
                    if (!string.IsNullOrWhiteSpace(value)) {
                        foreach (var c in value)
                            mode.CompartCharFont.Add(c, wfColor);
                    }
                }
            }
            #endregion

            #region 成对字符
            var coupleEle = root.Element("Couple");
            if (coupleEle != null) {
                var ceChars = coupleEle.Elements("Char");
                mode.CoupleStart = new Dictionary<char, char>();
                var color = GetColor(coupleEle, "BackGoundColor");
                if (color != Color.Empty)
                    mode.CoupleBackGround = new SolidBrush(color);
                mode.CoupleEnd = new Dictionary<char, char>();
                foreach (var ce in ceChars) {
                    mode.CoupleStart.Add(ce.Attribute("Start").Value[0], ce.Attribute("End").Value[0]);
                    mode.CoupleEnd.Add(ce.Attribute("End").Value[0], ce.Attribute("Start").Value[0]);
                }
            }
            #endregion

            #region 块
            var rangeEle = root.Element("Range");
            if (rangeEle != null)
                mode.Range = Tuple.Create(rangeEle.Attribute("Start").Value[0], rangeEle.Attribute("End").Value[0]);
            #endregion

            #region 字体样式
            var wordEles = root.Elements("Word");
            if (wordEles != null) {
                mode.WordFonts = new Dictionary<string, WFontColor>();
                foreach (var wordE in wordEles) {
                    var wfColor = GetWFontColor(wordE, font);
                    var valueEs = wordE.Elements("Value");
                    if (valueEs != null) {
                        foreach (var v in valueEs) {
                            var values = v.Value;
                            if (string.IsNullOrWhiteSpace(values))
                                continue;

                            var ts = values.Split(',');
                            foreach (var t in ts) {
                                if (!mode.WordFonts.ContainsKey(t))
                                    mode.WordFonts.Add(t, wfColor);
                            }
                        }
                    }
                }
            }
            #endregion

            #region 开始和结束之间的颜色
            var startEndWordE = root.Element("StartEndWord");
            if (startEndWordE != null) {
                var valueEs = startEndWordE.Elements("Value");
                mode.PStartWordStyle = new StartWordStyle();
                mode.PEndWordStyle = new EndWordStyle();
                //mode.PStartEndWordEnd = new Dictionary<string, string>();
                //mode.PStartWords = new HashSet<string>();
                foreach (var valueE in valueEs) {
                    var wfc = GetWFontColor(valueE, font);
                    var start = valueE.Attribute("Start").Value;
                    char c = '\0';
                    var d = mode.PStartWordStyle;
                    foreach (var nc in start) {
                        if (d.Father != null)
                            d = d.Father.Add(nc, c, wfc);
                        else
                            d = d.Add(nc, c, wfc);
                        c = nc;
                    }
                    d.IsComplete = true;
                    if (valueE.Attribute("More") != null)
                        d.MoreLine = valueE.Attribute("More").Value == "true";
                    if (valueE.Attribute("End") != null) {
                        d.EndString = valueE.Attribute("End").Value;
                        if (d.MoreLine)
                            mode.PEndWordStyle.Add(d.EndString, start);
                    }
                    d.Line = valueE.Attribute("Line") == null ? false : valueE.Attribute("Line").Value == "true";
                    if (valueE.Attribute("BeforeNoChar") != null)
                        d.BeforeNoChar = valueE.Attribute("BeforeNoChar").Value[0];

                }
            }
            #endregion

            #region 自定义
            var useE = root.Element("Use");
            if (useE != null) {
                var wfc = GetWFontColor(useE, font);
                mode.CharUseStyle = new Dictionary<char, UseStyle>();
                mode.TagUseStyle = new Dictionary<string, UseStyle>();

                #region befores
                var befores = useE.Elements("before");
                if (befores != null) {
                    foreach (var be in befores) {
                        var ustyle = new UseStyle() {
                            PEUserStyle = EUseStyle.Before,
                            PWFontColor = wfc
                        };
                        if (be.Attribute("String") != null)
                            mode.TagUseStyle.Add(be.Attribute("String").Value, ustyle);
                        else
                            mode.CharUseStyle.Add(be.Attribute("Char").Value[0], ustyle);
                    }
                }
                #endregion

                #region after
                var afters = useE.Elements("after");
                if (afters != null) {
                    foreach (var af in afters) {
                        var ustyle = new UseStyle() {
                            PEUserStyle = EUseStyle.After,
                            PWFontColor = wfc
                        };
                        if (af.Attribute("String") != null)
                            mode.TagUseStyle.Add(af.Attribute("String").Value, ustyle);
                        else
                            mode.CharUseStyle.Add(af.Attribute("Char").Value[0], ustyle);
                    }
                }
                #endregion

            }
            #endregion

            #region 缩进
            var retractionE = root.Element("Retraction");
            if (retractionE != null) {
                var valueE = retractionE.Elements("Value");
                if (valueE != null) {
                    mode.Retraction = new HashSet<string>();
                    foreach (var v in valueE) {
                        var value = v.Value.Split(',');
                        foreach (var tag in value)
                            mode.Retraction.Add(tag);
                    }
                }
                var berforChar = retractionE.Elements("BerforChar");
                if (berforChar != null && berforChar.Count() > 0) {
                    mode.RetractionBerforChars = new HashSet<char>();
                    foreach (var n in berforChar)
                        mode.RetractionBerforChars.Add(n.Value[0]);

                }
                var afterNoChar = retractionE.Elements("AfterNoChar");
                if (afterNoChar != null && afterNoChar.Count() > 0) {
                    mode.RetractionAfterNoChar = new HashSet<char>();
                    foreach (var n in afterNoChar)
                        mode.RetractionAfterNoChar.Add(n.Value[0]);

                }

                //var noCharE = retractionE.Elements("NoChar");
                //if (noCharE != null && noCharE.Count() > 0) {
                //    mode.RetractionNoChar = new HashSet<char>();
                //    foreach (var n in noCharE)
                //        mode.RetractionNoChar.Add(n.Value[0]);

                //}

                //var beforeCharE = retractionE.Elements("AfterFirsNoChar");
                //if (beforeCharE != null && beforeCharE.Count() > 0) {
                //    mode.RetractionAfterFirsNoChar = new HashSet<char>();
                //    foreach (var n in beforeCharE)
                //        mode.RetractionAfterFirsNoChar.Add(n.Value[0]);

                //}
            }
            #endregion

            #region 自动添加字符
            var autoInsertE = root.Element("AutoInsert");
            if (autoInsertE != null) {
                var charEs = autoInsertE.Elements("Char");
                if (charEs != null) {
                    mode.AutoInsertChar = new Dictionary<char, char>();
                    foreach (var c in charEs)
                        mode.AutoInsertChar.Add(c.Attribute("name").Value[0], c.Value[0]);
                }
            }
            #endregion

            #region 连接字符
            var concatChar = root.Element("ConcatChar");
            if (concatChar != null) {
                var cs = concatChar.Elements("string");
                mode.ConcatChars = new string[cs.Count()];
                int c_index = 0;
                foreach (var c in cs) {
                    mode.ConcatChars[c_index] = c.Attribute("value").Value.ToString();
                    c_index++;
                }
            }
            #endregion

            #region 自定义样式

            var use = root.Element("Use");
            if (use != null) {
                var items = use.Elements("Item");
                if (items != null) {
                    mode.UseAfter = new Dictionary<string, UseWordFont>();
                    mode.UseBefore = new Dictionary<string, UseWordFont>();

                    foreach (var im in items) {
                        var attrName = im.Attribute("BeforeName");
                        if (attrName != null)
                            SetUseFont(im, attrName.Value, font, mode.UseBefore, UseWordFontType.Before);
                        else {
                            attrName = im.Attribute("AfterName");
                            SetUseFont(im, attrName.Value, font, mode.UseAfter, UseWordFontType.After);
                        }

                    }

                }
            }

            #endregion
        }

        private static void SetUseFont(XElement xe, string name, Font defFont, Dictionary<string, UseWordFont> dict, UseWordFontType type) {

            var uwf = new UseWordFont();
            dict.Add(name, uwf);
            uwf.PType = type;
            var valueE = xe.Element("Value");
            if (valueE != null)
                uwf.PFont = GetWFontColor(valueE, defFont);
            var afterE = xe.Elements("After");
            if (afterE != null && afterE.Count() > 0) {
                uwf.After = new Dictionary<string, UseWordFont>();
                foreach (var after in afterE) {
                    var uwfC = new UseWordFont();
                    uwfC.PType = UseWordFontType.After;
                    uwfC.PFont = GetWFontColor(after, defFont);
                    uwf.After.Add(after.Value, uwfC);
                }
            }
        }

        /// <summary>
        /// 获取样式
        /// </summary>
        /// <param name="ele"></param>
        /// <param name="font"></param>
        /// <returns></returns>
        private static WFontColor GetWFontColor(XElement ele, Font font) {
            FontStyle fs = font.Style;
            if (ele.Attribute("FontStyle") != null)
                fs = (FontStyle)(int.Parse(ele.Attribute("FontStyle").Value));
            Color c = GetColor(ele);
            return new WFontColor(font.FontFamily.Name, font.Size, fs, c);
        }

        /// <summary>
        /// 获取颜色
        /// </summary>
        /// <param name="ele"></param>
        /// <returns></returns>
        public static Color GetColor(XElement ele, string attrName = "Color") {
            if (ele.Attribute(attrName) != null) {
                string cs = ele.Attribute(attrName).Value;
                string[] crs = cs.Split(',');
                if (crs.Length == 3)
                    return Color.FromArgb(int.Parse(crs[0]), int.Parse(crs[1]), int.Parse(crs[2]));
            }
            return Color.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public static LanguageMode GetLanguageMode(string language) {
            LanguageMode mode;
            if (pLanguageMode.TryGetValue(language.ToLower(), out mode))
                return mode;

            throw new Exception("未加载" + language + "语言文件配置");
        }
    }
}
