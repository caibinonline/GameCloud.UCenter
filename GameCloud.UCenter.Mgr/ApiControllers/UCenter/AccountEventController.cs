﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GameCloud.Database.Adapters;
using GameCloud.Manager.PluginContract.Requests;
using GameCloud.Manager.PluginContract.Responses;
using GameCloud.UCenter.Api.Manager.Models;
using GameCloud.UCenter.Common.Settings;
using GameCloud.UCenter.Database;
using GameCloud.UCenter.Database.Entities;
using GameCloud.UCenter.Manager.Api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace GameCloud.UCenter.Api.Manager.ApiControllers
{
    /// <summary>
    /// Provide a controller for users.
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class AccountEventsController : ManagerApiControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountEventsController" /> class.
        /// </summary>
        /// <param name="ucenterDb">Indicating the database context.</param>
        /// <param name="ucenterventDb">Indicating the database context.</param>
        /// <param name="settings">Indicating the settings.</param>
        [ImportingConstructor]
        public AccountEventsController(
            UCenterDatabaseContext ucenterDb,
            UCenterEventDatabaseContext ucenterventDb,
            Settings settings)
            : base(ucenterDb, ucenterventDb, settings)
        {
        }

        /// <summary>
        /// Get user list.
        /// </summary>
        /// <param name="request">Indicating the count.</param>
        /// <param name="token">Indicating the cancellation token.</param>
        /// <returns>Async return account event list.</returns>
        [Route("api/ucenter/accountEvents")]
        public async Task<PluginPaginationResponse<AccountEventEntity>> AccountEvents([FromBody]SearchRequestInfo<AccountEventEntity> request, CancellationToken token)
        {
            string keyword = request.GetParameterValue<string>("keyword");
            int page = request.GetParameterValue<int>("page", 1);
            int count = request.GetParameterValue<int>("pageSize", 10);

            Expression<Func<AccountEventEntity, bool>> filter = null;

            if (!string.IsNullOrEmpty(keyword))
            {
                filter = a => a.AccountName.Contains(keyword);
            }

            var total = await this.UCenterEventDatabase.AccountEvents.CountAsync(filter, token);

            IQueryable<AccountEventEntity> queryable = this.UCenterEventDatabase.AccountEvents.Collection.AsQueryable();
            if (filter != null)
            {
                queryable = queryable.Where(filter);
            }
            queryable = queryable.OrderByDescending(a => a.CreatedTime);

            var result = queryable.Skip((page - 1) * count).Take(count).ToList();

            // todo: add orderby support.
            var model = new PluginPaginationResponse<AccountEventEntity>
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
