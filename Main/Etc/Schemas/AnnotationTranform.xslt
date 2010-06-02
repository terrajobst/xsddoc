<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:xsd="http://schemas.xsddoc.codeplex.com/schemaDoc/2009/3"
                xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
                xmlns:xs="http://www.w3.org/2001/XMLSchema">
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="*">
    <xsl:variable name="schemaDoc" select="xs:annotation/xs:appinfo/xsd:schemaDoc" />
    <xsl:variable name="doc" select="xs:annotation/xs:documentation/text()" />

    <xsl:choose>
      <xsl:when test="$schemaDoc">
        <xsl:apply-templates select="$schemaDoc" mode="copy"/>
      </xsl:when>
      <xsl:when test="$doc">
        <xsd:schemaDoc>
          <ddue:summary>
            <ddue:para>
              <xsl:apply-templates select="$doc" mode="copy"/>
            </ddue:para>
          </ddue:summary>
        </xsd:schemaDoc>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="@*|node()" mode="copy">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()" mode="copy"/>
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>
