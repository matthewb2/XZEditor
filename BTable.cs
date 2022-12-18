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
            
            SelectionColor = Color.LightSteelBlue;
            
            MinimumRowHeight = 20;

            Table = new cTable(this);
            Table.Dock = DockStyle.Fill;


            Controls.Add(Table);
            
        }

        protected override void OnGotFocus(System.EventArgs e)
        {
            Table.Select();
            base.OnGotFocus(e);
        }


        public class cTable : ScrollableControl
        {

            private BTable BParent;
            public cTable(BTable mParent)
            {
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
                        //
                        int cellwidth = BParent.Columns[row.Cells.IndexOf(cell)].ColumnWidth;

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

                    //Generating cell rectangles
                    int counter = 0;
                    leftx = 0;
                    foreach (Cell cell in row.Cells)
                    {
                        
                        int cellwidth = BParent.Columns[counter].ColumnWidth;
                        

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
                    //At last painting all cells
                    foreach (Cell cell in row.Cells)
                    {
                 
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
        public class Row
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