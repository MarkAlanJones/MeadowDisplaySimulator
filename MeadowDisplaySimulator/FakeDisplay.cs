﻿using Meadow.Foundation;
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

        public override int Width => width;

        public override int Height => height;

        protected int width;
        protected int height;
        protected WriteableBitmap bitmap;
        protected Int32Rect r;
        protected Color Default;
        protected byte[] pixels;    // we will "draw" on this byte arrary until Show then it is written to the WBM

        /// <summary>
        /// Construct the display with a WritableBitmap
        /// </summary>
        public FakeDisplay(int width, int height, WriteableBitmap bitmap)
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

        public override void DrawPixel(int x, int y, Color color)
        {
            if (x >= width || x < 0)
            {
                if (IgnoreOutOfBoundsPixels)
                    return;
                else
                    throw new ArgumentException("OutOfBounds", nameof(x));
            }
            if (y >= height || y < 0)
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

        /// <summary>
        /// Invert the color of a single pixel
        /// </summary>
        public override void InvertPixel(int x, int y)
        {
            if (x >= width || x < 0)
            {
                if (IgnoreOutOfBoundsPixels)
                    return;
                else
                    throw new ArgumentException("OutOfBounds", nameof(x));
            }
            if (y >= height || y < 0)
            {
                if (IgnoreOutOfBoundsPixels)
                    return;
                else
                    throw new ArgumentException("OutOfBounds", nameof(y));
            }

            var pos = (int)(x + y * width) * 4;
            byte[] b = new byte[4] { pixels[pos], pixels[pos + 1], pixels[pos + 2], pixels[pos + 3] };

            // invert BRG not A
            b[0] = (byte)(255 - b[0]);
            b[1] = (byte)(255 - b[1]);
            b[2] = (byte)(255 - b[2]);

            pixels[pos] = b[0];
            pixels[pos + 1] = b[1];
            pixels[pos + 2] = b[2];
            pixels[pos + 3] = b[3];
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
