using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfUserControl;

namespace DirectXTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MyRenderer _rendereEngine;

        public MainWindow()
        {
            InitializeComponent();

            if (SlimDxControl.DirectXStatus != DirectXStatus.Available)
            {
                switch (SlimDxControl.DirectXStatus)
                {
                    case DirectXStatus.Unavailable_RemoteSession:
                        MessageBox.Show("DirectX not supported when using Remote Desktop", "Error intializing DirectX");
                        Environment.Exit(1);
                        break;
                    case DirectXStatus.Unavailable_LowTier:
                        MessageBox.Show("Insufficient graphics acceleration on this machine", "Error intializing DirectX");
                        Environment.Exit(1);
                        break;
                    case DirectXStatus.Unavailable_MissingDirectX:
                        MessageBox.Show("DirectX libraries are missing or need to be updated", "Error intializing DirectX");
                        Environment.Exit(1);
                        break;
                    default:
                        MessageBox.Show("Unable to start DirectX (reason unknown)", "Error intializing DirectX");
                        Environment.Exit(1);
                        break;
                }
            }

            _rendereEngine = new MyRenderer();

            Loaded += Window_Loaded;
            Closed += Window_Closed;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SlimDxControl.SetRenderEngine(_rendereEngine);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SlimDxControl.Shutdown();
        }

        private void SlimDxControl_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Add:
                case Key.OemPlus:
                    _rendereEngine.ZoomPlus();
                    break;
                case Key.Subtract:
                case Key.OemMinus:
                    _rendereEngine.ZoomMinus();
                    break;
                case Key.Up:
                    _rendereEngine.MapUp();
                    break;
                case Key.Down:
                    _rendereEngine.MapDown();
                    break;
                case Key.Left:
                    _rendereEngine.MapLeft();
                    break;
                case Key.Right:
                    _rendereEngine.MapRight();
                    break;
                case Key.A:
                    _rendereEngine.ShipLeft();
                    break;
                case Key.S:
                    _rendereEngine.ShipDown();
                    break;
                case Key.D:
                    _rendereEngine.ShipRight();
                    break;
                case Key.W:
                    _rendereEngine.ShipUp();
                    break;
                case Key.Q:
                    _rendereEngine.ShipRotationLeft();
                    break;
                case Key.E:
                    _rendereEngine.ShipRotationRight();
                    break;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point posPoint = e.GetPosition(SlimDxControl);

            _rendereEngine.SetMousePosition(posPoint);
        }
    }
}
