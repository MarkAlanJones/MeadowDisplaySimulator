using Meadow.Foundation;
using Meadow.Foundation.Displays;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MeadowDisplaySimulator
{
    // Implement a Meadow Display that can write to the WPF gui
    public class FakeDisplay : DisplayBase
    {
        public override DisplayColorMode ColorMode => DisplayColorMode.Format24bppRgb888;

        public override uint Width => width;

        public override uint Height => height;

        protected uint width;
        protected uint height;
        protected WriteableBitmap bitmap;
        protected Int32Rect r;
        protected Color Default;
        protected byte[] pixels;    // we will "draw" on this byte arrary until Show then it is written to the WBM

        /// <summary>
        /// Construct the display with a WritableBitmap
        /// </summary>
        public FakeDisplay(uint width, uint height, WriteableBitmap bitmap)
        {
            if (bitmap.Height >= height && bitmap.Width >= width)
                this.bitmap = bitmap;
            else
                throw new ArgumentException($"bitmap is too small for indicated size {width}x{height}", nameof(bitmap));

            this.width = width;
            this.height = height;

            r = new Int32Rect(0, 0, (int)width, (int)height);
            Default = Color.White;

            pixels = new byte[width * height * 4];

            // Display noise until clear like a real display
            AddNoise();
        }

        public override void Clear(bool updateDisplay = false)
        {
            byte[] b = Color2Byte(Color.Black);
            pixels = new byte[width * height * 4];
            for (int pos = 0; pos < pixels.Length; pos += 4)
            {
                pixels[pos] = b[0];
                pixels[pos + 1] = b[1];
                pixels[pos + 2] = b[2];
                pixels[pos + 3] = b[3];
            }

            if (updateDisplay)
                Show();
        }

        /// <summary>
        ///     Display a 1-bit bitmap - specify width in bytes not pixels (8 pixels per byte)
        ///     copied from TftSpiBase.cs - this version ignores the bitmapMode and draws in the default color
        ///     I guess Bitmapmode is assumed to be AND
        /// </summary>
        public override void DrawBitmap(int x, int y, int width, int height, byte[] bitmap, BitmapMode bitmapMode)
        {
            if ((width * height) != bitmap.Length)
            {
                throw new ArgumentException("Width and height do not match the bitmap size.");
            }

            for (var ordinate = 0; ordinate < height; ordinate++)
            {
                for (var abscissa = 0; abscissa < width; abscissa++)
                {
                    var b = bitmap[(ordinate * width) + abscissa];
                    byte mask = 0x01;

                    for (var pixel = 0; pixel < 8; pixel++)
                    {
                        DrawPixel(x + (8 * abscissa) + pixel, y + ordinate, (b & mask) > 0);
                        mask <<= 1;
                    }
                }
            }
        }

        /// <summary>
        ///     Display a 1-bit bitmap - specify width in bytes not pixels (8 pixels per byte)
        ///     copied from TftSpiBase.cs - this version only draws non black pixels
        /// </summary>

        public override void DrawBitmap(int x, int y, int width, int height, byte[] bitmap, Color color)
        {
            if ((width * height) != bitmap.Length)
            {
                throw new ArgumentException("Width and height do not match the bitmap size.");
            }

            for (var ordinate = 0; ordinate < height; ordinate++)
            {
                for (var abscissa = 0; abscissa < width; abscissa++)
                {
                    var b = bitmap[(ordinate * width) + abscissa];
                    byte mask = 0x01;

                    for (var pixel = 0; pixel < 8; pixel++)
                    {
                        if ((b & mask) > 0)
                        {
                            DrawPixel(x + (8 * abscissa) + pixel, y + ordinate, color);
                        }
                        mask <<= 1;
                    }
                }
            }
        }

        public override void DrawPixel(int x, int y, Color color)
        {
            if (x >= width)
            {
                if (IgnoreOutOfBoundsPixels)
                    return;
                else
                    throw new ArgumentException("OutOfBounds", nameof(x));
            }
            if (y >= height)
            {
                if (IgnoreOutOfBoundsPixels)
                    return;
                else
                    throw new ArgumentException("OutOfBounds", nameof(y));
            }

            byte[] b = Color2Byte(color);
            var pos = (int)(x + y * width) * 4;
            pixels[pos] = b[0];
            pixels[pos + 1] = b[1];
            pixels[pos + 2] = b[2];
            pixels[pos + 3] = b[3];
        }

        public override void DrawPixel(int x, int y, bool colored)
        {
            if (colored)
                DrawPixel(x, y, Default);
            else
                DrawPixel(x, y, Color.Black);
        }

        public override void DrawPixel(int x, int y)
        {
            DrawPixel(x, y, Default);
        }

        public override void SetPenColor(Color pen)
        {
            Default = pen;
        }

        public override void Show()
        {
            // update on the UI thread
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                bitmap.WritePixels(r, pixels, bitmap.BackBufferStride, 0, 0);

                if (Directory.Exists(SnapShotPath))
                    SaveSnapShot(bitmap.Clone());
            }));
        }

        // Set the SnapShotPath to a valid directory, and a screenshot will be saved each time show is called
        public string SnapShotPath { get; set; } = null;

        // Meadow colors are 4 doubles, 0.0 to 1.0 but we want bytes 0 - 255
        // PixelFormats.Pbgra32
        private byte[] Color2Byte(Color c)
        {
            return new byte[]
            {
                (byte)(c.B * 255),
                (byte)(c.G * 255),
                (byte)(c.R * 255),
                (byte)(c.A * 255)
            };
        }

        private void AddNoise()
        {
            var rand = new Random();
            rand.NextBytes(pixels);
            Show();
        }

        // Writes the display image to a new PNG image in the SnapShotPath
        private void SaveSnapShot(BitmapSource img)
        {
            string filename = SnapShotFilename();
            {
                using (FileStream stream = new FileStream(filename, FileMode.Create))
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(img));
                    encoder.Save(stream);
                }
            }
        }

        // Timestamped filename .png
        private string SnapShotFilename()
        {
            return Path.Combine(SnapShotPath, "MeadowSS_" + DateTime.Now.ToString("o").Replace(":", "") + ".png");
        }
    }
}
