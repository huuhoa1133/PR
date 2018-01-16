using Emgu.CV;
using Emgu.CV.Structure;
using FaceRecognization_v1.Model;
using SharpLearning.Containers.Matrices;
using SharpLearning.InputOutput.Csv;
using SharpLearning.Neural;
using SharpLearning.Neural.Activations;
using SharpLearning.Neural.Layers;
using SharpLearning.Neural.Learners;
using SharpLearning.Neural.Loss;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceRecognization_v1
{
    public class MLLib
    {
        HaarCascade face;
        public List<ImageBindingModel> imageBindingModel { get; set; }
        ResolutionImage resolutionImage;//độ phân giải ảnh khuôn mặt làm đầu vào cho model training

        public MLLib()
        {
            face = new HaarCascade("haarcascade_frontalface_default.xml");
            imageBindingModel = new List<ImageBindingModel>();
            resolutionImage = new ResolutionImage() { Height = 500, Width = 500 };
        }

        /// <summary>
        /// load tất cả ảnh từ folder Data gán vào list imageBindingModel
        /// </summary>
        private void LoadDataImg()
        {
            try
            {
                string[] fileArray = Directory.GetDirectories(Application.StartupPath + "\\Data");
                var task = new List<Task>();

                //load 3 folder de test
                fileArray = new string[] { fileArray[0], fileArray[1], fileArray[2] };

                foreach (var item in fileArray)
                {
                    task.Add(Task.Run(async () =>
                    {
                        string label = item.Split('\\').Last();
                        string[] fileImg = Directory.GetFiles(item, "*.*",
                                             SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".jpg") || s.EndsWith(".png")).ToArray();
                        for (int j = 0; j < fileImg.Length; j++)
                        {
                            var img = new Image<Rgb, byte>(fileImg[j]);
                            imageBindingModel.Add(new ImageBindingModel()
                            {
                                ImageGray = img.Convert<Gray, Byte>(),
                                ImageRgb = img,
                                Label = label
                            });
                        }
                    }));
                }
                Task.WaitAll(task.ToArray());
            }
            catch (Exception ex) { }
        }

        /// <summary>
        /// tách khuôn mặt của từng ảnh, xóa ảnh nếu ko tìm thấy khuôn mặt trong ảnh
        /// </summary>
        private void DetectFaceImg()
        {
            foreach (var item in imageBindingModel)
            {
                MCvAvgComp[][] facesDetected = item.ImageGray.DetectHaarCascade(
                    face,
                    1.2,
                    10,
                    Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                    new Size(20, 20));

                foreach (MCvAvgComp f in facesDetected[0])
                {
                    item.ImageFaceRgb = item.ImageRgb.Copy(f.rect);
                    break;
                }
            }
            imageBindingModel = imageBindingModel.Where(x => x.ImageFaceRgb != null).ToList();
        }

        /// <summary>
        /// resize ảnh lại cho cùng kích thước trước khi cho vào train
        /// </summary>
        private void ResizeBitmap()
        {
            foreach (var item in imageBindingModel)
            {
                var b = ResizeImageFile(item.ImageFaceRgb.Bitmap, resolutionImage.Height);
                var c = Image.FromStream(new MemoryStream(b));
                item.ImageFaceResizeRgb = new Image<Rgb, byte>((Bitmap)c);
            }
        }

        private static byte[] ResizeImageFile(Image oldImage, int targetSize) // Set targetSize to 1024
        {
            Size newSize = CalculateDimensions(oldImage.Size, targetSize);
            using (Bitmap newImage = new Bitmap(newSize.Width, newSize.Height, PixelFormat.Format24bppRgb))
            {
                using (Graphics canvas = Graphics.FromImage(newImage))
                {
                    canvas.SmoothingMode = SmoothingMode.AntiAlias;
                    canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    canvas.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    canvas.DrawImage(oldImage, new Rectangle(new Point(0, 0), newSize));
                    MemoryStream m = new MemoryStream();
                    newImage.Save(m, ImageFormat.Jpeg);
                    return m.GetBuffer();
                }
            }

        }

        public static Size CalculateDimensions(Size oldSize, int targetSize)
        {
            Size newSize = new Size();
            if (oldSize.Height > oldSize.Width)
            {
                newSize.Width = (int)(oldSize.Width * ((float)targetSize / (float)oldSize.Height));
                newSize.Height = targetSize;
            }
            else
            {
                newSize.Width = targetSize;
                newSize.Height = (int)(oldSize.Height * ((float)targetSize / (float)oldSize.Width));
            }
            return newSize;
        }

        public void Training()
        {
            LoadDataImg();
            DetectFaceImg();
            ResizeBitmap();

            // the output layer must know the number of classes.
            var numberOfClasses = imageBindingModel.ConvertAll(x => x.Label).Distinct().Count();

            var net = new NeuralNet();
            net.Add(new InputLayer(width: resolutionImage.Width, height: resolutionImage.Height, depth: 3)); // MNIST data is 28x28x1.
            net.Add(new DropoutLayer(0.2));
            net.Add(new DenseLayer(800, Activation.Relu));
            net.Add(new DropoutLayer(0.5));
            net.Add(new DenseLayer(800, Activation.Relu));
            net.Add(new DropoutLayer(0.5));
            net.Add(new SoftMaxLayer(numberOfClasses));


            var learner = new ClassificationNeuralNetLearner(net, iterations: 10, loss: new AccuracyLoss());
            var model = learner.Learn()

        }
    }
}
