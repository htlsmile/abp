﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;
using Volo.CmsKit.Ratings;

namespace Volo.CmsKit.MongoDB.Ratings
{
    public class MongoRatingRepository : MongoDbRepository<ICmsKitMongoDbContext, Rating, Guid>, IRatingRepository
    {
        public MongoRatingRepository(IMongoDbContextProvider<ICmsKitMongoDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<List<Rating>> GetListAsync(string entityType, string entityId, CancellationToken cancellationToken = default)
        {
            Check.NotNullOrWhiteSpace(entityType, nameof(entityType));
            Check.NotNullOrWhiteSpace(entityId, nameof(entityId));

            var query = GetMongoQueryable().Where(r => r.EntityType == entityType && r.EntityId == entityId);
            var ratings = await query.ToListAsync(GetCancellationToken(cancellationToken));

            return ratings;
        }

        public async Task<Rating> GetCurrentUserRatingAsync(string entityType, string entityId, Guid userId,
            CancellationToken cancellationToken = default)
        {
            Check.NotNullOrWhiteSpace(entityType, nameof(entityType));
            Check.NotNullOrWhiteSpace(entityId, nameof(entityId));

            var rating = await GetMongoQueryable()
                .FirstOrDefaultAsync(r => r.EntityType == entityType && r.EntityId == entityId && r.CreatorId == userId,
                    GetCancellationToken(cancellationToken));
            
            return rating;
        }

        public async Task<List<RatingWithStarCountQueryResultItem>> GetGroupedStarCountsAsync(string entityType,
            string entityId, CancellationToken cancellationToken = default)
        {
            Check.NotNullOrWhiteSpace(entityType, nameof(entityType));
            Check.NotNullOrWhiteSpace(entityId, nameof(entityId));

            var query = from rating in GetMongoQueryable()
                where rating.EntityType == entityType && rating.EntityId == entityId
                orderby rating.StarCount descending
                group rating by rating.StarCount
                into g
                select new RatingWithStarCountQueryResultItem
                {
                    StarCount = g.Key,
                    Count = g.Count()
                };

            var ratings = await query.ToListAsync(GetCancellationToken(cancellationToken));

            return ratings;
        }
    }
}