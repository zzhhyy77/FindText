using System.Windows;

namespace FindText.LocalControls
{

    public class TabControlDP
    {
        public static readonly DependencyProperty TabsDisabledProperty =
            DependencyProperty.RegisterAttached("TabsDisabled", typeof(bool), typeof(TabControlDP), new UIPropertyMetadata(false));

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
