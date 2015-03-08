using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Umbraco.Core;
using Umbraco.Core.Models;

namespace MediaTagger.Installer
{
    public partial class Setup : System.Web.UI.UserControl
    {
        private const int TagsDataTypeID = 1041;
        private const string PropertyName = "Tags";
        private const string PropertyAlias = "tags";

        protected void Page_Load(object sender, EventArgs e)
        {
            // Add a new Tags property to the 'Image' media type
            IMediaType documentType = ApplicationContext.Current.Services.ContentTypeService.GetMediaType("Image");

            // Check whether the property already exists
            if (!documentType.PropertyTypeExists(PropertyAlias))
            {
                // Find the DataTypeDefinition that the PropertyType should be based on
                var dataTypeDefinition = ApplicationContext.Current.Services.DataTypeService.GetDataTypeDefinitionById(TagsDataTypeID);
                // Create the new property type
                var propertyType = new PropertyType(dataTypeDefinition)
                {
                    Alias = PropertyAlias,
                    Name = PropertyName,
                    SortOrder = 1,
                    Mandatory = false,
                    ValidationRegExp = string.Empty,
                    Description = string.Empty
                };
                // Add the property to the document type
                documentType.AddPropertyType(propertyType, "Image");
                // Save changes
                ApplicationContext.Current.Services.ContentTypeService.Save(documentType);
            }

        }
    }
}