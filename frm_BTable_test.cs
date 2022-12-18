using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BTable_control
{
    public partial class frm_BTable_test : Form
    {
        public frm_BTable_test()
        {
            InitializeComponent();
        }

        private void frm_BTable_test_Load(object sender, EventArgs e)
        {
                //generate columns
                BTable.Column col1 = new BTable.Column("№", 100);
                BTable.Column col2 = new BTable.Column("Description", 300);
                
                bTable1.Columns.Add(col1);
                bTable1.Columns.Add(col2);
                
                

                for (int j = 0; j < 10; j++)
                {
                    BTable.Row row = new BTable.Row(new string[] { j.ToString(), "Some long description" });
                    row.Cells[0].Editable = true;
                    row.Cells[0].BackColor = Color.LightGreen;
                    row.Cells[1].DateTime = true;
                    row.Cells[1].Editable = true;
                    row.Cells[1].BackColor = Color.LightGray;
                    bTable1.Rows.Add(row);
                }
            
        }

        private void frm_BTable_test_Resize(object sender, EventArgs e)
        {
            bTable1.Refresh();
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            bTable1.Refresh();
        }
    }
}
