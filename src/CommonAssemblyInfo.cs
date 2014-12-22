using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Resources;
using System.Runtime.InteropServices;

[assembly: AssemblyCopyright(XsdDocMetadata.Copyright)]
[assembly: AssemblyCompany("Immo Landwerth")]
[assembly: AssemblyProduct("XML Schema Documenter")]

[assembly: CLSCompliant(false)]
[assembly: ComVisible(false)]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: AssemblyVersion(XsdDocMetadata.Version)]
[assembly: AssemblyFileVersion(XsdDocMetadata.Version)]

internal static class XsdDocMetadata
{
    public const string Version = "14.11.22.0";
    public const string Copyright = "Copyright © 2009-2014 Immo Landwerth";
}