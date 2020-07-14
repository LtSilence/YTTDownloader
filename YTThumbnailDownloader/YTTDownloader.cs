using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace YTThumbnailDownloader {
    public partial class YTThumbnailDownloader : Form {
        public YTThumbnailDownloader() {
            InitializeComponent();
        }

        private void YTThumbnailDownloader_Load(object sender, EventArgs e) {
            MsgLabel.Text = null;
        }

        bool isBusy = false;
        private void DownloadButton_Click(object sender, EventArgs e) {
            MsgLabel.Text = null;

            if (!isBusy)
                new System.Threading.Thread(DownloadThumbnail) { IsBackground = true }.Start();
        }

        private void LinkBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                DownloadButton_Click(sender, e);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        void DownloadThumbnail() {
            if (isBusy)
                return;
            isBusy = true;

            try {
                string url = LinkBox.Text;
                if (url.Contains("youtube.com") || url.Contains("youtu.be")) {
                    byte[] html = null;
                    html = new System.Net.WebClient().DownloadData(url);                   
                    char[] htmlChars = Encoding.Default.GetString(html).ToCharArray();
                    List<string> htmlLinks = new List<string>();

                    string link = "";
                    bool afterQuotes = false;
                    foreach (char chr in htmlChars) {
                        if (chr == '"') {
                            afterQuotes = !afterQuotes;
                            if (!afterQuotes) {
                                htmlLinks.Add(link);
                                link = "";
                            }
                        } else if (afterQuotes) {
                            link += chr;
                        }
                    }

                    foreach (string imageLink in htmlLinks) {
                        if (imageLink.Contains("maxres") || imageLink.Contains("hqdefault")) {
                            thumbBox.LoadAsync(imageLink);
                        }
                    }
                } else
                    PrintError(ErrorTypes.link);
            }
            catch (Exception) { PrintError(ErrorTypes.exception); isBusy = false; return; }

            isBusy = false;
        }
       
        enum ErrorTypes { save,link,exception }
        void PrintError(ErrorTypes erType) {
            this.Invoke((MethodInvoker)delegate {
                switch (erType) {
                    case ErrorTypes.save:
                        MsgLabel.Text = "Can't save.";
                        break;
                    case ErrorTypes.link:
                        MsgLabel.Text = "Try a youtube.com or youtu.be link instead.";
                        break;
                    case ErrorTypes.exception:
                        MsgLabel.Text = "An error occurred.";
                        break;
                }
            });        
        }

        private void SaveButton_Click(object sender, EventArgs e) {
            if (thumbBox.Image != null) {
                MsgLabel.Text = null;

                SaveFileDialog sfd = new SaveFileDialog();
                ImageFormat imgFormat = ImageFormat.Png;
                sfd.FileName = "thumbnail";
                sfd.Filter = "PNG (*.PNG)|*.png|JPG (*.JPG;*.JPEG)|*.jpg|All files(*.*)|*.*";
                if (sfd.ShowDialog() == DialogResult.OK) {
                    thumbBox.Image.Save(sfd.FileName, imgFormat);
                }
            } else
                PrintError(ErrorTypes.save);
        }        
    }
}