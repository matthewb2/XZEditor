using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System;
namespace BTable_control
{
    /// <summary>
    /// (c) BANJEN Software
    /// Evgeny Bannikov, 2011
    /// </summary>
    public class BTable : Control
    {
        //selection change event delegate
        public delegate void EditingStoped(string prevval, ref string newval);
        public event EditingStoped EndEdit;

        //we need it to call from inner table onDoubleClick
        public new event MouseEventHandler MouseDoubleClick;
        //we need it to call from inner table onDoubleClick
        public new event MouseEventHandler MouseClick;

        public List<Column> Columns = new List<Column>();
        public List<Row> Rows = new List<Row>();
        public cTable Table;
        public cHeader Header;
        public Color GridColor { get; set; }
        public bool ColumnGridLines { get; set; }
        public bool RowGridLines { get; set; }
        public Font HeaderFont { get; set; }
        public Color SelectionColor { get; set; }
        public Color HeaderBackColor { get; set; }
        public Color HeaderForeColor { get; set; }
        public Color DisabledColor { get; set; }
        public bool HideDisabledRow { get; set; }
        public Row SelectedRow { get; set; }
        public Cell SelectedCell { get; set; }
        public int MinimumRowHeight { get; set; }
        public int SelectedColumn { get; set; }
        private int SortedColumn = 0;
        private bool SortedDesc = false;
        public bool UserSorting { get; set; }

        public BTable()
        {
            GridColor = Color.Black;
            ColumnGridLines = true;
            RowGridLines = true;
            UserSorting = true;
            HeaderFont = this.Font;
            SelectionColor = Color.LightSteelBlue;
            HeaderBackColor = Color.Gray;
            HeaderForeColor = Color.Black;

            Header = new cHeader(this);
            Header.Dock = DockStyle.Top;
            Header.Height = 30;
            MinimumRowHeight = 20;

            Table = new cTable(this);
            Table.Dock = DockStyle.Fill;


            Controls.Add(Table);
            Controls.Add(Header);
        }

        protected override void OnGotFocus(System.EventArgs e)
        {
            Table.Select();
            base.OnGotFocus(e);
        }

        /// <summary>
        /// Sorting column by Value1
        /// </summary>
        /// <param name="Column"></param>
        public void SortValue(int Column)
        {
            if (Column < Columns.Count)
            {
                if (SortedDesc)
                {
                    Rows.Sort(delegate(Row p1, Row p2)
                    {
                        try
                        {
                            return p1.Cells[Column].Value.CompareTo(p2.Cells[Column].Value);
                        }
                        catch
                        {
                            return 0;
                        }
                        ;

                    });
                }
                else
                {
                    Rows.Sort(delegate(Row p1, Row p2)
                    {
                        try
                        {
                            return -p1.Cells[Column].Value.CompareTo(p2.Cells[Column].Value);
                        }
                        catch
                        {
                            return 0;
                        }

                    });
                }

                SortedColumn = Column;
                SortedDesc = !SortedDesc;
            }
        }

        /// <summary>
        /// Sorting column by Value2
        /// </summary>
        /// <param name="Column"></param>
        public void SortValue2(int Column)
        {
            if (Column < Columns.Count)
            {
                Rows.Sort(delegate(Row p1, Row p2) { return p1.Cells[Column].Value2.CompareTo(p2.Cells[Column].Value2); });
            }
        }

        /// <summary>
        /// Sorting column by Value2 as float
        /// </summary>
        /// <param name="Column"></param>
        public void SortValue2Float(int Column)
        {
            if (Column < Columns.Count)
            {
                Rows.Sort(delegate(Row p1, Row p2) { return Convert.ToDouble(p1.Cells[Column].Value2.Replace(".", ",")).CompareTo(Convert.ToDouble(p2.Cells[Column].Value2.Replace(".", ","))); });
            }
        }

        public class cTable : ScrollableControl
        {

            private BTable BParent;
            public cTable(BTable mParent)
            {
                VScroll = true;
                HScroll = true;
                AutoScroll = true;
                BParent = mParent;
            }

            private TextBox edittextbox = null;
            private DateTimePicker edittime = null;
            public bool AntiAliasText { get; set; }
            public int HeaderHeight { get; set; }



            /// <summary>
            /// Painting table
            /// </summary>
            /// <param name="e"></param>
            protected override void OnPaint(PaintEventArgs e)
            {
                Pen GridPen = new Pen(BParent.GridColor);

                DoubleBuffered = true;
                if (AntiAliasText)
                {
                    e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                }
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                Matrix m = new Matrix();
                m.Translate(this.AutoScrollPosition.X, this.AutoScrollPosition.Y, MatrixOrder.Append);
                e.Graphics.Transform = m;

                int topx = HeaderHeight;
                int maxwidth = 0;
                foreach (BTable.Column col in BParent.Columns)
                {
                    maxwidth += col.ColumnWidth;
                }


                //Loop through all rows
                foreach (Row row in BParent.Rows)
                {
                    //check if all cells are disabled and we hide them
                    if (BParent.HideDisabledRow)
                    {
                        bool alldisabled = true;
                        foreach (Cell cell in row.Cells)
                        {
                            if (cell.Enabled)
                            {
                                alldisabled = false;
                                break;
                            }
                        }
                        if (alldisabled)
                        {
                            foreach (Cell cell in row.Cells)
                            {
                                cell.Rectangle = new Rectangle(0, 0, 0, 0);
                            }
                            continue;
                        }
                    }


                    //Find maximum height
                    int rowheight = 0;
                    int leftx = 0;
                    foreach (Cell cell in row.Cells)
                    {
                        
                        if (cell.Value == "~")
                        {
                            continue;
                        }

                        int cellwidth = BParent.Columns[row.Cells.IndexOf(cell)].ColumnWidth;
                        
                        int i = 1;
                        int cellind = row.Cells.IndexOf(cell);
                        if (cellind + i < BParent.Columns.Count)
                        {
                            while (row.Cells[cellind + i].Value == "~")
                            {
                                cellwidth += BParent.Columns[cellind + i].ColumnWidth;
                                i++;
                                if (cellind + i == BParent.Columns.Count)
                                    break;
                            }
                        }


                        Font drawfont = this.Font;
                        if (cell.Font != null)
                        {
                            drawfont = cell.Font;
                        }
                        else
                        {
                            cell.Font = drawfont;
                        }

                        int currowheight = (int)e.Graphics.MeasureString(cell.Value.ToString(), drawfont, cellwidth).Height;
                        if (currowheight > rowheight)
                        {
                            rowheight = currowheight;
                        }

                        leftx += cellwidth;
                    }

                    if (rowheight < BParent.MinimumRowHeight)
                    {
                        rowheight = BParent.MinimumRowHeight;
                    }

                    //Generating cell rectangles
                    int counter = 0;
                    leftx = 0;
                    foreach (Cell cell in row.Cells)
                    {
                        if (cell.Value == "~")
                        {
                            continue;
                        }

                        int cellwidth = BParent.Columns[counter].ColumnWidth;
                        int i = 1;
                        int cellind = row.Cells.IndexOf(cell);
                        if (cellind + i < BParent.Columns.Count)
                        {
                            while (row.Cells[cellind + i].Value == "~")
                            {
                                cellwidth += BParent.Columns[cellind + i].ColumnWidth;
                                i++;
                                if (cellind + i == BParent.Columns.Count)
                                    break;
                            }
                        }

                        Rectangle cellrectangle = new Rectangle(leftx, topx, cellwidth, rowheight);
                        cell.Rectangle = cellrectangle;

                        leftx += cellwidth;
                        counter++;
                    }

                    topx += rowheight;
                }


                //Loop through all rows, Again)
                foreach (Row row in BParent.Rows)
                {
                    //check if all cells are disabled and we hide them
                    if (BParent.HideDisabledRow)
                    {
                        bool alldisabled = true;
                        foreach (Cell cell in row.Cells)
                        {
                            if (cell.Enabled)
                            {
                                alldisabled = false;
                                break;
                            }
                        }
                        if (alldisabled)
                        {
                            continue;
                        }
                    }

                    //At last painting all cells
                    foreach (Cell cell in row.Cells)
                    {
                        if (cell.Value == "~")
                        {
                            continue;
                        }

                        Brush ForeBrush = new SolidBrush(ForeColor);
                        Brush BackBrush = new SolidBrush(BackColor);
                        if (!cell.ForeColor.IsEmpty)
                        {
                            ForeBrush = new SolidBrush(cell.ForeColor);
                        }
                        if (!cell.BackColor.IsEmpty)
                        {
                            BackBrush = new SolidBrush(cell.BackColor);
                        }

                        //Painting background
                        e.Graphics.FillRectangle(BackBrush, cell.Rectangle);

                        //Painting selection background
                        if (BParent.SelectedRow == cell.ParentRow)
                        {
                            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(100, BParent.SelectionColor)), cell.Rectangle);
                        }

                        //Draw ColumnGridLine if needed
                        if (BParent.ColumnGridLines)
                        {
                            e.Graphics.DrawLine(GridPen, cell.Rectangle.X, cell.Rectangle.Y, cell.Rectangle.X, cell.Rectangle.Y + cell.Rectangle.Height);
                        }

                        //Draw RowGridLine if needed
                        if (BParent.RowGridLines)
                        {
                            e.Graphics.DrawLine(GridPen, cell.Rectangle.X, cell.Rectangle.Y, cell.Rectangle.X + cell.Rectangle.Width, cell.Rectangle.Y);

                            //for the last row we must draw footer line
                            if (BParent.Rows.IndexOf(row) == BParent.Rows.Count - 1)
                            {
                                e.Graphics.DrawLine(GridPen, cell.Rectangle.X, cell.Rectangle.Y + cell.Rectangle.Height, cell.Rectangle.X + cell.Rectangle.Width, cell.Rectangle.Y + cell.Rectangle.Height);
                            }
                        }

                        Pen ForePen = new Pen(ForeColor);
                        Pen CheckPen = new Pen(ForeColor, 2);
                        if (!cell.Enabled)
                        {
                            ForeBrush = new SolidBrush(BParent.DisabledColor);
                            ForePen = new Pen(BParent.DisabledColor);
                            CheckPen = new Pen(BParent.DisabledColor, 2);
                        }

                        //if cell has checkbox
                        if (cell.CheckBox)
                        {
                            Rectangle checkrect = new Rectangle(cell.Rectangle.X + cell.Rectangle.Width / 2 - 7, cell.Rectangle.Y + cell.Rectangle.Height / 2 - 7, 15, 15);
                            e.Graphics.DrawRectangle(ForePen, checkrect);
                            if (cell.Checked)
                            {
                                Point[] check =
                            {
                            new Point( 2+checkrect.X, checkrect.Y+8),
                            new Point( 7+checkrect.X, checkrect.Y+12),
                            new Point(13+checkrect.X, checkrect.Y+1),
                            };
                                e.Graphics.DrawCurve(CheckPen, check);
                            }
                        }
                        StringFormat sf = new StringFormat();
                        if (cell.CenterText)
                        {
                            sf.Alignment = StringAlignment.Center;
                            sf.LineAlignment = StringAlignment.Center;
                        }
                        e.Graphics.DrawString(cell.Value.ToString(), cell.Font, ForeBrush, cell.Rectangle, sf);

                    }
                }

                AutoScrollMinSize = new Size(maxwidth, topx);
                BParent.Header.scrolposx = this.AutoScrollPosition.X;
                BParent.Header.Refresh();
                base.OnPaint(e);
            }

            /// <summary>
            /// Selecting cell
            /// </summary>
            /// <param name="e"></param>
            protected override void OnMouseDown(MouseEventArgs e)
            {

                //First update edit cell
                if (edittextbox != null)
                {
                    string newval = edittextbox.Text;
                    if (BParent.EndEdit != null)
                    {
                        BParent.EndEdit(BParent.SelectedCell.Value, ref newval);
                    }
                    BParent.SelectedCell.Value = newval;

                    edittextbox.Dispose();
                    edittextbox = null;
                    Invalidate();
                }
                if (edittime != null)
                {
                    string newval = edittime.Value.ToShortDateString();
                    if (BParent.EndEdit != null)
                    {
                        BParent.EndEdit(BParent.SelectedCell.Value, ref newval);
                    }
                    BParent.SelectedCell.Value = newval;

                    edittime.Dispose();
                    edittime = null;
                    Invalidate();
                }

                //Second select new cell
                foreach (Row row in BParent.Rows)
                {
                    int column = 0;
                    foreach (Cell cell in row.Cells)
                    {

                        if (cell.Rectangle.Contains(e.X - this.AutoScrollPosition.X, e.Y - this.AutoScrollPosition.Y))
                        {
                            BParent.SelectedColumn = column;
                            BParent.SelectedCell = cell;
                            BParent.SelectedRow = row;

                            if (BParent.SelectedCell.Enabled)
                            {
                                //if selected cell is checkbox
                                if (BParent.SelectedCell.CheckBox)
                                {
                                    BParent.SelectedCell.Checked = !BParent.SelectedCell.Checked;
                                }
                            }
                            Invalidate();
                            break;
                        }
                        column++;
                    }
                }



                base.OnMouseDown(e);
            }


            /// <summary>
            /// Show editing textbox
            /// </summary>
            /// <param name="e"></param>
            protected override void OnMouseDoubleClick(MouseEventArgs e)
            {
                if (BParent.SelectedCell != null)
                {
                    if (BParent.SelectedCell.Editable && BParent.SelectedCell.Enabled && !BParent.SelectedCell.CheckBox && !BParent.SelectedCell.DateTime)
                    {
                        edittextbox = new TextBox();
                        edittextbox.Multiline = true;
                        edittextbox.Font = BParent.SelectedCell.Font;
                        edittextbox.Left = BParent.SelectedCell.Rectangle.X + 1 + this.AutoScrollPosition.X;
                        edittextbox.Top = BParent.SelectedCell.Rectangle.Y + 1 + this.AutoScrollPosition.Y;
                        edittextbox.Width = BParent.SelectedCell.Rectangle.Width - 2;
                        edittextbox.Height = BParent.SelectedCell.Rectangle.Height - 2;
                        edittextbox.Text = BParent.SelectedCell.Value.ToString();
                        edittextbox.BorderStyle = BorderStyle.None;
                        edittextbox.KeyDown += new KeyEventHandler(edittextbox_KeyDown);
                        edittextbox.KeyPress += new KeyPressEventHandler(edittextbox_KeyPress);
                        this.Controls.Add(edittextbox);
                        edittextbox.Focus();
                        base.OnMouseDoubleClick(e);
                    }
                    if (BParent.SelectedCell.Editable && BParent.SelectedCell.Enabled && BParent.SelectedCell.DateTime)
                    {
                        edittime = new DateTimePicker();
                        edittime.Font = BParent.SelectedCell.Font;
                        edittime.Left = BParent.SelectedCell.Rectangle.X + 1 + this.AutoScrollPosition.X;
                        edittime.Top = BParent.SelectedCell.Rectangle.Y + 1 + this.AutoScrollPosition.Y;
                        edittime.Width = BParent.SelectedCell.Rectangle.Width - 2;
                        edittime.Height = BParent.SelectedCell.Rectangle.Height - 2;
                        if (BParent.SelectedCell.Value != "")
                        {
                            edittime.Value = Convert.ToDateTime(BParent.SelectedCell.Value);
                        }
                        else
                        {
                            edittime.Value = DateTime.Now;
                        }
                        edittime.CloseUp += new EventHandler(edittime_CloseUp);
                        this.Controls.Add(edittime);
                        edittime.Focus();
                        base.OnMouseDoubleClick(e);
                    }
                    // raise the event
                    if (BParent.MouseDoubleClick != null)
                    {
                        BParent.MouseDoubleClick(BParent, e);
                    }
                }
            }

            protected override void OnMouseClick(MouseEventArgs e)
            {
                // raise the event
                if (BParent.MouseClick != null)
                {
                    BParent.MouseClick(BParent, e);
                }
            }

            void edittime_CloseUp(object sender, EventArgs e)
            {
                string newval = edittime.Value.ToShortDateString();
                if (BParent.EndEdit != null)
                {
                    BParent.EndEdit(BParent.SelectedCell.Value, ref newval);
                }
                BParent.SelectedCell.Value = newval;
                edittime.Dispose();
                edittime = null;
            }

            void edittextbox_KeyPress(object sender, KeyPressEventArgs e)
            {
                //always allow enter and backspace
                if (!(e.KeyChar == 13 || e.KeyChar == '\b'))
                {
                    if (BParent.SelectedCell.AllowedSymbols != "")
                    {
                        if (!BParent.SelectedCell.AllowedSymbols.Contains(e.KeyChar.ToString()))
                        {

                            ToolTip tp = new ToolTip();
                            tp.Show(BParent.SelectedCell.UnAllowedSymbolText, this, ((Control)sender).Left + ((Control)sender).Width / 2, ((Control)sender).Top + ((Control)sender).Height, 1000);
                            e.Handled = true;
                        }
                    }
                }
            }

            void edittextbox_KeyDown(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    string newval = edittextbox.Text;
                    if (BParent.EndEdit != null)
                    {
                        BParent.EndEdit(BParent.SelectedCell.Value, ref newval);
                    }
                    BParent.SelectedCell.Value = newval;
                    edittextbox.Dispose();
                    edittextbox = null;
                    Invalidate();
                }
                if (e.KeyCode == Keys.Escape)
                {
                    string newval = "";
                    if (BParent.EndEdit != null)
                    {
                        BParent.EndEdit(BParent.SelectedCell.Value, ref newval);
                    }
                    edittextbox.Dispose();
                    edittextbox = null;
                }
            }

            protected override void OnResize(System.EventArgs e)
            {
                if (edittextbox != null)
                {
                    edittextbox.Left = BParent.SelectedCell.Rectangle.X + 1 + this.AutoScrollPosition.X;
                    edittextbox.Top = BParent.SelectedCell.Rectangle.Y + 1 + this.AutoScrollPosition.Y;
                    edittextbox.Width = BParent.SelectedCell.Rectangle.Width - 2;
                    edittextbox.Height = BParent.SelectedCell.Rectangle.Height - 2;
                }
                base.OnResize(e);
            }




        }

        public class cHeader : Control
        {
            private int ResizingColumn = -1;
            private BTable BParent;
            public cHeader(BTable mParent)
            {
                BParent = mParent;
            }

            public int scrolposx = 0;

            /// <summary>
            /// Painting Table Header
            /// </summary>
            /// <param name="e"></param>
            protected override void OnPaint(PaintEventArgs e)
            {
                DoubleBuffered = true;
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

                Matrix m = new Matrix();
                m.Translate(scrolposx, 0, MatrixOrder.Append);
                e.Graphics.Transform = m;

                int maxwidth = 0;
                foreach (BTable.Column col in BParent.Columns)
                {
                    maxwidth += col.ColumnWidth;
                }
                Rectangle hrect = new Rectangle(0, 0, maxwidth, Height);

                if (e.ClipRectangle.Height > 0 && e.ClipRectangle.Width > 0)
                {
                    Pen GridPen = new Pen(BParent.HeaderBackColor);
                    Brush TextBrush = new SolidBrush(BParent.HeaderForeColor);
                    Brush HeaderBrush = new LinearGradientBrush(e.ClipRectangle, BParent.HeaderBackColor, ControlPaint.Light(BParent.HeaderBackColor), 90, false);

                    e.Graphics.FillRectangle(HeaderBrush, hrect);

                    //draw Column headers
                    if (BParent.Columns.Count > 0)
                    {
                        int leftx = 0;
                        for (int i = 0; i < BParent.Columns.Count; i++)
                        {
                            int cellwidth = BParent.Columns[i].ColumnWidth;
                
                            Rectangle ColumnHeaderRect = new Rectangle(leftx, 0, cellwidth, Height - 1);

                            if (i > 0)
                            {
                                e.Graphics.DrawLine(GridPen, leftx, 0, leftx, Height - 1);
                            }

                            //SortedSymbol
                            if (i == BParent.SortedColumn)
                            {
                                if (BParent.SortedDesc)
                                {
                                    Point[] sortriag = { new Point(leftx + cellwidth - 10, Height - 5), new Point(leftx + cellwidth - 2, Height - 5), new Point(leftx + cellwidth - 6, Height - 12) };
                                    e.Graphics.FillPolygon(TextBrush, sortriag);
                                }
                                else
                                {
                                    Point[] sortriag = { new Point(leftx + cellwidth - 10, Height - 12), new Point(leftx + cellwidth - 2, Height - 12), new Point(leftx + cellwidth - 6, Height - 5) };
                                    e.Graphics.FillPolygon(TextBrush, sortriag);
                                }
                            }

                            e.Graphics.DrawString(BParent.Columns[i].ColumnName, BParent.HeaderFont, TextBrush, ColumnHeaderRect);

                            leftx += cellwidth;
                        }
                    }
                    e.Graphics.DrawLine(GridPen, 0, Height - 1, maxwidth, Height - 1);
                }
                
            }


            protected override void OnMouseMove(MouseEventArgs e)
            {
                this.Cursor = Cursors.Default;
                if (BParent.Columns.Count > 0)
                {
                    int leftx = 0;
                    for (int i = 0; i < BParent.Columns.Count; i++)
                    {
                        leftx += BParent.Columns[i].ColumnWidth;
                        if (e.X > leftx - 3 + scrolposx && e.X < leftx + 3 + scrolposx)
                        {
                            this.Cursor = Cursors.VSplit;
                            break;
                        }
                    }
                }

                if (ResizingColumn > -1)
                {
                    int leftx = 0;
                    for (int i = 0; i < ResizingColumn; i++)
                    {
                        leftx += BParent.Columns[i].ColumnWidth;
                    }
                    int newcolwidth = e.X - leftx-scrolposx;
                    //check minimum column width
                    if (newcolwidth < 15)
                    {
                        newcolwidth = 15;
                    }

                    BParent.Columns[ResizingColumn].ColumnWidth = newcolwidth;
                    Invalidate();
                    BParent.Table.Invalidate();
                }

                base.OnMouseMove(e);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                if (BParent.Columns.Count > 0)
                {
                    int leftx = 0;
                    for (int i = 0; i < BParent.Columns.Count; i++)
                    {
                        leftx += BParent.Columns[i].ColumnWidth;
                        if (e.X > leftx - 3 + scrolposx && e.X < leftx + 3 + scrolposx)
                        {
                            ResizingColumn = i;
                            break;
                        }
                    }
                }
                base.OnMouseDown(e);
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                ResizingColumn = -1;
                this.Cursor = Cursors.Default;
                base.OnMouseUp(e);
            }

            protected override void OnMouseClick(MouseEventArgs e)
            {
                if (BParent.UserSorting)
                {
                    if (BParent.Columns.Count > 0)
                    {
                        int leftx = 0;
                        for (int i = 0; i < BParent.Columns.Count; i++)
                        {
                            if (e.X > leftx + scrolposx && e.X < leftx + BParent.Columns[i].ColumnWidth + scrolposx)
                            {
                                BParent.SortValue(i);
                                BParent.Refresh();
                                break;
                            }
                            leftx += BParent.Columns[i].ColumnWidth;
                        }
                    }
                }
                base.OnMouseClick(e);
            }
        }


        /// <summary>
        /// Cell representation
        /// </summary>
        public class Cell : ICloneable
        {
            public Row ParentRow;
            public string Value;
            public string Value2;
            public Font Font;
            public Color BackColor;
            public Color ForeColor;
            public Rectangle Rectangle;
            public bool DateTime = false;
            public bool CheckBox = false;
            public bool Checked = false;
            private bool mEnabled = true;
            public bool Enabled
            {
                get
                {
                    if (ParentRow.Enabled)
                    {
                        return mEnabled;
                    }
                    else
                    {
                        return false;
                    }
                }
                set
                {
                    mEnabled = value;
                }
            }

            public bool Editable = false;
            public bool CenterText = false;
            public string AllowedSymbols = "";
            public string UnAllowedSymbolText = "";
            public Cell(string mValue)
            {
                Value = mValue;
                Value2 = mValue;
            }
            public object Clone()
            {
                return MemberwiseClone();
            }
        }

        /// <summary>
        /// Row representation
        /// </summary>
        public class Row : ICloneable
        {
            public bool boolparam { get; set; }
            public string stringparam { get; set; }
            public bool Enabled { get; set; }
            public List<Cell> Cells;
            public Row(List<Cell> cells)
            {
                Cells = cells;
                boolparam = false;
                stringparam = "";
                Enabled = true;
                foreach (Cell cl in Cells)
                {
                    cl.ParentRow = this;
                }
            }

            public Row(string[] cells)
            {
                Cells = new List<Cell>();
                Enabled = true;
                foreach (string celltext in cells)
                {
                    Cells.Add(new Cell(celltext));
                }
                foreach (Cell cl in Cells)
                {
                    cl.ParentRow = this;
                }
            }

            public object Clone()
            {
                List<Cell> newcells = new List<Cell>();

                foreach (Cell cl in Cells)
                {
                    newcells.Add((Cell)cl.Clone());
                }

                return new Row(newcells);
            }
        }

        /// <summary>
        /// Column representation
        /// </summary>
        public class Column
        {
            public string ColumnName;
            public int ColumnWidth;
            public Column(string mColumnName, int mColumnWidth)
            {
                ColumnName = mColumnName;
                ColumnWidth = mColumnWidth;
            }
        }
    }
}