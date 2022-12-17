using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using XZ.Edit.Entity;

namespace XZ.Edit {
    public class CharCommand {

        #region 常量 初始化
        public const int WM_HSCROLL = 0x114;
        public const int WM_VSCROLL = 0x115;
        public const int SB_ENDSCROLL = 0x8;

        /// <summary>
        /// 回车
        /// </summary>
        public const char Char_Enter = '\r';

        /// <summary>
        /// 退格
        /// </summary>
        public const char Char_BackSpace = '\b';

        /// <summary>
        /// 空格字符
        /// </summary>
        public const char Char_Space = ' ';

        /// <summary>
        /// 换行
        /// </summary>
        public const char Char_Newline = '\n';

        public const char Char_Tab = '\t';

        /// <summary>
        /// 空字节
        /// </summary>
        public const char Char_Empty = '\0';

        #endregion

        #region 获取字体宽度
        private static object lockObject = new object();
        public const TextFormatFlags CTextFormatFlags =
           TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.PreserveGraphicsClipping;
        private static Dictionary<string, int> WordWidthCache = new Dictionary<string, int>();
        private const int WordWidthCacheMaxLength = 5000;

        /// <summary>
        /// 获取字体的宽度
        /// </summary>
        /// <param name="g"></param>
        /// <param name="word"></param>
        /// <param name="font"></param>
        /// <returns></returns>
        public static int GetCharWidth(Graphics g, string word, Font font) {
            if (word == string.Empty)
                return 0;

            string key = word + font.Name + font.Size + font.Style;

            //if (!WordWidthCache.ContainsKey(key)) {
            //    lock (lockObject) {
            //        if (!WordWidthCache.ContainsKey(key)) {
            //            int width = TextRenderer.MeasureText(g, word, font, new Size(short.MaxValue, short.MaxValue), CTextFormatFlags).Width;
            //            WordWidthCache.Add(key, width);
            //            return width;
            //        }
            //    }
            //}
            //return WordWidthCache[key];
            int outWidth;
            if (WordWidthCache.TryGetValue(key, out outWidth))
                return outWidth;

            if (WordWidthCache.Count > WordWidthCacheMaxLength)
                WordWidthCache.Clear();


            int width = TextRenderer.MeasureText(g, word, font, new Size(short.MaxValue, short.MaxValue), CTextFormatFlags).Width;
            WordWidthCache.Add(key, width);
            return width;
        }

        #endregion


        public static Font GetFont(Word w, WFontColor defFont) {
            if (defFont != null)
                return defFont.PFont;
            else
                return GetFont(w);
        }

        public static Font GetFont(WFontColor defFont) {
            if (defFont == null)
                return FontContainer.DefaultFont;
            else
                return defFont.PFont;
        }

        /// <summary>
        /// 获取字体样式
        /// </summary>
        /// <param name="w"></param>
        /// <returns></returns>
        public static Font GetFont(Word w) {
            if (w.PIncluedFont != null && w.PIncluedFont.PFont != null)
                return w.PIncluedFont.PFont;

            if (w.PFont != null && w.PFont.PFont != null) {
                var font = w.PFont.PFont;
                return font;
            }
            return FontContainer.DefaultFont;

        }

        /// <summary>
        /// 获取文本的宽度
        /// </summary>
        /// <param name="w"></param>
        /// <param name="g"></param>
        /// <returns></returns>
        public static int GetWrodWidth(Word w, Graphics g, int tabSpaceCount) {
            return GetWrodWidth(w, g, GetFont(w), tabSpaceCount);
        }

        /// <summary>
        /// 获取文本的宽度
        /// </summary>
        /// <param name="w"></param>
        /// <param name="g"></param>
        /// <param name="f"></param>
        /// <param name="tagSpaceCount"></param>
        /// <returns></returns>
        public static int GetWrodWidth(Word w, Graphics g, Font f, int tabSpaceCount) {
            switch (w.PEWordType) {
                case EWordType.Tab:
                    return FontContainer.GetSpaceWidth(g) * tabSpaceCount;
                case EWordType.Space:
                    return FontContainer.GetSpaceWidth(g);
                default:
                    return CharCommand.GetCharWidth(g, w.Text, f ?? FontContainer.DefaultFont);
            }
        }


        /// <summary>
        /// 获取LineString 的宽度
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="g"></param>
        /// <param name="tapSpaceCount"></param>
        /// <returns></returns>
        public static int GetLineStringWidth(LineString ls, Graphics g, int tapSpaceCount) {
            if (ls.Width > 0)
                return ls.Width;

            foreach (var w in ls.PWord) {
                w.Width = CharCommand.GetWrodWidth(w, g, tapSpaceCount);
                ls.Width += w.Width;
            }
            return ls.Width;

        }

        /// <summary>
        /// 获取颜色
        /// </summary>
        /// <param name="w"></param>
        /// <returns></returns>
        public static Color GetColor(Word w) {
            if (w.PIncluedFont != null && w.PIncluedFont.PColor != null)
                return w.PIncluedFont.PColor;

            if (w.PFont != null && w.PFont.PColor != null) {
                var font = w.PFont.PColor;
                return font;
            }
            return FontContainer.ForeColor;
        }

        public static Color GetColor(Word w, WFontColor defColor) {
            if (defColor != null)
                return defColor.PColor;
            else
                return GetColor(w);
        }

        /// <summary>
        /// 分隔字符串
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static List<Word> CompartString(string line, params char[] charComparts) {
            if (string.IsNullOrEmpty(line))
                return null;
            List<Word> word = new List<Word>();
            string[] arrays = line.Split(charComparts);
            int arrayIndex = 0;
            int length = 0;
            while (length < line.Length) {
                char c = line[length];
                if (charComparts.Contains(c)) {
                    var w = new Word(c.ToString());
                    if (c == Char_Space)
                        w.PEWordType = EWordType.Space;
                    else if (c == Char_Tab)
                        w.PEWordType = EWordType.Tab;
                    word.Add(w);
                    length++;
                } else {
                    string tag = arrays[arrayIndex++];
                    word.Add(new Word(tag) { PEWordType = EWordType.Word });

                    length += tag.Length;
                }
            }
            return word;
        }
    }
}
