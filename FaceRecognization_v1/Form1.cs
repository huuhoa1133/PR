using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceRecognization_v1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //var a = new TrainRepo();
            //a.LoadData();
            //var labels = a.GetLabel();
            //var imgs = a.GetTrainImg();
            //var b = a.DetectFace(imgs[0].Bitmap);

            var train = new MLLib();
            train.Training();
            pictureBox1.Image = train.imageBindingModel[1].ImageFaceRgb.Bitmap;
            pictureBox2.Image = train.imageBindingModel[1].ImageFaceResizeRgb.Bitmap;

            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
