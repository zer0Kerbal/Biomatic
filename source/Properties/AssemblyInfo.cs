using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// [assembly: KSPAssemblyDependency("ClickThroughBlocker", 1, 0)]
[assembly: KSPAssemblyDependency("ToolbarController", 1, 0)]

[assembly: AssemblyTitle("Biomatic")]
[assembly: AssemblyDescription("Biomatic expands the capabilities of Kerbal Space Program (KSP)")]
[assembly: AssemblyCompany("zer0Kerbal")]
[assembly: AssemblyProduct("Biomatic")]
[assembly: AssemblyCopyright("GPL-3.0; © 2014 Matt Reed, 2019,2023 zer0Kerbal")]
[assembly: AssemblyTrademark("™ 2014 Matt Reed, 2019,2023 zer0Kerbal")]
[assembly: AssemblyCulture("Neutral")]

#if DEBUG 
  [assembly: AssemblyConfiguration("Debug")]
#else
   [assembly: AssemblyConfiguration("Release")]
#endif
[assembly: ComVisible(false)]

[assembly: Guid("9EBB3205-7041-4D60-9F8D-81BD0407DE94")]