using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace XZ.Edit {
    public class FontContainer {
        private static Font _font = System.Drawing.SystemFonts.DefaultFont;
        /// <summary>
        /// 默认字体颜色
        /// </summary>
        private static Color _foreColor = Color.Empty;

        /// <summary>
        /// 背景颜色
        /// </summary>
        private static Color _backgroudColor = Color.Empty;
        private static int _fontHeight;

        /// <summary>
        /// 初始化字体
        /// </summary>
        /// <param name="font"></param>
        public static void InitFont(Font font) {
            _font = font;
            _spaceWidth = 0;
            _minWidth = 0;
            _fontHeight = GetFontHeight(font);
        }

        /// <summary>
        /// 初始化字体默认颜色
        /// </summary>
        /// <param name="foreColor"></param>
        public static void InitForeColor(Color foreColor) {
            _foreColor = foreColor;
        }

        /// <summary>
        /// 背景颜色
        /// </summary>
        /// <param name="bgColor"></param>
        public static void InitBackGroudColor(Color bgColor) {
            _backgroudColor = bgColor;
        }

        /// <summary>
        /// 获取默认字体
        /// </summary>
        public static Font DefaultFont {
            get {
                if (_font == null) {
                    _font = System.Drawing.SystemFonts.DefaultFont;
                    InitFont(_font);
                }
                return _font;
            }
        }

        /// <summary>
        /// 字体默认颜色 默认为黑色
        /// </summary>
        public static Color ForeColor {
            get {
                if (_foreColor == Color.Empty)
                    _foreColor = Color.Black;

                return _foreColor;
            }
        }

        /// <summary>
        /// 获取背景颜色
        /// </summary>
        public static Color BackGroudColor {
            get {
                if (_backgroudColor == Color.Empty)
                    _backgroudColor = Color.White;

                return _backgroudColor;
            }
        }

        /// <summary>
        /// 获取字体高度
        /// </summary>
        public static int FontHeight {
            get {
                if (_fontHeight == 0)
                    _fontHeight = GetFontHeight(DefaultFont);
                return _fontHeight;
            }
        }

        /// <summary>
        /// 获取字体宽度
        /// </summary>
        private static int FontWidth { get; set; }

        /// <summary>
        /// 空格宽度
        /// </summary>
        private static int _spaceWidth;

        private static int _minWidth;

        /// <summary>
        /// 获取字体的高度
        /// </summary>
        /// <param name="font"></param>
        /// <returns></returns>
        public static int GetFontHeight(Font f) {
            int height1 = TextRenderer.MeasureText("_", f).Height;
            int height2 = (int)Math.Ceiling(_font.GetHeight());
            return Math.Max(height1, height2) + 1;
        }

        /// <summary>
        /// 设置字体宽度
        /// </summary>
        private static void SetFontWidth() {
            var regularfont = new Font(_font.FontFamily, _font.Size, FontStyle.Regular);
            var boldfont = new Font(regularfont, FontStyle.Bold);
            var italicfont = new Font(regularfont, FontStyle.Italic);
            var bolditalicfont = new Font(regularfont, FontStyle.Bold | FontStyle.Italic);

        }


        /// <summary>
        /// 获取空格的宽度
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static int GetSpaceWidth(Graphics g) {
            if (_spaceWidth == 0 && _font != null)
                _spaceWidth = Math.Max(CharCommand.GetCharWidth(g, " ", _font), 1);
            return _spaceWidth;
        }

        /// <summary>
        /// 获取最小宽度
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static int GetMinWidth(Graphics g) {
            if (_minWidth == 0 && _font != null) {
                int mw1 = CharCommand.GetCharWidth(g, "1", _font);
                int mwl = CharCommand.GetCharWidth(g, "l", _font);
                _minWidth = Math.Min(mw1, mwl);
            }
            return _minWidth;
        }
    }
}
