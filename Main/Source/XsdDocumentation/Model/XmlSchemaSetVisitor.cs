using System;
using System.Xml.Schema;

namespace XsdDocumentation.Model
{
	internal abstract class XmlSchemaSetVisitor
	{
		#region Traversing

		public void Traverse(XmlSchemaSet schemaSet)
		{
			Visit(schemaSet);
		}

		public void Traverse(XmlSchemaObject obj)
		{
			Visit(obj);
		}

		public void Traverse(XmlSchemaObjectCollection objects)
		{
			foreach (var obj in objects)
				Traverse(obj);
		}

		#endregion

		#region Root

		protected virtual void Visit(XmlSchemaSet schemaSet)
		{
			foreach (var schema in schemaSet.GetAllSchemas())
				Traverse(schema);
		}

		protected virtual void Visit(XmlSchemaObject obj)
		{
			XmlSchema schema;
			XmlSchemaAnnotated annotated;
			XmlSchemaAnnotation annotation;
			XmlSchemaAppInfo appInfo;
			XmlSchemaDocumentation documentation;
			XmlSchemaExternal external;

			if (Casting.TryCast(obj, out schema))
				Visit(schema);
			else if (Casting.TryCast(obj, out annotated))
				Visit(annotated);
			else if (Casting.TryCast(obj, out annotation))
				Visit(annotation);
			else if (Casting.TryCast(obj, out appInfo))
				Visit(appInfo);
			else if (Casting.TryCast(obj, out documentation))
				Visit(documentation);
			else if (Casting.TryCast(obj, out external))
				Visit(external);
			else
				throw ExceptionBuilder.UnexpectedSchemaObjectType(obj);
		}

		#endregion

		#region XmlSchemaObject

		protected virtual void Visit(XmlSchema schema)
		{
			foreach (var item in schema.Items)
				Traverse(item);
		}

		protected virtual void Visit(XmlSchemaAnnotated annotated)
		{
			XmlSchemaAnyAttribute anyAttribute;
			XmlSchemaAttribute attribute;
			XmlSchemaAttributeGroup attributeGroup;
			XmlSchemaAttributeGroupRef attributeGroupRef;
			XmlSchemaContent content;
			XmlSchemaContentModel contentModel;
			XmlSchemaFacet facet;
			XmlSchemaGroup group;
			XmlSchemaIdentityConstraint constraint;
			XmlSchemaNotation notation;
			XmlSchemaParticle particle;
			XmlSchemaSimpleTypeContent schemaSimpleTypeContent;
			XmlSchemaType type;
			XmlSchemaXPath xPath;

			if (Casting.TryCast(annotated, out anyAttribute))
				Visit(anyAttribute);
			else if (Casting.TryCast(annotated, out attribute))
				Visit(attribute);
			else if (Casting.TryCast(annotated, out attributeGroup))
				Visit(attributeGroup);
			else if (Casting.TryCast(annotated, out attributeGroupRef))
				Visit(attributeGroupRef);
			else if (Casting.TryCast(annotated, out content))
				Visit(content);
			else if (Casting.TryCast(annotated, out contentModel))
				Visit(contentModel);
			else if (Casting.TryCast(annotated, out facet))
				Visit(facet);
			else if (Casting.TryCast(annotated, out group))
				Visit(group);
			else if (Casting.TryCast(annotated, out constraint))
				Visit(constraint);
			else if (Casting.TryCast(annotated, out notation))
				Visit(notation);
			else if (Casting.TryCast(annotated, out particle))
				Visit(particle);
			else if (Casting.TryCast(annotated, out schemaSimpleTypeContent))
				Visit(schemaSimpleTypeContent);
			else if (Casting.TryCast(annotated, out type))
				Visit(type);
			else if (Casting.TryCast(annotated, out xPath))
				Visit(xPath);
			else
				throw ExceptionBuilder.UnexpectedSchemaObjectType(annotated);
		}

		protected virtual void Visit(XmlSchemaAnnotation annotation)
		{
			Traverse(annotation.Items);
		}

		protected virtual void Visit(XmlSchemaAppInfo appInfo)
		{
		}

		protected virtual void Visit(XmlSchemaDocumentation documentation)
		{
		}

		protected virtual void Visit(XmlSchemaExternal external)
		{
			XmlSchemaImport import;
			XmlSchemaInclude include;
			XmlSchemaRedefine redefine;

			if (Casting.TryCast(external, out import))
				Visit(import);
			else if (Casting.TryCast(external, out include))
				Visit(include);
			else if (Casting.TryCast(external, out redefine))
				Visit(redefine);
			else
				throw ExceptionBuilder.UnexpectedSchemaObjectType(external);
		}

		#endregion

		#region XmlSchemaAnnotated

		protected virtual void Visit(XmlSchemaAnyAttribute attribute)
		{
		}

		protected virtual void Visit(XmlSchemaAttribute attribute)
		{
			if (attribute.RefName.IsEmpty && attribute.Use != XmlSchemaUse.Prohibited)
				Traverse(attribute.AttributeSchemaType);
		}

		protected virtual void Visit(XmlSchemaAttributeGroup group)
		{
			Traverse(group.Attributes);

			if (group.AnyAttribute != null)
				Traverse(group.AnyAttribute);
		}

		protected virtual void Visit(XmlSchemaAttributeGroupRef groupRef)
		{
		}

		protected virtual void Visit(XmlSchemaContent content)
		{
			XmlSchemaSimpleContentExtension simpleContentExtension;
			XmlSchemaSimpleContentRestriction simpleContentRestriction;
			XmlSchemaComplexContentExtension complexContentExtension;
			XmlSchemaComplexContentRestriction complexContentRestriction;

			if (Casting.TryCast(content, out simpleContentExtension))
				Visit(simpleContentExtension);
			else if (Casting.TryCast(content, out simpleContentRestriction))
				Visit(simpleContentRestriction);
			else if (Casting.TryCast(content, out complexContentExtension))
				Visit(complexContentExtension);
			else if (Casting.TryCast(content, out complexContentRestriction))
				Visit(complexContentRestriction);
			else
				throw ExceptionBuilder.UnexpectedSchemaObjectType(content);
		}

		protected virtual void Visit(XmlSchemaContentModel model)
		{
			XmlSchemaSimpleContent simpleContent;
			XmlSchemaComplexContent complexContent;

			if (Casting.TryCast(model, out simpleContent))
				Visit(simpleContent);
			else if (Casting.TryCast(model, out complexContent))
				Visit(complexContent);
			else
				throw ExceptionBuilder.UnexpectedSchemaObjectType(model);
		}

		protected virtual void Visit(XmlSchemaFacet facet)
		{
			XmlSchemaEnumerationFacet enumerationFacet;
			XmlSchemaMaxExclusiveFacet maxExclusiveFacet;
			XmlSchemaMaxInclusiveFacet maxInclusiveFacet;
			XmlSchemaMinExclusiveFacet minExclusiveFacet;
			XmlSchemaMinInclusiveFacet minInclusiveFacet;
			XmlSchemaNumericFacet numericFacet;
			XmlSchemaPatternFacet patternFacet;
			XmlSchemaWhiteSpaceFacet whiteSpaceFacet;

			if (Casting.TryCast(facet, out enumerationFacet))
				Visit(enumerationFacet);
			else if (Casting.TryCast(facet, out maxExclusiveFacet))
				Visit(maxExclusiveFacet);
			else if (Casting.TryCast(facet, out maxInclusiveFacet))
				Visit(maxInclusiveFacet);
			else if (Casting.TryCast(facet, out minExclusiveFacet))
				Visit(minExclusiveFacet);
			else if (Casting.TryCast(facet, out minInclusiveFacet))
				Visit(minInclusiveFacet);
			else if (Casting.TryCast(facet, out numericFacet))
				Visit(numericFacet);
			else if (Casting.TryCast(facet, out patternFacet))
				Visit(patternFacet);
			else if (Casting.TryCast(facet, out whiteSpaceFacet))
				Visit(whiteSpaceFacet);
			else
				throw ExceptionBuilder.UnexpectedSchemaObjectType(facet);
		}

		protected virtual void Visit(XmlSchemaGroup group)
		{
			Traverse(group.Particle);
		}

		protected virtual void Visit(XmlSchemaIdentityConstraint constraint)
		{
			XmlSchemaKey key;
			XmlSchemaKeyref keyref;
			XmlSchemaUnique unique;

			if (Casting.TryCast(constraint, out key))
				Visit(key);
			else if (Casting.TryCast(constraint, out keyref))
				Visit(keyref);
			else if (Casting.TryCast(constraint, out unique))
				Visit(unique);
			else
				throw ExceptionBuilder.UnexpectedSchemaObjectType(constraint);
		}

		protected virtual void Visit(XmlSchemaNotation notation)
		{
		}

		protected virtual void Visit(XmlSchemaParticle particle)
		{
			XmlSchemaAny any;
			XmlSchemaGroupBase groupBase;
			XmlSchemaElement element;
			XmlSchemaGroupRef groupRef;

			if (Casting.TryCast(particle, out any))
				Visit(any);
			else if (Casting.TryCast(particle, out groupBase))
				Visit(groupBase);
			else if (Casting.TryCast(particle, out element))
				Visit(element);
			else if (Casting.TryCast(particle, out groupRef))
				Visit(groupRef);
			else
				throw ExceptionBuilder.UnexpectedSchemaObjectType(particle);
		}

		protected virtual void Visit(XmlSchemaSimpleTypeContent simpleTypeContent)
		{
			XmlSchemaSimpleTypeList list;
			XmlSchemaSimpleTypeRestriction restriction;
			XmlSchemaSimpleTypeUnion union;

			if (Casting.TryCast(simpleTypeContent, out list))
				Visit(list);
			else if (Casting.TryCast(simpleTypeContent, out restriction))
				Visit(restriction);
			else if (Casting.TryCast(simpleTypeContent, out union))
				Visit(union);
			else
				throw ExceptionBuilder.UnexpectedSchemaObjectType(simpleTypeContent);
		}

		protected virtual void Visit(XmlSchemaType type)
		{
			XmlSchemaComplexType complexType;
			XmlSchemaSimpleType simpleType;

			if (Casting.TryCast(type, out complexType))
				Visit(complexType);
			else if (Casting.TryCast(type, out simpleType))
				Visit(simpleType);
			else
				throw ExceptionBuilder.UnexpectedSchemaObjectType(type);
		}

		protected virtual void Visit(XmlSchemaXPath xPath)
		{
		}

		#endregion

		#region XmlSchemaContent

		protected virtual void Visit(XmlSchemaComplexContentExtension extension)
		{
			if (extension.Particle != null)
				Traverse(extension.Particle);

			Traverse(extension.Attributes);

			if (extension.AnyAttribute != null)
				Traverse(extension.AnyAttribute);
		}

		protected virtual void Visit(XmlSchemaComplexContentRestriction restriction)
		{
			if (restriction.Particle != null)
				Traverse(restriction.Particle);

			Traverse(restriction.Attributes);

			if (restriction.AnyAttribute != null)
				Traverse(restriction.AnyAttribute);
		}

		protected virtual void Visit(XmlSchemaSimpleContentExtension extension)
		{
			Traverse(extension.Attributes);

			if (extension.AnyAttribute != null)
				Traverse(extension.AnyAttribute);
		}

		protected virtual void Visit(XmlSchemaSimpleContentRestriction restriction)
		{
			Traverse(restriction.Facets);

			Traverse(restriction.Attributes);

			if (restriction.AnyAttribute != null)
				Traverse(restriction.AnyAttribute);
		}

		#endregion

		#region XmlSchemaContentModel

		protected virtual void Visit(XmlSchemaComplexContent content)
		{
			Traverse(content.Content);
		}

		protected virtual void Visit(XmlSchemaSimpleContent content)
		{
			Traverse(content.Content);
		}
		#endregion

		#region XmlSchemaFacet

		protected virtual void Visit(XmlSchemaEnumerationFacet facet)
		{
		}

		protected virtual void Visit(XmlSchemaMaxExclusiveFacet facet)
		{
		}

		protected virtual void Visit(XmlSchemaMaxInclusiveFacet facet)
		{
		}

		protected virtual void Visit(XmlSchemaMinExclusiveFacet facet)
		{
		}

		protected virtual void Visit(XmlSchemaMinInclusiveFacet facet)
		{
		}

		protected virtual void Visit(XmlSchemaNumericFacet facet)
		{
			XmlSchemaFractionDigitsFacet digitsFacet;
			XmlSchemaLengthFacet lengthFacet;
			XmlSchemaMaxLengthFacet maxLengthFacet;
			XmlSchemaMinLengthFacet minLengthFacet;
			XmlSchemaTotalDigitsFacet totalDigitsFacet;

			if (Casting.TryCast(facet, out digitsFacet))
				Visit(digitsFacet);
			else if (Casting.TryCast(facet, out lengthFacet))
				Visit(lengthFacet);
			else if (Casting.TryCast(facet, out maxLengthFacet))
				Visit(maxLengthFacet);
			else if (Casting.TryCast(facet, out minLengthFacet))
				Visit(minLengthFacet);
			else if (Casting.TryCast(facet, out totalDigitsFacet))
				Visit(totalDigitsFacet);
			else
				throw ExceptionBuilder.UnexpectedSchemaObjectType(facet);
		}

		protected virtual void Visit(XmlSchemaPatternFacet facet)
		{
		}

		protected virtual void Visit(XmlSchemaWhiteSpaceFacet facet)
		{
		}

		#endregion

		#region XmlSchemaNumericFacet

		protected virtual void Visit(XmlSchemaFractionDigitsFacet facet)
		{
		}

		protected virtual void Visit(XmlSchemaLengthFacet facet)
		{
		}

		protected virtual void Visit(XmlSchemaMaxLengthFacet facet)
		{
		}

		protected virtual void Visit(XmlSchemaMinLengthFacet facet)
		{
		}

		protected virtual void Visit(XmlSchemaTotalDigitsFacet facet)
		{
		}

		#endregion

		#region XmlSchemaIdentityConstraint

		protected virtual void Visit(XmlSchemaKey key)
		{
			Traverse(key.Selector);
			Traverse(key.Fields);
		}

		protected virtual void Visit(XmlSchemaKeyref keyref)
		{
			Traverse(keyref.Selector);
			Traverse(keyref.Fields);
		}

		protected virtual void Visit(XmlSchemaUnique unique)
		{
			Traverse(unique.Selector);
			Traverse(unique.Fields);
		}

		#endregion

		#region XmlSchemaParticle

		protected virtual void Visit(XmlSchemaAny particle)
		{
		}

		protected virtual void Visit(XmlSchemaElement element)
		{
			if (element.RefName.IsEmpty && element.MaxOccurs > 0)
			{
				Traverse(element.ElementSchemaType);

				foreach (XmlSchemaIdentityConstraint constraint in element.Constraints)
					Traverse(constraint);
			}
		}

		protected virtual void Visit(XmlSchemaGroupBase particle)
		{
			XmlSchemaAll all;
			XmlSchemaChoice choice;
			XmlSchemaSequence sequence;

			if (Casting.TryCast(particle, out all))
				Visit(all);
			else if (Casting.TryCast(particle, out choice))
				Visit(choice);
			else if (Casting.TryCast(particle, out sequence))
				Visit(sequence);
			else
				throw ExceptionBuilder.UnexpectedSchemaObjectType(particle);
		}

		protected virtual void Visit(XmlSchemaGroupRef groupRef)
		{
		}

		#endregion

		#region XmlSchemaGroupBase

		protected virtual void Visit(XmlSchemaAll particle)
		{
			if (particle.MaxOccurs > 0)
				Traverse(particle.Items);
		}

		protected virtual void Visit(XmlSchemaChoice particle)
		{
			if (particle.MaxOccurs > 0)
				Traverse(particle.Items);
		}

		protected virtual void Visit(XmlSchemaSequence particle)
		{
			if (particle.MaxOccurs > 0)
				Traverse(particle.Items);
		}

		#endregion

		#region XmlSchemaSimpleTypeContent

		protected virtual void Visit(XmlSchemaSimpleTypeList list)
		{
			Traverse(list.BaseItemType);
		}

		protected virtual void Visit(XmlSchemaSimpleTypeRestriction restriction)
		{
			if (restriction.BaseType != null)
				Traverse(restriction.BaseType);

			Traverse(restriction.Facets);
		}

		protected virtual void Visit(XmlSchemaSimpleTypeUnion union)
		{
			foreach (var simpleType in union.BaseMemberTypes)
				Traverse(simpleType);
		}

		#endregion

		#region XmlSchemaType

		protected virtual void Visit(XmlSchemaComplexType type)
		{
			if (type.ContentModel != null)
			{
				Traverse(type.ContentModel);
			}
			else
			{
				if (type.Particle != null)
					Traverse(type.Particle);

				Traverse(type.Attributes);

				if (type.AnyAttribute != null)
					Traverse(type.AnyAttribute);
			}
		}

		protected virtual void Visit(XmlSchemaSimpleType type)
		{
			if (type.Content != null)
				Traverse(type.Content);
		}

		#endregion

		#region XmlSchemaExternal

		protected virtual void Visit(XmlSchemaImport import)
		{
		}

		protected virtual void Visit(XmlSchemaInclude include)
		{
		}

		protected virtual void Visit(XmlSchemaRedefine redefine)
		{
		}

		#endregion
	}
}