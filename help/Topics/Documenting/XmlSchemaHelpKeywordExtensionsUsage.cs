using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;

using XsdDocumentation;

namespace WinFormsHelpIntegration
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Button1Click(object sender, EventArgs e)
        {
            #region Invoke Help

            string helpFilePath = GetHelpFilePath();

            XmlSchemaSet schemaSet = GetXmlSchemaSet();
            XmlSchemaElement element = GetSelectedElement(schemaSet);
            string keyword = schemaSet.GetHelpKeyword(element);

            Help.ShowHelp(this, helpFilePath, HelpNavigator.KeywordIndex, keyword);

            #endregion
        }

        private static XmlSchemaElement GetSelectedElement(XmlSchemaSet schemaSet)
        {
            return schemaSet.GlobalElements.Values.Cast<XmlSchemaElement>().First();
        }

        private static string GetHelpFilePath()
        {
            return @"P:\XsdDoc\Output\Samples\WiX\WiX.chm";
        }

        private static XmlSchemaSet GetXmlSchemaSet()
        {
            var schemaFiles = Directory.EnumerateFiles(@"P:\XsdDoc\Samples\WiX\Schemas", "*.xsd");

            var schemaSet = new XmlSchemaSet();

            foreach (var schemaFile in schemaFiles)
            {
                using (var nodeReader = new XmlTextReader(schemaFile))
                {
                    var xmlSchema = XmlSchema.Read(nodeReader, null);
                    schemaSet.Add(xmlSchema);
                }
            }

            schemaSet.Compile();

            return schemaSet;
        }
    }
}