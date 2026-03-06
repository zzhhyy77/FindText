using System.Windows;

namespace FindText.LocalControls
{

    public class TabControlHelper
    {
        public static readonly DependencyProperty TabsDisabledProperty =
            DependencyProperty.RegisterAttached("TabsDisabled", typeof(bool), typeof(TabControlHelper), new UIPropertyMetadata(false));

        public static bool GetTabsDisabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(TabsDisabledProperty);
        }

        public static void SetTabsDisabled(DependencyObject obj, bool value)
        {
            obj.SetValue(TabsDisabledProperty, value);
        }

    }


}
