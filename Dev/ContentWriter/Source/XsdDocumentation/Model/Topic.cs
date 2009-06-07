using System;
using System.Collections.Generic;
using System.Xml.Schema;

namespace XsdDocumentation.Model
{
	internal sealed class Topic
	{
		public Topic()
		{
			Children = new List<Topic>();
			KeywordsK = new List<string>();
			KeywordsF = new List<string>();
		}

		public string Id { get; set; }
		public string Title { get; set; }
		public string LinkUri { get; set; }
		public string LinkIdUri { get; set; }
		public string LinkTitle { get; set; }
		public TopicType TopicType { get; set; }
		public string Namespace { get; set; }
		public XmlSchemaObject SchemaObject { get; set; }
		public List<Topic> Children { get; private set; }
		public List<string> KeywordsK { get; private set; }
		public List<String> KeywordsF { get; private set; }
		public string FileName { get; set; }
	}
}