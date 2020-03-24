using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace rens
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //创建多个窗体资源，将其全部设为隐藏
            GData.frm_dict = new Dictionary<string, Form>();
            GData.frm_main= new F_main();
            GData.frm_main.Hide();

            GData.frm_login = new F_login();
            GData.frm_dict.Add("管理员登录", GData.frm_login);
            GData.frm_dbconf = new F_dbconf();  
            GData.frm_dict.Add("数据库服务器设置", GData.frm_dbconf); 
            Application.Run(GData.frm_main);
        }
    }
}
