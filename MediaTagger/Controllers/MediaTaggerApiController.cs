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
        public IEnumerable<IMedia> GetMedias(string tags, int pageNumber = 0, int pageSize = 0)
        {
            IEnumerable<IMedia> result = new List<IMedia>();

            // Get the media items
            List<int> mediaIds = GetMediasByTags(tags).Select(x => x.Id).Skip(pageNumber * pageSize).Take(pageSize != 0 ? pageSize : 999).ToList<int>();
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
        public PagedResult<ContentItemBasic<ContentPropertyBasic, IMedia>> GetPagedMedias(string tags, int pageNumber = 0, int pageSize = 0)
        {
            int totalItems;
            IMedia[] items;

            items = GetMedias(tags, pageNumber, pageSize).ToArray();

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
        public bool GetPagedMediasHasMoreResults(string tags, int pageNumber, int pageSize)
        {
            bool result = false;
            if (!string.IsNullOrWhiteSpace(tags))
            {
                List<int> mediaIds = GetMediasByTags(tags).Select(x => x.Id).Skip((pageNumber + 1) * pageSize).Take(pageSize != 0 ? pageSize : 999).ToList<int>();
                if (mediaIds.Count > 0)
                {
                    result = true;
                }
            }
            return result;
        }


        private ISearchResults GetMediasByTags(string tags)
        {
            ISearchResults result = SearchResults.Empty();

            // Querying by path for user permissions or starting node:  https://our.umbraco.org/forum/developers/extending-umbraco/11659-Examine-quering-path

            // User examine to search media matching the selected tags
            var searchEngine = ExamineManager.Instance.SearchProviderCollection["ExternalSearcher"];
            var query = searchEngine.CreateSearchCriteria(UmbracoExamine.IndexTypes.Media, BooleanOperation.And).NodeTypeAlias("Image");
            if (!string.IsNullOrWhiteSpace(tags))
            {
                // Filter by tags
                query = query.And().GroupedOr(new string[] { "tags" }, tags.Split(',').ToArray());
            }
            result = searchEngine.Search(query.Compile());

            return result;
        }

    }
}




