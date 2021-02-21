using Meadow.Foundation.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media.Imaging;
using Color = Meadow.Foundation.Color;

namespace MeadowDisplaySimulator
{
    /// <summary>
    /// Put your Meadow code here - note this does not inherit from App<,>
    ///
    /// You need to initialize a FakeDisplay rather than your normal SPI one but otherwise any graphics related code can be tested from here
    /// The initialize (where the bitmap is given to the display driver) must be on the UI thread.
    /// </summary>
    public class MeadowApp
    {
        FakeDisplay display;
        GraphicsLibrary graphics;
        int displayWidth = 240;
        int displayHeight = 240;

        Random rand = new Random();

        public MeadowApp(System.Windows.Controls.Image wpfimage)
        {
            // initialize on UI thread - pass bitmap to display initializer
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                Initialize((WriteableBitmap)wpfimage.Source);
            }));

            int timms = BenchCircles(50);
            Thread.Sleep(1000);
            timms += BenchLines(100);
            Thread.Sleep(1000);
            timms += BenchPix(100);
            Thread.Sleep(1000);
            timms += BenchRect(50);
            Thread.Sleep(1000);
            timms += BenchTriangle(50);
            Thread.Sleep(1000);
            timms += BenchText(50);
            Thread.Sleep(1000);

            Console.WriteLine($"Done {timms}ms");
            DrawMeadowLogo();

        }

        void Initialize(WriteableBitmap wbm)
        {
            Console.WriteLine("Initializing...");

            display = new FakeDisplay(width: displayWidth, height: displayHeight, bitmap: wbm);
            display.IgnoreOutOfBoundsPixels = true;
            graphics = new GraphicsLibrary(display);
            graphics.Rotation = GraphicsLibrary.RotationType.Default;
            graphics.Clear(true);
        }

        int BenchCircles(int num)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            graphics.Stroke = 1;
            graphics.Clear(true);
            for (int i = 1; i < num; i++)
            {
                graphics.DrawCircle(rand.Next(displayWidth), rand.Next(displayHeight), rand.Next(displayHeight / 2) + 1, RandColor(), false);
                graphics.Show();
            }
            int empty = (int)stopWatch.Elapsed.TotalMilliseconds;
            Console.WriteLine($"{num} Circles {empty}ms");
            stopWatch.Restart();

            graphics.Clear(true);
            for (int i = 1; i < num; i++)
            {
                graphics.DrawCircle(rand.Next(displayWidth), rand.Next(displayHeight), rand.Next(displayHeight / 2) + 1, RandColor(), true);
                graphics.Show();
            }
            int full = (int)stopWatch.Elapsed.TotalMilliseconds;
            Console.WriteLine($"{num} Circles filled {full}ms");
            stopWatch.Restart();

            stopWatch.Stop();

            return empty + full;
        }

        int BenchLines(int num)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            graphics.Stroke = 1;
            graphics.Clear(true);
            for (int i = 1; i < num; i++)
            {
                graphics.DrawLine(rand.Next(displayWidth), rand.Next(displayHeight), rand.Next(displayWidth), rand.Next(displayHeight), RandColor());
                graphics.Show();
            }
            int l1 = (int)stopWatch.Elapsed.TotalMilliseconds;
            Console.WriteLine($"{num} lines {l1}ms");
            stopWatch.Restart();

            graphics.Stroke = 2;
            graphics.Clear(true);
            for (int i = 1; i < num; i++)
            {
                graphics.DrawLine(rand.Next(displayWidth), rand.Next(displayHeight), rand.Next(displayWidth), rand.Next(displayHeight), RandColor());
                graphics.Show();
            }
            int l2 = (int)stopWatch.Elapsed.TotalMilliseconds;
            Console.WriteLine($"{num} 2x lines {l2}ms");
            stopWatch.Restart();

            graphics.Stroke = 3;
            graphics.Clear(true);
            for (int i = 1; i < num; i++)
            {
                graphics.DrawLine(rand.Next(displayWidth), rand.Next(displayHeight), rand.Next(displayWidth), rand.Next(displayHeight), RandColor());
                graphics.Show();
            }
            int l3 = (int)stopWatch.Elapsed.TotalMilliseconds;
            Console.WriteLine($"{num} 3x lines {l3}ms");
            stopWatch.Restart();

            graphics.Stroke = 2;
            graphics.Clear(true);
            for (int i = 1; i < num; i++)
            {
                graphics.DrawHorizontalLine(rand.Next(displayWidth), rand.Next(displayHeight), rand.Next(displayWidth), RandColor());
                graphics.Show();
            }
            int lh = (int)stopWatch.Elapsed.TotalMilliseconds;
            Console.WriteLine($"{num} horz lines {lh}ms");
            stopWatch.Restart();

            graphics.Stroke = 2;
            graphics.Clear(true);
            for (int i = 1; i < num; i++)
            {
                graphics.DrawVerticalLine(rand.Next(displayWidth), rand.Next(displayHeight), rand.Next(displayWidth), RandColor());
                graphics.Show();
            }
            int lv = (int)stopWatch.Elapsed.TotalMilliseconds;
            Console.WriteLine($"{num} vert lines {lh}ms");
            stopWatch.Restart();

            stopWatch.Stop();

            return l1 + l2 + l3 + lh + lv;
        }

        int BenchPix(int num)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            graphics.Stroke = 1;
            graphics.Clear(true);
            for (int i = 1; i < num; i++)
            {
                graphics.DrawPixel(rand.Next(displayWidth), rand.Next(displayHeight), RandColor());
                graphics.Show();
            }
            int p1 = (int)stopWatch.Elapsed.TotalMilliseconds;
            Console.WriteLine($"{num} Pixel {p1}ms");
            stopWatch.Stop();

            return p1;
        }

        int BenchRect(int num)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            graphics.Stroke = 1;
            graphics.Clear(true);
            for (int i = 1; i < num; i++)
            {
                graphics.DrawRectangle(rand.Next(displayWidth), rand.Next(displayHeight), rand.Next(displayWidth), rand.Next(displayHeight), RandColor(), false);
                graphics.Show();
            }
            int empty = (int)stopWatch.Elapsed.TotalMilliseconds;
            Console.WriteLine($"{num} Rectangle {empty}ms");
            stopWatch.Restart();

            graphics.Clear(true);
            for (int i = 1; i < num; i++)
            {
                graphics.DrawRectangle(rand.Next(displayWidth), rand.Next(displayHeight), rand.Next(displayWidth), rand.Next(displayHeight), RandColor(), true);
                graphics.Show();
            }
            int filled = (int)stopWatch.Elapsed.TotalMilliseconds;
            Console.WriteLine($"{num} Rectangle Filled {filled}ms");
            stopWatch.Stop();

            return empty + filled;
        }

        int BenchTriangle(int num)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            graphics.Stroke = 1;
            graphics.Clear(true);
            for (int i = 1; i < num; i++)
            {
                graphics.DrawTriangle(rand.Next(displayWidth), rand.Next(displayHeight),
                                      rand.Next(displayWidth), rand.Next(displayHeight),
                                      rand.Next(displayWidth), rand.Next(displayHeight), RandColor(), false);
                graphics.Show();
            }
            int empty = (int)stopWatch.Elapsed.TotalMilliseconds;
            Console.WriteLine($"{num} Triangles {empty}ms");
            stopWatch.Restart();

            graphics.Clear(true);
            for (int i = 1; i < num; i++)
            {
                graphics.DrawTriangle(rand.Next(displayWidth), rand.Next(displayHeight),
                                      rand.Next(displayWidth), rand.Next(displayHeight),
                                      rand.Next(displayWidth), rand.Next(displayHeight), RandColor(), true);
                graphics.Show();
            }
            int filled = (int)stopWatch.Elapsed.TotalMilliseconds;
            Console.WriteLine($"{num} Triangles Filled {filled}ms");
            stopWatch.Stop();

            return empty + filled;
        }


        int BenchText(int num)
        {
            List<FontBase> AvailbleFonts = new List<FontBase>() { new Font8x8(), new Font8x12(), new Font4x8(), new Font12x20(), new Font12x16() };

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            graphics.Stroke = 1;
            int f1 = 0;

            foreach (var font in AvailbleFonts)
            {
                graphics.CurrentFont = font;
                graphics.Clear(true);
                for (int i = 1; i < num / 3; i++)
                {
                    graphics.DrawText(rand.Next(displayWidth), rand.Next(displayHeight),
                                      "Meadow F7", RandColor(), GraphicsLibrary.ScaleFactor.X1);
                    graphics.DrawText(rand.Next(displayWidth), rand.Next(displayHeight),
                                      "Meadow F7", RandColor(), GraphicsLibrary.ScaleFactor.X2);
                    graphics.DrawText(rand.Next(displayWidth), rand.Next(displayHeight),
                                      "Meadow F7", RandColor(), GraphicsLibrary.ScaleFactor.X3);
                    graphics.Show();
                }
                int f0 = (int)stopWatch.Elapsed.TotalMilliseconds;
                f1 += f0;
                Console.WriteLine($"{num} Text Font{font.Width}x{font.Height} {f0}ms");
                stopWatch.Restart();
            }

            return f1;
        }

        void DrawMeadowLogo()
        {
            graphics.Clear();

            var bottom = 200;
            var height = 54;

            graphics.DrawLine(4, bottom, 44, bottom - height, Color.White);
            graphics.DrawLine(4, bottom, 44, bottom, Color.White);
            graphics.DrawLine(44, 200 - height, 64, bottom - height / 2, Color.White);
            graphics.DrawLine(44, bottom, 84, bottom - height, Color.White);
            graphics.DrawLine(84, bottom - height, 124, bottom, Color.White);

            for (int tree = 0; tree < 3; tree++)
            {
                for (int branch = 0; branch < 3; branch++)
                {
                    graphics.DrawLine(64 + (tree * 15), bottom - (branch * 6), 69 + (tree * 15), bottom - (6 * branch) - 5, Color.ForestGreen);
                    graphics.DrawLine(69 + (tree * 15), bottom - (6 * branch) - 5, 74 + (tree * 15), bottom - (branch * 6), Color.ForestGreen);
                }
            }

            //mountain fill
            int lineWidth, x, y;

            for (int i = 0; i < height - 1; i++)
            {
                y = bottom - i;
                x = 5 + i * 20 / 27;

                //fill bottom of mountain
                if (i < height / 2)
                {
                    lineWidth = 38;
                    graphics.DrawLine(x, y, x + lineWidth, y, Color.YellowGreen);
                }
                else
                {   //fill top of mountain
                    lineWidth = 38 - (i - height / 2) * 40 / 27;
                    graphics.DrawLine(x, y, x + lineWidth, y, Color.YellowGreen);
                }
            }

            graphics.CurrentFont = new Font8x12();
            graphics.DrawText(4, bottom + 10, "meadow", Color.DimGray, GraphicsLibrary.ScaleFactor.X2);
            graphics.DrawText(112, bottom + 18, "F7", Color.DimGray, GraphicsLibrary.ScaleFactor.X1);
            graphics.Show();

        }

        Color RandColor()
        {
            return Color.FromRgb(rand.Next(255), rand.Next(255), rand.Next(255));
        }
    }
}
