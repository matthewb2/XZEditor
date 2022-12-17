using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using XZ.Edit.Interfaces;
using XZ.Edit.Entity;
using XZ.Edit.Forms;

namespace XZ.Edit {
    public partial class EditTextBox : EventEdit, IEdit {

        
        private bool IsGotFocus;

        public EditTextBox() {
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.PCursor = new CursorAndIME(this);
            this.PParser = new Parser(this, this.PCursor);
            this.PkeyEvent = new KeyEvent(this.PParser);
        }

        

        [Browsable(false)]
        public bool ImeAllowed {
            get {
                return ImeMode != ImeMode.Disable &&
                       ImeMode != ImeMode.Off &&
                       ImeMode != ImeMode.NoControl;
            }
        }

        public new ImeMode ImeMode {
            get {
                return ImeMode.On;
            }
            set {
                base.ImeMode = value;
            }
        }
        
        [Browsable(true)]
        public override Font Font {
            get {
                return FontContainer.DefaultFont;
            }
            set {
                FontContainer.InitFont(value);
                this.OptionsChanged();
            }
        }

        public override Color ForeColor {
            get {
                return base.ForeColor;
            }
            set {
                base.ForeColor = value;
                FontContainer.InitForeColor(value);
                this.OptionsChanged();
            }
        }
        public override Color BackColor {
            get {
                return base.BackColor;
            }
            set {
                base.BackColor = value;
                FontContainer.InitBackGroudColor(value);
            }
        }

        private Color _leftNumBackGroundColor = Color.FromArgb(224, 224, 224);
        
        public Color LeftNumBackGroundColor {
            get { return this._leftNumBackGroundColor; }
            set { this._leftNumBackGroundColor = value; }
        }
        private Color _leftNumSeparatorColor = Color.FromArgb(192, 192, 192);
        
        public Color LeftNumSeparatorColor {
            get { return _leftNumSeparatorColor; }
            set { this._leftNumSeparatorColor = value; }
        }

        private Color _leftNumColor = Color.FromArgb(0, 128, 128);
        
        public Color LeftNumColor {
            get { return _leftNumColor; }
            set { this._leftNumColor = value; }
        }

        private Color _selectLineColor = Color.FromArgb(234, 234, 234);
        
        public Color SelectLineColor {
            get { return _selectLineColor; }
            set { this._selectLineColor = value; }
        }

        private ESelectLineStyle _selectLineStyle = ESelectLineStyle.Fill;
        
        public ESelectLineStyle SelectLineStyle {
            get { return _selectLineStyle; }
            set { _selectLineStyle = value; }
        }

        private Color _selectBackGroundColor = Color.FromArgb(173, 214, 255);
        
        public Color SelectBackGroundColor {
            get { return _selectBackGroundColor; }
            set { _selectBackGroundColor = value; }
        }

        private Color _puckerColor = Color.FromArgb(183, 181, 221);
        
        public Color PuckerColor {
            get { return _puckerColor; }
            set { _puckerColor = value; }
        }

        private Color _puckeBackGrounColor = Color.FromArgb(247, 247, 247);
        
        public Color PuckeBackGrounColor {
            get { return _puckeBackGrounColor; }
            set { _puckeBackGrounColor = value; }
        }

        private Color _findBackGroundColor = Color.FromArgb(246, 185, 77);
        
        public Color FindBackGroundColor {
            get { return _findBackGroundColor; }
            set { _findBackGroundColor = value; }
        }

        private Color _findSelectBackGroundColor = Color.FromArgb(144, 156, 127);
        
        public Color FindSelectBackGroundColor {
            get { return _findSelectBackGroundColor; }
            set { _findSelectBackGroundColor = value; }
        }

        private int repealCount = 1500;
        
        public int RepealCount {
            get { return repealCount; }
            set { repealCount = value; }
        }
        /// <summary>
        /// 文本内容
        /// </summary>        
        public override string Text {
            get {
                return this.PParser.GetText();
            }
            set {
                this.PParser.AddText(value);
                this.OptionsChanged();
                this.ChangeScollSize();

            }
        }

        #region 实现接口

        public int GetRepealCount { get { return this.RepealCount; } }

        public CursorAndIME GetCursor { get { return this.PCursor; } }

        public Parser GetParser { get { return this.PParser; } }

        public IntPtr GetHandle {
            get { return this.Handle; }
        }

        public Graphics GetGraphics {
            get {
                return this.CreateGraphics();
            }
        }

        public Form GetParentForm {
            get {
                return this.ParentForm;
            }
        }

        // <summary>
        /// 添加自动补全窗口
        /// </summary>
        /// <param name="cw"></param>
        public void AddCompletionWindow(CompletionWindow cw) {
            this.CloseCompletionWindow();
            this.pCompletionWindow = cw;
        }

        public void GetFocues() {
            this.Focus();
        }

        /// <summary>
        /// 获取光标对应的屏幕坐标
        /// </summary>
        /// <returns></returns>
        public Point GetPointCursorForScreen(Point point) {
            return PointToScreen(point);
        }

        #endregion

        public void SetLanguage(string language) {
            InitLanguageMode.Init(language, this.Font);
            this.PParser.SetLangeMode(language);
        }

        #region 滚动条

        /// <summary>
        /// 选项发生改变
        /// </summary>
        public void OptionsChanged() {
            this.InitScroll();
        }

        #endregion


        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            this.PParser.DrawContents(e.Graphics);
            if (this.IsGotFocus) {
                this.PParser.SetCurosrPosition();
                this.IsGotFocus = false;
            }
        }


        protected override void OnLostFocus(EventArgs e) {
            base.OnLostFocus(e);
            this.PCursor.Hide();
            
        }

        protected override void OnGotFocus(EventArgs e) {
            base.OnGotFocus(e);
            this.PCursor.Create();
            this.PParser.SetCurosrPosition();
            this.IsGotFocus = true;
        }

        
        protected override void WndProc(ref Message m) {
            //Console.WriteLine("wndproc called");
            if (m.Msg == CharCommand.WM_HSCROLL || m.Msg == CharCommand.WM_VSCROLL)
                if (m.WParam.ToInt32() != CharCommand.SB_ENDSCROLL)
                    Invalidate();

            base.WndProc(ref m);

            if (ImeAllowed && m.Msg == CursorAndIME.WM_IME_SETCONTEXT && m.WParam.ToInt32() == 1)
            {
                Console.WriteLine("hangul key pressed");
                this.PCursor.CreateImmAssociateContext();
            }
        }



        /// <summary>
        /// 查找
        /// </summary>
        /// <param name="findText"></param>
        public void FindTexts(FindText findText) {
            this.PParser.ClearFindDatas();

            if (findText == null || string.IsNullOrEmpty(findText.FindString)) {
                this.Invalidate();
                return;
            }
            var result = new List<FindTextResult>();
            if (!findText.Multiline) {
                if (findText.IsRegex)
                    this.FindSingleRegex(findText);
                else
                    this.FindSingleLine(findText);
            } else
                FindTextMultiline(findText);

            this.Invalidate();
        }



        /// <summary>
        /// 搜索单行
        /// </summary>
        /// <param name="findText"></param>
        private void FindSingleLine(FindText findText) {
            StringComparison sc = StringComparison.CurrentCulture;
            if (findText.IgnoreCase)
                sc = StringComparison.CurrentCultureIgnoreCase;

            findText.FindString = findText.FindString.Trim(CharCommand.Char_Enter, CharCommand.Char_Newline);
            for (var i = 0; i < this.PParser.PLineString.Count; i++) {
                this.FindLine(this.PParser.PLineString[i], 0, findText.FindString, sc);
                this.FindSingleLine(findText, this.PParser.PLineString[i], sc);
            }
        }

        private void FindSingleLine(FindText findText, LineString ls, StringComparison sc) {
            if (ls.IsFurl()) {
                LineString[] outValue;
                if (this.PParser.PPucker.PDictPuckerList.TryGetValue(ls.ID, out outValue)) {
                    foreach (var l in outValue) {
                        this.FindLine(l, 0, findText.FindString, sc);
                        this.FindSingleLine(findText, l, sc);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="findText"></param>
        private void FindSingleRegex(FindText findText) {
            var rp = RegexOptions.None;
            if (findText.IgnoreCase)
                rp = RegexOptions.IgnoreCase;

            var r = new Regex(findText.FindString, rp);
            foreach (var ls in this.PParser.PLineString) {
                FindSingleRegex(findText, ls, r);
            }
        }

        private void FindSingleRegex(FindText findText, LineString ls, Regex regex) {
            var ms = regex.Matches(ls.Text + "\r\n");
            if (ms != null && ms.Count > 0) {
                foreach (Match m in ms) {
                    var findLocation = new FindTextLocation();
                    findLocation.FineString = m.Value;
                    findLocation.IndexX = m.Index;
                    findLocation.ID = ls.ID;
                    this.PParser.AddFindData(findLocation);
                }
            }
            if (ls.IsFurl()) {
                LineString[] outValue;
                if (this.PParser.PPucker.PDictPuckerList.TryGetValue(ls.ID, out outValue)) {
                    foreach (var l in outValue) {
                        FindSingleRegex(findText, l, regex);
                    }
                }
            }


        }


        private void FindLine(LineString ls, int startIndex, string findStr, StringComparison sc) {
            var i = ls.Text.IndexOf(findStr, startIndex, sc);
            if (i > -1) {
                var findLocation = new FindTextLocation();
                findLocation.FineString = findStr;
                findLocation.IndexX = i;
                findLocation.ID = ls.ID;
                this.PParser.AddFindData(findLocation);
                if (i + findStr.Length < ls.Text.Length - 1)
                    this.FindLine(ls, i + findStr.Length, findStr, sc);
            }
        }


        /// <summary>
        /// 多行查找文本
        /// </summary>
        /// <param name="findText"></param>
        private void FindTextMultiline(FindText findText) {
            StringComparison sc = StringComparison.CurrentCulture;

            var text = this.PParser.GetText();
            if (findText.IsRegex)
                this.FindTextMulineRegex(text, findText.FindString, findText.IgnoreCase);
            else {
                if (findText.IgnoreCase)
                    sc = StringComparison.CurrentCultureIgnoreCase;
                FindTextMulineString(text, 0, findText.FindString, sc);
            }
        }
        /// <summary>
        /// 多行正则表达式
        /// </summary>
        /// <param name="content"></param>
        /// <param name="startIndex"></param>
        /// <param name="findStr"></param>
        /// <param name="sc"></param>
        private void FindTextMulineRegex(string content, string findStr, bool ignoreCase) {
            var l = content.Length;
            var rp = RegexOptions.None;
            if (ignoreCase)
                rp = RegexOptions.IgnoreCase;

            var reg = new Regex(findStr, rp);
            var ms = reg.Matches(content);
            if (ms != null && ms.Count > 0) {
                foreach (Match m in ms) {
                    int lineIndex = 0;
                    int lineStartIndex = 0;
                    var vStr = content.Substring(m.Index);
                    var ls = FindTextMulineIndexToLineString(m.Index, out lineIndex, out lineStartIndex);
                    if (ls == null)
                        return;
                    FindTextAdd(m.Value, lineIndex, lineStartIndex);
                }
            }
        }

        /// <summary>
        /// 查找多行 字符串形式
        /// </summary>
        /// <param name="content"></param>
        /// <param name="startIndex"></param>
        /// <param name="findStr"></param>
        /// <param name="sc"></param>
        private void FindTextMulineString(string content, int startIndex, string findStr, StringComparison sc) {
            findStr = findStr.Replace(CharCommand.Char_Tab.ToString(), " ".PadRight(this.PParser.PLanguageMode.TabSpaceCount));
            var index = content.IndexOf(findStr, startIndex, sc);
            if (index > -1) {
                int lineIndex = 0;
                int lineStartIndex = 0;
                var ls = FindTextMulineIndexToLineString(index, out lineIndex, out lineStartIndex);
                if (ls == null)
                    return;
                FindTextAdd(findStr, lineIndex, lineStartIndex);

                if (index + findStr.Length < content.Length - 1)
                    FindTextMulineString(content, index + findStr.Length, findStr, sc);

            }
        }

        /// <summary>
        /// 将查找到的内容添加到列表中
        /// </summary>
        /// <param name="findStr"></param>
        /// <param name="findStartLineStringIndex"></param>
        /// <param name="lineStartIndex"></param>
        private void FindTextAdd(string findStr, int findStartLineStringIndex, int lineStartIndex) {
            var array = findStr.Split(CharCommand.Char_Newline);
            FindTextLocation ftLocation = null;
            for (var i = 0; i < array.Length; i++) {
                var ls = this.PParser.PLineString[findStartLineStringIndex + i];
                ftLocation = new FindTextLocation();
                ftLocation.ID = ls.ID;
                this.PParser.AddFindData(ftLocation);
                if (i == 0) {
                    ftLocation.FineString = ls.Text.Substring(lineStartIndex);
                    ftLocation.IndexX = lineStartIndex;
                } else if (i == array.Length - 1) {
                    ftLocation.FineString = ls.Text.Substring(0, array[i].Length);
                    ftLocation.IndexX = 0;
                } else {
                    ftLocation.FineString = ls.Text;
                    ftLocation.IndexX = 0;
                }
            }
        }

        private LineString FindTextMulineIndexToLineString(int index, out int lineIndex, out int startIndex) {
            lineIndex = 0;
            startIndex = 0;
            var findLength = 0;
            for (int i = 0; i < this.PParser.PLineString.Count; i++) {
                findLength += this.PParser.PLineString[i].Length + 2;
                if (findLength > index) {
                    lineIndex = i;
                    startIndex = this.PParser.PLineString[i].Length + 2 - (findLength - index);
                    return this.PParser.PLineString[i];
                } else if (findLength == index) {
                    lineIndex = i + 1;
                    startIndex = 0;
                    return this.PParser.PLineString[lineIndex];
                }
            }

            return null;
        }


        #region 注释

        /// <summary>
        /// 注释
        /// </summary>
        /// <param name="cancel"></param>
        public void AddComment(string commentStartStr, bool cancel = false) {
            var action = new Actions.CommentAction(this.PParser);
            action.PCancel = cancel;
            action.PCommentStartStr = commentStartStr;
            if (cancel)
                action.IsExternal = true;
            action.Execute();
            if (action.PIsAddUndo)
                this.PParser.AddAction(action);
        }

        #endregion
    }
}
