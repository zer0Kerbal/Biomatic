using System.Reflection;
using System.Runtime.InteropServices;

// using System.Runtime.CompilerServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Biomatic")]
[assembly: AssemblyDescription("Biomatic expands the capabilities of Kerbal Space Program (KSP)")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyCompany("KGEx")]
[assembly: AssemblyProduct("Biomatic")]
[assembly: AssemblyCopyright("GPLv3")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("Neutral")]
[assembly: KSPAssemblyDependency("ToolbarController", 1, 0)]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("9EBB3205-7041-4D60-9F8D-81BD0407DE94")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

[assembly: AssemblyFileVersion(Biomatic.Version.Number)]
// [assembly: AssemblyFileVersion("1.4.0.0")]


[assembly: KSPAssemblyDependency("ClickThroughBlocker", 1, 0)]
[assembly: KSPAssemblyDependency("ToolbarController", 1, 0)]