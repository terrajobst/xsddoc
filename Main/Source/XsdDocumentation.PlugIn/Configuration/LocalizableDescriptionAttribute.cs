using System;
using System.ComponentModel;

using XsdDocumentation.PlugIn.Properties;

namespace XsdDocumentation.PlugIn
{
    public sealed class LocalizableDescriptionAttribute : DescriptionAttribute
    {
        public LocalizableDescriptionAttribute(string description)
            : base(description)
        {
        }

        public override string Description
        {
            get { return Resources.ResourceManager.GetString(DescriptionValue); }
        }
    }
}