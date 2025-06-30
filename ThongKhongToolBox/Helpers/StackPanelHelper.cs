using System.Windows;
using System.Windows.Controls;

namespace ThongKhongToolBox.Helpers
{
    public static class StackPanelHelper
    {
        public static readonly DependencyProperty SpacingProperty =
            DependencyProperty.RegisterAttached(
                "Spacing",
                typeof(double),
                typeof(StackPanelHelper),
                new PropertyMetadata(0.0, OnSpacingChanged));

        public static double GetSpacing(DependencyObject obj)
            => (double)obj.GetValue(SpacingProperty);

        public static void SetSpacing(DependencyObject obj, double value)
            => obj.SetValue(SpacingProperty, value);


        private static void OnSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StackPanel panel)
            {
                ApplySpacing(panel, (double)e.NewValue);
                panel.Loaded -= Panel_Loaded;
                panel.Loaded += Panel_Loaded;
            }
        }

        private static void Panel_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is StackPanel panel)
                ApplySpacing(panel, GetSpacing(panel));
        }

        private static void ApplySpacing(StackPanel panel, double spacing)
        {
            for (int i = 0; i < panel.Children.Count; i++)
            {
                if (panel.Children[i] is FrameworkElement fe)
                {
                    var m = fe.Margin;
                    if (panel.Orientation == Orientation.Vertical)
                        fe.Margin = new Thickness(m.Left, i == 0 ? m.Top : spacing, m.Right, m.Bottom);
                    else
                        fe.Margin = new Thickness(i == 0 ? m.Left : spacing, m.Top, m.Right, m.Bottom);
                }
            }
        }
    }
}