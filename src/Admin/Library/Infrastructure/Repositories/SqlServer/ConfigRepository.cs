﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetModular.Lib.Data.Abstractions;
using NetModular.Lib.Data.Core;
using NetModular.Lib.Data.Query;
using NetModular.Module.Admin.Domain.Account;
using NetModular.Module.Admin.Domain.Config;
using NetModular.Module.Admin.Domain.Config.Models;

namespace NetModular.Module.Admin.Infrastructure.Repositories.SqlServer
{
    public class ConfigRepository : RepositoryAbstract<ConfigEntity>, IConfigRepository
    {
        public ConfigRepository(IDbContext context) : base(context)
        {
        }

        public Task<bool> Exists(string key)
        {
            return ExistsAsync(m => m.Key.Equals(key));
        }

        public Task<bool> Exists(ConfigEntity entity)
        {
            var query = Db.Find(m => m.Key == entity.Key);
            query.WhereIf(entity.Id > 0, m => m.Id != entity.Id);

            return query.ExistsAsync();
        }

        public Task<IList<ConfigEntity>> QueryByPrefix(string prefix)
        {
            return Db.Find(m => m.Key.StartsWith(prefix)).ToListAsync();
        }

        public async Task<IList<ConfigEntity>> Query(ConfigQueryModel model)
        {
            var paging = model.Paging();
            var query = Db.Find();
            query.WhereNotNull(model.Key, m => m.Key.Contains(model.Key) || m.Value.Contains(model.Key));

            var joinQuery = query.LeftJoin<AccountEntity>((x, y) => x.CreatedBy == y.Id);
            if (!paging.OrderBy.Any())
            {
                joinQuery.OrderByDescending((x, y) => x.Id);
            }

            joinQuery.Select((x, y) => new { x, Creator = y.Name });

            var list = await joinQuery.PaginationAsync(paging);
            model.TotalCount = paging.TotalCount;
            return list;
        }

        public Task<ConfigEntity> GetByKey(string key)
        {
            return Db.Find(m => m.Key == key).FirstAsync();
        }

        public override async Task<bool> UpdateAsync(ConfigEntity entity, IUnitOfWork uow)
        {
            if (await ExistsAsync(m => m.Key.Equals(entity.Key), uow))
                return await Db.Find(m => m.Key == entity.Key).UseUow(uow).UpdateAsync(m => new ConfigEntity { Value = entity.Value, Remarks = entity.Remarks });

            return await AddAsync(entity, uow);
        }
    }
}
