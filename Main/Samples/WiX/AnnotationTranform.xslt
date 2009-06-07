<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:msxsl="urn:schemas-microsoft-com:xslt"
				xmlns:xsd="http://schemas.xsddoc.codeplex.com/schemaDoc/2009/3"
				xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
				xmlns:xlink="http://www.w3.org/1999/xlink"
				xmlns:xs="http://www.w3.org/2001/XMLSchema"
				xmlns:xse="http://schemas.microsoft.com/wix/2005/XmlSchemaExtension"
				xmlns:xhtml="http://www.w3.org/1999/xhtml"
				exclude-result-prefixes="msxsl xs xse">
	<xsl:output method="xml" indent="yes"/>

	<xsl:param name="parentItemType"/>
	<xsl:param name="parentItemNamespace"/>
	<xsl:param name="parentItemUri"/>

	<xsl:param name="currentItemType"/>
	<xsl:param name="currentItemNamespace"/>
	<xsl:param name="currentItemUri"/>

	<xsl:template match="*">
		<xsl:apply-templates select="xs:annotation" />
	</xsl:template>

	<!-- The WiX schema uses the following tags for documentation:
	
			xse:deprecated
			   @ref (optional)
			xse:parent
			   namespace
			   ref
			xse:remarks
			xse:seeAlso
			   ref
			xse:msiRef
			   @table | @action
			   @href	   
	-->

	<xsl:template match="xs:annotation">
		<xsd:schemaDoc>
			<!-- Obsolete -->

			<xsl:variable name="deprecated" select="xs:appinfo/xse:deprecated" />
			<xsl:if test="$deprecated">
				<xsd:obsolete>
					<xsl:if test="$deprecated/@ref">
						<xsl:choose>
							<xsl:when test="$currentItemType='attribute'">
								<xsl:attribute name="uri" xml:space="preserve"><xsl:value-of select="$parentItemUri"/>/@<xsl:value-of select="$deprecated/@ref"/></xsl:attribute>
							</xsl:when>
							<xsl:when test="$currentItemType='element'">
								<xsl:attribute name="uri" xml:space="preserve">http://schemas.microsoft.com/wix/2006/wi#E/<xsl:value-of select="$deprecated/@ref"/></xsl:attribute>
							</xsl:when>
						</xsl:choose>
					</xsl:if>
				</xsd:obsolete>
			</xsl:if>

			<!-- Parents -->

			<xsl:for-each select="xs:appinfo/xse:parent">
				<xsd:parent>
					<xsl:attribute name="uri">
						<xsl:value-of select="@namespace"/>#E/<xsl:value-of select="@ref"/>
					</xsl:attribute>
				</xsd:parent>
			</xsl:for-each>

			<!-- Summary -->

			<xsl:apply-templates select="xs:documentation" />

			<!-- Remarks -->

			<xsl:apply-templates select="xs:appinfo/xse:remarks" />

			<!-- Related Topics -->

			<xsl:variable name="msiRefs" select="xs:appinfo/xse:msiRef" />
			<xsl:variable name="seeAlsos" select="xs:appinfo/xse:seeAlso" />

			<xsl:if test="$seeAlsos|$msiRefs">
				<ddue:relatedTopics>
					<xsl:apply-templates select="$seeAlsos" />
					<xsl:apply-templates select="$msiRefs"/>
				</ddue:relatedTopics>
			</xsl:if>
		</xsd:schemaDoc>
	</xsl:template>

	<xsl:template match="xs:documentation">
		<ddue:summary>
			<ddue:para>
				<xsl:apply-templates select="@* | node()" mode="copy"/>
			</ddue:para>
		</ddue:summary>
	</xsl:template>

	<xsl:template match="xse:remarks">
		<ddue:remarks>
			<ddue:content>
				<xsl:apply-templates select="@* | node()" mode="copy"/>
			</ddue:content>
		</ddue:remarks>
	</xsl:template>

	<xsl:template match="xse:seeAlso">
		<xsd:xmlEntityReference xml:space="preserve">http://schemas.microsoft.com/wix/2006/wi#E/<xsl:value-of select="@ref"/></xsd:xmlEntityReference>
	</xsl:template>

	<xsl:template match="xse:msiRef">
		<ddue:externalLink>
			<ddue:linkText>
				<xsl:choose>
					<xsl:when test="@table">
						Windows Installer Table <xsl:value-of select="@table"/>
					</xsl:when>
					<xsl:otherwise>
						Windows Installer Action <xsl:value-of select="@action"/>
					</xsl:otherwise>
				</xsl:choose>
			</ddue:linkText>
			<ddue:linkUri>
				<xsl:value-of select="@href"/>
			</ddue:linkUri>
		</ddue:externalLink>
	</xsl:template>

	<!-- Special conversion for certain HTML elements that are used in WiX documentation. -->

	<xsl:template match="xhtml:a|a" mode="copy">
		<ddue:externalLink>
			<ddue:linkText>
				<xsl:value-of select="text()"/>
			</ddue:linkText>
			<ddue:linkUri>
				<xsl:value-of select="@href"/>
			</ddue:linkUri>
		</ddue:externalLink>
	</xsl:template>

	<xsl:template match="xhtml:p|p" mode="copy">
		<ddue:para>
			<xsl:apply-templates mode="copy" />
		</ddue:para>
	</xsl:template>

	<xsl:template match="xhtml:b|b" mode="copy">
		<ddue:legacyBold>
			<xsl:apply-templates mode="copy" />
		</ddue:legacyBold>
	</xsl:template>

	<xsl:template match="xhtml:i|i" mode="copy">
		<ddue:legacyItalic>
			<xsl:apply-templates mode="copy" />
		</ddue:legacyItalic>
	</xsl:template>

	<xsl:template match="xhtml:ul|ul" mode="copy">
		<ddue:list class="bullet">
			<xsl:apply-templates mode="copy" />
		</ddue:list>
	</xsl:template>

	<xsl:template match="xhtml:ol|ol" mode="copy">
		<ddue:list class="ordered">
			<xsl:apply-templates mode="copy" />
		</ddue:list>
	</xsl:template>

	<xsl:template match="xhtml:li|li" mode="copy">
		<ddue:listItem>
			<ddue:para>
				<xsl:apply-templates mode="copy" />
			</ddue:para>
		</ddue:listItem>
	</xsl:template>

	<xsl:template match="xhtml:dl|dl" mode="copy">
		<ddue:definitionTable>
			<xsl:apply-templates mode="copy" />
		</ddue:definitionTable>
	</xsl:template>

	<xsl:template match="xhtml:dt|dt" mode="copy">
		<ddue:definedTerm>
			<xsl:apply-templates mode="copy" />
		</ddue:definedTerm>
	</xsl:template>

	<xsl:template match="xhtml:dd|dd" mode="copy">
		<ddue:definition>
			<ddue:para>
				<xsl:apply-templates mode="copy" />
			</ddue:para>
		</ddue:definition>
	</xsl:template>

	<xsl:template match="xhtml:dfn|dfn" mode="copy">
		<ddue:newTerm>
			<xsl:apply-templates mode="copy" />
		</ddue:newTerm>
	</xsl:template>

	<xsl:template match="xhtml:code|code" mode="copy">
		<ddue:codeInline xml:space="preserve"><xsl:apply-templates mode="copy" /></ddue:codeInline>
	</xsl:template>

	<xsl:template match="*" mode="copy">
		<xsl:element name="{local-name()}" namespace="">
			<xsl:apply-templates select="@*|node()" mode="copy"/>
		</xsl:element>
	</xsl:template>

	<xsl:template match="@*|text()" mode="copy">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()" mode="copy"/>
		</xsl:copy>
	</xsl:template>

</xsl:stylesheet>
