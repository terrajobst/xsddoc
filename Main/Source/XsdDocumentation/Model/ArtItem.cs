using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace XsdDocumentation.Model
{
    public sealed class ArtItem
    {
        public ArtItem(string id, string fileName, string alternateText)
        {
            Id = id;
            FileName = fileName;
            AlternateText = alternateText;
        }

        public string Id { get; private set; }
        public string FileName { get; private set; }
        public string AlternateText { get; private set; }

        public static ArtItem Namespace = new ArtItem("2d7c0638-b9b1-4fc5-b296-853465d7d487", "XsdNamespaceIcon.png", "Namespace");
        public static ArtItem Unique = new ArtItem("b2e63cca-7fd7-46bc-bb59-da0a9dd3a4c9", "XsdUniqueIcon.png", "Unique Constraint");
        public static ArtItem Union = new ArtItem("58dd1301-95dc-4170-8c0d-baaefb7e47b3", "XsdUnionIcon.png", "Union");
        public static ArtItem SimpleType = new ArtItem("9a1e7872-98a2-4751-aa25-0ae2c64d481d", "XsdSimpleTypeIcon.png", "Simple Type");
        public static ArtItem Sequence = new ArtItem("64f503f1-7b43-400c-b524-242215e6343f", "XsdSequenceIcon.png", "Sequence");
        public static ArtItem Schema = new ArtItem("45956914-1d6e-4e93-99a7-cc26f63d43a8", "XsdSchemaIcon.png", "Schema");
        public static ArtItem Restriction = new ArtItem("78dbb981-8116-4d75-b8cd-eef4d20fe5ce", "XsdRestrictionIcon.png", "Restriction");
        public static ArtItem Group = new ArtItem("0d8c7186-3b0c-41be-89dd-4e77cbd7f127", "XsdGroupIcon.png", "Group");
        public static ArtItem List = new ArtItem("0c24674e-a394-4f7e-a4b3-55ee63bd6dde", "XsdListIcon.png", "List");
        public static ArtItem KeyRef = new ArtItem("035d29e6-1bc2-4fd3-a328-2a611d80fc24", "XsdKeyRefIcon.png", "Keyref Constraint");
        public static ArtItem Key = new ArtItem("5a1b2b9a-e86d-4a2b-8fec-290d25a5dbc9", "XsdKeyIcon.png", "Key Constraint");
        public static ArtItem Extension = new ArtItem("82b464ce-1a12-4980-91b3-f1687d59839d", "XsdExtensionIcon.png", "Extension");
        public static ArtItem Element = new ArtItem("f32fbe7a-d21d-4d6c-925d-3578da0ad709", "XsdElementIcon.png", "Element");
        public static ArtItem ComplexType = new ArtItem("9193b822-8db2-488e-88de-919e0f845655", "XsdComplexTypeIcon.png", "Complex Type");
        public static ArtItem Choice = new ArtItem("8a940784-9c48-4ded-93ec-b57ff9881a65", "XsdChoiceIcon.png", "Choice");
        public static ArtItem Attribute = new ArtItem("d73fb973-e3c0-4eeb-904a-c083066fde0e", "XsdAttributeIcon.png", "Attribute");
        public static ArtItem AttributeGroup = new ArtItem("ab0c13cc-f452-46b5-8207-f0d6f05c4532", "XsdAttributeGroupIcon.png", "Attribute Group");
        public static ArtItem AnyElement = new ArtItem("65681e84-c7ff-4507-915b-caabdeaf6db9", "XsdAnyElementIcon.png", "Any Element");
        public static ArtItem AnyAttribute = new ArtItem("d6eb221a-85e6-49f8-b652-676f28f964d9", "XsdAnyAttributeIcon.png", "Any Attribute");
        public static ArtItem All = new ArtItem("06faa053-6739-411a-98be-aa6139cd452e", "XsdAllIcon.png", "All");
        public static ArtItem Facet = new ArtItem("9d0fc6b1-d6e8-45fa-adad-fe3d5a481210", "XsdFacetIcon.png", "Facet");
        public static ArtItem AttributeRef = new ArtItem("563e71d0-f2c9-407d-a4bc-6f06dab63121", "XsdAttributeRefIcon.png", "Attribute Reference");
        public static ArtItem ElementRef = new ArtItem("de7773ee-1d2c-4ad4-bd99-155467e03315", "XsdElementRefIcon.png", "Element Reference");

        public static ReadOnlyCollection<ArtItem> ArtItems = new ReadOnlyCollection<ArtItem>(new List<ArtItem>
                                                                                                 {
                                                                                                     Namespace,
                                                                                                     Unique,
                                                                                                     Union,
                                                                                                     SimpleType,
                                                                                                     Sequence,
                                                                                                     Schema,
                                                                                                     Restriction,
                                                                                                     Group,
                                                                                                     List,
                                                                                                     KeyRef,
                                                                                                     Key,
                                                                                                     Extension,
                                                                                                     Element,
                                                                                                     ComplexType,
                                                                                                     Choice,
                                                                                                     Attribute,
                                                                                                     AttributeGroup,
                                                                                                     AnyElement,
                                                                                                     AnyAttribute,
                                                                                                     All,
                                                                                                     Facet,
                                                                                                     AttributeRef,
                                                                                                     ElementRef,
                                                                                                 });

    }
}