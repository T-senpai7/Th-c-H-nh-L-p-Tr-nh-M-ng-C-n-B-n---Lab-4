using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bai_02
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private string getHTML(string szURL)
        {   //tao yeu cau cho URL
            WebRequest request = WebRequest.Create(szURL);
            //tao phan hoi
            WebResponse response = request.GetResponse();
            //lay dong du lieu
            Stream dataStream = response.GetResponseStream();
            //Mo stream bang StreamReader doc du lieu
            StreamReader reader = new StreamReader(dataStream);
            //doc toan bo noi dung roi luu vao string
            string responseFromServer = reader.ReadToEnd();
            //dong luong roi giai phong tai nguyen
            reader.Close();
            response.Close();
            return responseFromServer;


        }

        private void url_TextChanged(object sender, EventArgs e)
        {

        }

        private void btn_download_Click(object sender, EventArgs e)
        {
            string URL = url.Text.Trim();
            string filePath = html.Text.Trim();
            if (string.IsNullOrEmpty(URL))
            {
                MessageBox.Show("Vui lòng nhập URL");
                return;

            }
            try
            {
                WebClient myClient = new WebClient();
                myClient.DownloadFile(URL, filePath);
                MessageBox.Show("Đã tải xong! File lưu tại: " + filePath);
                //Dung openRead de mo luong doc du lieu
                Stream response = myClient.OpenRead(URL);
                //dung streamreader de doc stream va doi thanh string
                StreamReader reader = new StreamReader(response);
                string responseFromServer = reader.ReadToEnd();
                richTextBox1.Text = responseFromServer;
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi" + ex.Message);
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
