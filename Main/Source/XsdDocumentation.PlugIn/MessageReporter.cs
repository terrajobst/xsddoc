using System;

using SandcastleBuilder.Utils.BuildEngine;

namespace XsdDocumentation.PlugIn
{
    internal sealed class MessageReporter : IMessageReporter
    {
        private BuildProcess _buildProcess;

        public MessageReporter(BuildProcess buildProcess)
        {
            _buildProcess = buildProcess;
        }

        public void ReportWarning(string warningCode, string message)
        {
            _buildProcess.ReportWarning(warningCode, message);
        }
    }
}