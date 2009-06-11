using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal sealed class MamlWriter : IDisposable
	{
		private const string NonBlockingSpaceEntityName = "&#160;";
		private XmlWriter _xmlWriter;

		public MamlWriter(TextWriter textWriter)
		{
			var settings = new XmlWriterSettings
			               {
			               	Indent = true,
			               	IndentChars = "\t",
			               	OmitXmlDeclaration = true,
			               	Encoding = Encoding.UTF8
			               };
			_xmlWriter = XmlWriter.Create(textWriter, settings);
		}

		public MamlWriter(Stream stream)
		{
			var settings = new XmlWriterSettings
			               {
			               	Indent = true,
			               	IndentChars = "\t",
			               	Encoding = Encoding.UTF8
			               };
			_xmlWriter = XmlWriter.Create(stream, settings);
		}

		public void Dispose()
		{
			_xmlWriter.Close();
		}

		#region Topic

		public void StartTopic(string topicId)
		{
			_xmlWriter.WriteStartElement("topic");
			_xmlWriter.WriteAttributeString("id", topicId);
			_xmlWriter.WriteAttributeString("revisionNumber", "1");
			_xmlWriter.WriteStartElement("developerConceptualDocument", Namespaces.Maml);
			_xmlWriter.WriteAttributeString("xmlns", "xlink", null, Namespaces.XLink);
		}

		public void EndTopic()
		{
			_xmlWriter.WriteEndElement(); // developerConceptualDocument
			_xmlWriter.WriteEndElement(); // topic
		}

		#endregion

		#region Introduction

		public void StartIntroduction()
		{
			_xmlWriter.WriteStartElement("introduction", Namespaces.Maml);
		}

		public void EndIntroduction()
		{
			_xmlWriter.WriteEndElement(); // introduction
		}

		#endregion

		#region Related Topics

		public void StartRelatedTopics()
		{
			_xmlWriter.WriteStartElement("relatedTopics", Namespaces.Maml);
		}

		public void EndRelatedTopics()
		{
			_xmlWriter.WriteEndElement(); // relatedTopics
		}

		#endregion

		#region Section

		public void StartSection(string title, string address)
		{
			_xmlWriter.WriteStartElement("section", Namespaces.Maml);
			_xmlWriter.WriteAttributeString("address", address);

			_xmlWriter.WriteStartElement("title", Namespaces.Maml);
			_xmlWriter.WriteString(title);
			_xmlWriter.WriteEndElement();

			_xmlWriter.WriteStartElement("content");
		}

		public void EndSection()
		{
			_xmlWriter.WriteEndElement(); // content
			_xmlWriter.WriteEndElement(); // section
		}

		#endregion

		#region Tables

		public void StartTable()
		{
			_xmlWriter.WriteStartElement("table", Namespaces.Maml);
		}

		public void EndTable()
		{
			_xmlWriter.WriteEndElement(); // table
		}

		public void StartTableHeader()
		{
			_xmlWriter.WriteStartElement("tableHeader", Namespaces.Maml);
		}

		public void EndTableHeader()
		{
			_xmlWriter.WriteEndElement(); // tableHeader
		}

		public void StartTableRow()
		{
			_xmlWriter.WriteStartElement("row", Namespaces.Maml);
		}

		public void EndTableRow()
		{
			_xmlWriter.WriteEndElement(); // row
		}

		public void StartTableRowEntry()
		{
			_xmlWriter.WriteStartElement("entry", Namespaces.Maml);
		}

		public void EndTableRowEntry()
		{
			_xmlWriter.WriteEndElement(); // entry
		}

		#endregion

		#region List

		public void StartList(ListClass listClass)
		{
			_xmlWriter.WriteStartElement("list", Namespaces.Maml);
			switch (listClass)
			{
				case ListClass.Bullet:
					_xmlWriter.WriteAttributeString("class", "bullet");
					break;
				case ListClass.NoBullet:
					_xmlWriter.WriteAttributeString("class", "nobullet");
					break;
				case ListClass.Ordered:
					_xmlWriter.WriteAttributeString("class", "ordered");
					break;
				default:
					throw ExceptionBuilder.UnhandledCaseLabel(listClass);
			}
		}

		public void EndList()
		{
			_xmlWriter.WriteEndElement(); // list
		}

		public void StartListItem()
		{
			_xmlWriter.WriteStartElement("listItem", Namespaces.Maml);
		}

		public void EndListItem()
		{
			_xmlWriter.WriteEndElement(); // listItem
		}

		#endregion

		#region Alert

		public void StartAlert(AlertClass alertClass)
		{
			_xmlWriter.WriteStartElement("alert", Namespaces.Maml);
			switch (alertClass)
			{
				case AlertClass.Note:
					_xmlWriter.WriteAttributeString("class", "note");
					break;
				default:
					throw ExceptionBuilder.UnhandledCaseLabel(alertClass);
			}
		}

		public void EndAlert()
		{
			_xmlWriter.WriteEndElement(); // alert
		}

		#endregion

		#region Paragraph

		public void StartParagraph()
		{
			_xmlWriter.WriteStartElement("para", Namespaces.Maml);
		}

		public void EndParagraph()
		{
			_xmlWriter.WriteEndElement(); // para
		}

		#endregion

		#region Simple

		public void WriteCode(string source, string language)
		{
			_xmlWriter.WriteStartElement("code", Namespaces.Maml);
			_xmlWriter.WriteAttributeString("language", language);
			_xmlWriter.WriteAttributeString("xml", "space", null, "preserve");
			_xmlWriter.WriteString(source);
			_xmlWriter.WriteEndElement();
		}

		public void WriteMediaInline(string mediaLink)
		{
			_xmlWriter.WriteStartElement("mediaLinkInline", Namespaces.Maml);
			_xmlWriter.WriteStartElement("image", Namespaces.Maml);
			_xmlWriter.WriteAttributeString("href", Namespaces.XLink, mediaLink);
			_xmlWriter.WriteEndElement();
			_xmlWriter.WriteEndElement();
		}

		public void WriteLink(string link, string linkText)
		{
			_xmlWriter.WriteStartElement("link", Namespaces.Maml);
			_xmlWriter.WriteAttributeString("href", Namespaces.XLink, link);
			_xmlWriter.WriteString(linkText);
			_xmlWriter.WriteEndElement();
		}

		public void WriteLink(string link, string linkText, string linkType)
		{
			_xmlWriter.WriteStartElement("link", Namespaces.Maml);
			_xmlWriter.WriteAttributeString("href", Namespaces.XLink, link);
			_xmlWriter.WriteAttributeString("topicType_id", linkType);
			_xmlWriter.WriteString(linkText);
			_xmlWriter.WriteEndElement();
		}

		public void WriteString(string text)
		{
			_xmlWriter.WriteString(text);
		}

		public void WriteString(string format, params object[] args)
		{
			var text = string.Format(CultureInfo.InvariantCulture, format, args);
			WriteString(text);
		}

		public void WriteRaw(string data)
		{
			_xmlWriter.WriteRaw(data);
		}

		public void WriteRaw(XmlNode node)
		{
			node.WriteTo(_xmlWriter);
		}

		public void WriteRawContent(XmlNode node)
		{
			node.WriteContentTo(_xmlWriter);
		}

		#endregion

		#region HTML Legacy

		public void StartHtmlImage(string mediaLink)
		{
			_xmlWriter.WriteStartElement("markup", Namespaces.Maml);
			_xmlWriter.WriteStartElement("nobr", string.Empty);
			_xmlWriter.WriteStartElement("artLink");
			_xmlWriter.WriteAttributeString("target", mediaLink);
			_xmlWriter.WriteEndElement();
			_xmlWriter.WriteRaw(NonBlockingSpaceEntityName);
		}

		public void EndHtmlImage()
		{
			_xmlWriter.WriteEndElement(); // nobr
			_xmlWriter.WriteEndElement(); // markup
		}

		public void WriteHtmlLink(string link, string linkText)
		{
			var url = string.Format(CultureInfo.InvariantCulture, "/html/{0}.htm", link);
			_xmlWriter.WriteStartElement("a", Namespaces.Maml);
			_xmlWriter.WriteAttributeString("href", url);
			_xmlWriter.WriteString(linkText);
			_xmlWriter.WriteEndElement();
		}

		public void WriteHtmlImageWithText(string mediaLink, string text)
		{
			StartHtmlImage(mediaLink);
			WriteString(text);
			EndHtmlImage();
		}

		public void WriteHtmlImageWithLink(string mediaLink, string link, string linkText)
		{
			StartHtmlImage(mediaLink);
			WriteHtmlLink(link, linkText);
			EndHtmlImage();
		}

		public void WriteHtmlIndent(int level)
		{
			_xmlWriter.WriteStartElement("markup", Namespaces.Maml);
			for (var i = 0; i < level * 6; i++)
				_xmlWriter.WriteRaw(NonBlockingSpaceEntityName);
			_xmlWriter.WriteEndElement();
		}

		#endregion
	}
}