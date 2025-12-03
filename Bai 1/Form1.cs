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
namespace Bai_1
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
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void btn_get_Click(object sender, EventArgs e)
        {
            string URL = Link.Text;
            if (string.IsNullOrEmpty(URL))
            {
                MessageBox.Show("Vui long nhap dia chi URL!");
            }
            try
            {
                string KetQua = getHTML(URL);
                richTextBox1.Text = KetQua;

            }
            catch (Exception ex) {
                MessageBox.Show("Lỗi: " + ex.Message);
            }

        }

        private void Link_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
