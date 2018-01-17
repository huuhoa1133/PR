using Emgu.CV;
using Emgu.CV.Structure;
using System.Collections.Generic;
using System.Drawing;

namespace FaceRecognization_v1.Model
{
    public class ImageBindingModel
    {

        public static double Iterations { get; set; }

        /// <summary>
        /// tên của ảnh, định danh
        /// </summary>
        public double Id { get; set; }

        /// <summary>
        /// nhãn của hình
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// ảnh khuôn mặt gray
        /// </summary>
        public Image<Gray, byte> Image { get; set; }

        public List<double> dImage { get; set; }
    }

    /// <summary>
    /// độ phân giải ảnh đầu vào
    /// </summary>
    public class ResolutionImage
    {
        public int Width { get; set; }

        public int Height { get; set; }
    }
}
