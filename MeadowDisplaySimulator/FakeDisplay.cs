using Meadow.Foundation;
using Meadow.Foundation.Displays;
using System;
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

        public override void DrawBitmap(int x, int y, int width, int height, byte[] bitmap, BitmapMode bitmapMode)
        {
            throw new NotImplementedException();
        }

        public override void DrawBitmap(int x, int y, int width, int height, byte[] bitmap, Color color)
        {
            throw new NotImplementedException();
        }

        public override void DrawPixel(int x, int y, Color color)
        {
            if (!IgnoreOutOfBoundsPixels)
            {
                if (x > width)
                    throw new ArgumentException("OutOfBounds", nameof(x));
                if (y > height)
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
            }));

        }

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
    }
}
