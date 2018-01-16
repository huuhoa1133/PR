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

                fileArray = new[] { fileArray[0], fileArray[1] };

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
                            img.Bitmap = await ResizeBitmap(img.Bitmap, 320, 240);
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

        private async Task<Bitmap> ResizeBitmap(Bitmap bitmap, int width, int height)
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

        public async Task<Bitmap> DetectFace(Bitmap bitmap)
        {
            currentFrame = new Image<Bgr, byte>(bitmap);
            currentFrame.Bitmap = await ResizeBitmap(bitmap, 320, 240);
            gray = currentFrame.Convert<Gray, Byte>();
            // Face Detector
            MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
                  face,
                 1.2,
                 10,
                 Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                 new Size(20, 20));

            foreach (MCvAvgComp f in facesDetected[0])
            {
                t = t + 1;
                result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                //draw the face detected in the 0th (gray) channel with blue color
                currentFrame.Draw(f.rect, new Bgr(Color.Red), 2);

               
                /*
                if (trainingImages.ToArray().Length != 0)
                {
                    //TermCriteria for face recognition with numbers of trained images like maxIteration
                    MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain, 0.001);

                    //Eigen face recognizer
                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(
                       trainingImages.ToArray(),
                       labels.ToArray(),
                       3000,
                       ref termCrit);

                    name = recognizer.Recognize(result);

                    //Draw the label for each face detected and recognized
                    currentFrame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.LightGreen));

                }

                NamePersons[t - 1] = name;
                NamePersons.Add("");
                */

                //Set the number of faces detected on the scene
                //label3.Text = facesDetected[0].Length.ToString();

                /*
                //Set the region of interest on the faces

                gray.ROI = f.rect;
                MCvAvgComp[][] eyesDetected = gray.DetectHaarCascade(
                   eye,
                   1.1,
                   10,
                   Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                   new Size(20, 20));
                gray.ROI = Rectangle.Empty;

                foreach (MCvAvgComp ey in eyesDetected[0])
                {
                    Rectangle eyeRect = ey.rect;
                    eyeRect.Offset(f.rect.X, f.rect.Y);
                    currentFrame.Draw(eyeRect, new Bgr(Color.Blue), 2);
                }
                 */

            }

            return currentFrame.Bitmap;
        }

        public async Task FrameGrabber(Bitmap bitmap)
        {
            try
            {
                NamePersons.Add("");
                currentFrame = new Image<Bgr, byte>(bitmap);
                currentFrame.Bitmap = await ResizeBitmap(bitmap, 320, 240);
                gray = currentFrame.Convert<Gray, Byte>();
                // Face Detector
                MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
                      face,
                     1.2,
                     10,
                     Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                     new Size(20, 20));

                //Action for each element detected
                foreach (MCvAvgComp f in facesDetected[0])
                {
                    t = t + 1;
                    result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                    //draw the face detected in the 0th (gray) channel with blue color
                    currentFrame.Draw(f.rect, new Bgr(Color.Red), 2);


                    if (trainingImages.ToArray().Length != 0)
                    {
                        //TermCriteria for face recognition with numbers of trained images like maxIteration
                        MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain, 0.001);

                        //Eigen face recognizer
                        EigenObjectRecognizer recognizer = new EigenObjectRecognizer(
                           trainingImages.ToArray(),
                           labels.ToArray(),
                           3000,
                           ref termCrit);

                        name = recognizer.Recognize(result);

                        //Draw the label for each face detected and recognized
                        currentFrame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.LightGreen));

                    }

                    NamePersons[t - 1] = name;
                    NamePersons.Add("");


                    //Set the number of faces detected on the scene
                    //label3.Text = facesDetected[0].Length.ToString();

                    /*
                    //Set the region of interest on the faces

                    gray.ROI = f.rect;
                    MCvAvgComp[][] eyesDetected = gray.DetectHaarCascade(
                       eye,
                       1.1,
                       10,
                       Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                       new Size(20, 20));
                    gray.ROI = Rectangle.Empty;

                    foreach (MCvAvgComp ey in eyesDetected[0])
                    {
                        Rectangle eyeRect = ey.rect;
                        eyeRect.Offset(f.rect.X, f.rect.Y);
                        currentFrame.Draw(eyeRect, new Bgr(Color.Blue), 2);
                    }
                     */

                }
                t = 0;

                //Names concatenation of persons recognized
                for (int nnn = 0; nnn < facesDetected[0].Length; nnn++)
                {
                    names = names + NamePersons[nnn] + ", ";
                }
                //Show the faces procesed and recognized
                //imageBoxFrameGrabber.Image = currentFrame;
                //label4.Text = names;
                names = "";
                //Clear the list(vector) of names
                NamePersons.Clear();
            }catch(Exception ex)
            {

            }
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
