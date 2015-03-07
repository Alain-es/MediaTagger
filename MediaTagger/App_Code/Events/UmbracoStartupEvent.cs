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

using MediaTagger.Helpers;
using MediaTagger.EmbeddedAssembly;

using Examine;
using Examine.SearchCriteria;
using Examine.Providers;
using Examine.LuceneEngine;
using Examine.Config;
using UmbracoExamine;

using umbraco.NodeFactory;


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
            ExamineManager.Instance.IndexProviderCollection["ExternalIndexer"].GatheringNodeData += ExamineEvents_GatheringNodeData;
        }

        // Removing commas for medias' tags and path property
        private void ExamineEvents_GatheringNodeData(object sender, IndexingNodeDataEventArgs e)
        {   
            // Check whether it is a Media type Image
            if (e.IndexType != IndexTypes.Media || (e.IndexType == IndexTypes.Media && e.NodeId < 1) || e.Node.ExamineNodeTypeAlias() != "Image") return;

            // Remove the commas for the path property (that is necessary to filter queries to list all child documents of a particular parent)
            // Add as new search field
            e.Fields.Add("mediaTaggerImagePath", e.Fields["path"].Replace(",", " "));

            // Check whether the tags property exists and it is not empty
            var media = ApplicationContext.Current.Services.MediaService.GetById(e.NodeId);
            if (media == null || !media.HasProperty("tags") || string.IsNullOrWhiteSpace(media.GetValue("tags").ToString())) return;

            // Write the values back into the index without commas so they are indexed correctly
            e.Fields.Add("mediaTaggerImageTags", media.GetValue("tags").ToString().Replace(",", " "));
        }
    }
}