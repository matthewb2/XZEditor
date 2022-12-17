using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XZ.Edit.Actions;
using XZ.Edit.Entity;
using XZ.Edit.Forms;
using XZ.Edit.Interfaces;

namespace XZ.Edit {

    #region 委托
    /// <summary>
    /// 插入字符
    /// </summary>
    /// <param name="c"></param>
    /// <param name="location"></param>
    /// <param name="edit"></param>
    public delegate void InsertCharEventHandler(char c, Point charLocation, IEdit edit, Point XyLocation);
    /// <summary>
    /// 文本发生改变
    /// </summary>
    /// <param name="c"></param>
    public delegate void DocumentChangedEventHandler();

    /// <summary>
    /// 鼠标按下之后字符位置
    /// </summary>
    /// <param name="location"></param>
    public delegate void MouseLeftDownCharPointHandler(Point location);

    public delegate void ChangeActionHandler(Parser parser);

    #endregion

    public class EventEdit : UserControl {

        #region 私有
        private bool pIsMouseDown;
        private Keys pDownKey = Keys.None;
        private bool pDownKeyAction;
        private int pMouseStartY;
        private int pMouseStartX;
        private CPoint pCursorEditPoint;
        private bool pIsMouseLeave;
        private bool pIsMouseHover;
        private Rectangle pTipRectanle = Rectangle.Empty;
        private Point pMousePoint;
        private CToolTip pTooLTip;
        protected CompletionWindow pCompletionWindow;
        public EventEdit() {
            
        }
        #endregion

        #region 属性
        protected Parser PParser { get; set; }
        protected CursorAndIME PCursor { get; set; }
        protected KeyEvent PkeyEvent { get; set; }
        public bool IsMouseWheel { get; private set; }
        #endregion

        #region 事件

        /// <summary>
        /// 补全框选择事件
        /// </summary>
        public event CompletionWindowSelectEventHandler CompletionWindowSelectEvent;

        /// <summary>
        /// 显示提示
        /// </summary>
        public event ToolTipMessageEventHandler ToolTipMessageEvent;

        public event InsertCharEventHandler InsertCharEvent;

        public event DocumentChangedEventHandler DocumentChangeEvent;

        public event MouseLeftDownCharPointHandler MouseLeftDownPointCharEvent;

        public event ChangeActionHandler ChangeActionEvent;

        /// <summary>
        /// 设置文本样式
        /// </summary>
        public Func<string, WFontColor> SetWordStyleEvent { get; set; }

        public Action<Exception> ErrorEvent { get; set; }

        public void SetChangeAction() {
            if (this.ChangeActionEvent != null)
                this.ChangeActionEvent(this.PParser);
        }

        public void SetErrorEvent(Exception ex) {
            if (this.ErrorEvent != null)
                this.ErrorEvent(ex);
            else
                throw ex;
        }

        #endregion

        #region 滚动条

        public int GetHeight {
            get {
                return this.ClientRectangle.Height;
            }
        }

        public int GetWidth {
            get { return this.ClientRectangle.Width; }
        }

        /// <summary>
        /// 获取滚动条垂直隐藏的值
        /// </summary>
        public int GetVerticalScrollValue {
            get {
                if (this.VerticalScroll.Visible)
                    return this.VerticalScroll.Value;
                else
                    return 0;
            }
        }

        /// <summary>
        /// 获取滚动条水平隐藏的值
        /// </summary>
        public int GetHorizontalScrollValue {
            get {
                if (this.HorizontalScroll.Visible)
                    return HorizontalScroll.Value;
                else
                    return 0;
            }
        }

        /// <summary>
        /// 获取滚动条的宽度
        /// </summary>
        public int GetScrollWidth {
            get {
                return this.AutoScrollMinSize.Width;
            }
        }

        /// <summary>
        /// 设置垂直滚动条
        /// </summary>
        /// <param name="changeMinSize"></param>
        public void SetVerticalScrollValue(bool changeMinSize = true) {
            if (changeMinSize)
                this.ChangeScollSize();
            if ((this.PCursor.CousorPointForEdit.Y + FontContainer.FontHeight) > (this.GetVerticalScrollValue + this.GetHeight)) {
                this.VerticalScroll.Value = this.PCursor.CousorPointForEdit.Y - this.GetHeight + FontContainer.FontHeight;
                this.UpdateScrollbars();
            } else if (this.PCursor.CousorPointForEdit.Y < this.GetVerticalScrollValue) {
                this.VerticalScroll.Value = this.PCursor.CousorPointForEdit.Y;
                this.UpdateScrollbars();
            }

        }

        /// <summary>
        /// 改变自动滚动最小尺寸
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void ChangeScollSize(int width = 0, int height = 0) {
            height = this.GetScollMaxHeight(height);
            if (width == 0)
                width = Math.Max(this.Width, this.PParser.GetMaxWidth + 200 + this.PParser.GetLeftSpace);

            this.AutoScrollMinSize = new Size(width, height);
            if (VerticalScroll.SmallChange == 0)
                InitScroll();
            
        }

        public void SetMaxScollMaxWidth(int width) {
            width += 200;
            if (width > this.GetWidth) {
                int height = this.GetScollMaxHeight(0);
                this.AutoScrollMinSize = new Size(width, height);
                if (VerticalScroll.SmallChange == 0)
                    InitScroll();
            }
        }

        /// <summary>
        /// 获取最大高度
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        private int GetScollMaxHeight(int height = 0) {
            if (height == 0)
                height = this.PParser.GetCount() * FontContainer.FontHeight;

            return height;
        }

        /// <summary>
        /// 初始化滚动条
        /// </summary>
        protected void InitScroll() {
            var charHeight = FontContainer.FontHeight;
            VerticalScroll.SmallChange = charHeight;
            VerticalScroll.LargeChange = 10 * charHeight;
            HorizontalScroll.SmallChange = charHeight;
        }

        /// <summary>
        /// 更新滚动条值
        /// </summary>
        public void UpdateScrollbars() {
            this.AutoScrollMinSize -= new Size(1, 0);
            this.AutoScrollMinSize += new Size(1, 0);
        }

        /// <summary>
        /// 设置水平滚动条值
        /// </summary>
        /// <param name="value">移动的值</param>
        /// <param name="lorR">左移动 -1 右移  1</param>
        /// <param name="resetCursor">是否重新设置光标</param>
        public void SetHorizontalScrollValue(int value, int lorR, bool resetCursor = false) {
            if (!this.HorizontalScroll.Visible ||
                (lorR == -1 && this.HorizontalScroll.Value == 0))
                return;
            if (value == 0) {
                //
                if (lorR == 1)
                    value = FontContainer.GetSpaceWidth(this.CreateGraphics()) * 20;
            }


            this.HorizontalScroll.Value = value;
            this.UpdateScrollbars();
            if (resetCursor)
                this.PCursor.SetPosition();
        }


        protected override void OnScroll(ScrollEventArgs se) {
            this.CloseCompletionWindow();
            if (se.ScrollOrientation == ScrollOrientation.HorizontalScroll) {
                HorizontalScroll.Value = se.NewValue;
                if (se.NewValue > this.PCursor.CousorPointForEdit.X - this.PParser.GetLeftSpace)
                    this.PCursor.Hide();
                else
                    this.PCursor.Create();
            } else
                VerticalScroll.Value = Math.Max(VerticalScroll.Minimum, (se.NewValue / FontContainer.FontHeight) * FontContainer.FontHeight);


            if (IsMouseWheel) {
                this.UpdateScrollbars();
                IsMouseWheel = false;
            }
            base.OnScroll(se);
        }



        #endregion

        #region 鼠标

        /// <summary>
        /// 滑轮
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e) {
            int linesPerClick = Math.Max(SystemInformation.MouseWheelScrollLines, 1);
            if (VerticalScroll.Visible) {
                IsMouseWheel = true;
                int value = linesPerClick * Math.Sign(e.Delta) * FontContainer.FontHeight;
                value = VerticalScroll.Value - value;
                var ea = new ScrollEventArgs(e.Delta > 0 ? ScrollEventType.SmallDecrement : ScrollEventType.SmallIncrement,
                                        VerticalScroll.Value,
                                        value,
                                        ScrollOrientation.VerticalScroll);

                OnScroll(ea);
                ((HandledMouseEventArgs)e).Handled = true;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 松开
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e) {
            base.OnMouseUp(e);
            this.pIsMouseDown = false;
        }

        /// <summary>
        /// 按下
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e) {
            this.CloseCompletionWindow();
            if (e.X < this.PParser.GetLeftSpace) {
                base.OnMouseDown(e);
                this.PParser.PPucker.ClickPucker(e.X, e.Y);
                return;
            }
            this.pIsMouseDown = true;
            if (e.Button == MouseButtons.Left) {
                this.pMouseStartY = e.Y;
                this.pMouseStartX = e.X;
                var isMoreSelect = this.pDownKey == Keys.Shift;
                var curPoint = this.PParser.PCursor.CousorPointForEdit.Create();
                var downPoint = this.PParser.SetCurosrPosition(e.X, e.Y, !isMoreSelect);
                Parallel.Invoke(() => { this.PParser.CoupleChar(); },
                    () => { this.PParser.PPucker.SelectPucker(e.Y); });
                this.Invalidate();
                if (isMoreSelect) {
                    if (this.PParser.GetSelectPartPoint != null)
                        this.PParser.MouseLeftClickSelect(downPoint, curPoint);
                    else
                        this.PParser.MouseMoveSelect(this.pCursorEditPoint, downPoint);
                }

                this.pCursorEditPoint = downPoint.Create();
                this.Invalidate();

                if (this.MouseLeftDownPointCharEvent != null)
                    this.MouseLeftDownPointCharEvent(new Point(this.PCursor.CousorPointForWord.X + 1, this.PCursor.CousorPointForWord.Y));
            }
            base.OnMouseDown(e);
        }

        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (e.X < this.PParser.GetLeftSpace)
                this.Cursor = Cursors.Default;
            else
                this.Cursor = Cursors.IBeam;
            this.pMousePoint = e.Location;
            if (!this.pIsMouseLeave) {
                if (this.pIsMouseHover) {
                    this.pIsMouseHover = false;
                    this.pIsMouseLeave = true;
                    ResetMouseEventArgs();
                }
            }

            if (this.pIsMouseDown) {
                int y = Math.Abs(this.pMouseStartY - e.Y);
                int x = Math.Abs(this.pMouseStartX - e.X);
                if (y > FontContainer.FontHeight || x > FontContainer.GetMinWidth(this.CreateGraphics())) {
                    this.pMouseStartY = e.Y;
                    this.pMouseStartX = e.X;
                    this.PParser.MouseMoveSelect(e.X, e.Y, this.pCursorEditPoint);
                    this.Invalidate();
                }
            }
        }

        /// <summary>
        /// 双击
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDoubleClick(MouseEventArgs e) {
            this.CloseCompletionWindow();
            if (e.X < this.PParser.GetLeftSpace) {
                base.OnMouseDown(e);
                return;
            }
            this.HideToolTip();
            this.PParser.DoubleSelectCrusorWord();
            this.Invalidate();
        }

        /// <summary>
        /// 鼠标停留
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseHover(EventArgs e) {
            base.OnMouseHover(e);

            this.pIsMouseLeave = false;
            this.pIsMouseHover = true;
            if (this.pTipRectanle.Contains(this.pMousePoint))
                return;

            var hidePuckers = this.PParser.PPucker.GetHidePucker(this.pMousePoint, out pTipRectanle);
            if (hidePuckers != null) {
                this.CreateTip();
                this.pTooLTip.SetLineString(hidePuckers);
                this.pTooLTip.SetPosition(this.GetTipLocation(), Control.MousePosition.Y - this.pMousePoint.Y / FontContainer.FontHeight);
                this.pTooLTip.Show();
                return;
            }
            this.HideToolTip();
            if (this.ToolTipMessageEvent == null)
                return;
            var tipMessage = this.PParser.GetToolTipMessage(this.pMousePoint, out pTipRectanle);
            if (tipMessage == null) {
                this.HideToolTip();
                return;
            }
            this.ToolTipMessageEvent(this.PParser.PIEdit, tipMessage);
            if (!string.IsNullOrWhiteSpace(tipMessage.GetToolTipMessage())) {
                this.CreateTip();
                this.pTooLTip.SetText(tipMessage.GetToolTipMessage());
                this.pTooLTip.SetPosition(this.GetTipLocation(), Control.MousePosition.Y - this.pMousePoint.Y / FontContainer.FontHeight);
                this.pTooLTip.Show();

                return;
            }
            this.HideToolTip();
        }
        private void CreateTip() {
            if (this.pTooLTip == null)
                this.pTooLTip = new CToolTip(this.FindForm(), this.PParser.PLanguageMode.TabSpaceCount);
        }
        private Point GetTipLocation() {
            return new Point(Control.MousePosition.X, Control.MousePosition.Y + FontContainer.FontHeight);
        }
        private void HideToolTip() {
            if (this.pTooLTip != null) {
                this.pTooLTip.Hide();
                this.pTipRectanle = Rectangle.Empty;
            }
        }
        protected override void OnMouseLeave(EventArgs e) {
            base.OnMouseLeave(e);
            this.pIsMouseLeave = true;
            this.HideToolTip();
        }



        #endregion

        #region 键盘

        /// <summary>
        /// 设置文本修改
        /// </summary>
        public void SetChangeText() {
            if (this.DocumentChangeEvent != null)
                this.DocumentChangeEvent();
        }

        protected override void OnKeyPress(KeyPressEventArgs e) {
            base.OnKeyPress(e);
            if (this.pDownKeyAction || this.pDownKey == Keys.Control || this.pDownKey == Keys.Alt
                || this.pDownKey == (Keys.Shift | Keys.Control))
                return;
            char c = e.KeyChar;
            if (this.PCursor.CousorPointForEdit.X + this.PParser.GetLeftSpace > this.Width - 20)
                this.Invalidate();
            var cwLocationX = this.PCursor.CousorPointForEdit.X;
            this.PParser.InsertChar(c);
            if (c != CharCommand.Char_Empty && this.InsertCharEvent != null) {
                this.InsertCharEvent(c,
                                     new Point(this.PCursor.CousorPointForWord.X, this.PCursor.CousorPointForWord.Y),
                                     this.PParser.PIEdit, new Point(cwLocationX, this.PCursor.CousorPointForEdit.Y));
                this.SetChangeText();
                this.SetChangeAction();
            }

            e.Handled = true;
        }
        private BaseAction pBAction;
        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            this.pDownKeyAction = false;
            if (this.CompletionWindowSelectEvent != null && this.CompletionWindowSelectEvent(e.KeyData)) {
                this.pDownKeyAction = true;
                return;
            }
            DownShift(e);
            this.SetAction(e.KeyData);
            e.Handled = true;
        }

        public void SetAction(Keys k) {
            pBAction = this.PkeyEvent.GetKeyEvent(k);
            if (pBAction != null) {
                try {
                    pBAction.Execute();
                    if (pBAction != null && pBAction.PIsAddUndo)
                        this.PParser.AddAction(pBAction);

                    this.SetChangeAction();
                    this.pDownKeyAction = true;
                } catch (Exception ex) {
                    this.SetErrorEvent(ex);
                }
            }
        }

        protected override bool IsInputKey(Keys keyData) {
            switch ((keyData & Keys.KeyCode)) {
                case Keys.Prior:
                case Keys.Next:
                case Keys.End:
                case Keys.Home:
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    return true;

                case Keys.Escape:
                    return false;

                case Keys.Tab:
                    return (keyData & Keys.Control) == Keys.None;
            }
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            base.OnKeyUp(e);

            if (e.KeyCode == Keys.ShiftKey) {
                this.pDownKey &= ~Keys.Shift;
            }
            if (e.KeyCode == Keys.Alt)
                this.pDownKey &= ~Keys.Alt;
            if (e.KeyCode == Keys.ControlKey)
                this.pDownKey &= ~Keys.Control;
        }

        private void DownShift(KeyEventArgs e) {
            if (this.pDownKey == Keys.Shift && KeyEvent.Contains(e.KeyData, Keys.Shift))
                return;

            this.pDownKey = e.Modifiers;
        }

        /// <summary>
        /// 查看是否包含了键盘按下的键
        /// </summary>
        /// <param name="k"></param>
        /// <returns></returns>
        public bool KeyWordDownContains(Keys k) {
            return KeyEvent.Contains(this.pDownKey, k);
        }

        /// <summary>
        /// 隐藏自动不全
        /// </summary>
        protected void CloseCompletionWindow() {
            if (pCompletionWindow != null && !pCompletionWindow.IsDisposed) {
                pCompletionWindow.Close();
                pCompletionWindow = null;
            }
        }

        #endregion
    }
}
