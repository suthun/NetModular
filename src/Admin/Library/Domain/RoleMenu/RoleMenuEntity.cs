﻿using System;
using NetModular.Lib.Data.Abstractions.Attributes;
using NetModular.Lib.Data.Core.Entities;

namespace NetModular.Module.Admin.Domain.RoleMenu
{
    /// <summary>
    /// 角色菜单
    /// </summary>
    [Table("Role_Menu")]
    public partial class RoleMenuEntity : Entity<int>
    {
        /// <summary>
        /// 角色编号
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// 菜单编号
        /// </summary>
        public Guid MenuId { get; set; }
    }
}
