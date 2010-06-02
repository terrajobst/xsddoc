using System;
using System.Collections.Generic;
using System.Xml.Schema;

namespace XsdDocumentation.Model
{
    internal sealed class ChildEntry
    {
        public ChildEntry()
        {
            Children = new List<ChildEntry>();
        }

        public ChildType ChildType { get; set; }
        public decimal MinOccurs { get; set; }
        public decimal MaxOccurs { get; set; }
        public XmlSchemaParticle Particle { get; set; }
        public XmlSchemaObject Parent { get; set; }
        public List<ChildEntry> Children { get; private set; }
    }
}