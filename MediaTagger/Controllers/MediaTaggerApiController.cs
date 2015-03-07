using System;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Dynamic;
using System.ComponentModel;
using System.Text;

using System.Web.Http;
using System.Net.Http;

using umbraco;
using Umbraco.Core;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.IO;
using umbraco.BusinessLogic.Actions;
using umbraco.BusinessLogic;
using Umbraco.Web;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.Models.ContentEditing;
using Examine;
using Examine.Providers;
using Examine.LuceneEngine;
using Examine.SearchCriteria;

using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Web.Hosting;
using System.IO;

using MediaTagger.Helpers;

namespace MediaTagger.Controllers.Api
{
    [PluginController("MediaTagger")]
    [IsBackOffice]
    public class MediaTaggerApiController : UmbracoAuthorizedJsonController
    {

        /// <summary>
        /// Retrieve all Medias that match the tags
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public IEnumerable<IMedia> GetMedias(string tags, int pageNumber = 0, int pageSize = 0, int startMediaId = 0, bool restrictPrivateFolder = false)
        {
            IEnumerable<IMedia> result = new List<IMedia>();

            // Get the media items
            List<int> mediaIds = GetMediasByTags(tags, startMediaId, restrictPrivateFolder).Select(x => x.Id).Skip(pageNumber * pageSize).Take(pageSize != 0 ? pageSize : 999).ToList<int>();
            if (mediaIds.Count > 0)
            {
                //IEnumerable<IMedia> medias = ApplicationContext.Services.MediaService.GetByIds(mediaIds);

                //// Check whether the logged in user got browsing rights on medias
                //foreach (var media in medias)
                //{
                //    var mediaChecked = PermissionHelper.CheckMediaPermissions(ApplicationContext.Current.Services.UserService, UmbracoContext.Current.Security.CurrentUser, ApplicationContext.Current.Services.MediaService, media.Id, new char[] { ActionBrowse.Instance.Letter });
                //    if (mediaChecked != null)
                //        result.Add(mediaChecked);
                //}

                result = ApplicationContext.Services.MediaService.GetByIds(mediaIds);
            }
            return result;
        }

        [System.Web.Http.HttpGet]
        public PagedResult<ContentItemBasic<ContentPropertyBasic, IMedia>> GetPagedMedias(string tags, int pageNumber = 0, int pageSize = 0, int startMediaId = 0, bool restrictPrivateFolder = false)
        {
            int totalItems;
            IMedia[] items;

            items = GetMedias(tags, pageNumber, pageSize, startMediaId, restrictPrivateFolder).ToArray();

            totalItems = items.Length;
            if (totalItems == 0)
            {
                return new PagedResult<ContentItemBasic<ContentPropertyBasic, IMedia>>(0, 0, 0);
            }

            var pagedResult = new PagedResult<ContentItemBasic<ContentPropertyBasic, IMedia>>(totalItems, pageNumber, pageSize);
            pagedResult.Items = items.Select(Mapper.Map<IMedia, ContentItemBasic<ContentPropertyBasic, IMedia>>);

            return pagedResult;
        }

        [System.Web.Http.HttpGet]
        public bool GetPagedMediasHasMoreResults(string tags, int pageNumber, int pageSize, int startMediaId = 0, bool restrictPrivateFolder = false)
        {
            bool result = false;
            List<int> mediaIds = GetMediasByTags(tags, startMediaId, restrictPrivateFolder).Select(x => x.Id).Skip((pageNumber + 1) * pageSize).Take(pageSize != 0 ? pageSize : 999).ToList<int>();
            if (mediaIds.Count > 0)
            {
                result = true;
            }
            return result;
        }


        private ISearchResults GetMediasByTags(string tags, int startMediaId, bool restrictPrivateFolder)
        {
            ISearchResults result = SearchResults.Empty();

            // Get the current user's start media Id
            var currentUserStartMediaId = UmbracoContext.Current.Security.CurrentUser.StartMediaId;
            if (startMediaId == 0)
            {
                startMediaId = currentUserStartMediaId;
            }
            // Check whether the selected startMediaId node is underneath the current user StartMediaId
            else
            {
                var startMedia = UmbracoContext.Current.Application.Services.MediaService.GetById(startMediaId);
                if (startMedia != null)
                {
                    if (!startMedia.Path.Contains(startMediaId.ToString()))
                    {
                        startMediaId = currentUserStartMediaId;
                    }
                }
            }

            // User examine to search media matching the selected tags
            var searchEngine = ExamineManager.Instance.SearchProviderCollection["ExternalSearcher"];
            var query = searchEngine.CreateSearchCriteria(UmbracoExamine.IndexTypes.Media, BooleanOperation.And).NodeTypeAlias("Image");
            query = query.And().Field("mediaTaggerImagePath", startMediaId.ToString());
            if (!string.IsNullOrWhiteSpace(tags))
            {
                // Filter by tags
                query = query.And().GroupedOr(new string[] { "mediaTaggerImageTags" }, tags.Split(',').ToArray());
            }
            result = searchEngine.Search(query.Compile());

            return result;
        }

    }
}




