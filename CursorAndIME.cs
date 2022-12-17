using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using XZ.Edit.Entity;
using XZ.Edit.Interfaces;

namespace XZ.Edit {
    public class CursorAndIME {


        public const int WM_IME_SETCONTEXT = 0x0281;
        private const int CFS_POINT = 0x0002;
        private const int WM_IME_CONTROL = 0x0283;
        private const int IMC_SETCOMPOSITIONWINDOW = 0x000c;
        #region 私有字段和构造

        /// <summary>
        /// 窗口
        /// </summary>
        private IEdit pIEdit;
        private IntPtr pIMEWnd;
        private IntPtr HIMEContext { get; set; }
        public CursorAndIME(IEdit controls) {
            this.CousorPointForEdit = new Point();
            this.CousorPointForWord = new Point();
            this.pIEdit = controls;
            this.HIMEContext = ImmGetContext(this.pIEdit.GetHandle);
        }

        #endregion

        #region 调用WIN32API
        [DllImport("User32.dll")]
        static extern bool CreateCaret(IntPtr hWnd, int hBitmap, int nWidth, int nHeight);

        [DllImport("User32.dll")]
        static extern bool SetCaretPos(int x, int y);

        [DllImport("User32.dll")]
        static extern bool DestroyCaret();

        [DllImport("User32.dll")]
        static extern bool ShowCaret(IntPtr hWnd);

        [DllImport("User32.dll")]
        static extern bool HideCaret(IntPtr hWnd);

        [DllImport("imm32.dll")]
        private static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, COMPOSITIONFORM lParam);

        [DllImport("Imm32.dll")]
        public static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("Imm32.dll")]
        public static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);

        #region 私有类
        [StructLayout(LayoutKind.Sequential)]
        private class COMPOSITIONFORM {
            public int dwStyle = 0;
            public POINT ptCurrentPos = null;
            public RECT rcArea = null;
        }

        [StructLayout(LayoutKind.Sequential)]
        private class POINT {
            public int x = 0;
            public int y = 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        private class RECT {
            public int left = 0;
            public int top = 0;
            public int right = 0;
            public int bottom = 0;
        }

        private void SetIMEWindowLocation(int x, int y) {
            if (pIMEWnd == IntPtr.Zero)
                return;

            POINT p = new POINT();
            p.x = x;
            p.y = y;

            COMPOSITIONFORM lParam = new COMPOSITIONFORM();
            lParam.dwStyle = CFS_POINT;
            lParam.ptCurrentPos = p;
            lParam.rcArea = new RECT();

            try {
                SendMessage(
                    pIMEWnd,
                    WM_IME_CONTROL,
                    new IntPtr(IMC_SETCOMPOSITIONWINDOW),
                    lParam
                );
            }
            catch (AccessViolationException ex) {
                MessageBox.Show("调用 IME: " + ex.Message);
            }
        }
        #endregion

        #endregion

        #region 属性

        //光标宽度
        public const int Width = 1;
        /// <summary>
        /// 光标是否显示
        /// </summary>
        public bool IsShowCurosr { get; set; }

        /// <summary>
        /// 不算数字和折叠部分 x距离
        /// </summary>
        public int XForLeft { get; set; }

        /// <summary>
        /// 光标在文本中的索引位置
        /// </summary>
        public Point CousorPointForWord;

        /// <summary>       
        /// 光标位置的位置
        /// x = 右侧默认宽度（数字和折叠部分） + X - 水平滚动条宽度
        /// y = Y + 垂直滚动条的高度
        /// </summary>
        public Point CousorPointForEdit;

        /// <summary>
        /// 光标选择的内容
        /// </summary>
        public SursorSelectWord PSursorSelectWord { get; set; }

        #endregion

        public void CreateImmAssociateContext() {
            ImmAssociateContext(this.pIEdit.GetHandle, this.HIMEContext);
        }

        /// <summary>
        /// 创建光标
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public bool Create() {
            if (CreateCaret(this.pIEdit.GetHandle, 0, Width, FontContainer.FontHeight)) {
                this.Show();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 重置光标位置
        /// </summary>
        /// <returns></returns>
        public bool SetPosition() {
            if (pIMEWnd == IntPtr.Zero)
                this.pIMEWnd = ImmGetDefaultIMEWnd(this.pIEdit.GetHandle);
            int y = CousorPointForEdit.Y - this.pIEdit.GetVerticalScrollValue;
            //System.Diagnostics.Debug.WriteLine(CousorPointForEdit.X + " ," + y);
            this.SetIMEWindowLocation(CousorPointForEdit.X, y);
            return SetCaretPos(CousorPointForEdit.X, y);
        }

        public void SetPosition(int x, int y, int leftWidth) {
            this.CousorPointForEdit.X = x;
            if (y > -1)
                this.CousorPointForEdit.Y = y;
            this.XForLeft = x - leftWidth;
        }

        /// <summary>
        /// 隐藏
        /// </summary>
        public void Hide() {
            this.IsShowCurosr = false;
            HideCaret(this.pIEdit.GetHandle);
        }
        /// <summary>
        /// 显示
        /// </summary>
        public void Show() {
            this.IsShowCurosr = true;
            ShowCaret(this.pIEdit.GetHandle);
        }

        public void Dispose() {
            this.IsShowCurosr = false;
            DestroyCaret();
        }
    }
}
