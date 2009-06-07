using System;
using System.Collections.Generic;
using System.Xml.Schema;

namespace XsdDocumentation.Model
{
	internal sealed class ChildrenFinder : XmlSchemaSetVisitor
	{
		private SchemaSetManager _schemaSetManager;
		private Stack<List<ChildEntry>> _childEntryStack = new Stack<List<ChildEntry>>();

		public ChildrenFinder(SchemaSetManager schemaSetManager)
		{
			_schemaSetManager = schemaSetManager;
		}

		public List<ChildEntry> GetChildren()
		{
			if (_childEntryStack.Count == 0)
				return null;

			var children = _childEntryStack.Pop();
			if (children.Count == 0)
				return children;

			children[0] = InlineParticles(children[0]);
			return children;
		}

		private static ChildEntry InlineParticles(ChildEntry parent)
		{
			var particleAllowsInlining = parent.ChildType == ChildType.All ||
			                             parent.ChildType == ChildType.Choice ||
			                             parent.ChildType == ChildType.Sequence;

			if (particleAllowsInlining && parent.Children.Count == 1)
			{
				var child = parent.Children[0];
				child.MinOccurs = parent.MinOccurs * child.MinOccurs;
				child.MaxOccurs = (parent.MaxOccurs == Decimal.MaxValue ||
				                   child.MaxOccurs == Decimal.MaxValue)
				                  	? Decimal.MaxValue
				                  	: parent.MaxOccurs * child.MaxOccurs;
				return child;
			}

			var newChildren = new List<ChildEntry>();
			foreach (var child in parent.Children)
			{
				var replacedChild = InlineParticles(child);

				if (CanInline(parent.ChildType, replacedChild))
					newChildren.AddRange(replacedChild.Children);
				else
					newChildren.Add(replacedChild);
			}

			parent.Children.Clear();
			parent.Children.AddRange(newChildren);
			return parent;
		}

		private static bool CanInline(ChildType parentChildType, ChildEntry child)
		{
			if (child.ChildType == parentChildType)
			{
				if (child.ChildType == ChildType.Sequence ||
					child.ChildType == ChildType.Choice)
				{
					if (child.MinOccurs == 1 && child.MaxOccurs == 1)
						return true;
				}
			}

			return false;
		}

		private void Push(List<ChildEntry> children)
		{
			_childEntryStack.Push(children);
		}

		private void PushNode(ChildType childType, XmlSchemaObject parent, XmlSchemaParticle particle)
		{
			var childEntry = AddLeaf(childType, parent, particle);
			Push(childEntry.Children);
		}

		private void PopNode()
		{
			_childEntryStack.Pop();
		}

		private ChildEntry AddLeaf(ChildType childType, XmlSchemaObject parent, XmlSchemaParticle particle)
		{
			if (_childEntryStack.Count == 0)
			{
				var root = new List<ChildEntry>();
				_childEntryStack.Push(root);
			}

			var childEntry = new ChildEntry
			                 {
			                 	ChildType = childType,
								MinOccurs = particle == null ? 1 : particle.MinOccurs,
								MaxOccurs = particle == null ? 1 : particle.MaxOccurs,
			                 	Particle = particle,
								Parent = parent
			                 };
			_childEntryStack.Peek().Add(childEntry);
			return childEntry;
		}

		protected override void Visit(XmlSchemaElement element)
		{
			if (element.MaxOccurs == 0)
				return;

			if (_childEntryStack.Count > 0)
			{
				AddLeaf(ChildType.Element, element.Parent, element);
			}
			else
			{
				Traverse(element.ElementSchemaType);
			}
		}

		protected override void Visit(XmlSchemaComplexContentExtension extension)
		{
			var baseType = _schemaSetManager.SchemaSet.GlobalTypes[extension.BaseTypeName] as XmlSchemaComplexType;

			if (baseType != null)
			{
				if (baseType.ContentType == XmlSchemaContentType.ElementOnly ||
                    baseType.ContentType == XmlSchemaContentType.Mixed)
				{
					if (extension.Particle == null)
					{
						Traverse(baseType);
					}
					else
					{
						PushNode(ChildType.Sequence, extension, null);
						Traverse(baseType);
						Traverse(extension.Particle);
						PopNode();
					}

					return;
				}
			}

			if (extension.Particle != null)
				Traverse(extension.Particle);
		}

		protected override void Visit(XmlSchemaAny particle)
		{
			if (particle.MaxOccurs == 0)
				return;

			PushNode(ChildType.Any, particle.Parent, particle);

			foreach (XmlSchemaElement extensionElement in _schemaSetManager.GetExtensionElements(particle))
				AddLeaf(ChildType.ElementExtension, particle, extensionElement);

			PopNode();
		}

		protected override void Visit(XmlSchemaAll particle)
		{
			if (particle.MaxOccurs == 0)
				return;

			PushNode(ChildType.All, particle.Parent, particle);
			base.Visit(particle);
			PopNode();
		}

		protected override void Visit(XmlSchemaChoice particle)
		{
			if (particle.MaxOccurs == 0)
				return;

			PushNode(ChildType.Choice, particle.Parent, particle);
			base.Visit(particle);
			PopNode();
		}

		protected override void Visit(XmlSchemaSequence particle)
		{
			if (particle.MaxOccurs == 0)
				return;

			PushNode(ChildType.Sequence, particle.Parent, particle);
			base.Visit(particle);
			PopNode();
		}

		protected override void Visit(XmlSchemaGroupRef groupRef)
		{
			if (groupRef.MaxOccurs == 0)
				return;

			var group = _schemaSetManager.SchemaSet.ResolveGroup(groupRef);
			Traverse(group.Particle);
		}
	}
}