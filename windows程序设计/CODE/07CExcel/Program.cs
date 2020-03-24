using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MsExcel = Microsoft.Office.Interop.Excel;
namespace CExcel
{
    class Program
    {
        static void Main(string[] args)
        {

            MsExcel.Application oExcApp;//Excel Application;
            MsExcel.Workbook oExcBook;//

            try
            {
                oExcApp = new MsExcel.Application();
                object missing = System.Reflection.Missing.Value;
                oExcBook = oExcApp.Workbooks.Add(true);
                MsExcel.Worksheet worksheet1 = (MsExcel.Worksheet)oExcBook.Worksheets["sheet1"];
                worksheet1.Activate(); 
                oExcApp.Visible = false;
                oExcApp.DisplayAlerts = false;  
                MsExcel.Range range1 = worksheet1.get_Range("B1", "H2");
                range1.Columns.ColumnWidth = 8;
                range1.Columns.RowHeight = 20;
                range1.Merge(false);
                //设置垂直居中和水平居中
                range1.VerticalAlignment = MsExcel.XlVAlign.xlVAlignCenter;
                range1.HorizontalAlignment = MsExcel.XlHAlign.xlHAlignCenter;
                range1.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.Blue);
                range1.Font.Size = 20;
                range1.Font.Bold = true;

                worksheet1.Cells[1, 2] = "学生成绩单";
                worksheet1.Cells[3, 1] = "学号";
                worksheet1.Cells[3, 2] = "姓名";
                worksheet1.Columns[1].ColumnWidth = 12;
                StreamReader sw = new StreamReader("list.csv");
                string a_str;
                string[] str_list;
                int i = 4;
                a_str = sw.ReadLine();
                while (a_str != null)
                {
                    str_list = a_str.Split(",".ToCharArray());
                    worksheet1.Cells[i, 1] = str_list[0];
                    worksheet1.Cells[i, 2] = str_list[1];
                    i++;
                    a_str = sw.ReadLine();
                }
                sw.Close(); 
                for (int i1 = 0; i1 < 5; i1++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        worksheet1.Cells[i1 + 18, j + 3].Value2 = "=CEILING.MATH(RAND()*100)";
                        worksheet1.Cells[i1 + 4, j + 3].Value2 = worksheet1.Cells[i1 + 18, j + 3].Value;
                    }
                } 

                //添加图表
                MsExcel.Shape theShape =
                worksheet1.Shapes.AddChart2(Type.Missing,
                MsExcel.XlChartType.xl3DColumn, 120, 130, 380, 250, Type.Missing); 
                //设置图标题文本
                theShape.Chart.ChartTitle.Caption = "学生成绩";  
                worksheet1.Cells[3, 3].Value2 = "美术"; 
                worksheet1.Cells[3, 4].Value2 = "物理";
                worksheet1.Cells[3, 5].Value2 = "政治";
                worksheet1.Cells[3, 6].Value2 = "化学";
                worksheet1.Cells[3, 7].Value2 = "体育";
                worksheet1.Cells[3, 8].Value2 = "英语";
                worksheet1.Cells[3, 9].Value2 = "数学";
                worksheet1.Cells[3, 10].Value2 = "历史"; 
                //设定图表的数据区域
                MsExcel.Range range = worksheet1.get_Range("b3:j8");  
                theShape.Chart.SetSourceData(range, Type.Missing);
                //设置单元格边框线型
                range1 = worksheet1.get_Range("a3", "j8");
                range1.Borders.LineStyle = MsExcel.XlLineStyle.xlContinuous;

                oExcBook.RefreshAll(); 
                worksheet1 = null;
                object file_name = Directory.GetCurrentDirectory() + @"\one.xlsx"; 
                oExcBook.Close(true, file_name, null);
                oExcApp.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oExcApp);
                oExcApp = null;
                System.GC.Collect(); 
            }
            catch (Exception e2)
            {
                MessageBox.Show(e2.Message);
            } 
        }
    }
}
