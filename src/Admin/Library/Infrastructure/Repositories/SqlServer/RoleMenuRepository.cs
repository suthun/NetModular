﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetModular.Lib.Data.Abstractions;
using NetModular.Lib.Data.Core;
using NetModular.Module.Admin.Domain.Menu;
using NetModular.Module.Admin.Domain.RoleMenu;

namespace NetModular.Module.Admin.Infrastructure.Repositories.SqlServer
{
    public class RoleMenuRepository : RepositoryAbstract<RoleMenuEntity>, IRoleMenuRepository
    {
        public RoleMenuRepository(IDbContext context) : base(context)
        {
        }
        public Task<IList<RoleMenuEntity>> GetByRoleId(Guid roleId)
        {
            return Db.Find(e => e.RoleId == roleId)
                .LeftJoin<MenuEntity>((x, y) => x.MenuId == y.Id)
                .Select((x, y) => new { x, MenuType = y.Type })
                .ToListAsync();
        }

        public Task<bool> DeleteByMenuId(Guid menuId, IUnitOfWork uow)
        {
            return Db.Find(e => e.MenuId == menuId).UseUow(uow).DeleteAsync();
        }

        public Task<bool> ExistsWidthMenu(Guid menuId)
        {
            return Db.Find(e => e.MenuId == menuId).ExistsAsync();
        }

        public Task<bool> DeleteByRoleId(Guid roleId, IUnitOfWork uow)
        {
            return Db.Find(e => e.RoleId == roleId).UseUow(uow).DeleteAsync();
        }
    }
}
