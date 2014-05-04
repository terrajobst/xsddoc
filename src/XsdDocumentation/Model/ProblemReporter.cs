using System;
using System.Globalization;
using System.Xml.Schema;

using XsdDocumentation.Properties;

namespace XsdDocumentation.Model
{
    internal sealed class ProblemReporter
    {
        private IMessageReporter _messageReporter;

        public ProblemReporter(IMessageReporter messageReporter)
        {
            _messageReporter = messageReporter;
        }

        private static string GetSchemaObjectType(XmlSchemaObject extension)
        {
            const int xmlSchemaPrefixLength = 9; // == "XmlSchema".Length;
            return extension.GetType().Name.Substring(xmlSchemaPrefixLength);
        }

        public void InvalidNamespaceUriInSchemaSet(string externalDocFileName)
        {
            var message = string.Format(CultureInfo.CurrentCulture, Resources.ProblemReporterInvalidNamespaceUriInSchemaSet, externalDocFileName);
            _messageReporter.ReportWarning("XSD0001", message);
        }

        public void InvalidObsoleteUri(string obsoleteUri)
        {
            var message = string.Format(CultureInfo.CurrentCulture, Resources.ProblemReporterInvalidObsoleteUri, obsoleteUri);
            _messageReporter.ReportWarning("XSD0002", message);
        }

        public void InvalidParentUri(string parentUri)
        {
            var message = string.Format(CultureInfo.CurrentCulture, Resources.ProblemReporterInvalidParentUri, parentUri);
            _messageReporter.ReportWarning("XSD0003", message);
        }

        public void InvalidExtensionType(XmlSchemaObject extension)
        {
            var extensionType = GetSchemaObjectType(extension);
            var message = string.Format(CultureInfo.CurrentCulture, Resources.ProblemReporterInvalidExtensionType, extensionType);
            _messageReporter.ReportWarning("XSD0004", message);
        }

        public void InvalidParentForExtension(XmlSchemaObject parent, XmlSchemaElement element)
        {
            var extensionType = GetSchemaObjectType(parent);
            var message = string.Format(CultureInfo.CurrentCulture, Resources.ProblemReporterInvalidParentForExtension, extensionType);
            _messageReporter.ReportWarning("XSD0005", message);
        }

        public void InvalidParentForExtension(XmlSchemaObject parent, XmlSchemaAttribute attribute)
        {
            var extensionType = GetSchemaObjectType(parent);
            var message = string.Format(CultureInfo.CurrentCulture, Resources.ProblemReporterInvalidParentForExtension, extensionType);
            _messageReporter.ReportWarning("XSD0006", message);
        }

        public void ParentOffersNoExtension(XmlSchemaElement parentElement, XmlSchemaElement extensionElement)
        {
            var message = string.Format(CultureInfo.CurrentCulture, Resources.ProblemReporterElementParentElementOffersNoExtension, parentElement.QualifiedName, extensionElement.QualifiedName);
            _messageReporter.ReportWarning("XSD0007", message);
        }

        public void ParentOffersNoExtension(XmlSchemaGroup parentGroup, XmlSchemaElement extensionElement)
        {
            var message = string.Format(CultureInfo.CurrentCulture, Resources.ProblemReporterElementParentGroupOffersNoExtension, parentGroup.QualifiedName, extensionElement.QualifiedName);
            _messageReporter.ReportWarning("XSD0008", message);
        }

        public void ParentOffersNoExtension(XmlSchemaComplexType parentComplexType, XmlSchemaElement extensionElement)
        {
            var message = string.Format(CultureInfo.CurrentCulture, Resources.ProblemReporterElementParentComplexTypeOffersNoExtension, parentComplexType.QualifiedName, extensionElement.QualifiedName);
            _messageReporter.ReportWarning("XSD0009", message);
        }

        public void ParentOffersNoExtension(XmlSchemaElement parentElement, XmlSchemaAttribute extensionAttribute)
        {
            var message = string.Format(CultureInfo.CurrentCulture, Resources.ProblemReporterAttributeParentElementOffersNoExtension, parentElement.QualifiedName, extensionAttribute.QualifiedName);
            _messageReporter.ReportWarning("XSD0010", message);
        }

        public void ParentOffersNoExtension(XmlSchemaAttributeGroup parentGroup, XmlSchemaAttribute extensionAttribute)
        {
            var message = string.Format(CultureInfo.CurrentCulture, Resources.ProblemReporterAttributeParentAttributGroupOffersNoExtension, parentGroup.QualifiedName, extensionAttribute.QualifiedName);
            _messageReporter.ReportWarning("XSD0011", message);
        }

        public void ParentOffersNoExtension(XmlSchemaComplexType parentComplexType, XmlSchemaAttribute extensionAttribute)
        {
            var message = string.Format(CultureInfo.CurrentCulture, Resources.ProblemReporterAttributeParentComplexTypeOffersNoExtension, parentComplexType.QualifiedName, extensionAttribute.QualifiedName);
            _messageReporter.ReportWarning("XSD0012", message);
        }
    }
}