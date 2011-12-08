using System;

namespace XsdDocumentation.Model
{
    public interface IMessageReporter
    {
        void ReportWarning(string warningCode, string message);
    }
}