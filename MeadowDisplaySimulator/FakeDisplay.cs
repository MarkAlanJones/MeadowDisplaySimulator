using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Peripherals.Sensors.Location.Gnss;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using static System.Windows.Media.Imaging.WriteableBitmapExtensions;
using Color = Meadow.Foundation.Color;

namespace MeadowDisplaySimulator
{
    // Implement a Meadow Display that can write to the WPF gui
    public class FakeDisplay : IGraphicsDisplay
    {
        public ColorType ColorMode => pixelbuffer.ColorMode;

        public int Width => width;

        public int Height => height;

        protected int width;
        protected int height;
        protected WriteableBitmap bitmap;
        protected Int32Rect r;
        protected Color Default;

        private IPixelBuffer pixelbuffer;

        public IPixelBuffer PixelBuffer => pixelbuffer;

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

            r = new Int32Rect(0, 0, width - 1, height - 1);
            Default = Color.White;

            pixelbuffer = new PixelBuffer(width, height);

            // Display noise until clear like a real display
            AddNoise();
        }

        // Draw a Buffer to the Display
        public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
        {
            for (int xx = x; x < width; x++)
                for (int yy = y; y < height; y++)
                    DrawPixel(xx, yy, displayBuffer.GetPixel(xx, yy));
        }

        public void Clear(bool updateDisplay = false)
        {
            Debug.WriteLine($"Clear {updateDisplay}");
            pixelbuffer.Clear();

            if (updateDisplay)
                Show();
        }

        public void DrawPixel(int x, int y, Color color)
        {
            if (x >= width || x < 0)
                return;
            if (y >= height || y < 0)
                return;

            pixelbuffer.SetPixel(x, y, color);
        }

        public void DrawPixel(int x, int y, bool colored)
        {
            if (colored)
                DrawPixel(x, y, Default);
            else
                DrawPixel(x, y, Color.Black);
        }

        /// <summary>
        /// Invert the color of a single pixel
        /// </summary>
        public void InvertPixel(int x, int y)
        {
            pixelbuffer.InvertPixel(x, y);
        }

        int showcount = 0;
        public void Show()
        {
            Debug.WriteLine($"Show! {showcount++} {DateTime.Now.TimeOfDay}");

            // update on the UI thread
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                bitmap.WritePixels(r, pixelbuffer.Buffer, bitmap.BackBufferStride, 0, 0);

                if (Directory.Exists(SnapShotPath))
                    SaveSnapShot(bitmap.Clone());
            }));
        }

        public void Show(int left, int top, int right, int bottom)
        {
            // just show everything
            Show();
        }

        public void Fill(Color fillColor, bool updateDisplay = false)
        {
            Fill(0, 0, Width, Height, fillColor);

            if (updateDisplay)
                Show();
        }

        public void Fill(int x, int y, int width, int height, Color fillColor)
        {
            for (int yy = y; yy < y + height; yy++)
                for (int xx = x; xx < x + width; xx++)
                    DrawPixel(xx, yy, fillColor);
        }

        #region Snapshot

        // Set the SnapShotPath to a valid directory, and a screenshot will be saved each time show is called
        public string SnapShotPath { get; set; } = null;

        private void AddNoise()
        {
            var rand = new Random();
            rand.NextBytes(pixelbuffer.Buffer);
            Show();
        }

        // Writes the display image to a new PNG image in the SnapShotPath
        private void SaveSnapShot(WriteableBitmap img)
        {
            var filename = SnapShotFilename();
            using FileStream stream = new(filename, FileMode.Create);
            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(img.Flip(FlipMode.Horizontal).Rotate(90)));
            encoder.Save(stream);
        }

        // Timestamped filename .png
        private string SnapShotFilename()
        {
            return Path.Combine(SnapShotPath, "MeadowSS_" + DateTime.Now.ToString("o").Replace(":", "") + ".png");
        }

        #endregion
    }
}
