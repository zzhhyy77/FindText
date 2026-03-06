using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FindText.UC
{
    /// <summary>
    /// NumberPicker.xaml 的交互逻辑
    /// </summary>
    public partial class NumericPicker : UserControl
    {
        public event DependencyPropertyChangedEventHandler OnNumericChanged;
        
        #region 附加属性

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value" , typeof(int), typeof(NumericPicker)
            , new PropertyMetadata((int)0, new PropertyChangedCallback(OnValuePropertyChanged)));

        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((NumericPicker)d).OnValuePropertyChanged(e);
        }

        protected virtual void OnValuePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue)
                return;
            //this.textboxValue.TextChanged -= textboxValue_TextChanged;
            this.textboxValue.Text = e.NewValue.ToString();
            //this.textboxValue.TextChanged += textboxValue_TextChanged;

            if (this.OnNumericChanged != null)
                this.OnNumericChanged(this, e);
        }

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }


        #region MaxLength (DependencyProperty)

        /// <summary>
        /// A description of the property.
        /// </summary>
        public int MaxLength
        {
            get { return (int)GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }
        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register("MaxLength", typeof(int), typeof(NumericPicker),
            new PropertyMetadata(2, new PropertyChangedCallback(OnMaxLengthChanged)));

        private static void OnMaxLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((NumericPicker)d).OnMaxLengthChanged(e);
        }

        protected virtual void OnMaxLengthChanged(DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion


        #region MaxValue (DependencyProperty)

        public static readonly DependencyProperty MaxValueProperty =  DependencyProperty.Register("MaxValue", typeof(int), typeof(NumericPicker),
            new PropertyMetadata(999, new PropertyChangedCallback(OnMaxValueChanged)));

        public int MaxValue
        {
            get { return (int)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        private static void OnMaxValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((NumericPicker)d).OnMaxValueChanged(e);
        }

        protected virtual void OnMaxValueChanged(DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion


        #region MinValue (DependencyProperty)

        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register("MinValue", typeof(int), typeof(NumericPicker),
            new PropertyMetadata(0, new PropertyChangedCallback(OnMinValueChanged)));

        public int MinValue
        {
            get { return (int)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        private static void OnMinValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((NumericPicker)d).OnMinValueChanged(e);
        }

        protected virtual void OnMinValueChanged(DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion


        #endregion 附加属性


        public NumericPicker()
        {
            InitializeComponent();
            //this.textboxValue.GotFocus += TextboxValue_GotFocus;
            this.textboxValue.PreviewTextInput += TextboxValue_PreviewTextInput;
            this.textboxValue.TextChanged += TextboxValue_TextChanged;
        }

        private void TextboxValue_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int tmp = 0;
            if (!int.TryParse(e.Text, out tmp))
            {
                e.Handled = true;
            }
            else
            {
                if (this.textboxValue.Text.Length > this.MaxLength && string.IsNullOrEmpty(this.textboxValue.SelectedText))
                {
                    e.Handled = true;
                }
            }
        }

        private void TextboxValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            int tmp = 0;
            if (int.TryParse(this.textboxValue.Text, out tmp))
                this.Value = tmp;
        }


        private void TextboxValue_GotFocus(object sender, RoutedEventArgs e)
        {
            this.textboxValue.SelectAll();
        }

        private void ButtonUp_Click(object sender, RoutedEventArgs e)
        {
            if (this.Value < this.MaxValue)
            {
                this.Value++;
                //if (OnValueChanged != null)
                //{
                //    this.OnValueChanged(this, new NumericPickerEventArgs() { Value = this.Value });
                //}
            }
            else
            {
                this.Value = this.MinValue;
            }
        }

        private void buttonDown_Click(object sender, RoutedEventArgs e)
        {
            if (this.Value > this.MinValue)
            {
                this.Value--;
                //if (OnValueChanged != null)
                //{
                //    this.OnValueChanged(this, new NumericPickerEventArgs() { Value = this.Value });
                //}
            }
            else
            {
                this.Value = this.MaxValue;
            }

        }

    }
}
