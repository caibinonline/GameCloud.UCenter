﻿using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GameCloud.Database.Adapters;
using GameCloud.Manager.PluginContract.Requests;
using GameCloud.Manager.PluginContract.Responses;
using GameCloud.UCenter.Common.Settings;
using GameCloud.UCenter.Database;
using GameCloud.UCenter.Database.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace GameCloud.UCenter.Api.Manager.ApiControllers
{
    /// <summary>
    /// Provide a controller for users.
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class AppsController : ManagerApiControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppsController" /> class.
        /// </summary>
        /// <param name="ucenterDb">Indicating the database context.</param>
        /// <param name="ucenterventDb">Indicating the database context.</param>
        /// <param name="settings">Indicating the settings.</param>
        [ImportingConstructor]
        public AppsController(
            UCenterDatabaseContext ucenterDb,
            UCenterEventDatabaseContext ucenterventDb,
            Settings settings)
            : base(ucenterDb, ucenterventDb, settings)
        {
        }

        /// <summary>
        /// Get app list.
        /// </summary>
        /// <param name="request">Indicating the count.</param>
        /// <param name="token">Indicating the cancellation token.</param>
        /// <returns>Async return user list.</returns>
        [Route("api/ucenter/apps")]
        public async Task<PluginPaginationResponse<AppEntity>> Post([FromBody]SearchRequestInfo<AppEntity> request, CancellationToken token)
        {
            if (request.Method == PluginRequestMethod.Update)
            {
                var updateRawData = request.RawData;
                if (updateRawData != null)
                {
                    var filterDefinition = Builders<AppEntity>.Filter.Where(e => e.Id == updateRawData.Id);
                    var updateDefinition = Builders<AppEntity>.Update
                        .Set(e => e.Name, updateRawData.Name)
                        .Set(e => e.AppSecret, updateRawData.AppSecret);
                    await this.UCenterDatabase.Apps.UpdateOneAsync(updateRawData, filterDefinition, updateDefinition, token);
                }
            }
            else if (request.Method == PluginRequestMethod.Delete)
            {
                var deleteRawData = request.RawData;
                if (deleteRawData != null)
                {
                    await this.UCenterDatabase.AppConfigurations.DeleteAsync(v => v.Id == deleteRawData.Id, token);
                    await this.UCenterDatabase.Apps.DeleteAsync(v => v.Id == deleteRawData.Id, token);
                }
            }

            string keyword = request.GetParameterValue<string>("keyword");
            int page = request.GetParameterValue<int>("page", 1);
            int count = request.GetParameterValue<int>("pageSize", 10);

            Expression<Func<AppEntity, bool>> filter = null;

            if (!string.IsNullOrEmpty(keyword))
            {
                filter = a => a.Name.Contains(keyword);
            }

            var total = await this.UCenterDatabase.Apps.CountAsync(filter, token);

            IQueryable<AppEntity> queryable = this.UCenterDatabase.Apps.Collection.AsQueryable();
            if (filter != null)
            {
                queryable = queryable.Where(filter);
            }
            queryable = queryable.OrderByDescending(a => a.CreatedTime);

            var result = queryable.Skip((page - 1) * count).Take(count).ToList();

            // todo: add orderby support.
            var model = new PluginPaginationResponse<AppEntity>
            {
                Page = page,
                PageSize = count,
                Raws = result,
                Total = total
            };

            return model;
        }
    }
}
