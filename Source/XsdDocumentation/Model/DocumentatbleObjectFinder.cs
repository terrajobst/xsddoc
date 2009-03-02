using System;
using System.Collections.Generic;
using System.Xml.Schema;

namespace XsdDocumentation.Model
{
	internal sealed class DocumentatbleObjectFinder : XmlSchemaSetVisitor
	{
		private HashSet<XmlSchemaAnnotated> _documentatbleObjects;

		public DocumentatbleObjectFinder(HashSet<XmlSchemaAnnotated> documentatbleObjects)
		{
			_documentatbleObjects = documentatbleObjects;
		}

		private bool Add(XmlSchemaAnnotated obj)
		{
			return _documentatbleObjects.Add(obj);
		}

		protected override void Visit(XmlSchemaAnnotated annotated)
		{
			if (Add(annotated))
				base.Visit(annotated);
		}
	}
}