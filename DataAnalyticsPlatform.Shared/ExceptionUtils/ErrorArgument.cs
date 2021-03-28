using System;
using System.Collections.Generic;
using System.Text;

namespace DataAnalyticsPlatform.Shared.ExceptionUtils
{
    public class ErrorArgument : EventArgs
    {
        public string ErrorMessage { get; set; }
    }
}
