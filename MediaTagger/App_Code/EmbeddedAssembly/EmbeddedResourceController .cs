using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Umbraco.Core.Logging;

namespace MediaTagger.EmbeddedAssembly
{
    // Get embedded resources files (.html, .js, .css, ...) 
    public class EmbeddedResourceController : Controller
    {

        public FileStreamResult GetRootResource(string resource)
        {
            return GetResource(EmbeddedResourcePaths.Root, resource);
        }

        public FileStreamResult GetDashboardResource(string resource)
        {
            return GetResource(EmbeddedResourcePaths.Dashboard, resource);
        }
        public FileStreamResult GetPickerResource(string resource)
        {
            return GetResource(EmbeddedResourcePaths.Picker, resource);
        }

        public FileStreamResult GetDialogResource(string resource)
        {
            return GetResource(EmbeddedResourcePaths.Dialog, resource);
        }

        private FileStreamResult GetResource(string url, string resource)
        {
            try
            {
                //LogHelper.Info(typeof(EmbeddedResourceController), string.Format("Getting the resource: {0}{1}", url, resource));

                // get this assembly
                Assembly assembly = typeof(EmbeddedResourceController).Assembly;

                // if resource can be found
                string resourceName = string.Format("{0}.{1}{2}{3}", assembly.GetName().Name, "App_Plugins", url.Replace("/", "."), resource);
                //LogHelper.Info(typeof(EmbeddedResourceController), string.Format("Getting the resource: {0}", resourceName));
                if (Assembly.GetExecutingAssembly().GetManifestResourceNames().Any(x => x.Equals(resourceName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return new FileStreamResult(assembly.GetManifestResourceStream(resourceName), this.GetMimeType(resourceName));
                }
                else
                {
                    LogHelper.Warn(typeof(EmbeddedResourceController), string.Format("Couldn't get the resource: {0}{1}", url, resource));
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(EmbeddedResourceController), string.Format("Couldn't get the resource: {0}{1}", url, resource), ex);
            }

            return null;
        }

        private string GetMimeType(string resource)
        {
            switch (Path.GetExtension(resource))
            {
                case ".html": return "text/html";
                case ".css": return "text/css";
                case ".js": return "text/javascript";
                case ".png": return "image/png";
                default: return "text";
            }
        }

    }
}