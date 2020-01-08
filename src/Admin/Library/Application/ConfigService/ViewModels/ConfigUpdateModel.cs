﻿using System.ComponentModel.DataAnnotations;

namespace NetModular.Module.Admin.Application.ConfigService.ViewModels
{
    public class ConfigUpdateModel : ConfigAddModel
    {
        [Required(ErrorMessage = "请选择配置项")]
        public int Id { get; set; }
    }
}
