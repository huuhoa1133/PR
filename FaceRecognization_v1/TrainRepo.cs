using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceRecognization_v1
{
    public class TrainRepo
    {
        Image<Bgr, Byte> currentFrame;
        Capture grabber;
        HaarCascade face;
        HaarCascade eye;
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);
        Image<Gray, byte> result, TrainedFace = null;
        Image<Gray, byte> gray = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();//load from data
        List<string> labels = new List<string>();//load from data
        List<string> NamePersons = new List<string>();
        int ContTrain, NumLabels, t;
        string name, names = null;

        public TrainRepo()
        {
            face = new HaarCascade("haarcascade_frontalface_default.xml");

        }

        public void LoadData()
        {
            try
            {
                string[] fileArray = Directory.GetDirectories(Application.StartupPath + "\\Data");
                var task = new List<Task>();

                foreach (var item in fileArray)
                {
                    task.Add(Task.Run(async () =>
                    {
                        string label = item.Split('\\').Last();
                        string[] fileImg = Directory.GetFiles(item, "*.*",
                                             SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".jpg") || s.EndsWith(".png")).ToArray();
                        for (int j = 0; j < fileImg.Length; j++)
                        {
                            labels.Add(label);
                            var img = new Image<Gray, byte>(fileImg[j]);
                            img.Bitmap = await ResizeBitmap(img.Bitmap, 240, 320);
                            trainingImages.Add(img);
                        }
                    }));
                }
                Task.WaitAll(task.ToArray());
            }
            catch (Exception ex)
            {

            }
        }

        public async Task<Bitmap> ResizeBitmap(Bitmap bitmap, int width, int height)
        {
            Bitmap newImage = new Bitmap(width, height);
            newImage.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);
            using (Graphics gr = Graphics.FromImage(newImage))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.DrawImage(bitmap, new Rectangle(0, 0, width, height));
            }

            return newImage;
        }

        public List<Image<Gray, byte>> GetTrainImg()
        {
            return trainingImages;
        }

        public List<string> GetLabel()
        {
            return labels;
        }
    }
}
