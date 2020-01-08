﻿using System;

namespace NetModular.Lib.Utils.Core.Attributes
{
    /// <summary>
    /// 单例注入
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
    public class SingletonAttribute : Attribute
    {
    }
}
