using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace FaceRecognization_v1.Model
{
    public class ImageBindingModel
    {
        /// <summary>
        /// ảnh load từ db lên
        /// </summary>
        public Image<Rgb, byte> ImageRgb { get; set; }

        /// <summary>
        /// ảnh trắng đen convert từ ảnh RGB
        /// </summary>
        public Image<Gray, byte> ImageGray { get; set; }

        /// <summary>
        /// nhãn của hình
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// ảnh khuôn mặt cắt ra từ ảnh rgb
        /// </summary>
        public Image<Rgb, byte> ImageFaceRgb { get; set; }

        /// <summary>
        /// ảnh khuôn mặt sau khi resize
        /// </summary>
        public Image<Rgb, byte> ImageFaceResizeRgb { get; set; }
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
