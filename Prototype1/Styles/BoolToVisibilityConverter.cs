using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Prototype1.Styles
{
    /// <summary>
    /// Converter to convert a boolean value to a visibility
    /// True = Visible
    /// False = Collapsed
    /// </summary>
    public class BoolToVisibilityConverter : MarkupExtension, IValueConverter
    {
        private static BoolToVisibilityConverter _instance;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new BoolToVisibilityConverter());
        }
    }
}
