using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using MSWord = Microsoft.Office.Interop.Word;
/*创建日期 :2014.05.18
 *修改日期 :2014.05.19
 *类名称    :KeChengTiKu
 * 类说明   :课程题库类
 * 功能      :管理当前课程所有考题
 */
namespace ChuJuan1
{
    //当前项目仅有一个考卷静态实例，采用静态类进行操作
    public static class KeChengTiKu
    {
        public static int iMingLen;
        public static string strKCMingCheng;
        public static long lTotalDataLen;

        public static string strTiXingMingCheng;

        //打开课程题库文件
        public static void OpenKCFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return;
            }
            FileStream fsInputFile = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            byte[] dataOBuf = new byte[1000000];
            fsInputFile.Read(dataOBuf, 0, 4);
            iMingLen = BitConverter.ToInt32(dataOBuf, 0);
            fsInputFile.Read(dataOBuf, 0, iMingLen);
            //课程名称字符串
            strKCMingCheng = Encoding.UTF8.GetString(dataOBuf, 0, iMingLen);
            //题型数量
            fsInputFile.Read(dataOBuf, 0, 4);
            int alnum = 0, itheNum = 0;
            alnum = BitConverter.ToInt32(dataOBuf, 0);
            //新建题型数组
            alTiXing = new ArrayList();
            for (int i = 0; i < alnum; i++)
            {
                fsInputFile.Read(dataOBuf, 0, 4);
                itheNum = BitConverter.ToInt32(dataOBuf, 0);
                fsInputFile.Read(dataOBuf, 0, itheNum);
                string strTiXingName = Encoding.UTF8.GetString(dataOBuf, 0, itheNum);
                alTiXing.Add(strTiXingName);
            }
            //新建考题数组
            alKaoti = new ArrayList();
            fsInputFile.Read(dataOBuf, 0, 4);
            int iKaoTiNum = BitConverter.ToInt32(dataOBuf, 0);
            for (int i = 0; i < iKaoTiNum; i++)
            {
                Kaoti theKaoti = new Kaoti(i);
                //考题创建时间
                fsInputFile.Read(dataOBuf, 0, 8);
                long theLong = BitConverter.ToInt64(dataOBuf, 0);
                theKaoti.dtCreateTime = new DateTime(theLong, DateTimeKind.Utc);
                //考题编号
                fsInputFile.Read(dataOBuf, 0, 4);
                theKaoti.iTiNum = BitConverter.ToInt32(dataOBuf, 0);
                //考题题型名称
                fsInputFile.Read(dataOBuf, 0, 4);
                itheNum = BitConverter.ToInt32(dataOBuf, 0);
                fsInputFile.Read(dataOBuf, 0, itheNum);
                theKaoti.strTiXing = Encoding.UTF8.GetString(dataOBuf, 0, itheNum);

                //考题内容数量
                fsInputFile.Read(dataOBuf, 0, 4);
                itheNum = BitConverter.ToInt32(dataOBuf, 0);
                for (int k = 0; k < itheNum; k++)
                {
                    TxtMixPic theTmp = new TxtMixPic();
                    fsInputFile.Read(dataOBuf, 0, 4);
                    itheNum = BitConverter.ToInt32(dataOBuf, 0);
                    fsInputFile.Read(dataOBuf, 0, itheNum);
                    theTmp.strChengFen = Encoding.UTF8.GetString(dataOBuf, 0, itheNum);

                    fsInputFile.Read(dataOBuf, 0, 4);
                    itheNum = BitConverter.ToInt32(dataOBuf, 0);
                    fsInputFile.Read(dataOBuf, 0, itheNum);
                    theTmp.strDataLeiXing = Encoding.UTF8.GetString(dataOBuf, 0, itheNum);


                    //读出数据
                    if (theTmp.strDataLeiXing == "图片")
                    {
                        fsInputFile.Read(dataOBuf, 0, 4);
                        itheNum = BitConverter.ToInt32(dataOBuf, 0);
                        fsInputFile.Read(KeChengTiKu.bMsBuf, 0, itheNum);
                        Bitmap bmpData = (Bitmap)Image.FromStream(KeChengTiKu.msBmp);
                        theTmp.objData = bmpData;
                    }
                    if (theTmp.strDataLeiXing == "文本")
                    {
                        fsInputFile.Read(dataOBuf, 0, 4);
                        itheNum = BitConverter.ToInt32(dataOBuf, 0);
                        fsInputFile.Read(dataOBuf, 0, itheNum);
                        theTmp.objData = Encoding.UTF8.GetString(dataOBuf, 0, itheNum);
                    }
                    theKaoti.alNeiRong.Add(theTmp);
                }                
                alKaoti.Add(theKaoti);
            }


            //关闭文件
            fsInputFile.Close();
            fsInputFile.Dispose();
        }
        //保存课程题库文件
        public static void SaveKCFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            byte[] StrDataBuf;
            byte[] DataBuf;
            //课程名称+题型+题集

            FileStream fsOutputFile = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            //课程名称
            StrDataBuf = Encoding.UTF8.GetBytes(strKCMingCheng);
            DataBuf = BitConverter.GetBytes(StrDataBuf.Length);
            //课程名称字节长度
            fsOutputFile.Write(DataBuf, 0, 4);
            fsOutputFile.Write(StrDataBuf, 0, StrDataBuf.Length);

            //题型集合             　
            DataBuf = BitConverter.GetBytes(alTiXing.Count);
            //题型数量
            fsOutputFile.Write(DataBuf, 0, DataBuf.Length);
            for (int i = 0; i < alTiXing.Count; i++)
            {
                StrDataBuf = Encoding.UTF8.GetBytes(alTiXing[i].ToString());
                DataBuf = BitConverter.GetBytes(StrDataBuf.Length);
                //题型名称字节长度
                fsOutputFile.Write(DataBuf, 0, 4);
                //题型名称字节内容
                fsOutputFile.Write(StrDataBuf, 0, StrDataBuf.Length);
            }
            //考题集合
            DataBuf = BitConverter.GetBytes(alKaoti.Count);
            //考题数量字节内容
            fsOutputFile.Write(DataBuf, 0, DataBuf.Length);
            for (int i = 0; i < alKaoti.Count; i++)
            {
                Kaoti theKaoti = (Kaoti)alKaoti[i];
                //考题创建时间
                DataBuf = BitConverter.GetBytes(theKaoti.dtCreateTime.ToBinary());
                fsOutputFile.Write(DataBuf, 0, DataBuf.Length);
                //考题编号
                DataBuf = BitConverter.GetBytes(theKaoti.iTiNum);
                fsOutputFile.Write(DataBuf, 0, DataBuf.Length);
                //考题题型名称
                StrDataBuf = Encoding.UTF8.GetBytes(theKaoti.strTiXing);
                DataBuf = BitConverter.GetBytes(StrDataBuf.Length);
                fsOutputFile.Write(DataBuf, 0, DataBuf.Length);
                fsOutputFile.Write(StrDataBuf, 0, StrDataBuf.Length);


                //考题内容数量
                DataBuf = BitConverter.GetBytes(theKaoti.alNeiRong.Count);
                fsOutputFile.Write(DataBuf, 0, DataBuf.Length); 
                for (int j = 0; j < theKaoti.alNeiRong.Count; j++)
                {
                    TxtMixPic theTmp=(TxtMixPic)theKaoti.alNeiRong[j];
                    //写入成分
                    StrDataBuf = Encoding.UTF8.GetBytes(theTmp.strChengFen);
                    DataBuf = BitConverter.GetBytes(StrDataBuf.Length);
                    fsOutputFile.Write(DataBuf, 0, DataBuf.Length);
                    fsOutputFile.Write(StrDataBuf, 0, StrDataBuf.Length);

                    //写入数据类型
                    StrDataBuf = Encoding.UTF8.GetBytes(theTmp.strDataLeiXing);
                    DataBuf = BitConverter.GetBytes(StrDataBuf.Length);
                    fsOutputFile.Write(DataBuf, 0, DataBuf.Length);
                    fsOutputFile.Write(StrDataBuf, 0, StrDataBuf.Length);
                    //写入数据
                    if (theTmp.strDataLeiXing == "图片")
                    {
                        Bitmap bmpData = (Bitmap)theTmp.objData;
                        msBmp.Seek(0, SeekOrigin.Begin);
                        bmpData.Save(msBmp,ImageFormat.Png);
                        DataBuf = BitConverter.GetBytes((int)msBmp.Position);
                        fsOutputFile.Write(DataBuf, 0, DataBuf.Length);                        
                        fsOutputFile.Write(bMsBuf, 0, (int)msBmp.Position);                         
                    }
                    if (theTmp.strDataLeiXing == "文本")
                    {
                        StrDataBuf = Encoding.UTF8.GetBytes(theTmp.objData.ToString());
                        DataBuf = BitConverter.GetBytes(StrDataBuf.Length);
                        fsOutputFile.Write(DataBuf, 0, DataBuf.Length);
                        fsOutputFile.Write(StrDataBuf, 0, StrDataBuf.Length);
                    }  
                } 
                //考题数据序列化全部完成　
            }


            fsOutputFile.Close();
            fsOutputFile.Dispose();
        }
        //生成课程复习题-word版
        public static void GenerateFuxi(string fuxiFileName)
        {
            if (!File.Exists(fuxiFileName))
            {
                return;
            }



            MSWord.ApplicationClass oWordApplic;//a reference to Wordapplication
            MSWord.Document oDoc = null;//a reference to thedocument
            //打开Word
            oWordApplic = new MSWord.ApplicationClass();
            object missing = System.Reflection.Missing.Value;
            try
            {

            }
            catch { }
            //结束Word
            finally
            {
                if (oDoc != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oDoc);
                    oDoc = null;
                }
                oWordApplic.NormalTemplate.Saved = true;
                oWordApplic.Quit(ref missing, ref missing, ref missing);
            }



        }


        //当前题目数
        public static int iTotalNum;

        //节点对，父节点与子节点
        public static Dictionary<int, int> dictNodes;

        //题型集合
        public static ArrayList alTiXing;
        //考题集合
        public static ArrayList alKaoti;

        //初始化类成员变量
        public static void InitKeChengTiKu()
        {
            iTotalNum = 0;
            alTiXing = new ArrayList();
            alKaoti = new ArrayList();
            dictNodes = new Dictionary<int, int>();
        }


        //向题库添加一个考题实例
        public static void AddNewKaoTi()
        {
            //新建考题实例
            KeChengTiKu.curKaoTi = new Kaoti(KeChengTiKu.iTotalNum + 1);
            //新建题目、答案、复习知识点的实例，准备接收用户输入
            KeChengTiKu.curKaoTi.alNeiRong = new ArrayList();
        }

        public static Kaoti curKaoTi;
        public static ArrayList alcurTiMuPic;
        public static ArrayList alcurDaAnPic;
        public static ArrayList alcurFuXiPic;
        public static string curKaoTiXing;
        public static MemoryStream msBmp;
        public static byte[] bMsBuf;
    }

}
