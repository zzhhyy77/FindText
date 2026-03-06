using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace FindText.Utils
{
    internal class UiHelper
    {
        /// <summary>
        /// 适合于需要按视觉顺序遍历元素的场景
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static IEnumerable<DependencyObject> EnumerateVisualChildren(DependencyObject parent)
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                yield return child;
                foreach (DependencyObject subChild in EnumerateVisualChildren(child))
                {
                    yield return subChild;
                }
            }
        }

        //适合于需要按逻辑结构遍历元素的场景。例如，当你需要根据父子关系而不是视觉顺序来处理元素时
        public void PrintLogicalTree(DependencyObject parent, int level = 0)
        {
            if (parent == null) return;

            string indent = new string(' ', level * 2);
            string name = parent.GetValue(FrameworkElement.NameProperty)?.ToString() ?? parent.GetType().Name;
            Console.WriteLine($"{indent}{name}");

            foreach (object child in LogicalTreeHelper.GetChildren(parent))
            {
                if (child is DependencyObject depChild)
                {
                    PrintLogicalTree(depChild, level + 1);
                }
            }
        }


        public void UpdateBindingSource(DependencyObject parent, int level = 0)
        {
            if (parent == null) return;
            string indent = new string(' ', level * 2);

            var data = parent.GetValue(FrameworkElement.DataContextProperty);
            if(data != null && data.GetType() == typeof(TextCache))
            {
                //Console.WriteLine($"{indent}{name}");
            }


            foreach (object child in LogicalTreeHelper.GetChildren(parent))
            {
                if (child is DependencyObject depChild)
                {
                    PrintLogicalTree(depChild, level + 1);
                }
            }
        }

    }
}

