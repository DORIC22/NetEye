using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace NetEye.res.Converters
{
    internal class NumberToRepairStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int statusNumber)
            {
                switch (statusNumber)
                {
                    case 0:
                        return "Принята";
                    case 1:
                        return "В работе";
                    case 2:
                        return "Архив";
                    case 3:
                        return "Отменена";
                }
            }

            throw new ArgumentException($"Status number {value} is not valid");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
