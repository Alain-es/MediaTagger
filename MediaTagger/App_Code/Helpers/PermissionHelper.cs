using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;

using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

namespace MediaTagger.Helpers
{
    public class PermissionHelper
    {

        public static IContent CheckContentPermissions(IUserService userService, IUser user, IContentService contentService, int nodeId, char[] permissionsToCheck = null)
        {
            if (nodeId == Constants.System.Root || nodeId == Constants.System.RecycleBinContent)
            {
                return null;
            }

            var contentItem = contentService.GetById(nodeId);

            if (contentItem == null)
            {
                return null;
            }

            var hasPathAccess = (nodeId == Constants.System.Root)
                                    ? UserExtensions.HasPathAccess(
                                        Constants.System.Root.ToInvariantString(),
                                        user.StartContentId,
                                        Constants.System.RecycleBinContent)
                                    : (nodeId == Constants.System.RecycleBinContent)
                                          ? UserExtensions.HasPathAccess(
                                              Constants.System.RecycleBinContent.ToInvariantString(),
                                              user.StartContentId,
                                              Constants.System.RecycleBinContent)
                                          : user.HasPathAccess(contentItem);
            if (!hasPathAccess)
            {
                return null;
            }

            if (permissionsToCheck == null || permissionsToCheck.Any() == false)
            {
                return contentItem ;
            }

            var permission = userService.GetPermissions(user, nodeId).FirstOrDefault();
            var allowed = true;
            foreach (var p in permissionsToCheck)
            {
                if (permission == null || permission.AssignedPermissions.Contains(p.ToString(CultureInfo.InvariantCulture)) == false)
                {
                    allowed = false;
                    break;
                }
            }

            return allowed ? contentItem : null; 
        }

        public static IMedia CheckMediaPermissions(IUserService userService, IUser user, IMediaService mediaService, int nodeId, char[] permissionsToCheck = null)
        {
            if (nodeId == Constants.System.Root || nodeId == Constants.System.RecycleBinMedia)
            {
                return null;
            }

            var mediaItem = mediaService.GetById(nodeId);

            if (mediaItem == null)
            {
                return null;
            }

            var hasPathAccess = (nodeId == Constants.System.Root)
                                    ? UserExtensions.HasPathAccess(
                                        Constants.System.Root.ToInvariantString(),
                                        user.StartMediaId,
                                        Constants.System.RecycleBinMedia)
                                    : (nodeId == Constants.System.RecycleBinMedia)
                                          ? UserExtensions.HasPathAccess(
                                              Constants.System.RecycleBinMedia.ToInvariantString(),
                                              user.StartMediaId,
                                              Constants.System.RecycleBinMedia)
                                          : user.HasPathAccess(mediaItem);

            return hasPathAccess ? mediaItem : null;
        }

    }
}