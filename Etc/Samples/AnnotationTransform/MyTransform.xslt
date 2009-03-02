<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:xsd="http://schemas.xsddoc.codeplex.com/schemaDoc/2009/3"
				xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
				xmlns:xs="http://www.w3.org/2001/XMLSchema"
				xmlns:doc="http://schemas.example.com/MyDocSchema">
	<xsl:output method="xml" indent="yes"/>

	<xsl:template match="*">
		<xsl:variable name="summary" select="xs:annotation/xs:documentation" />
		<xsl:variable name="remarks" select="xs:annotation/xs:appinfo/doc:remarks" />
		<xsl:variable name="seeAlsos" select="xs:annotation/xs:appinfo/doc:seeAlso" />

		<xsd:schemaDoc>
			<ddue:summary>
				<ddue:para>
					<xsl:apply-templates select="$summary/@*|$summary/node()" mode="copy"/>
				</ddue:para>
			</ddue:summary>
			<xsl:if test="$remarks">
				<ddue:remarks>
					<ddue:content>
						<ddue:para>
							<xsl:apply-templates select="$remarks/@*|$remarks/node()" mode="copy"/>
						</ddue:para>
					</ddue:content>
				</ddue:remarks>
			</xsl:if>
			<xsl:if test="$seeAlsos">
				<ddue:relatedTopics>
					<xsl:for-each select="$seeAlsos">
						<xsd:xmlEntityReference xml:space="preserve"><xsl:value-of select="@namespace"/>#E/<xsl:value-of select="@element"/></xsd:xmlEntityReference>
					</xsl:for-each>
				</ddue:relatedTopics>
			</xsl:if>
		</xsd:schemaDoc>
	</xsl:template>

	<xsl:template match="@*|node()" mode="copy">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()" mode="copy"/>
		</xsl:copy>
	</xsl:template>

</xsl:stylesheet>
