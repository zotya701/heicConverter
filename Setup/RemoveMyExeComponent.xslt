<?xml version="1.0" encoding="utf-8"?>
<!-- https://stackoverflow.com/questions/57163041/how-to-make-wix-heat-exe-retain-custom-file-id -->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:wix="http://schemas.microsoft.com/wix/2006/wi"
                xmlns="http://schemas.microsoft.com/wix/2006/wi"
                version="1.0"
                exclude-result-prefixes="xsl wix">
  <xsl:output method="xml"
              indent="yes"
              omit-xml-declaration="yes" />
  <xsl:strip-space elements="*" />
  <xsl:key name="ExeToRemove"
           match="wix:Component[ substring( wix:File/@Source, string-length( wix:File/@Source ) - 16 ) = 'heicConverter.exe' ]"
           use="@Id" />
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()" />
    </xsl:copy>
  </xsl:template>
  <xsl:template match="*[ self::wix:Component or self::wix:ComponentRef ][ key( 'ExeToRemove', @Id ) ]" />
</xsl:stylesheet>