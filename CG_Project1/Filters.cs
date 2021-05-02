using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace CG_lab1KM
{
    public class Filters
    {

        public static int c_brightness = 50;
        public static double c_gamma = 3.0;
        public static int c_contrast = 20;

        public static List<CustomFilter> cfilters = new List<CustomFilter>();


        public static float[,] DitherMap2x2 = new float[,]
        {
                { 1,  3,},
                { 4,  2 }
        };
        public static float[,] DitherMap3x3 = new float[,]
        {
                { 3,  7, 4 },
                { 6,  1, 9 },
                { 2,  8, 5 }
        };
        public static float[,] DitherMap4x4 = new float[,]
        {
                {  1,  9,  3, 11 },
                { 13,  5, 15,  7 },
                {  4, 12,  2, 10 },
                { 16,  8, 14,  6 }
        };
        public static float[,] DitherMap6x6 = new float[,]
        {
                {  9, 25, 13, 11, 27, 15 },
                { 21,  1, 33, 23,  3, 35 },
                {  5, 29, 17,  7, 31, 19 },
                { 12, 28, 16, 10, 26, 14 },
                { 24,  4, 36, 22,  2, 34 },
                {  8, 32, 20,  6, 30, 16 },
        };
        public static float[,] ThresholdMap;

        public static labFilter lb =
            new labFilter()
            {
                Matrix = new float[,]
                    {
                        { 1, 1, 1 },
                        { 1, 1, 1 },
                        { 1, 1, 1 }
                    }
            };
        public static ConvoFilter blurFilter =
            new BlurFilter()
            {
                Divisor = 9,
                Offset = 0,
                Kernel = new float[,]
                    {
                        { 1, 1, 1 },
                        { 1, 1, 1 },
                        { 1, 1, 1 }
                    },
                AnchorX = 0,
                AnchorY = 0
            };
        public static ConvoFilter GBlurFilter =
            new GBlurFilter()
            {
                Divisor = 16,
                Offset = 0,
                Kernel = new float[,]
                    {
                        { 1, 2, 1 },
                        { 2, 4, 2 },
                        { 1, 2, 1 }
                    },
                AnchorX = 0,
                AnchorY = 0
            };
        public static ConvoFilter SharpenFilter =
            new SharpenFilter()
            {
                Divisor = 1,
                Offset = 0,
                Kernel = new float[,]
                    {
                        { -1, -1, -1 },
                        { -1, 9, -1 },
                        { -1, -1, -1 }
                    },
                AnchorX = 0,
                AnchorY = 0
            };
        //TopToBottom
        public static ConvoFilter EdgeDetectionFilter =
            new EdgeFilter()
            {
                Divisor = 1,
                Offset = 0,
                Kernel = new float[,]
                {
                        { -1, -1, -1 },
                        { -1, 8, -1 },
                        { -1, -1, -1 } ,
                },
                AnchorX = 0,
                AnchorY = 0
            };
        public static ConvoFilter EmbossingFilter =
            new EmbossFilter()
            {
                Divisor = 1,
                Offset = 128,
                Kernel = new float[,]
                    {
                        { -1, 0, 0 },
                        { 0, 0, 0 },
                        { 0, 0, 1 },
                    },
                AnchorX = 0,
                AnchorY = 0
            };
        
        
        
        private static (byte[] bytes, int stride) ImageToByteArray(BitmapSource img)
        {
            int height = img.PixelHeight;
            int width = img.PixelWidth;
            int stride = width * ((img.Format.BitsPerPixel + 7) / 8);

            byte[] bits = new byte[height * stride];
            img.CopyPixels(bits, stride, 0);
            return (bits,stride);
        }

        private static int adjustValues(int value)
        {
            if (value > 255)
                value = 255;
            else if (value < 0)
                value = 0;
            return value;
        }

        public static BitmapSource Inverse(BitmapSource Source_Image)
        {
            (byte[] image_bytes, int stride ) = ImageToByteArray(Source_Image);
            int length = stride * Source_Image.PixelHeight;
            byte[] resultBuffer = new byte[length];

            for (int i = 0; i < length; i += 4)
            {
                resultBuffer[i] = (byte)(adjustValues(255 - image_bytes[i]));
                resultBuffer[i + 1] = (byte)(adjustValues(255 - image_bytes[i + 1]));
                resultBuffer[i + 2] = (byte)(adjustValues(255 - image_bytes[i + 2]));
            }

            return BitmapSource.Create(
                Source_Image.PixelWidth, Source_Image.PixelHeight,
                Source_Image.DpiX, Source_Image.DpiY, Source_Image.Format,
                null, resultBuffer, stride);
        }

        public static BitmapSource BrightnessCorr(BitmapSource Source_Image)
        {
            (byte[] image_bytes, int stride) = ImageToByteArray(Source_Image);
            int length = stride * Source_Image.PixelHeight;

            for (int i = 0; i < length; i += 4)
            {
                image_bytes[i] = (byte)(adjustValues(image_bytes[i] + c_brightness));
                image_bytes[i + 1] = (byte)(adjustValues(image_bytes[i + 1] + c_brightness));
                image_bytes[i + 2] = (byte)(adjustValues(image_bytes[i + 2] + c_brightness));
            }

            return BitmapSource.Create(
                Source_Image.PixelWidth, Source_Image.PixelHeight,
                Source_Image.DpiX, Source_Image.DpiY, Source_Image.Format,
                null, image_bytes, stride);
        }

        public static BitmapSource ContrastEnh(BitmapSource Source_Image)
        {
            double contrastLevel = Math.Pow((100.0 + c_contrast) / 100.0, 2);
            (byte[] image_bytes, int stride) = ImageToByteArray(Source_Image);
            int length = stride * Source_Image.PixelHeight;

            for (int i = 0; i < length; i += 4)
            {
                image_bytes[i] = (byte)(adjustValues((int)
                    (((((image_bytes[i] / 255.0) - 0.5) * contrastLevel) + 0.5) * 255.0))
                    );
                image_bytes[i + 1] = (byte)(adjustValues((int)
                    (((((image_bytes[i+1] / 255.0) - 0.5) * contrastLevel) + 0.5) * 255.0))
                    );
                image_bytes[i + 2] = (byte)(adjustValues((int)
                    (((((image_bytes[i+2] / 255.0) - 0.5) * contrastLevel) + 0.5) * 255.0))
                    );
            }

            return BitmapSource.Create(
                Source_Image.PixelWidth, Source_Image.PixelHeight,
                Source_Image.DpiX, Source_Image.DpiY, Source_Image.Format,
                null, image_bytes, stride);

        }

        public static BitmapSource GammaCorr(BitmapSource Source_Image)
        {

            (byte[] image_bytes, int stride) = ImageToByteArray(Source_Image);
            int length = stride * Source_Image.PixelHeight;
            double cons = 1.0d;
            for (int i = 0; i < length; i += 4)
            {
                image_bytes[i] = (byte)(adjustValues((int)
                    ((cons * Math.Pow((double)image_bytes[i] / 255, 1.0/c_gamma))*255 ))
                    );
                image_bytes[i + 1] = (byte)(adjustValues((int)
                    ((cons * Math.Pow((double)image_bytes[i + 1] / 255, 1.0/c_gamma))*255 ))
                    );
                image_bytes[i + 2] = (byte)(adjustValues((int)
                    ((cons * Math.Pow((double)image_bytes[i + 2] / 255, 1.0 / c_gamma))*255 ))
                    );
            }

            return BitmapSource.Create(
                Source_Image.PixelWidth, Source_Image.PixelHeight,
                Source_Image.DpiX, Source_Image.DpiY, Source_Image.Format,
                null, image_bytes, stride);
        }

        public static BitmapSource LabFilter(BitmapSource Source_Image)
        {
            (byte[] image_bytes, int stride) = ImageToByteArray(Source_Image);
            int length = stride * Source_Image.PixelHeight;
            byte[] resultBuffer = new byte[length];

            for (int i = 0; i < length; i += 4)
            {
                //blue
                byte gray = (byte)(adjustValues((int)
                    (image_bytes[i] * 0.299 +
                    image_bytes[i + 1] * 0.587 +
                    image_bytes[i + 2] * 0.114))
                    );
                resultBuffer[i] = gray;
                resultBuffer[i + 1] = gray;
                resultBuffer[i + 2] = gray;
            }

            return BitmapSource.Create(
                Source_Image.PixelWidth, Source_Image.PixelHeight,
                Source_Image.DpiX, Source_Image.DpiY, Source_Image.Format,
                null, resultBuffer, stride);
        }
        public static BitmapSource ConvolutionFilter(BitmapSource Source_Image, ConvoFilter filter)
        {
            (byte[] image_bytes, int stride) = ImageToByteArray(Source_Image);
            int length = stride * Source_Image.PixelHeight;

            byte[] resultBuffer = new byte[length];


            for (int i = 0; i < length; i += 4)
            {
                resultBuffer[i] = image_bytes[i];
                resultBuffer[i + 1] = image_bytes[i + 1];
                resultBuffer[i + 2] = image_bytes[i + 2];
                resultBuffer[i + 3] = image_bytes[i + 3];

            }


            int filterWidth = filter.Kernel.GetLength(1);
            int filterHeight = filter.Kernel.GetLength(0);

            int filterOffset = (filterWidth - 1) / 2;
           
            int calcOffset = 0;

            int byteOffset = 0;
            
            for (int offsetY = (filterOffset); offsetY <
                Source_Image.PixelHeight - filterOffset; offsetY++)
            {
                for (int offsetX = (filterOffset); offsetX <
                    Source_Image.PixelWidth - filterOffset; offsetX++)
                {


                    double blue = 0.0;
                    double green = 0.0;
                    double red = 0.0;

                    byteOffset = offsetY *
                                 stride +
                                 offsetX * 4;



                    for (int filterY = -filterOffset + filter.AnchorX;
                        filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset + filter.AnchorY;
                            filterX <= filterOffset; filterX++)
                        {

                            calcOffset = byteOffset +
                                         (filterX * 4) +
                                         (filterY * stride);


                            blue += (image_bytes[calcOffset]) *
                                filter.Kernel[filterY + filterOffset,
                                                    filterX + filterOffset];

                            green += (image_bytes[calcOffset + 1]) *
                                     filter.Kernel[filterY + filterOffset,
                                                        filterX + filterOffset];

                            red += (image_bytes[calcOffset + 2]) *
                                   filter.Kernel[filterY + filterOffset,
                                                      filterX + filterOffset];


                        }
                    }


                    blue = 1 / filter.Divisor * blue + filter.Offset;
                    green = 1 / filter.Divisor * green + filter.Offset;
                    red = 1 / filter.Divisor * red + filter.Offset;


                    resultBuffer[byteOffset] = (byte)(adjustValues((int)blue));
                    resultBuffer[byteOffset + 1] = (byte)(adjustValues((int)green));
                    resultBuffer[byteOffset + 2] = (byte)(adjustValues((int)red));
                    resultBuffer[byteOffset + 3] = image_bytes[byteOffset + 3];

                }
            }


            return BitmapSource.Create(
                Source_Image.PixelWidth, Source_Image.PixelHeight,
                Source_Image.DpiX, Source_Image.DpiY, Source_Image.Format,
                null, resultBuffer, stride);
        }

        public static BitmapSource OrderedDithering(BitmapSource Source_Image, int red, int green, int blue)
        {
            // FOR GRAYSCALE DITHERING SELECT RED = GREEN = BLUE NUMBER OF COLORS PALETTE
            (byte[] image_bytes, int stride) = ImageToByteArray(Source_Image);
            int length = stride * Source_Image.PixelHeight;

            byte[] resultBuffer = new byte[length];

            float[,] bayerMatrix = new float[ThresholdMap.GetLength(0), ThresholdMap.GetLength(1)];

            for (int i = 0; i < ThresholdMap.GetLength(0); ++i)
                for (int j = 0; j < ThresholdMap.GetLength(1); ++j)
                    bayerMatrix[i, j] = (ThresholdMap[j, i]) / ((float)Math.Pow(ThresholdMap.GetLength(0),2)+1.0f);

            byte[] redIntervals = GetIntervals(red);
            byte[] greenIntervals = GetIntervals(green);
            byte[] blueIntervals = GetIntervals(blue);
            byte[][] colorIntervals = new byte[3][] { redIntervals, greenIntervals, blueIntervals };
            int[] intervals = new int[3] { red, green, blue };

            for (int i = 0; i < Source_Image.PixelHeight; ++i)
            {
                for (int j = 0; j < Source_Image.PixelWidth; ++j)
                {
                    int current = i * stride + j * 4;
                    for (int c = 0; c < 3; c++)
                    {
                        int colorLevel = CalculateThreshold(j, i, image_bytes[current + c], intervals[c], bayerMatrix);
                        resultBuffer[current + c] = colorIntervals[c][colorLevel];
                    }
                }
            }


            return BitmapSource.Create(
                Source_Image.PixelWidth, Source_Image.PixelHeight,
                Source_Image.DpiX, Source_Image.DpiY, Source_Image.Format,
                null, resultBuffer, stride);
        }

        public static int CalculateThreshold(int x, int y, byte oldp, int k, float[,] matrix)
        {
            double temp = ((double)k - 1) * (double)oldp / 255.0;
            int col = (int)Math.Floor(temp);
            double re = temp - col;

            if (re >= matrix[x % matrix.GetLength(0), y % matrix.GetLength(1)])
                ++col;
            return col;
        }
        public static byte[] GetIntervals(int number)
        {
            byte[] values = new byte[number];

            for (int i = 0; i < number; ++i)
            {
                values[i] = (byte)((255 * i) / (number - 1));
            }
            return values;
        }

        public static BitmapSource PopularityAlgorithm(BitmapSource Source_Image, int NoColors)
        {
            (byte[] image_bytes, int stride) = ImageToByteArray(Source_Image);
            int length = stride * Source_Image.PixelHeight;

            byte[] resultBuffer = new byte[length];

            List<(byte, byte, byte)> Colors = new List<(byte, byte, byte)>();

            for (int i = 0; i < length; i += 4)
            {
                Colors.Add((image_bytes[i], image_bytes[i+1], image_bytes[i+2]));
            }

            var KPopularColors = Colors
                                    .GroupBy(i => i)
                                    .OrderByDescending(g => g.Count())
                                    .Take(NoColors)
                                    .Select(g => g.Key).ToList<(byte, byte, byte)>();

            for (int i = 0; i < length; i += 4)
            {
                Dictionary<int, double> temp = new Dictionary<int, double>();
                for (int j = 0; j < KPopularColors.Count; ++j)
                {
                    temp.Add(j, (
                        Math.Sqrt(Math.Pow(image_bytes[i] - KPopularColors[j].Item1,2)+ 
                        Math.Pow(image_bytes[i + 1] - KPopularColors[j].Item2, 2)+ 
                        Math.Pow(image_bytes[i + 2] - KPopularColors[j].Item3, 2))
                        ));
                }
                var ordered = temp.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                image_bytes[i] = Colors[ordered.First().Key].Item1;
                image_bytes[i + 1] = Colors[ordered.First().Key].Item2;
                image_bytes[i + 2] = Colors[ordered.First().Key].Item3;
            }


            return BitmapSource.Create(
                Source_Image.PixelWidth, Source_Image.PixelHeight,
                Source_Image.DpiX, Source_Image.DpiY, Source_Image.Format,
                null, image_bytes, stride);
        }



    }
}
