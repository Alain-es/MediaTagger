using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;

using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace MediaTagger.Extensions
{
    public static class IContentExtensions
    {

        public static string GetPropertyValueAsString(this IContent content, string propertyName)
        {
            string result = string.Empty;
            if (content.HasProperty(propertyName) && content.GetValue(propertyName) != null)
            {
                result = content.GetValue(propertyName).ToString();
            }
            return result;
        }

    }
}