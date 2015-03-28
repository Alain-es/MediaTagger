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
    public static class IMediaExtensions
    {

        public static string GetPropertyValueAsString(this IMedia media, string propertyName)
        {
            string result = string.Empty;
            if (media.HasProperty(propertyName) && media.GetValue(propertyName) != null)
            {
                result = media.GetValue(propertyName).ToString();
            }
            return result;
        }

    }
}