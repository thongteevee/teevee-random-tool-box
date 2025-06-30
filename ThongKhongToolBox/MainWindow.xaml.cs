using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using ThongKhongToolBox.Views;

namespace ThongKhongToolBox
{
    public partial class MainWindow : Window
    {
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;
        private const int WM_NCLBUTTONDOWN = 0xA1;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr SendMessage(
            IntPtr hWnd, int Msg, int wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool ReleaseCapture();

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (ToolHost == null)
            {
                MessageBox.Show(
                    "Frame named 'ToolHost' not found in XAML. " +
                    "Check x:Name spelling.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            ToolHost.Navigate(new HomePage());
            ToolListBox.SelectedIndex = 0;
            UpdateToolbarVisibility("Home");
            ToolListBox.SelectionChanged += ToolListBox_SelectionChanged;
        }

        private void UpdateToolbarVisibility(string tag)
        {
            if (tag == "Home")
            {
                toolsMenuBorder.Visibility = Visibility.Collapsed;
                MenuColumn.Width = new GridLength(0);
            }
            else
            {
                toolsMenuBorder.Visibility = Visibility.Visible;
                MenuColumn.Width = new GridLength(200);
            }
        }

        public void SelectTool(string tag)
        {
            var item = ToolListBox.Items
                                 .OfType<ListBoxItem>()
                                 .FirstOrDefault(x => (string)x.Tag == tag);
            if (item != null)
                ToolListBox.SelectedItem = item;
        }

        private void ToolListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ToolHost == null) return;

            if (ToolListBox.SelectedItem is ListBoxItem item)
            {
                string tag = item.Tag as string ?? "Home";
                UpdateToolbarVisibility(tag);
                switch (tag)
                {
                    case "Formatter":
                        ToolHost.Navigate(new FormatterPage());
                        Title = "Date and SSN formatter";
                        break;

                    case "ButtonTool":
                        ToolHost.Navigate(new ButtonToolPage());
                        Title = "Button tool";
                        break;

                    default:
                        ToolHost.Navigate(new HomePage());
                        Title = "ThongKhongToolBox";
                        break;
                }
            }
        }

        private void MinimizeButton_Click(object s, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void MaxRestoreButton_Click(object s, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                MaxRestoreButton.Content = "☐";
            }
            else
            {
                WindowState = WindowState.Maximized;
                MaxRestoreButton.Content = "❐";
            }
        }

        private void CloseButton_Click(object s, RoutedEventArgs e)
            => Close();
        private void ResizeBorder_MouseDown(object s, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            var pos = e.GetPosition(this);
            int dir = GetResizeDirection(pos);
            if (dir == 0) return;
            ReleaseCapture();
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            SendMessage(hwnd, WM_NCLBUTTONDOWN, dir, IntPtr.Zero);
        }

        private void ResizeBorder_MouseMove(object s, MouseEventArgs e)
        {
            var pos = e.GetPosition(this);
            int dir = GetResizeDirection(pos);
            Cursor = dir switch
            {
                HTLEFT or HTRIGHT => Cursors.SizeWE,
                HTTOP or HTBOTTOM => Cursors.SizeNS,
                HTTOPLEFT or HTBOTTOMRIGHT => Cursors.SizeNWSE,
                HTTOPRIGHT or HTBOTTOMLEFT => Cursors.SizeNESW,
                _ => Cursors.Arrow
            };
        }

        private void ResizeBorder_MouseLeave(object s, MouseEventArgs e)
            => Cursor = Cursors.Arrow;

        private int GetResizeDirection(Point p)
        {
            const int grip = 8;
            if (p.X <= grip && p.Y <= grip) return HTTOPLEFT;
            if (p.X >= ActualWidth - grip && p.Y <= grip) return HTTOPRIGHT;
            if (p.X <= grip && p.Y >= ActualHeight - grip) return HTBOTTOMLEFT;
            if (p.X >= ActualWidth - grip && p.Y >= ActualHeight - grip)
                return HTBOTTOMRIGHT;
            if (p.X <= grip) return HTLEFT;
            if (p.X >= ActualWidth - grip) return HTRIGHT;
            if (p.Y <= grip) return HTTOP;
            if (p.Y >= ActualHeight - grip) return HTBOTTOM;
            return 0;
        }
    }
}