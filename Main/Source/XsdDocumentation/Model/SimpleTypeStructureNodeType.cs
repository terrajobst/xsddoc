using System;

namespace XsdDocumentation.Model
{
	internal enum SimpleTypeStructureNodeType
	{
		Root,
		Any,
		Mixed,
		NamedType,
		List,
		Union,
		Restriction,
		FacetEnumeration,
		FacetMaxExclusive,
		FacetMaxInclusive,
		FacetMinExclusive,
		FacetMinInclusive,
		FacetFractionDigits,
		FacetLength,
		FacetMaxLength,
		FacetMinLength,
		FacetTotalDigits,
		FacetPattern,
		FacetWhiteSpace,
	}
}