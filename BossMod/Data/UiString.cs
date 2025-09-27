using System.ComponentModel;
using System.Reflection;

namespace BossModReborn.Data
{
    internal enum UiString
    {

        [Description("This config is job-specific")]
        JobConfigTip

    }

    public static class EnumExtensions
    {
        private static readonly Dictionary<Enum, string> _enumDescriptions = [];

        public static string GetDescription(this Enum value)
        {
            if (_enumDescriptions.TryGetValue(value, out string? description))
            {
                return description;
            }

            FieldInfo? field = value.GetType().GetField(value.ToString());
            if (field == null)
            {
                _enumDescriptions.Add(value, value.ToString());
                return value.ToString();
            }

            DescriptionAttribute? attribute = field.GetCustomAttribute<DescriptionAttribute>();

            string descString = attribute == null ? value.ToString() : attribute.Description;
            _enumDescriptions.Add(value, descString);
            return descString;
        }
    }
}
