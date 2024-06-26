using System;

namespace DataAnalyticsPlatform.Shared.ExceptionUtils
{
    public class ErrorArgument : EventArgs
    {
        public string ErrorMessage { get; set; }
    }
}
