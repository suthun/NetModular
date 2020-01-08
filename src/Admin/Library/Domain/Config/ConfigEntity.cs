﻿using NetModular.Lib.Data.Abstractions.Attributes;
using NetModular.Lib.Data.Core.Entities.Extend;

namespace NetModular.Module.Admin.Domain.Config
{
    /// <summary>
    /// 配置项
    /// </summary>
    [Table("Config")]
    public class ConfigEntity : EntityBase<int>
    {
        /// <summary>
        /// 键名
        /// </summary>
        [Length(250)]
        public string Key { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        [Length(500)]
        public string Value { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [Length(250)]
        public string Remarks { get; set; }
    }
}
