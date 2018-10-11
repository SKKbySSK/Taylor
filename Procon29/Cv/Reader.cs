using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;

namespace Procon29.Cv
{
    public partial class Reader : Form
    {
        /// <summary>
        /// QRコードを認識した際に発生します
        /// </summary>
        public event Action<ZXing.Result> Captured;

        private bool isRunning = false;

        private object lockObj = new object();

        private BarcodeReaderImage reader = new BarcodeReaderImage();

        public Reader()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (isRunning) return;
            isRunning = true;
            worker.RunWorkerAsync();
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var img = (Mat)e.UserState;
            lock (lockObj)
            {
                if (img.IsDisposed) return;
                RecognizeQR(img);
            }
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            VideoCapture capture = new VideoCapture(0);
            Mat img = new Mat();
            while (isRunning)
            {
                if (capture.Read(img))
                {
                    worker.ReportProgress(0, img);
                }
            }

            lock (lockObj)
            {
                qrbox.ImageIpl = null;
                img.Release();
                img.Dispose();

                capture.Release();
                capture.Dispose();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            isRunning = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                using (var file = new System.IO.FileStream(ofd.FileName, System.IO.FileMode.Open))
                using (Mat img = Mat.FromStream(file, ImreadModes.AnyColor))
                {
                    RecognizeQR(img);
                }
            }
        }

        void RecognizeQR(Mat img)
        {
            qrbox.ImageIpl = img;
            var res = reader.Decode(img);

            if (res != null)
            {
                resultT.Text = res.Text;
                Captured?.Invoke(res);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            isRunning = false;
        }
    }
}
