using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace _3DVisualizerNI.Helpers
{
    public static class EnumHelper
    {
        public static string Description(this Enum eValue)
        {
            var nAttributes = eValue.GetType().GetField(eValue.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (nAttributes.Any())
                return (nAttributes.First() as DescriptionAttribute).Description;

            // If no description is found, the least we can do is replace underscores with spaces
            TextInfo oTI = CultureInfo.CurrentCulture.TextInfo;
            return oTI.ToTitleCase(oTI.ToLower(eValue.ToString().Replace("_", " ")));
        }

        public static IEnumerable<KeyValuePair<Enum, string>> GetAllValuesAndDescriptions(Type t)
        {
            if (!t.IsEnum)
                throw new ArgumentException("t must be an enum type");

            return Enum.GetValues(t).Cast<Enum>().Select((e) => new KeyValuePair<Enum, string>(e,e.Description()));
        }
    }

    [ValueConversion(typeof(Enum), typeof(IEnumerable<KeyValuePair<Enum, string>>))]
    public class EnumToCollectionConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {        
            return EnumHelper.GetAllValuesAndDescriptions(value.GetType());
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
