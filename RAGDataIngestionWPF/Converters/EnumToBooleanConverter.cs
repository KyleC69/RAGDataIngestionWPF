// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         EnumToBooleanConverter.cs
// Author: Kyle L. Crowder
// Build Num: 202423



using System.Globalization;
using System.Windows.Data;




namespace RAGDataIngestionWPF.Converters;





public sealed class EnumToBooleanConverter : IValueConverter
{
    public Type EnumType { get; init; }








    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is string enumString)
        {
            if (Enum.IsDefined(EnumType, value))
            {
                var enumValue = Enum.Parse(EnumType, enumString);

                return enumValue.Equals(value);
            }
        }

        return false;
    }








    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return parameter is string enumString ? Enum.Parse(EnumType, enumString) : null;

    }
}