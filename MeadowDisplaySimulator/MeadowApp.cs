using Meadow.Foundation.Graphics;
using System;
using System.Windows.Media.Imaging;
using Color = Meadow.Foundation.Color;

namespace MeadowDisplaySimulator
{
    /// <summary>
    /// Put your Meadow code here - note this does not inherit from App<,>
    ///
    /// You need to initialize a FakeDisplay rather than your normal SPI one but otherwise any graphics related code can be tested from here
    /// </summary>
    public class MeadowApp
    {
        FakeDisplay display;
        GraphicsLibrary graphics;
        int displayWidth = 240;
        int displayHeight = 240;

        public MeadowApp(System.Windows.Controls.Image wpfimage)
        {
            display = new FakeDisplay(width: (uint)displayWidth, height: (uint)displayHeight, bitmap: (WriteableBitmap)wpfimage.Source);
            graphics = new GraphicsLibrary(display);
            graphics.Rotation = GraphicsLibrary.RotationType.Default;
            graphics.Clear(true);

            DrawMeadowLogo();
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
    }
}
