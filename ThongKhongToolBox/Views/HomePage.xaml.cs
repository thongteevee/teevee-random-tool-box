using System.Windows;
using System.Windows.Controls;

namespace ThongKhongToolBox.Views
{
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private void BtnGotoFormatter_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw)
                mw.SelectTool("Formatter");
        }

        private void BtnGotoButtonTool_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw)
                mw.SelectTool("ButtonTool");
        }
    }
}