using Emgu.CV;
using Emgu.CV.Structure;
using FaceRecognization_v1.Model;
using SharpLearning.Containers.Matrices;
using SharpLearning.InputOutput.Csv;
using SharpLearning.Metrics.Classification;
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

using System.Drawing;
using Emgu.CV.Structure;
using Emgu.CV.ML;
using Emgu.CV.ML.Structure;

namespace FaceRecognization_v1
{
    public class MLLib
    {
        HaarCascade face;
        public List<ImageBindingModel> imageBindingModel { get; set; }
        ResolutionImage resolutionImage;//độ phân giải ảnh khuôn mặt làm đầu vào cho model training
        Backpropagation bnn;

        public MLLib()
        {
            face = new HaarCascade("haarcascade_frontalface_default.xml");
            imageBindingModel = new List<ImageBindingModel>();
            resolutionImage = new ResolutionImage() { Height = 280, Width = 280 };
        }

        #region load data resize and save


        /// <summary>
        /// load tất cả ảnh từ folder Data gán vào list imageBindingModel
        /// </summary>
        private void LoadDataImg()
        {
            try
            {
                string[] fileArray = Directory.GetDirectories(Application.StartupPath + "\\Data");

                //fileArray = new string[] { fileArray[0], fileArray[1], fileArray[2], fileArray[3], fileArray[4], fileArray[5] };
                int i = 1;
                foreach (var item in fileArray)
                {
                    
                    string label = item.Split('\\').Last();
                    string[] fileImg = Directory.GetFiles(item, "*.*",
                                         SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".jpg") || s.EndsWith(".png")).ToArray();
                    for (int j = 0; j < fileImg.Length; j++)
                    {
                        var img = new Image<Rgb, byte>(fileImg[j]);
                        var face = DetectFaceImg(img);
                        if (face != null)
                        {
                            var face_resize = ResizeBitmap(face);
                            //if (face_resize != null)
                            //{
                            //    var id_old = imageBindingModel.FirstOrDefault(x => x.Label == label);
                            //    imageBindingModel.Add(new ImageBindingModel()
                            //    {
                            //        Id = id_old == null ? ImageBindingModel.Iterations++ : id_old.Id,
                            //        Label = label,
                            //        ImageFaceRgb = face,
                            //        ImageFaceResizeRgb = face_resize,
                            //        ImageFaceResizeGray = face_resize.Convert<Gray,byte>()
                            //    });
                            //}
                            //var label_id = fileImg[j].Split('\\').Last().Split('.').First();
                            Console.WriteLine($"load data {label} -- {i}");
                            SaveDataImg(face_resize.Convert<Gray, byte>(), $"{label}_{i++}");
                            
                        }
                    }
                    i = 1;
                    //Console.WriteLine($"load data {label}");
                };
            }
            catch (Exception ex) { };
        }

        private void SaveDataImg(Image<Gray,byte> img, string label)
        {
            string path =$"{Application.StartupPath }\\TrainedFaces\\{label}.bmp";
            img.Save(path);
            Console.WriteLine($"save data : {path}");

            //for (int i = 0; i < imageBindingModel.Count; i++)
            //{
            //    string label = $"/TrainedFaces/{imageBindingModel[i].Label}_{i + 1}.bmp";
            //    imageBindingModel[i].ImageFaceResizeGray.Save(Application.StartupPath + label);
            //    Console.WriteLine($"save data {label}");
            //}
        }

        /// <summary>
        /// tách khuôn mặt của từng ảnh
        /// </summary>
        private Image<Rgb, byte> DetectFaceImg(Image<Rgb, byte> input)
        {
            Image<Rgb, byte> output = null;
            MCvAvgComp[][] facesDetected = input.Convert<Gray, Byte>().DetectHaarCascade(
                    face,
                    1.2,
                    10,
                    Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                    new Size(20, 20));

            foreach (MCvAvgComp f in facesDetected[0])
            {
                output = input.Copy(f.rect);
                break;
            }

            return output;
        }

        /// <summary>
        /// resize ảnh lại cho cùng kích thước trước khi cho vào model
        /// </summary>
        private Image<Rgb, byte> ResizeBitmap(Image<Rgb, byte> input)
        {
            Image<Rgb, byte> output = null;
            //var b = ResizeImageFile(input.Bitmap, resolutionImage.Height);
            //var c = Image.FromStream(new MemoryStream(b));
            //output = new Image<Rgb, byte>((Bitmap)c);
            //var x = output.Bytes;


            var b = ResizeImage(input.Bitmap, resolutionImage.Width, resolutionImage.Height);
            output = new Image<Rgb, byte>(b);

            return output;
        }

        private byte[] ResizeImageFile(Image oldImage, int targetSize) // Set targetSize to 1024
        {
            Size newSize = CalculateDimensions(oldImage.Size, targetSize);
            using (Bitmap newImage = new Bitmap(newSize.Width, newSize.Height, PixelFormat.Format24bppRgb))
            {
                using (Graphics canvas = Graphics.FromImage(newImage))
                {
                    //int posx = (int)(resolutionImage.Width - newSize.Width) / 2;
                    //int posy = (int)(resolutionImage.Height - newSize.Height) / 2;
                    canvas.SmoothingMode = SmoothingMode.AntiAlias;
                    canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    canvas.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    var rect = new Rectangle(new Point(0, 0), newSize);
                    canvas.DrawImage(oldImage, rect);
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

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        #endregion

        #region load image gray

        public void LoadImageGray()
        {
            string folder = Application.StartupPath + "\\TrainedFaces";
            string[] fileImgs = Directory.GetFiles(folder, "*.*",
                                         SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".bmp")).ToArray();
            foreach (var item in fileImgs)
            {
                var labels = item.Split('\\').Last().Split('.').First();
                var label = labels.Split('_').First();
                var img = new Image<Gray, byte>(item);

                var id_old = imageBindingModel.FirstOrDefault(x => x.Label == label);
                imageBindingModel.Add(new ImageBindingModel()
                {
                    Id = id_old == null ? ImageBindingModel.Iterations++ : id_old.Id,
                    Label = label,
                    Image = img,
                    dImage = img.Bytes.Select(x=>(double)x).ToList()
                });

            }
        }

        #endregion


        public void Training()
        {
            LoadImageGray();

            // the output layer must know the number of classes.
            var numberOfClasses = imageBindingModel.ConvertAll(x => x.Label).Distinct().Count();
            var numberByteOfRow = resolutionImage.Width * resolutionImage.Height;

            //var net = new NeuralNet();
            //net.Add(new InputLayer(width: resolutionImage.Width, height: resolutionImage.Height, depth: 1)); // MNIST data is 28x28x3.
            //net.Add(new DropoutLayer(0.2));
            //net.Add(new DenseLayer(800, Activation.Relu));
            //net.Add(new DropoutLayer(0.5));
            //net.Add(new DenseLayer(800, Activation.Relu));
            //net.Add(new DropoutLayer(0.5));
            //net.Add(new SoftMaxLayer(numberOfClasses));

            var net = new NeuralNet();
            net.Add(new InputLayer(width: 28, height: 28, depth: 1)); // MNIST data is 28x28x1.
            net.Add(new SoftMaxLayer(numberOfClasses)); // No hidden layers and SoftMax output layer corresponds to logistic regression classifer.


            var learner = new ClassificationNeuralNetLearner(net, iterations: 10, loss: new AccuracyLoss());


            var inputmatrix = new List<double>();
            imageBindingModel = new List<ImageBindingModel>()
            {
                imageBindingModel[0]
            };
            foreach (var item in imageBindingModel)
            {
                inputmatrix.AddRange(item.dImage.Select(x=>x/255));
            }
            var f64 = new F64Matrix(inputmatrix.ToArray(), imageBindingModel.Count, numberByteOfRow) ;

            var labels = imageBindingModel.ConvertAll(x => x.Id).ToArray();


            var model = learner.Learn(f64, labels);

            var metric = new TotalErrorClassificationMetric<double>();
            var predictions = model.Predict(f64);

            var a = metric.Error(imageBindingModel.ConvertAll(x => x.Id).ToArray(), predictions);
        }

        public void TrainANNModel()
        {
            LoadImageGray();
            // the output layer must know the number of classes.
            //var numberOfClasses = imageBindingModel.ConvertAll(x => x.Label).Distinct().Count();
            var numberOfClasses = 1;
            var numberInput = resolutionImage.Width * resolutionImage.Height;

            int trainSampleCount = imageBindingModel.Count;
            Matrix<float> trainData = new Matrix<float>(trainSampleCount, numberInput);
            Matrix<float> trainClasses = new Matrix<float>(trainSampleCount, numberOfClasses);

            for (int i = 0; i < imageBindingModel.Count; i++)
            {
                for (int j = 0; j < numberInput; j++)
                {
                    trainData[i, j] = imageBindingModel[i].Image.Bytes[j];
                    trainClasses[i, 0] = (float)imageBindingModel[i].Id;
                }

            }

            Matrix<int> layerSize = new Matrix<int>(new int[] { numberInput, numberInput+ numberInput, numberOfClasses });

            MCvANN_MLP_TrainParams parameters = new MCvANN_MLP_TrainParams();
            parameters.term_crit = new MCvTermCriteria(100, 1.0e-8);
            parameters.train_method = Emgu.CV.ML.MlEnum.ANN_MLP_TRAIN_METHOD.BACKPROP;
            parameters.bp_dw_scale = 0.1;
            parameters.bp_moment_scale = 0.1;

            using (ANN_MLP network = new ANN_MLP(layerSize, Emgu.CV.ML.MlEnum.ANN_MLP_ACTIVATION_FUNCTION.SIGMOID_SYM, 1.0, 1.0))
            {
                network.Train(trainData, trainClasses, null, null, parameters, Emgu.CV.ML.MlEnum.ANN_MLP_TRAINING_FLAG.DEFAULT);


                Matrix<float> sample = new Matrix<float>(1, numberInput);
                Matrix<float> prediction = new Matrix<float>(1, numberOfClasses);

                int recog_true = 0;

                for(int i = 0;i< imageBindingModel.Count;i++)
                {
                    for (int j = 0; j < numberInput; j++)
                    {
                        sample[0, j] = trainData[i, j];
                    }

                    network.Predict(sample, prediction);
                    var response = prediction.Data[0,0];

                    if (Math.Abs(response - imageBindingModel[i].Id) < 0.5)
                        recog_true++;


                    Console.WriteLine($"recoge : {response} -- target: {imageBindingModel[i].Id} -- result: {Math.Abs(response - imageBindingModel[i].Id) < 0.5}");
                }

            }


        }
    }
}
