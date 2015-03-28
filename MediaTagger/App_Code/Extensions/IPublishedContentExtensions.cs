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
    public static class IPublishedContentExtensions
    {

        public static string GetPropertyValueAsString(this IPublishedContent publishedContent, string propertyName)
        {
            string result = string.Empty;
            if (publishedContent.HasProperty(propertyName) && publishedContent.GetPropertyValue(propertyName) != null)
            {
                result = publishedContent.GetPropertyValue(propertyName).ToString();
            }
            return result;
        }

    }
}