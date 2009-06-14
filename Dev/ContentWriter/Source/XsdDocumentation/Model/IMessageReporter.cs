using System;

namespace XsdDocumentation
{
	public interface IMessageReporter
	{
		void ReportWarning(string warningCode, string message);
	}
}