﻿namespace NetModular.Lib.Utils.Core.SystemConfig
{
    /// <summary>
    /// 系统工具栏配置
    /// </summary>
    public class SystemToolbarConfigModel
    {
        /// <summary>
        /// 全屏
        /// </summary>
        [ConfigDescription("sys_toolbar_fullscreen", "全屏按钮")]
        public bool Fullscreen { get; set; }

        /// <summary>
        /// 皮肤设置
        /// </summary>
        [ConfigDescription("sys_toolbar_skin", "皮肤设置按钮")]
        public bool Skin { get; set; }

        /// <summary>
        /// 退出
        /// </summary>
        [ConfigDescription("sys_toolbar_logout", "退出按钮")]
        public bool Logout { get; set; }

        /// <summary>
        /// 用户信息
        /// </summary>
        [ConfigDescription("sys_toolbar_userinfo", "用户信息按钮")]
        public bool UserInfo { get; set; }
    }
}
