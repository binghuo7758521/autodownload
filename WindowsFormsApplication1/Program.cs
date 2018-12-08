using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Configuration;


namespace WindowsFormsApplication1
{

    

    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {


            #region 
            bool createNew;
            using (Mutex mutex = new Mutex(true, Application.ProductName, out createNew))
            {
                if (createNew)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Form1());
                }
                else {

                    MessageBox.Show("应用程序已经在运行中...");
                    System.Threading.Thread.Sleep(1000);

                    //  终止此进程并为基础操作系统提供指定的退出代码。
                    System.Environment.Exit(1);
                }

            }
            #endregion
        }
    }
}
