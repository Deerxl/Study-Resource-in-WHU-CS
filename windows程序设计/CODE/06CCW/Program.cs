using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MsWord = Microsoft.Office.Interop.Word;
using System.Windows.Forms;
using System.IO;




namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            MsWord.Application oWordApplic;//a reference to Wordapplication
            MsWord.Document oDoc;//a reference to thedocument
            try
            {
                string doc_file_name = Directory.GetCurrentDirectory() + @"\content.doc";
                if (File.Exists(doc_file_name))
                {
                    File.Delete(doc_file_name);
                }
                oWordApplic = new MsWord.Application();
                object missing = System.Reflection.Missing.Value;

                MsWord.Range curRange;
                object curTxt;
                int curSectionNum = 1;
                oDoc = oWordApplic.Documents.Add(ref missing, ref missing, ref missing, ref missing);
                oDoc.Activate();
                Console.WriteLine(" 正在生成文档小节");
                object section_nextPage = MsWord.WdBreakType.wdSectionBreakNextPage;
                object page_break = MsWord.WdBreakType.wdPageBreak;
                //添加三个分节符，共四个小节
                for (int si = 0; si < 4; si++)
                {
                    oDoc.Paragraphs[1].Range.InsertParagraphAfter();
                    oDoc.Paragraphs[1].Range.InsertBreak(ref section_nextPage);
                }

                Console.WriteLine(" 正在插入摘要内容");
                #region 摘要部分
                curSectionNum = 1;
                curRange = oDoc.Sections[curSectionNum].Range.Paragraphs[1].Range;
                curRange.Select();
                string one_str, key_word;
                //摘要的文本来自 abstract.txt 文本文件
                StreamReader file_abstract = new StreamReader("abstract.txt");
                oWordApplic.Options.Overtype = false;//overtype 改写模式
                MsWord.Selection currentSelection = oWordApplic.Selection;
                if (currentSelection.Type == MsWord.WdSelectionType.wdSelectionNormal)
                {
                    one_str = file_abstract.ReadLine();//读入题目
                    currentSelection.TypeText(one_str);
                    currentSelection.TypeParagraph(); //添加段落标记
                    currentSelection.TypeText(" 摘要");//写入" 摘要" 二字
                    currentSelection.TypeParagraph(); //添加段落标记
                    key_word = file_abstract.ReadLine();//读入题目
                    one_str = file_abstract.ReadLine();//读入段落文本
                    while (one_str != null)
                    {
                        currentSelection.TypeText(one_str);
                        currentSelection.TypeParagraph(); //添加段落标记
                        one_str = file_abstract.ReadLine();
                    }
                    currentSelection.TypeText(" 关键字：");
                    currentSelection.TypeText(key_word);
                    currentSelection.TypeParagraph(); //添加段落标记
                }
                file_abstract.Close();
                //摘要的标题
                curRange = oDoc.Sections[curSectionNum].Range.Paragraphs[1].Range;
                curTxt = curRange.Paragraphs[1].Range.Text;
                curRange.Font.Name = " 宋体";
                curRange.Font.Size = 22;
                curRange.Paragraphs[1].Alignment =
                MsWord.WdParagraphAlignment.wdAlignParagraphCenter;
                //" 摘要" 两个字
                curRange = oDoc.Sections[curSectionNum].Range.Paragraphs[2].Range;
                curRange.Select();
                curRange.Paragraphs[1].Alignment =
                MsWord.WdParagraphAlignment.wdAlignParagraphCenter;
                curRange.Font.Name = " 黑体";
                curRange.Font.Size = 16;
                //摘要正文
                oDoc.Sections[curSectionNum].Range.Paragraphs[1].Alignment =
                MsWord.WdParagraphAlignment.wdAlignParagraphCenter;
                for (int i = 3; i < oDoc.Sections[curSectionNum].Range.Paragraphs.Count; i++)
                {
                    curRange = oDoc.Sections[curSectionNum].Range.Paragraphs[i].Range;
                    curTxt = curRange.Paragraphs[1].Range.Text;
                    curRange.Select();
                    curRange.Font.Name = " 宋体";
                    curRange.Font.Size = 12;
                    oDoc.Sections[curSectionNum].Range.Paragraphs[i].LineSpacingRule =
                        //oDoc.Paragraphs[i].LineSpacingRule =
                    MsWord.WdLineSpacing.wdLineSpaceMultiple;
                    //多倍行距，1.25 倍，这里的浮点值是以 point 为单位的，不是行距倍数
                    oDoc.Sections[curSectionNum].Range.Paragraphs[i].LineSpacing = 15f;
                    oDoc.Sections[curSectionNum].Range.Paragraphs[i].IndentFirstLineCharWidth(2);
                }
                //设置" 关键字：" 为黑体
                curRange = curRange.Paragraphs[curRange.Paragraphs.Count].Range;
                curTxt = curRange.Paragraphs[1].Range.Text;
                object range_start, range_end;
                range_start = curRange.Start;
                range_end = curRange.Start + 4;
                curRange = oDoc.Range(ref range_start, ref range_end);
                curTxt = curRange.Text;
                //curRange = curRange.Range(ref range_start, ref range_end);
                curRange.Select();
                curRange.Font.Bold = 1;
                #endregion 摘要部分




                Console.WriteLine(" 正在插入目录");
                #region 目录
                curSectionNum = 2;
                curRange = oDoc.Sections[curSectionNum].Range.Paragraphs[1].Range;
                curRange.Select();
                //插入目录时指定的参数
                object useheading_styles = true;//使用内置的目录标题样式
                object upperheading_level = 1;//最高的标题级别
                object lowerheading_level = 3;//最低标题级别
                object usefields = 1;//true 表示创建的是目录
                object tableid = 1;
                object RightAlignPageNumbers = true;//右边距对齐的页码
                object IncludePageNumbers = true;//目录中包含页码
                currentSelection = oWordApplic.Selection;
                currentSelection.TypeText(" 目录");
                currentSelection.TypeParagraph();
                currentSelection.Select();
                curRange = oDoc.Sections[curSectionNum].Range.Paragraphs[2].Range;
                //插入的表格会代替当前 range
                //range 为非折叠时，TablesOfContents 会代替 range，引起小节数减少。
                curRange.Collapse();
                oDoc.TablesOfContents.Add(curRange, ref useheading_styles, ref upperheading_level,
                ref lowerheading_level, ref usefields, ref tableid, ref RightAlignPageNumbers,
                ref IncludePageNumbers, ref missing, ref missing, ref missing, ref missing);
                oDoc.Sections[curSectionNum].Range.Paragraphs[1].Alignment =
                MsWord.WdParagraphAlignment.wdAlignParagraphCenter;
                oDoc.Sections[curSectionNum].Range.Paragraphs[1].Range.Font.Bold = 1;
                oDoc.Sections[curSectionNum].Range.Paragraphs[1].Range.Font.Name = " 黑体";
                oDoc.Sections[curSectionNum].Range.Paragraphs[1].Range.Font.Size = 16;
                #endregion 目录


                #region 第一章
                curSectionNum = 3;
                oDoc.Sections[curSectionNum].Range.Paragraphs[1].Range.Select();
                curRange = oDoc.Sections[curSectionNum].Range.Paragraphs[1].Range;
                Console.WriteLine(" 正在设置标题样式");
                object wdFontSizeIndex;
                wdFontSizeIndex = 14;//此序号在 word 中的编号是格式 > 显示格式 > 样式和格式 > 显示所有样式的序号
                //14 即是标题一一级标题：三号黑体。
                oWordApplic.ActiveDocument.Styles.get_Item(ref wdFontSizeIndex).ParagraphFormat.Alignment =
                MsWord.WdParagraphAlignment.wdAlignParagraphCenter;
                oWordApplic.ActiveDocument.Styles.get_Item(ref wdFontSizeIndex).Font.Name = " 黑体";
                oWordApplic.ActiveDocument.Styles.get_Item(ref wdFontSizeIndex).Font.Size = 16;//三号
                wdFontSizeIndex = 15;//15 即是标题二二级标题：小三号黑体。
                oWordApplic.ActiveDocument.Styles.get_Item(ref wdFontSizeIndex).Font.Name = " 黑体";
                oWordApplic.ActiveDocument.Styles.get_Item(ref wdFontSizeIndex).Font.Size = 15;//小三
                //用指定的标题来设定文本格式
                object Style1 = MsWord.WdBuiltinStyle.wdStyleHeading1;//一级标题：三号黑体。
                object Style2 = MsWord.WdBuiltinStyle.wdStyleHeading2;//二级标题：小三号黑体。
                oDoc.Sections[curSectionNum].Range.Select();
                currentSelection = oWordApplic.Selection;
                //读入第一章文本信息
                StreamReader file_content = new StreamReader("content.txt");
                one_str = file_content.ReadLine();//一级标题
                currentSelection.TypeText(one_str);
                currentSelection.TypeParagraph(); //添加段落标记
                one_str = file_content.ReadLine();//二级标题
                currentSelection.TypeText(one_str);
                currentSelection.TypeParagraph(); //添加段落标记
                one_str = file_content.ReadLine();//正文
                while (one_str != null)
                {
                    currentSelection.TypeText(one_str);
                    currentSelection.TypeParagraph(); //添加段落标记
                    one_str = file_content.ReadLine();//正文
                }
                file_content.Close();
                //段落的对齐方式
                curRange = oDoc.Sections[curSectionNum].Range.Paragraphs[1].Range;
                curRange.set_Style(ref Style1);
                oDoc.Sections[curSectionNum].Range.Paragraphs[1].Alignment =
                MsWord.WdParagraphAlignment.wdAlignParagraphCenter;
                curRange = oDoc.Sections[curSectionNum].Range.Paragraphs[2].Range;
                curRange.set_Style(ref Style2);
                //第一章正文文本格式
                for (int i = 3; i < oDoc.Sections[curSectionNum].Range.Paragraphs.Count; i++)
                {
                    curRange = oDoc.Sections[curSectionNum].Range.Paragraphs[i].Range;
                    curRange.Select();
                    curRange.Font.Name = " 宋体";
                    curRange.Font.Size = 12;
                    oDoc.Sections[curSectionNum].Range.Paragraphs[i].LineSpacingRule =
                    MsWord.WdLineSpacing.wdLineSpaceMultiple;
                    //多倍行距，1.25 倍，这里的浮点值是以 point 为单位的，不是行距的倍数
                    oDoc.Sections[curSectionNum].Range.Paragraphs[i].LineSpacing = 15f;
                    oDoc.Sections[curSectionNum].Range.Paragraphs[i].IndentFirstLineCharWidth(2);
                }
                #endregion 第一章

                Console.WriteLine(" 正在插入第二章内容");
                #region 第二章表格
                curSectionNum = 4;
                oDoc.Sections[curSectionNum].Range.Select();
                curRange = oDoc.Sections[curSectionNum].Range.Paragraphs[1].Range;
                currentSelection = oWordApplic.Selection;
                currentSelection.TypeText("2 表格");
                currentSelection.TypeParagraph();
                currentSelection.TypeText(" 表格示例");
                currentSelection.TypeParagraph();
                currentSelection.TypeParagraph();
                curRange = oDoc.Sections[curSectionNum].Range.Paragraphs[3].Range;

                oDoc.Sections[curSectionNum].Range.Paragraphs[3].Range.Select();
                currentSelection = oWordApplic.Selection;
                MsWord.Table oTable;
                oTable = curRange.Tables.Add(curRange, 5, 3, ref missing, ref missing);
                oTable.Range.ParagraphFormat.Alignment =
                MsWord.WdParagraphAlignment.wdAlignParagraphCenter;
                oTable.Range.Font.Name = " 宋体";
                oTable.Range.Font.Size = 16;
                oTable.Range.Cells.VerticalAlignment =
                MsWord.WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                oTable.Range.Rows.Alignment = MsWord.WdRowAlignment.wdAlignRowCenter;
                oTable.Columns[1].Width = 80;
                oTable.Columns[2].Width = 180;
                oTable.Columns[3].Width = 80;
                oTable.Cell(1, 1).Range.Text = " 字段";
                oTable.Cell(1, 2).Range.Text = " 描述";
                oTable.Cell(1, 3).Range.Text = " 数据类型";
                oTable.Cell(2, 1).Range.Text = "ProductID";
                oTable.Cell(2, 2).Range.Text = " 产品标识";
                oTable.Cell(2, 3).Range.Text = " 字符串";
                oTable.Borders.InsideLineStyle = MsWord.WdLineStyle.wdLineStyleSingle;
                oTable.Borders.OutsideLineStyle = MsWord.WdLineStyle.wdLineStyleSingle;
                curRange = oDoc.Sections[curSectionNum].Range.Paragraphs[1].Range;
                curRange.set_Style(ref Style1);
                curRange.ParagraphFormat.Alignment =
                MsWord.WdParagraphAlignment.wdAlignParagraphCenter;
                #endregion 第二章

                Console.WriteLine(" 正在插入第三章内容");
                #region 第三章图片
                curSectionNum = 5;
                oDoc.Sections[curSectionNum].Range.Paragraphs[1].Range.Select();
                curRange = oDoc.Sections[curSectionNum].Range.Paragraphs[1].Range;
                currentSelection = oWordApplic.Selection;
                currentSelection.TypeText("3 图片");
                currentSelection.TypeParagraph();
                currentSelection.TypeText(" 图片示例");
                currentSelection.TypeParagraph();
                //currentSelection.InlineShapes.AddPicture(@"D:\stu\cword\zsc-logo.png",
                //ref missing, ref missing, ref missing);

                currentSelection.InlineShapes.AddPicture(@"E:\ConsoleApplication1\zsc-logo.png",
                ref missing, ref missing, ref missing);
                curRange = oDoc.Sections[curSectionNum].Range.Paragraphs[1].Range;
                curRange.set_Style(ref Style1);
                curRange.ParagraphFormat.Alignment =
                MsWord.WdParagraphAlignment.wdAlignParagraphCenter;
                #endregion 第三章

                Console.WriteLine(" 正在设置第一节摘要页眉内容");
                //设置页脚 section 1 摘要
                curSectionNum = 1;
                oDoc.Sections[curSectionNum].Range.Select();
                //进入页脚视图
                oWordApplic.ActiveWindow.ActivePane.View.SeekView =
                MsWord.WdSeekView.wdSeekCurrentPageFooter;
                oDoc.Sections[curSectionNum].
                Headers[MsWord.WdHeaderFooterIndex.wdHeaderFooterPrimary].
                Range.Borders[MsWord.WdBorderType.wdBorderBottom].LineStyle =
                MsWord.WdLineStyle.wdLineStyleNone;
                oWordApplic.Selection.HeaderFooter.PageNumbers.RestartNumberingAtSection = true;
                oWordApplic.Selection.HeaderFooter.PageNumbers.NumberStyle
                = MsWord.WdPageNumberStyle.wdPageNumberStyleUppercaseRoman;
                oWordApplic.Selection.HeaderFooter.PageNumbers.StartingNumber = 1;
                //切换到文档
                oWordApplic.ActiveWindow.ActivePane.View.SeekView =
                MsWord.WdSeekView.wdSeekMainDocument;
                Console.WriteLine(" 正在设置第二节目录页眉内容");
                //设置页脚 section 2 目录
                curSectionNum = 2;
                oDoc.Sections[curSectionNum].Range.Select();
                //进入页脚视图
                oWordApplic.ActiveWindow.ActivePane.View.SeekView =
                MsWord.WdSeekView.wdSeekCurrentPageFooter;
                oDoc.Sections[curSectionNum].
                Headers[MsWord.WdHeaderFooterIndex.wdHeaderFooterPrimary].
                Range.Borders[MsWord.WdBorderType.wdBorderBottom].LineStyle =
                MsWord.WdLineStyle.wdLineStyleNone;
                oWordApplic.Selection.HeaderFooter.PageNumbers.RestartNumberingAtSection = false;
                oWordApplic.Selection.HeaderFooter.PageNumbers.NumberStyle
                = MsWord.WdPageNumberStyle.wdPageNumberStyleUppercaseRoman;
                //oWordApplic.Selection.HeaderFooter.PageNumbers.StartingNumber = 1;
                //切换到文档
                oWordApplic.ActiveWindow.ActivePane.View.SeekView =
                MsWord.WdSeekView.wdSeekMainDocument;
                //第一章页眉页码设置
                curSectionNum = 3;
                oDoc.Sections[curSectionNum].Range.Select();
                //切换入页脚视图
                oWordApplic.ActiveWindow.ActivePane.View.SeekView =
                MsWord.WdSeekView.wdSeekCurrentPageFooter;
                currentSelection = oWordApplic.Selection;
                curRange = currentSelection.Range;
                //本节页码不续上节
                oWordApplic.Selection.HeaderFooter.PageNumbers.RestartNumberingAtSection = true;
                //页码格式为阿拉伯
                oWordApplic.Selection.HeaderFooter.PageNumbers.NumberStyle
                = MsWord.WdPageNumberStyle.wdPageNumberStyleArabic;
                //起如页码为 1
                oWordApplic.Selection.HeaderFooter.PageNumbers.StartingNumber = 1;
                //添加页码域
                object fieldpage = MsWord.WdFieldType.wdFieldPage;
                oWordApplic.Selection.Fields.Add(oWordApplic.Selection.Range,
                ref fieldpage, ref missing, ref missing);
                //居中对齐
                oWordApplic.Selection.ParagraphFormat.Alignment =
                MsWord.WdParagraphAlignment.wdAlignParagraphCenter;
                //本小节不链接到上一节
                oDoc.Sections[curSectionNum].Headers[Microsoft.Office.Interop.
                Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].LinkToPrevious = false;
                //切换入正文视图

                oWordApplic.ActiveWindow.ActivePane.View.SeekView =
MsWord.WdSeekView.wdSeekMainDocument;
                //在输入全部内容后由于后面章节内容有变动，在此要更新目录，使页码正确
                Console.WriteLine(" 正在更新目录");
                oDoc.Fields[1].Update();
                #region 保存文档
                //保存文档
                Console.WriteLine(" 正在保存 Word 文档");
                object fileName;
                fileName = doc_file_name;
                oDoc.SaveAs2(ref fileName);
                oDoc.Close();
                //Word 文档任务完成后，需要释放 Document 对象和 Application 对象。 
                Console.WriteLine("正在释放 COM 资源");
                //释放 COM 资源
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oDoc);
                oDoc = null;
                oWordApplic.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oWordApplic);
                oWordApplic = null;
                #endregion 保存文档 
            }//try
            catch (Exception e2)
            {
                MessageBox.Show(e2.Message);
            }
            finally
            {
                Console.WriteLine(" 正在结束 Word 进程");
                //关闭 word 进程
                System.Diagnostics.Process[] AllProces = System.Diagnostics.Process.GetProcesses();
                for (int j = 0; j < AllProces.Length; j++)
                {
                    string theProcName = AllProces[j].ProcessName;
                    if (String.Compare(theProcName, "WINWORD") == 0)
                    {
                        if (AllProces[j].Responding && !AllProces[j].HasExited)
                        {
                            AllProces[j].Kill();
                        }
                    }
                }
                //Close word Process.
            }
        }
    }
}
