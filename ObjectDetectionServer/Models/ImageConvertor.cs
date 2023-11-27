using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ObjectDetectionServer.Models
{
    public class ImageConvertor<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {

        public static byte[] ToByteArray(Image<TPixel> source)
        {

            var encoder = new JpegEncoder();
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Encode(source, ms);
                return ms.ToArray();
            }
        }

        public static Image<TPixel> RemoveBlackStripes(Image<TPixel> source)
        {
            var isBlackStripeTop = true;
            int lastBlackStripeTop = 0;

            var isBlackStripeLeft = true;
            int lastBlackStripeLeft = 0;
            for (int i = 0; i < source.Width; i++)
            {
                for (int j = 0; j < source.Height; j++)
                {
                    var currentPixel = new Rgba32();
                    source[j, i].ToRgba32(ref currentPixel);
                    if (!(currentPixel.R == 0 && currentPixel.G == 0 && currentPixel.B == 0))
                    {
                        isBlackStripeTop = false;
                        lastBlackStripeTop = i;
                        break;
                    }
                }
                if (!isBlackStripeTop)
                    break;
            }

            for (int i = 0; i < source.Width; i++)
            {
                for (int j = 0; j < source.Height; j++)
                {
                    var currentPixel = new Rgba32();
                    source[i, j].ToRgba32(ref currentPixel);
                    if (!(currentPixel.R == 0 && currentPixel.G == 0 && currentPixel.B == 0))
                    {
                        isBlackStripeLeft = false;
                        lastBlackStripeLeft = i;
                        break;
                    }
                }
                if (!isBlackStripeLeft)
                    break;
            }

            return source.Clone(x =>
            {
                x.Crop(new Rectangle(lastBlackStripeLeft, lastBlackStripeTop, source.Width - 2 * lastBlackStripeLeft, source.Height - 2 * lastBlackStripeTop));
            });
        }
    }
}
