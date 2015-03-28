using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using umbraco.cms.presentation;
using Umbraco.Core;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using Umbraco.Core.Persistence;
using Umbraco.Core.Logging;
using Umbraco.Web;

using MediaTagger.Helpers;
using MediaTagger.EmbeddedAssembly;

using Examine;
using Examine.SearchCriteria;
using Examine.Providers;
using Examine.LuceneEngine;
using Examine.Config;
using UmbracoExamine;

using umbraco.NodeFactory;

using MediaTagger.Extensions;
using MediaTagger.Helpers;

namespace MediaTagger.Installer
{
    public class UmbracoStartupEvent : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            base.ApplicationStarted(umbracoApplication, applicationContext);

            //// Register routes for embedded files
            //RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Hooks Examine's gathering data event in order to remove commas for medias' tags property 
            UmbracoHelper umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            ExamineManager.Instance.IndexProviderCollection["ExternalIndexer"].GatheringNodeData += (sender, e) => ExamineEvents_GatheringNodeData(sender, e, umbracoHelper);
        }

        // Removing commas for medias' tags and path property
        private void ExamineEvents_GatheringNodeData(object sender, IndexingNodeDataEventArgs e, UmbracoHelper umbracoHelper)
        {
            try
            {
                // Check whether it is a Media type Image
                if (e.IndexType != IndexTypes.Media || (e.IndexType == IndexTypes.Media && e.NodeId < 1) || e.Node.ExamineNodeTypeAlias() != "Image") return;

                // Remove the commas for the path property (that is necessary to filter queries to list all child documents of a particular parent)
                // Add as new search field
                if (e.Fields.ContainsKey("path") && !string.IsNullOrWhiteSpace(e.Fields["path"]))
                {
                    e.Fields.Add("mediaTaggerImagePath", e.Fields["path"].Replace(",", " "));
                }

                // Check whether the tags property exists and it is not empty
                if (e.Fields.ContainsKey("tags"))
                {
                    if (!string.IsNullOrWhiteSpace(e.Fields["tags"]))
                    {
                        e.Fields.Add("mediaTaggerImageTags", e.Fields["tags"].Replace(",", " "));
                    }
                }
                else
                {
                    object media = null;
                    if (umbracoHelper != null)
                    {
                        media = umbracoHelper.Media(e.NodeId);
                    }
                    if (media == null)
                    {
                        media = ApplicationContext.Current.Services.MediaService.GetById(e.NodeId);
                    }
                    string mediaTags = ContentHelper.GetPropertyValueAsString(media, "tags");
                    if (!string.IsNullOrWhiteSpace(mediaTags))
                    {
                        e.Fields.Add("mediaTaggerImageTags", mediaTags.Replace(",", " "));
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<UmbracoStartupEvent>("Error adding the image's tags to the search index.", ex);
            }
        }

    }
}