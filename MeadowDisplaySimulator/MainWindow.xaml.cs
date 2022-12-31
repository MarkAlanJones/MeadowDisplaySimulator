using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MeadowDisplaySimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Debug.WriteLine($"Thread {Dispatcher.Thread.ManagedThreadId}");
            var bm = new WriteableBitmap((int)SPIDisplay.MinWidth, (int)SPIDisplay.MinHeight, 96, 96, PixelFormats.Bgra32, null);
            SPIDisplay.Source = bm;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeMeadow(SPIDisplay);
        }

        // Run the meadow app - may run forever !
        private void InitializeMeadow(Image meadowdisplay)
        {
            // Run the meadow code on a background thread, it will have to dispatch to UI to update bitmap
            Task.Factory.StartNew(() => new MeadowApp(meadowdisplay));
        }

        // Turn off HW acceleration so Writeable Bitmap updates - at the WPF window level
        // https://9to5answer.com/how-does-one-disable-hardware-acceleration-in-wpf
        // issue on Windows 11 - Intel(R) Iris(R) Xe Graphics
        protected override void OnSourceInitialized(EventArgs e)
        {
            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;

            if (hwndSource != null)
                hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;

            base.OnSourceInitialized(e);
        }
    }   
}
