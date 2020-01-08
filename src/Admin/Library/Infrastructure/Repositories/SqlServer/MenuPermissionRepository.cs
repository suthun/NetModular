﻿using System.Collections.Generic;
using System.Threading.Tasks;
using NetModular.Lib.Data.Abstractions;
using NetModular.Lib.Data.Core;
using NetModular.Module.Admin.Domain.MenuPermission;

namespace NetModular.Module.Admin.Infrastructure.Repositories.SqlServer
{
    public class MenuPermissionRepository : RepositoryAbstract<MenuPermissionEntity>, IMenuPermissionRepository
    {
        public MenuPermissionRepository(IDbContext dbContext) : base(dbContext)
        {
        }

        public Task<bool> ExistsBindPermission(string permissionCode)
        {
            return Db.Find(m => m.PermissionCode.Equals(permissionCode)).ExistsAsync();
        }

        public Task<bool> DeleteByPermission(string permissionCode, IUnitOfWork uow)
        {
            return Db.Find(e => e.PermissionCode == permissionCode).UseUow(uow).DeleteAsync();
        }

        public Task<bool> DeleteByMenu(string menuCode, IUnitOfWork uow)
        {
            return Db.Find(e => e.MenuCode == menuCode).UseUow(uow).DeleteAsync();
        }

        public Task<IList<MenuPermissionEntity>> GetListByMenu(string menuCode)
        {
            return Db.Find(e => e.MenuCode == menuCode).ToListAsync();
        }
    }
}
