using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net; // Dùng cho WebClient
using Microsoft.Web.WebView2.Core; // Thư viện WebView2
using HtmlAgilityPack;

namespace Bai_3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitWebView();
        }
        //khoi tao webview2
        async void InitWebView()
        {
            await webView.EnsureCoreWebView2Async();

        }
        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
          //de tai web
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            string url = textBox1.Text;
            if (string.IsNullOrEmpty(url)) return;

            FolderBrowserDialog folder = new FolderBrowserDialog();
            if (folder.ShowDialog() != DialogResult.OK) return;

            string saveFolder = folder.SelectedPath;

            try
            {
                WebClient client = new WebClient();

                
                client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                string htmlContent = client.DownloadString(url);

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(htmlContent);

                var listImages = doc.DocumentNode.SelectNodes("//img");

                if (listImages == null)
                {
                    MessageBox.Show("Trang web này không có ảnh nào");
                    return;
                }

                int count = 0;

                foreach (var img in listImages)
                {
                    string src = img.GetAttributeValue("src", "");

                    if (!string.IsNullOrEmpty(src))
                    {
                        Uri baseUri = new Uri(url);
                        Uri fullUri = new Uri(baseUri, src);

                        string fileName = Path.GetFileName(fullUri.LocalPath);

                        if (string.IsNullOrEmpty(fileName) || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                            fileName = "image_" + count + ".jpg";

                        
                        string savePath = Path.Combine(saveFolder, fileName);

                        try
                        {
                            client.DownloadFile(fullUri, savePath);
                            count++;
                        }
                        catch
                        {
                          
                        }
                    }
                }
                MessageBox.Show($"Đã tải xong {count} ảnh!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }



        private void DownFile_Click(object sender, EventArgs e)
        {
            try
            {
                string url = textBox1.Text;
                SaveFileDialog savefile = new SaveFileDialog();
                savefile.Filter = "HTML File|*.html";
                savefile.FileName = "index.html";
                if(savefile.ShowDialog() == DialogResult.OK)
                {
                    WebClient client = new WebClient();
                    client.Encoding = System.Text.Encoding.UTF8;
                    client.DownloadFile(url, savefile.FileName);
                    MessageBox.Show("Đã lưu xong", "Thông báo");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải file: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            webView.Reload();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            string url = textBox1.Text;
            if (string.IsNullOrEmpty(url)) return;
            try
            {
                webView.CoreWebView2.Navigate(url);
                textBox1.Text = url;
                //cap nhat lai
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi" + ex.Message);
            }
        }
    }
}
