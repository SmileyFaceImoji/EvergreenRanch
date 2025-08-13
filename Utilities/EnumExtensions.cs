using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EvergreenRanch.Utilities
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class EnumDisplayNameAttribute : Attribute
    {
        public string DisplayName { get; }

        public EnumDisplayNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }

    public static class EnumExtensions
    {
        public static string GetEnumDisplayName(this Enum enumValue)
        {
            if (enumValue == null) return string.Empty;

            var type = enumValue.GetType();
            var memberInfo = type.GetMember(enumValue.ToString());

            if (memberInfo.Length > 0)
            {
                var attr = memberInfo[0].GetCustomAttribute<EnumDisplayNameAttribute>();
                if (attr != null)
                {
                    return attr.DisplayName;
                }
            }

            return enumValue.ToString();
        }

        public static IEnumerable<SelectListItem> ToSelectList<TEnum>(this TEnum _, TEnum? selected = null) where TEnum : struct, Enum
        {
            return Enum.GetValues(typeof(TEnum))
                       .Cast<TEnum>()
                       .Select(e => new SelectListItem
                       {
                           Text = (e as Enum).GetEnumDisplayName(),
                           Value = e.ToString(),
                           Selected = selected.HasValue && selected.Value.Equals(e)
                       });
        }
    }
}
