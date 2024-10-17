using System.ComponentModel;
using System.Globalization;

namespace KeystoneLibrary.Services
{
    public class DefaultDateTimeFormat : TypeConverter 
    {
        // Overrides the CanConvertFrom method of TypeConverter.
        // The ITypeDescriptorContext interface provides the context for the
        // conversion. Typically, this interface is used at design time to 
        // provide information about the design-time container.
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
        {

            if (sourceType == typeof(string)) 
                return true;
            
            return base.CanConvertFrom(context, sourceType);
        }

        // Overrides the ConvertFrom method of TypeConverter.
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
        {
            if (value is string) 
            {
                //dd/mm/yyyy HH:mm
                if (DateTime.TryParse(((string)value), new CultureInfo("en-GB"), DateTimeStyles.None, out DateTime date))
                    return date;
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}