﻿using NetModular.Lib.Auth.Abstractions;
using NetModular.Lib.Excel.Abstractions;
using NetModular.Lib.Utils.Core.SystemConfig;

namespace NetModular.Lib.Excel.EPPlus
{
    public class EPPlusExcelHandler : ExcelHandlerAbstract
    {
        public EPPlusExcelHandler(ExcelOptions options, SystemConfigModel systemConfig, ILoginInfo loginInfo, IExcelExportHandler exportHandler) : base(options, systemConfig, loginInfo, exportHandler)
        {
        }
    }
}
