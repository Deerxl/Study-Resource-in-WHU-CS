using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MsExcel = Microsoft.Office.Interop.Excel;
using System.IO;
using System.Drawing;

namespace excel2
{
    class Program
    {
        static void Main(string[] args)
        {
            MsExcel.Application oExcApp;//Excel Application;
            MsExcel.Workbook oExcBook;//

            try
            {
                oExcApp = new MsExcel.ApplicationClass();
                object missing = System.Reflection.Missing.Value;
                oExcBook = oExcApp.Workbooks.Add(true);
                MsExcel.Worksheet worksheet1 = (MsExcel.Worksheet)oExcBook.Worksheets["sheet1"];
                worksheet1.Activate();


                oExcApp.Visible = false;
                oExcApp.DisplayAlerts = false;//不提示警告信息

                MsExcel.Range range1 = worksheet1.get_Range("B1", "E2");
                range1.Columns.ColumnWidth = 20;
                range1.Columns.RowHeight = 20;
                range1.Merge(false);
                //设置垂直居中和水平居中
                range1.VerticalAlignment = MsExcel.XlVAlign.xlVAlignCenter;
                range1.HorizontalAlignment = MsExcel.XlHAlign.xlHAlignCenter;
                range1.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.Blue);
                range1.Font.Size = 20;
                range1.Font.Bold = true;

                worksheet1.Cells[1, 2] = "C#Windows程序设计学生名单";
                worksheet1.Cells[3, 2] = "学号";
                worksheet1.Cells[3, 3] = "姓名";

                StreamReader sw = new StreamReader("list.csv");
                string a_str;
                string[] str_list;
                int i = 4;
                a_str = sw.ReadLine();
                while (a_str != null)
                {
                    str_list = a_str.Split(",".ToCharArray());
                    worksheet1.Cells[i, 2] = str_list[0];
                    worksheet1.Cells[i, 3] = str_list[1];
                    i++;
                    a_str = sw.ReadLine();
                }
                sw.Close();

                worksheet1 = null;
                object file_name = @"d:\xue\excel2\one.xlsx";

                oExcBook.Close(true, file_name, null);
                oExcApp.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oExcApp);
                oExcApp = null;
                System.GC.Collect();

            }
            catch (Exception e2)
            {
            } 
        }
    }
}
