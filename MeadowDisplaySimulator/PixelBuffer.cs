using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using System;

namespace MeadowDisplaySimulator
{
    internal class PixelBuffer : IPixelBuffer
    {
        private readonly int width;
        private readonly int height;
        private byte[] pixels;
        private Color[] c; // Our array of pixels holds color info - meadow definition
        private const int bpp = 4;

        public int Width => width;

        public int Height => height;

        public ColorType ColorMode => ColorType.Format32bppRgba8888;

        public int BitDepth => 24;

        public int ByteCount => pixels.Length;

        public byte[] Buffer  // A Byte buffer must be converted from the Color buffer to RGB
        {
            get
            {
                CtoP();
                return pixels;
            }
        }

        /// <summary>
        /// Create a Pixel Buffer the size of the screen
        /// </summary>
        /// <param name="w">screen width</param>
        /// <param name="h">screen height</param>
        public PixelBuffer(int w, int h)
        {
            width = w;
            height = h;

            Clear();
        }

        public void Clear()
        {
            pixels = new byte[Width * Height * bpp];
            c = new Color[Width * Height];
            Fill(Color.Black);
        }

        public void Fill(Color color)
        {
            Fill(0, 0, Width, Height, color);
        }

        public void Fill(int originX, int originY, int width, int height, Color color)
        {
            for (int y = originY; y < Math.Min(Height, originY + height); y++)
                for (int x = originX; x < Math.Min(Width, originX + width); x++)
                {
                    c[x * Width + y] = color;
                }
        }

        public Color GetPixel(int x, int y) => c[x * Width + y];

        public void InvertPixel(int x, int y)
        {
            throw new System.NotImplementedException();
        }

        public void SetPixel(int x, int y, Color color)
        {
            c[x * Width + y] = color;
        }

        public void WriteBuffer(int originX, int originY, IPixelBuffer buffer)
        {
            for (int y = 0; y < buffer.Height; y++)
                for (int x = 0; x < buffer.Width; x++)
                {
                    c[(x + originX) * Width + (y + originY)] = buffer.GetPixel(x, y);
                }
        }

        private void CtoP()
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var pc = (x * Width + y);
                    var b = pc * bpp;

                    // B G R A
                    pixels[b] = c[pc].B;
                    pixels[b + 1] = c[pc].G;
                    pixels[b + 2] = c[pc].R;
                    pixels[b + 3] = c[pc].A;
                }
        }

    }
}
