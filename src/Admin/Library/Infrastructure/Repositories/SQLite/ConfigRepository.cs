﻿using NetModular.Lib.Data.Abstractions;

namespace NetModular.Module.Admin.Infrastructure.Repositories.SQLite
{
    public class ConfigRepository : SqlServer.ConfigRepository
    {
        public ConfigRepository(IDbContext context) : base(context)
        {
        }
    }
}
