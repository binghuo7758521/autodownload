using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aliyun.OSS;
using System.IO;
using System.Configuration;



namespace WindowsFormsApplication1
{

    public partial class Form1 : Form
    {

        string endpoint = "oss-cn-beijing.aliyuncs.com";
        string accessKeyId = "LTAIpL2pPGEy5wFh";
        string accessKeySecret = "6FDB5fftlXxEyZ54obDjpXvtDwHrvC";
        string bucketName = "mywangqingguo1";
        string downok_bucketName = "down_ok";
        string prefix = "sb/";
        string Current;
        string downtodir = "d:\\autodown\\";
        


        public Form1()
        {
            InitializeComponent();



        }
        private void show_log(string logstr)
        {
            

            if (textBox_log.Lines.Count() > 500)
            {
                textBox_log.Text="日志已满清空之前日志";
            }
            if (checkBox_showlog.Checked)
            {

                textBox_log.AppendText(logstr);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {


            // 创建OSSClient实例。
            var client = new OssClient(endpoint, accessKeyId, accessKeySecret);
            try
            {
                // 创建存储空间。
                var bucket = client.CreateBucket(bucketName);

                show_log("Create bucket succeeded  ");
            }
            catch (Exception)
            {
                show_log("Create bucket false  ");
            }


        }

        private void button2_Click(object sender, EventArgs e)
        {

            // 创建OSSClient实例。
            try
            {
                var client = new OssClient(endpoint, accessKeyId, accessKeySecret);
                var result = client.ListObjects(bucketName);
                show_log("list object succeeded  ");
                foreach (var summary in result.ObjectSummaries)
                {

                    show_log(summary.Key + "\r\n");

                }


            }
            catch (Exception ex)
            {
                show_log("list object false  " + ex.Message);

            }



        }

        private void button3_Click(object sender, EventArgs e)
        {

            show_log("开始扫描阿里oss\r\n");   
            timer1.Enabled = false;
            var client = new OssClient(endpoint, accessKeyId, accessKeySecret);
            var keys = new List<string>();
            ObjectListing result = null;
            string nextMarker = string.Empty;
            string downloadFilename;
            string temdirname;
            do
            {
                var listObjectsRequest = new ListObjectsRequest(bucketName)
                {
                    Marker = nextMarker,
                    MaxKeys = 1000,
                    Prefix = prefix,
                };
                result = client.ListObjects(listObjectsRequest);
                
                foreach (var summary in result.ObjectSummaries)
                {

                    show_log("a--"+summary.Key + "\r\n");
                    keys.Add(summary.Key);
                    if (summary.Size==0) 
                    {
                        show_log(" 删除:" + summary.Key + "\r\n");
                        client.DeleteObject(bucketName, summary.Key);
                        continue;
                    }




            


                    var obj = client.GetObject(bucketName, summary.Key);
                   

                    using (var requestStream = obj.Content)
                    {
                        downloadFilename = downtodir + Path.GetDirectoryName(summary.Key);
                       //    downloadFilename = downtodir + summary.Key;
                   
                        if (Directory.Exists(downloadFilename))
                        {

                        }
                        else
                        {

                            Directory.CreateDirectory(downloadFilename);
                        }


                        downloadFilename= downloadFilename+"\\" + Path.GetFileName(summary.Key); 

                        byte[] buf = new byte[1024];
                        var fs = File.Open(downloadFilename, FileMode.OpenOrCreate);
                        var len = 0;
                        // 通过输入流将文件的内容读取到文件或者内存中。
                        while ((len = requestStream.Read(buf, 0, 1024)) != 0)
                        {
                            fs.Write(buf, 0, len);
                        }
                        fs.Close();                  

                       }

                    ///下载后，复制文件到down_ok目录                    
                    var req = new CopyObjectRequest(bucketName, summary.Key, downok_bucketName, summary.Key);                 
                    client.CopyObject(req);
                    client.DeleteObject(bucketName, summary.Key);
                    show_log("Get object succeeded" + downloadFilename + "\r\n");                  
                     

                }
                nextMarker = result.NextMarker;
            } while (result.IsTruncated);

            timer1.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string objectName = "abb/2018-11-21 22:37:407寸相纸SH20181121223740086346A帅丰集成灶/打印 1 张的图片/154281108799083.png";
            string downloadFilename;
            downloadFilename = downtodir + Path.GetFileName(objectName);
            // 创建OssClient实例。
            var client = new OssClient(endpoint, accessKeyId, accessKeySecret);
            try
            {
                // 下载文件到流。OssObject 包含了文件的各种信息，如文件所在的存储空间、文件名、元信息以及一个输入流。
                var obj = client.GetObject(bucketName, objectName);
                using (var requestStream = obj.Content)
                {
                    byte[] buf = new byte[1024];
                    var fs = File.Open(downloadFilename, FileMode.OpenOrCreate);
                    var len = 0;
                    // 通过输入流将文件的内容读取到文件或者内存中。
                    while ((len = requestStream.Read(buf, 0, 1024)) != 0)
                    {
                        fs.Write(buf, 0, len);
                    }
                    fs.Close();
                }
                show_log("Get object succeeded" + "\r\n");

            }
            catch (Exception ex)
            {

                show_log("Get object failed. " + ex.Message + "\r\n");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string downloadFilename =@"d:\autodown\2s22\rr";
            show_log("GGetFileName" + Path.GetFileName(downloadFilename) + "\r\n");

            if (Directory.Exists(downloadFilename))
            {
            }
            else
            {
                downloadFilename = Path.GetDirectoryName(downloadFilename);
                Directory.CreateDirectory(downloadFilename);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer1.Interval = 1000 * 30;
            show_log("30秒后自动开始下载，");

            timer_del_okpic.Interval = 1000 * 60 * 60;//一个小时监测一次是否有要删除的文件
            timer_del_okpic.Enabled = true;
            bucketName = ConfigurationManager.AppSettings["blucketname"];
            prefix = ConfigurationManager.AppSettings["blucketdir"];
            endpoint = ConfigurationManager.AppSettings["endpoint"];
            accessKeyId = ConfigurationManager.AppSettings["accessKeyId"];
            accessKeySecret = ConfigurationManager.AppSettings["accessKeySecret"];
            downtodir = ConfigurationManager.AppSettings["downtodir"];
            downok_bucketName = ConfigurationManager.AppSettings["downok_bucketName"];


     
        }



        private void timer1_Tick(object sender, EventArgs e)
        {
            button3_Click(null,null);
        }

        private void button6_Click(object sender, EventArgs e)
        {

            var client = new OssClient(endpoint, accessKeyId, accessKeySecret);
            var keys = new List<string>();
            ObjectListing result = null;
            string nextMarker = string.Empty;
            string downloadFilename;
            string temdirname;
            show_log(" 正在监测是否有过期文件\r\n");
            do 
            {

                var listObjectsRequest = new ListObjectsRequest(downok_bucketName)
                {
                    Marker = nextMarker,
                    MaxKeys = 1000,
                    Prefix = prefix,
                };
                result = client.ListObjects(listObjectsRequest);

                foreach (var summary in result.ObjectSummaries)
                {
                    
                    keys.Add(summary.Key);
                    
                    if ((summary.Size == 0)||((DateTime.Now- summary.LastModified).Days>3))
                    {
                        show_log(" 删除:" + summary.Key + "\r\n");
                        client.DeleteObject(bucketName, summary.Key);
                        continue;
                    }


                }
            
            }while (result.IsTruncated);

            show_log(" 完成监测是否有过期文件\r\n");
        


        }

        private void timer_del_okpic_Tick(object sender, EventArgs e)
        {
            timer_del_okpic.Enabled = false;
            button6_Click(null,null);
            timer_del_okpic.Enabled = true;
        }


    }
}
