using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Prototype1
{
    /// <summary>
    /// Used by the Activity Matrix to size each ActivityView in order to get a visually appealing grid 
    /// </summary>
    public class PercentageConverter : MarkupExtension, IValueConverter
    {
        private static PercentageConverter _instance;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //multiply by margin at end
            return System.Convert.ToDouble(value) * System.Convert.ToDouble(parameter) - (1 / System.Convert.ToDouble(parameter)) * 5;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new PercentageConverter());
        }
    }
}
