using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSISSharp;

namespace NSISSharpTestApp
{
    class Setup
    {
        static void Main(string[] args)
        {
            /* -----------------------------------------------------------------------------------------------
             * Setup code for creating the NSISSharp installation file.
             * ----------------------------------------------------------------------------------------------- */
            string NSISLocation = @"C:\Program Files (x86)\NSIS";

            //Create installation directory, and add files, which will be installed.
            Dir Core = new Dir();
            Core.Files.Add(new File(@"P:\Visual Studio\Projects\NSISSharp\NSISSharpTestApp\bin\Release\NSISSharp.dll"));
            Core.Files.Add(new File(@"P:\Visual Studio\Projects\NSISSharp\NSISSharpTestApp\bin\Release\NSISSharp.xml"));
            Core.Files.Add(new File(@"Z:\NSISSharp\readme.txt"));
            Core.Files.Add(new File(@"Z:\NSISSharp\changelog.txt"));
            Core.Files.Add(new File(@"Z:\NSISSharp\license.txt"));

            //Do the same as above, only for sub directories.
            Dir Scripts = Core.CreateSubDirectory("NSIS scripts");
            Scripts.Files.Add(new File(@"C:\Program Files (x86)\NSIS\Include\DotNetVersionNumber.nsh"));
            Scripts.Files.Add(new File(@"C:\Program Files (x86)\NSIS\Include\DotNetVer.nsh"));

            //VS example files.
            Dir Examples = Core.CreateSubDirectory("Examples");
            Examples.Files.Add(new File(@"Z:\NSISSharp\Examples\*.*", true));

            //Create a NSIS project.
            NSISProject project = new NSISProject(
                "NSISSharp", "v0.3", "OrangePlanet.org", "http://orangeplanet.org", "NSISSharp_v0.3.exe", Core,

                //Adding pages for the installer.
                //The following pages are default, and this is the same when you're not specifying any pages (except the license and components page).
                //It is possible to use certain pages only, instead of all; PAGE_INSTFILES and PAGE_LANGUAGE are always required, however.
                new Page(Page.PAGE_WELCOME),
                new Page(Page.PAGE_LICENSE, @"Z:\NSISSharp\license.txt"),
                new Page(Page.PAGE_COMPONENTS, true),
                new Page(Page.PAGE_DIRECTORY),
                new Page(Page.PAGE_INSTFILES),
                new Page(Page.PAGE_FINISH),
                new Page(Page.PAGE_UNINSTALL),
                new Page(Page.PAGE_LANGUAGE, null, Page.LANGUAGE_FILE_ENGLISH),
                
                new InstallType("Full"),
                new InstallType("Lite"),
                new InstallType("Minimal"),

                new Component("NSISSharp", "Contains the NSISSharp dll and various txt files (e.g. the readme.txt).", false, true, false, new int[] {0,1,2}, Core,
                    //x64/x86 specific actions. For now, only Registry related actions are supported. Future versions of NSISSharp should expand this to support more actions. 
                    new x64(Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v" + DotNetFramework.v40.Version + @"\AssemblyFoldersEx\NSISSharp").SetValue("", NSISProject.InstallDir)),
                    new x86(Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\v" + DotNetFramework.v40.Version + @"\AssemblyFoldersEx\NSISSharp").SetValue("", NSISProject.InstallDir))),
                new Component("NSIS Scripts", "Contains custom NSIS header files, related to .NET.", false, false, false, new int[] {0,1}, Scripts),
                new Component("Examples", "NSISSharp VS2010 examples.", false, false, false, new int[] {0}, Examples), 

                //Check if .NET Framework 4.0 is installed on the users system.
                DotNetFramework.CheckVersion(DotNetFramework.v40));

            //Retrieving .NET Framework version numbers, necessary if you want to use the .NET Framework version numbers (e.g. DotNetFramework.v40.Version).
            project.GetDotNetFrameworkVersion = true;
          
            //The values of the following four properties are generally left unchanged.
            //Therefore, these properties could be omitted, unless you want to change the values.
            project.CreateUninstaller = true;
            project.write_product_dir_regkey = false;
            project.write_product_uninst_key = true;
            project.RunAppAfterInstall = false;
            project.CreateStartMenuShortCut_Uninstaller = false;
            project.CreateStartMenuShortCut_Website = false;

            //Set icons, images, etc.
            project.InstallIcon = NSISLocation + @"\Contrib\Graphics\Icons\orange-install.ico";
            project.UnInstallIcon = NSISLocation + @"\Contrib\Graphics\Icons\orange-uninstall.ico";
            project.SetHeaderImage(NSISLocation + @"\Contrib\Graphics\Header\orange-r.bmp", true, true);
            project.SetWelcomeFinishPage(NSISLocation + @"\Contrib\Graphics\Wizard\orange.bmp");

            project.SetBrandingText(" ");

            //Path to the NSIS compiler.
            Compiler.NSISCompilerPath = NSISLocation + @"\makensis.exe";

            //Save script file. Set to true when testing/debugging. Default value = false.
            //Normally, this property could be omitted.
            Compiler.SaveScriptFile = true;

            //Build the installer using the settings from the above created NSIS project.
            Compiler.BuildInstaller(project, project.pages, false);
        }
    }
}
