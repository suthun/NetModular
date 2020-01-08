﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetModular.Lib.Data.Abstractions;
using NetModular.Lib.Data.Core;
using NetModular.Module.Admin.Domain.Button;
using NetModular.Module.Admin.Domain.Menu;
using NetModular.Module.Admin.Domain.RoleMenuButton;

namespace NetModular.Module.Admin.Infrastructure.Repositories.SqlServer
{
    public class RoleMenuButtonRepository : RepositoryAbstract<RoleMenuButtonEntity>, IRoleMenuButtonRepository
    {
        public RoleMenuButtonRepository(IDbContext context) : base(context)
        {
        }

        public Task<bool> DeleteByRole(Guid roleId, IUnitOfWork uow)
        {
            return Db.Find(m => m.RoleId == roleId).UseUow(uow).DeleteAsync();
        }

        public Task<bool> DeleteByMenu(Guid menuId, IUnitOfWork uow)
        {
            return Db.Find(m => m.MenuId == menuId).UseUow(uow).DeleteAsync();
        }

        public Task<bool> DeleteByButton(Guid buttonId, IUnitOfWork uow)
        {
            return Db.Find(m => m.ButtonId == buttonId).UseUow(uow).DeleteAsync();
        }

        public virtual Task<IList<ButtonEntity>> Query(Guid roleId, Guid menuId)
        {
            return DbContext.Set<ButtonEntity>().Find()
                 .InnerJoin<MenuEntity>((x, y) => x.MenuCode == y.RouteName && y.Id == menuId)
                 .LeftJoin<RoleMenuButtonEntity>((x, y, z) => x.Id == z.ButtonId && z.RoleId == roleId)
                 .Select((x, y, z) => new { x, z.RoleId })
                 .ToListAsync<ButtonEntity>();
        }

        public Task<bool> Exists(RoleMenuButtonEntity entity)
        {
            return Db.Find(m => m.RoleId == entity.RoleId && m.MenuId == entity.MenuId && m.ButtonId == entity.ButtonId)
                .ExistsAsync();
        }

        public Task<bool> Delete(RoleMenuButtonEntity entity)
        {
            return Db.Find(m => m.RoleId == entity.RoleId && m.MenuId == entity.MenuId && m.ButtonId == entity.ButtonId)
                .DeleteAsync();
        }

        public Task<bool> Delete(Guid roleId, Guid menuId, IUnitOfWork uow)
        {
            return Db.Find(m => m.RoleId == roleId && m.MenuId == menuId).UseUow(uow).DeleteAsync();
        }

        public Task<bool> ExistsWidthButton(Guid buttonId)
        {
            return Db.Find(m => m.ButtonId == buttonId).ExistsAsync();
        }
    }
}
