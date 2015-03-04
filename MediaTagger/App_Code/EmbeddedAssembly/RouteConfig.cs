using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Umbraco.Core.Logging;

namespace MediaTagger.EmbeddedAssembly
{

    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {

            const string pluginBasePath = "App_Plugins/MediaTagger";
            string url = string.Empty;

            url = string.Format("{0}{1}{2}", pluginBasePath, EmbeddedResourcePaths.Root, "{resource}");
            RouteTable.Routes.MapRoute(
                name: "MediaTagger.Root",
                url: url,
                defaults: new
                {
                    controller = "EmbeddedResource",
                    action = "GetRootResource"
                },
                namespaces: new[] { "MediaTagger.EmbeddedAssembly" }
            );
            //LogHelper.Info(typeof(RouteConfig), string.Format("Registering route : {0}", url).Replace("{", "{{").Replace("}", "}}"));

            url = string.Format("{0}{1}{2}", pluginBasePath, EmbeddedResourcePaths.Dashboard, "{resource}");
            RouteTable.Routes.MapRoute(
                name: "MediaTagger.Dashboard",
                url: url,
                defaults: new
                {
                    controller = "EmbeddedResource",
                    action = "GetDashboardResource"
                },
                namespaces: new[] { "MediaTagger.EmbeddedAssembly" }
            );
            //LogHelper.Info(typeof(RouteConfig), string.Format("Registering route : {0}", url).Replace("{", "{{").Replace("}", "}}"));

            url = string.Format("{0}{1}{2}", pluginBasePath, EmbeddedResourcePaths.Picker, "{resource}");
            RouteTable.Routes.MapRoute(
                name: "MediaTagger.Picker",
                url: url,
                defaults: new
                {
                    controller = "EmbeddedResource",
                    action = "GetPickerResource"
                },
                namespaces: new[] { "MediaTagger.EmbeddedAssembly" }
            );
            //LogHelper.Info(typeof(RouteConfig), string.Format("Registering route : {0}", url).Replace("{", "{{").Replace("}", "}}"));

            url = string.Format("{0}{1}{2}", pluginBasePath, EmbeddedResourcePaths.Dialog, "{resource}");
            RouteTable.Routes.MapRoute(
                name: "MediaTagger.Dialog",
                url: url,
                defaults: new
                {
                    controller = "EmbeddedResource",
                    action = "GetDialogResource"
                },
                namespaces: new[] { "MediaTagger.EmbeddedAssembly" }
            );
            //LogHelper.Info(typeof(RouteConfig), string.Format("Registering route : {0}", url).Replace("{", "{{").Replace("}", "}}"));

        }
    }

}