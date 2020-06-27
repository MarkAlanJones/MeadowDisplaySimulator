using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

            var bm = new WriteableBitmap(240, 240, 96, 96, PixelFormats.Pbgra32, null);
            SPIDisplay.Source = bm;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeMeadow(SPIDisplay);
        }

        // Run the meadow app - may run forever !
        private void InitializeMeadow(Image meadowdisplay)
        {
            var F7 = new MeadowApp(meadowdisplay);
        }
    }
}
