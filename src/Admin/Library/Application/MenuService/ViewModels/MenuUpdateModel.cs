﻿using System;
using System.ComponentModel.DataAnnotations;

namespace NetModular.Module.Admin.Application.MenuService.ViewModels
{
    /// <summary>
    /// 新增菜单
    /// </summary>
    public class MenuUpdateModel : MenuAddModel
    {
        //编号
        [Required(ErrorMessage = "请选择要编辑的菜单")]
        public Guid Id { get; set; }
    }
}
