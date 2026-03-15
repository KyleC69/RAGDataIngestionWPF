// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//



using System.Globalization;
using System.Windows.Data;




namespace RAGDataIngestionWPF.Converters;





public sealed class EnumToBooleanConverter : IValueConverter
    {
    public Type EnumType { get; init; } = typeof(Enum);








    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
        if (parameter is string enumString)
            {
            if (value != null && Enum.IsDefined(EnumType, value))
                {
                var enumValue = Enum.Parse(EnumType, enumString);

                return enumValue.Equals(value);
                }
            }

        return false;
        }








    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
        return parameter is string enumString ? Enum.Parse(EnumType, enumString) : Binding.DoNothing;

        }
    }