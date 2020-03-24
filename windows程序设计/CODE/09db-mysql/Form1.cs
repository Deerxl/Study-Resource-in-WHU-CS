using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data;
using MySql.Data.MySqlClient;
using CrystalDecisions.CrystalReports.Engine;


namespace db_op
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MySqlConnection conn;
            MySqlCommand cmd;
            MySqlDataAdapter adapter;
            DataSet ds = new DataSet();//保存查询的中间结果
            conn = new MySqlConnection();
            cmd = new MySqlCommand();
            adapter=new MySqlDataAdapter();
            conn.ConnectionString = "server=127.0.0.1;uid=root;" +
            "pwd=good;database=tea;";//设置mysql连接字串
            try 
            {
                conn.Open();//数据库连接
                cmd.Connection = conn;

                //查询数据库的select语句
                cmd.CommandText="select * from tb_student";
                //adapter与select语句关联
                adapter.SelectCommand = cmd;
                adapter.Fill(ds,"tb_student");
                //显示到listbox上
                listBox1.Items.Clear();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    listBox1.Items.Add(ds.Tables[0].Rows[i][1].ToString());
                }
                conn.Close();        
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            MySqlConnection conn;
            MySqlCommand cmd;
            MySqlDataAdapter adapter;
            DataSet ds = new DataSet();//保存查询的中间结果
            conn = new MySqlConnection();
            cmd = new MySqlCommand();
            adapter = new MySqlDataAdapter();
            conn.ConnectionString = "server=127.0.0.1;uid=root;" +
            "pwd=good;database=tea;";//设置mysql连接字串
            try
            {
                conn.Open();//数据库连接
                cmd.Connection = conn;

                //查询数据库的select语句
                cmd.CommandText = "select * from tb_student where Col_nianling<23";
                //adapter与select语句关联
                adapter.SelectCommand = cmd;
                adapter.Fill(ds, "tb_student");
                //显示到dataGridView1上
                dataGridView1.Columns.Add("Col_xuehao", "学号");
                dataGridView1.Columns.Add("Col_xingming", "姓名");
                dataGridView1.Columns.Add("Col_nianling", "年龄");
                int row_num;
                for (int ri = 0; ri < ds.Tables[0].Rows.Count; ri++)
                {
                    row_num = dataGridView1.Rows.Add(1);
                    dataGridView1.Rows[row_num].Cells["Col_xuehao"].Value = 
                        ds.Tables[0].Rows[ri]["Col_xuehao"];
                    dataGridView1.Rows[row_num].Cells["Col_xingming"].Value = 
                        ds.Tables[0].Rows[ri]["Col_xingming"];
                    dataGridView1.Rows[row_num].Cells["Col_nianling"].Value = 
                        ds.Tables[0].Rows[ri]["Col_nianling"];
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MySqlConnection conn;
            MySqlCommand cmd;
            MySqlDataAdapter adapter;
            DataSet ds = new DataSet();//保存查询的中间结果
            conn = new MySqlConnection();
            cmd = new MySqlCommand();
            adapter = new MySqlDataAdapter();
            conn.ConnectionString = "server=127.0.0.1;uid=root;" +
            "pwd=good;database=tea;";//设置mysql连接字串
            try
            {
                conn.Open();//数据库连接
                cmd.Connection = conn; 
                cmd.CommandText = "delete from tb_student where Col_nianling=19";
                cmd.ExecuteNonQuery(); 
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void reportDocument1_InitReport(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            MySqlConnection conn;
            MySqlCommand cmd;
            MySqlDataAdapter adapter;
            DataSet ds = new DataSet();//保存查询的中间结果
            ReportDocument myReport = new ReportDocument();

            conn = new MySqlConnection();
            cmd = new MySqlCommand();
            adapter = new MySqlDataAdapter();
            conn.ConnectionString = "server=127.0.0.1;uid=root;" +
            "pwd=good;database=tea;";//设置mysql连接字串
            try
            {
                conn.Open();//数据库连接
                cmd.Connection = conn;
                cmd.CommandText = "select *  from tb_student";
                adapter.SelectCommand = cmd;
                adapter.Fill(ds, "tb_student");

                ds.WriteXml(@"d:\mysqldataset.xml", XmlWriteMode.WriteSchema);

                //myReport.Load(@"CrystalReport1.rpt");
                //myReport.SetDataSource(ds.Tables["tb_student"]);
                //crystalReportViewer1.ReportSource = myReport;
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MySqlConnection conn;
            MySqlCommand cmd;
            MySqlDataAdapter adapter;
            DataSet ds = new DataSet();//保存查询的中间结果
            ReportDocument myReport = new ReportDocument();

            conn = new MySqlConnection();
            cmd = new MySqlCommand();
            adapter = new MySqlDataAdapter();
            conn.ConnectionString = "server=127.0.0.1;uid=root;" +
            "pwd=good;database=tea;";//设置mysql连接字串
            try
            {
                conn.Open();//数据库连接
                cmd.Connection = conn;
                cmd.CommandText = "select *  from tb_student where Col_nianling>18";
                adapter.SelectCommand = cmd;
                adapter.Fill(ds, "tb_student");

                //ds.WriteXml(@"d:\mysqldataset.xml", XmlWriteMode.WriteSchema);

                myReport.Load(@"../../CrystalReport1.rpt");
                
                myReport.SetDataSource(ds.Tables[0]);
                crystalReportViewer1.ReportSource = myReport;
                
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
