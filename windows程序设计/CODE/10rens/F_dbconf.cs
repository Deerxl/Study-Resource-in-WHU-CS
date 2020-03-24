using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Data.SqlClient;
using System.Data.OracleClient;
using System.Management;

namespace rens
{
    public partial class F_dbconf : Form
    {
        public F_dbconf()
        {
            InitializeComponent();
        }

        private void F_dbconf_Load(object sender, EventArgs e)
        {

            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //显示可用数据库 
            this.button3.Enabled = false;
            System.Data.Common.DbDataSourceEnumerator emumerator = 
                System.Data.SqlClient.SqlClientFactory.Instance.CreateDataSourceEnumerator();  
            DataTable dsrc_table = emumerator.GetDataSources();
            //dsrc_table.co
            if (dsrc_table != null && dsrc_table.Rows.Count > 0) 
            { 
                this.dataGridView1.DataSource = dsrc_table; 
                this.dataGridView1.Columns[0].HeaderText = "服务器名";
                this.dataGridView1.Columns[1].HeaderText = "实例名";
            }
            this.button3.Enabled = true;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool con_sucess = true;
            //测试连接
            SqlConnection hCon = new SqlConnection("Server=6BE5ACE6428D418\\SQLEC;User ID=sa;Password=mssql;database=model");
            try
            {
                hCon.Open();
                hCon.Close();
            }
            catch (Exception ee)
            {
                con_sucess = false;
                MessageBox.Show(ee.Message);
            }
            finally
            {
                if (con_sucess)
                {//连接成功
                    MessageBox.Show("数据库连接测试成功");
                }
                else
                { //不成功
                    
                }
            } 
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
            base.OnFormClosing(e);
        }
    }
}
