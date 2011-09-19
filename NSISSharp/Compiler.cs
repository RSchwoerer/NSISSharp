using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;


namespace NSISSharp
{
    /// <summary>
    /// Generates a NSIS script file, and calls the NSIS compiler. Ultimately the installer will be created.
    /// </summary>
    public static class Compiler
    {
        /// <summary>
        /// Path to the NSIS compiler.
        /// </summary>
        public static string NSISCompilerPath { get; set; }
        
        private static bool ssf = false;
        /// <summary>
        /// When set to true: the generated NSIS script file will not be deleted.
        /// </summary>
        public static bool SaveScriptFile
        {
            get
            {
                return ssf;
            }
            set
            {
                ssf = value;
            }
        }

        private static bool CloseConsole;
        internal static StringBuilder nsi;
        private static StringBuilder ComponentDescriptions;
        private static StringBuilder ComponentDescriptions_insertmacro;
        private static bool StartMenuDirectoryCreated = false;
        private static NSISProject Project;

        /// <summary>
        /// Creates a nsi script based on the defined NSIS project and builds an installer afterwards.
        /// </summary>
        /// <param name="_Project">A NSISProject that will be used for the installer.</param>
        /// <param name="Pages">An array containing pages that will be used for the installer.</param>
        /// <param name="AutoCloseConsoleWindow">Auto close console window after the installer has been built.</param>
        public static void BuildInstaller(NSISProject _Project, List<Page> Pages, bool AutoCloseConsoleWindow = true)
        {
            try
            {
                CloseConsole = AutoCloseConsoleWindow;
                nsi = new StringBuilder();

                Project = _Project;

                nsi.AppendLine("!define PRODUCT_NAME \"" + Project.product_name + "\"");
                nsi.AppendLine("!define PRODUCT_VERSION \"" + Project.product_version + "\"");
                nsi.AppendLine("!define PRODUCT_PUBLISHER \"" + Project.product_publisher + "\"");
                nsi.AppendLine("!define PRODUCT_WEB_SITE \"" + Project.product_web_site + "\"");
                nsi.AppendLine("!define PRODUCT_DIR_REGKEY \"" + Project.product_dir_regkey + "\"");
                nsi.AppendLine("!define PRODUCT_UNINST_KEY \"" + Project.product_uninst_key + "\"");
                nsi.AppendLine("!define PRODUCT_UNINST_ROOT_KEY \"" + Project.product_uninst_root_key + "\"");
                nsi.AppendLine("");

                nsi.AppendLine("!include \"" + Project.MUI_FILENAME + "\"");
                nsi.AppendLine("");

                /* MUI Settings. */
                if (Project.AbortWarning) //AbortWarning yes/no?
                {
                    nsi.AppendLine("!define MUI_ABORTWARNING");
                }

                nsi.AppendLine("!define MUI_ICON \"" + Project.InstallIcon + "\"");
                nsi.AppendLine("!define MUI_UNICON \"" + Project.UnInstallIcon + "\"");

                if (!string.IsNullOrEmpty(Project.HeaderImage))
                {
                    nsi.AppendLine("!define MUI_HEADERIMAGE");
                    nsi.AppendLine("!define MUI_HEADERIMAGE_BITMAP \"" + Project.HeaderImage +"\"");
                    if (!Project.StretchHeaderImage)
                    {
                        nsi.AppendLine("!define MUI_HEADERIMAGE_BITMAP_NOSTRETCH");
                    }
                    if (Project.HeaderImageRight)
                    {
                        nsi.AppendLine("!define MUI_HEADERIMAGE_RIGHT");
                    }
                }

                if (!string.IsNullOrEmpty(Project.WelcomeFinishImage))
                {
                    nsi.AppendLine("!define MUI_WELCOMEFINISHPAGE_BITMAP \"" + Project.WelcomeFinishImage + "\"");
                    if (!Project.StretchWelcomeFinishImage)
                    {
                        nsi.AppendLine("!define MUI_WELCOMEFINISHPAGE_BITMAP_NOSTRETCH");
                    }
                }

                nsi.AppendLine("");

                if(!string.IsNullOrEmpty(Project.BrandingText))
                {
                    if(!string.IsNullOrEmpty(Project.BrandingTextTrim))
                    {
                        if(Project.BrandingTextTrim != "left" && Project.BrandingTextTrim != "right" &&  Project.BrandingTextTrim != "center")
                        {
                            Project.BrandingTextTrim = "left";
                        }
                        nsi.AppendLine("BrandingText /TRIM"+ Project.BrandingTextTrim.ToUpper() + " \"" + Project.BrandingText + "\"");
                    }
                    else
                    {
                        nsi.AppendLine("BrandingText \"" + Project.BrandingText + "\"");
                    }
                }
                nsi.AppendLine("");


                //Check if there are any pages added in the users C# setup 'script'.
                if (Pages.Count == 0)
                {
                    //Add default pages for the installer.
                    Project.pages.Add(new Page(Page.PAGE_WELCOME));
                    Project.pages.Add(new Page(Page.PAGE_DIRECTORY));
                    Project.pages.Add(new Page(Page.PAGE_INSTFILES));
                    Project.pages.Add(new Page(Page.PAGE_FINISH));
                    Project.pages.Add(new Page(Page.PAGE_UNINSTALL));
                    Project.pages.Add(new Page(Page.PAGE_LANGUAGE, null, Page.LANGUAGE_FILE_ENGLISH));
                }

                //MUI(2) pages.
                bool ExistComponentPage = false;
                foreach (Page page in Pages)
                {
                    if (page.name == Page.PAGE_LICENSE)
                    {
                        nsi.AppendLine("!insertmacro " + page.name + " \"" + page.licensepath + "\"");
                    }
                    else if (page.name == Page.PAGE_FINISH)
                    {
                        if (Project.RunAppAfterInstall)
                        {
                            nsi.AppendLine("!define MUI_FINISHPAGE_RUN \"" + Project.InstallDirectory + "\\" + Project.MainAppName + "\"");
                        }
                        nsi.AppendLine("!insertmacro " + page.name);
                    }
                    else if (page.name == Page.PAGE_LANGUAGE)
                    {
                        nsi.AppendLine("!insertmacro " + page.name + " \"" + page.languagefile + "\"");
                    }
                    else if (page.name == Page.PAGE_COMPONENTS)
                    {
                        if (page.smalldescription)
                        {
                            nsi.AppendLine("!define MUI_COMPONENTSPAGE_SMALLDESC");
                        }
                        nsi.AppendLine("!insertmacro " + page.name);
                        ExistComponentPage = true;
                    }
                    else
                    {
                        nsi.AppendLine("!insertmacro " + page.name);
                    }
                }

                nsi.AppendLine(""); //MUI end.

                //Additional hdr includes.
                nsi.AppendLine("!include LogicLib.nsh");
                //if (Project.x64Actions.Capacity > 0 || Project.x86Actions.Capacity > 0)
                //{
                    nsi.AppendLine("!include x64.nsh");
                //}
                if (Project.GetDotNetFrameworkVersion)
                {
                    nsi.AppendLine("!include DotNetVersionNumber.nsh");
                }
                if (Project.CheckDotNetFrameworkVersion.Capacity > 0)
                {
                    nsi.AppendLine("!include DotNetVer.nsh");
                }
                nsi.AppendLine("");


                /* Configure certain settings. */
                nsi.AppendLine("Name \"" + Project.product_name + " " + Project.product_version + "\"");
                nsi.AppendLine("OutFile \"" + Project.outfile + "\"");
                nsi.AppendLine("InstallDir \"$PROGRAMFILES\\" + Project.product_name + "\"");
                if (Project.write_product_dir_regkey)
                {
                    nsi.AppendLine("InstallDirRegKey " + Project.product_uninst_root_key + " \"" + Project.product_dir_regkey + "\"" + " \"\"");
                }
                nsi.AppendLine(Project.ShowInstDetails(ShowHide.Show));
                
                if (Project.CreateUninstaller)
                {
                    nsi.AppendLine(Project.ShowUnInstDetails(ShowHide.Show));
                }
                
                nsi.AppendLine("RequestExecutionLevel user");
                nsi.AppendLine("");

                //First look if there has been any InstallType defined by the user.
                //If the conditional equals to true, iterate the InstallTypes collection.
                if (Project.InstallTypes.Count > 0)
                {
                    foreach (InstallType installtype in Project.InstallTypes)
                    {
                        nsi.AppendLine("InstType \"" + installtype.Name + "\"");
                    }
                    nsi.AppendLine("");
                }

                /* Pre install .NET actions. */
                DotNetActions();

                if (!ExistComponentPage)
                {
                    /* Main section */
                    MainInstallActions();
                }
                else
                {
                    //Component page exist. Loop through all components etc.
                    InstallComponents();
                }

                /* Post install actions */
                PostInstallActions();

                /* Custom actions */
                CustomActions();

                if (Project.CreateUninstaller)
                {
                    /* Uninstall actions */
                    Uninstall();
                }

                //Build installer.
                RunProcess(NSISCompilerPath, "/V3 script.nsi");
            }
            catch(Exception e)
            {
                //Error.
                Console.WriteLine(e.Message);
            }
        }

        private static void InstallComponents()
        {
            /* -------------------------------------------------------------------------------------------------------------------
             * Files will be installed per component, enabling the option to select the component that needs to be installed. 
             * ------------------------------------------------------------------------------------------------------------------- */
            //Count Section ID to prevent duplicates (ID must remain unique, obviously).
            int CountSectionId = 0;

            //Create two additional string builders for the Component descriptions. This will be added later to the main sb (nsi).
            //First sb = for the !defines. Second one is for the macro's.
            ComponentDescriptions = new StringBuilder();
            ComponentDescriptions_insertmacro = new StringBuilder();
            ComponentDescriptions_insertmacro.AppendLine("!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN");

            //Loop through each component.
            foreach (Component root in Project.components)
            {
                /* -------------------------------------------------------
                 * Determine/calculate where a Group Section will end.
                 * ------------------------------------------------------- */
                List<string> ComponentGroupEndList = new List<string>();

                Stack<Component> sCLoop = new Stack<Component>();
                Stack<Component> sC = new Stack<Component>();

                sCLoop.Push(root);

                while (sCLoop.Count > 0)
                {
                    Component c = sCLoop.Pop();
                    sC.Push(c);

                    foreach (Component cs in c.ComponentList)
                    {
                        if (cs.IsGroup)
                        {
                            sCLoop.Push(cs);
                        }
                    }

                    while (sC.Count > 0)
                    {
                        Component cCurrent = sC.Pop(); 

                        if (cCurrent.IsGroup)
                        {
                            int GroupCount = cCurrent.ComponentList.Count;

                            if (cCurrent.ComponentList[GroupCount - 1].IsGroup)
                            {
                                int countloop = 1;
                                foreach (Component cSub in cCurrent.ComponentList[GroupCount - 1].ComponentList)
                                {
                                    if (cSub.IsGroup)
                                    {
                                        //Loop.
                                        //Console.WriteLine(cSub.Name);
                                        if (sC.Count > 0)
                                        {
                                            //Remove group since we want the last group of the parent component, only.
                                            sC.Pop();
                                        }
                                        sC.Push(cSub);
                                    }
                                    else
                                    {
                                        //If there are no other groups in this component, thus this being the last component, the endpoint of parent group has been determined.
                                        if (countloop == cCurrent.ComponentList[GroupCount - 1].ComponentList.Count)
                                        {
                                            //Console.WriteLine(c.Name + " - " + cSub.Name);
                                            ComponentGroupEndList.Add(cSub.Name);
                                        }
                                    }
                                    countloop++; //Increase loop count.
                                }
                            }
                            else
                            {
                                //There are no other groups in this component, end point has been determined.
                                //Console.WriteLine(c.Name + " - " + cCurrent.ComponentList[GroupCount - 1].Name);
                                ComponentGroupEndList.Add(cCurrent.ComponentList[GroupCount - 1].Name);
                            }
                        }
                    }
                }
                
                //Clear stack so it can be reused. Under normal conditions, the stacks that we used earlier is empty already, however.
                sC.Clear();

                //Console.WriteLine(ComponentGroupEndList.Count);

                /* -----------------------------------------
                 * Creating sections, section groups, etc.
                 * ----------------------------------------- */

                sC.Push(root);

                while (sC.Count > 0)
                {
                    Component cCurrent = sC.Pop();

                    if (cCurrent.IsGroup)
                    {
                        if (!cCurrent.IsShortcutComponent)
                        {
                            string expanded = "";
                            string sgmode = "";

                            if (cCurrent.Expand)
                            {
                                expanded = "/e";
                            }
                            if (cCurrent.Bold)
                            {
                                sgmode = "!";
                            }

                            if (!string.IsNullOrEmpty(cCurrent.Description))
                            {
                                nsi.AppendLine("SectionGroup " + expanded + " \"" + sgmode + cCurrent.Name + "\" SEC" + CountSectionId);
                                CreateComponentDescription(cCurrent.Description, CountSectionId);
                                CountSectionId++;
                            }
                            else
                            {
                                nsi.AppendLine("SectionGroup " + expanded + " \"" + sgmode + cCurrent.Name + "\"");
                            }
                            List<Component> tempcomponentlist = new List<Component>();
                            for (int i = 0; i < cCurrent.ComponentList.Count; i++)
                            {
                                //Store the component temporary in a list collection.
                                //When all components are added, add them, in reverse order, to the stack.
                                //This ensures that every section is created in the order that the users has specified.
                                tempcomponentlist.Add(cCurrent.ComponentList[i]);
                                if (i == cCurrent.ComponentList.Count - 1)
                                {
                                    for (int count = tempcomponentlist.Count - 1; count >= 0; count--)
                                    {
                                        sC.Push(tempcomponentlist[count]);
                                    }
                                }

                                /*
                                if (cCurrent.ComponentList[i].IsGroup) //Sub component.
                                {
                                    //Store the component temporary in a list collection.
                                    //When all component groups are added, add them, in reverse order, to the stack.
                                    //This ensures that every section is created in the order that the users has specified.
                                    tempcomponentlist.Add(cCurrent.ComponentList[i]);
                                    if (i == cCurrent.ComponentList.Count - 1)
                                    {
                                        for (int count = tempcomponentlist.Count - 1; count >= 0; count--)
                                        {
                                            sC.Push(tempcomponentlist[count]);
                                        }
                                    }
                                }
                                else
                                {
                                    string optional;
                                    string smode;

                                    if (cCurrent.ComponentList[i].IsOptional)
                                    {
                                        optional = "/o";
                                    }
                                    else
                                    {
                                        optional = "";
                                    }
                                    if (cCurrent.ComponentList[i].Bold && cCurrent.ComponentList[i].Hidden)
                                    {
                                        smode = "-";
                                    }
                                    else if (cCurrent.ComponentList[i].Bold)
                                    {
                                        smode = "!";
                                    }
                                    else if (cCurrent.ComponentList[i].Hidden)
                                    {
                                        smode = "-";
                                    }
                                    else
                                    {
                                        smode = "";
                                    }

                                    if (!string.IsNullOrEmpty(cCurrent.ComponentList[i].Description))
                                    {
                                        nsi.AppendLine("Section " + optional + " \"" + smode + cCurrent.ComponentList[i].Name + "\" SEC" + CountSectionId);
                                        CreateComponentDescription(cCurrent.ComponentList[i].Description, CountSectionId);
                                        CountSectionId++;
                                    }
                                    else
                                    {
                                        nsi.AppendLine("Section " + optional + " \"" + smode + cCurrent.ComponentList[i].Name + "\"");
                                    }

                                    //IntType
                                    if (cCurrent.ComponentList[i].InstallType != null)
                                    {
                                        string temp = "SectionIn ";
                                        foreach (int index in InstallType.ConvertIndex(cCurrent.ComponentList[i].InstallType))
                                        {
                                            temp += index.ToString() + " ";
                                        }
                                        nsi.AppendLine(temp);
                                    }
                                    /* Add files etc. * /
                                    AddFilesToSections(cCurrent.ComponentList[i].Directories);
                                    ComponentSpecificCustomActions(cCurrent.ComponentSpecificActions);
                                    nsi.AppendLine("SectionEnd");

                                    foreach (string s in ComponentGroupEndList)
                                    {
                                        if (cCurrent.ComponentList[i].Name == s)
                                        {
                                            nsi.AppendLine("SectionGroupEnd");
                                        }
                                    }
                                }
                                */
                            }
                        }
                        else
                        {
                            /* -------------------------------------------------------------------------
                             * Component is a shortcut component. Adding shortcuts to section etc. 
                             * ------------------------------------------------------------------------- */
                            
                            bool CreateShortcutSectionGroup = false;
                            List<File> DesktopShortcuts = new List<File>();
                            List<File> StartMenuShortcuts = new List<File>();

                            foreach (KeyValuePair<string, Dir> kvp in Project.dir.dirinstances.instances)
                            {
                                foreach (File f in kvp.Value.Files)
                                {
                                    //Checking if there needs to be a section group created for the shortcuts; if there are no shortcuts, then the section group, including its section is not needed.
                                    if (f.createshortcut != CreateShortCut.None)
                                    {
                                        if (!CreateShortcutSectionGroup)
                                        {
                                            CreateShortcutSectionGroup = true;
                                        }

                                        if (f.createshortcut == CreateShortCut.Desktop)
                                        {
                                            DesktopShortcuts.Add(f);
                                        }
                                        else if (f.createshortcut == CreateShortCut.StartMenu)
                                        {
                                            StartMenuShortcuts.Add(f);
                                        }
                                        else
                                        {
                                            //Alll shortcuts.
                                            DesktopShortcuts.Add(f);
                                            StartMenuShortcuts.Add(f);
                                        }
                                    }
                                }
                            }

                            if (CreateShortcutSectionGroup)
                            {
                                //Extra condition: normally, this would always equal to true.
                                if (DesktopShortcuts.Count > 0 || StartMenuShortcuts.Count > 0)
                                {
                                    /* Descriptions */
                                    if (!string.IsNullOrEmpty(cCurrent.Description))
                                    {
                                        nsi.AppendLine("SectionGroup \"" + cCurrent.Name + "\" SEC" + CountSectionId);
                                        CreateComponentDescription(cCurrent.Description, CountSectionId);
                                        CountSectionId++;
                                    }
                                    else
                                    {
                                        nsi.AppendLine("SectionGroup \"" + cCurrent.Name + "\"");
                                    }
                                    

                                    string optional = "";
                                    if (cCurrent.ComponentList[0].IsOptional && cCurrent.ComponentList[1].IsOptional)
                                    {
                                        optional = "/o";
                                    }

                                    if (DesktopShortcuts.Count > 0)
                                    {
                                        /* Descriptions */
                                        if (!string.IsNullOrEmpty(cCurrent.ComponentList[0].Description))
                                        {
                                            nsi.AppendLine("Section " + optional + " \"Desktop\" SEC" + CountSectionId);
                                            CreateComponentDescription(cCurrent.ComponentList[0].Description, CountSectionId);
                                            CountSectionId++;
                                        }
                                        else
                                        {
                                            nsi.AppendLine("Section " + optional + " \"Desktop\"");
                                        }
                                        
                                        //InstType.
                                        if (cCurrent.ComponentList[0].InstallType != null)
                                        {
                                            string temp = "SectionIn ";
                                            foreach (int index in InstallType.ConvertIndex(cCurrent.ComponentList[0].InstallType))
                                            {
                                                temp += index.ToString() + " ";
                                            }
                                            nsi.AppendLine(temp);
                                        }
                                        //Desktop shortcuts.
                                        foreach (File f in DesktopShortcuts)
                                        {
                                            string ShortcutNameWithoutExtension = f.shortcutname;
                                            if (string.IsNullOrEmpty(f.shortcutname))
                                            {
                                                ShortcutNameWithoutExtension = RemoveExtensionFromFileName(f.filename);
                                            }

                                            CreateDesktopShortcut(ShortcutNameWithoutExtension, f);
                                        }
                                        nsi.AppendLine("SectionEnd");
                                    }
                                    if (StartMenuShortcuts.Count > 0)
                                    {
                                        /* Descriptions */
                                        if (!string.IsNullOrEmpty(cCurrent.ComponentList[1].Description))
                                        {
                                            nsi.AppendLine("Section " + optional + " \"Startmenu\" SEC" + CountSectionId);
                                            CreateComponentDescription(cCurrent.ComponentList[1].Description, CountSectionId);
                                            CountSectionId++;
                                        }
                                        else
                                        {
                                            nsi.AppendLine("Section " + optional + " \"Startmenu\"");
                                        }

                                        //InstType.
                                        if (cCurrent.ComponentList[1].InstallType != null)
                                        {
                                            string temp = "SectionIn ";
                                            foreach (int index in InstallType.ConvertIndex(cCurrent.ComponentList[1].InstallType))
                                            {
                                                temp += index.ToString() + " ";
                                            }
                                            nsi.AppendLine(temp);
                                        }
                                        //Startmenu shortcuts.
                                        foreach (File f in StartMenuShortcuts)
                                        {
                                            string ShortcutNameWithoutExtension = f.shortcutname;
                                            if (string.IsNullOrEmpty(f.shortcutname))
                                            {
                                                ShortcutNameWithoutExtension = RemoveExtensionFromFileName(f.filename);
                                            }

                                            CreateStartMenuShortcut(ShortcutNameWithoutExtension, f);
                                        }
                                        nsi.AppendLine("SectionEnd");
                                    }

                                    foreach (string s in ComponentGroupEndList)
                                    {
                                        //Since the Shortcut component always have two elements (Desktop and Start Menu), 
                                        //the following conditional will use the last element in the component, which is '1'.
                                        if (cCurrent.ComponentList[1].Name == s)
                                        {
                                            nsi.AppendLine("SectionGroupEnd");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        string optional;
                        string smode;
                        if (cCurrent.IsOptional)
                        {
                            optional = "/o";
                        }
                        else
                        {
                            optional = "";
                        }
                        if (cCurrent.Bold && cCurrent.Hidden)
                        {
                            smode = "-";
                        }
                        else if (cCurrent.Bold)
                        {
                            smode = "!";
                        }
                        else if (cCurrent.Hidden)
                        {
                            smode = "-";
                        }
                        else
                        {
                            smode = "";
                        }

                        if (!string.IsNullOrEmpty(cCurrent.Description))
                        {
                            nsi.AppendLine("Section " + optional + " \"" + smode + cCurrent.Name + "\" SEC" + CountSectionId);
                            CreateComponentDescription(cCurrent.Description, CountSectionId);
                            CountSectionId++;
                        }
                        else
                        {
                            nsi.AppendLine("Section " + optional + " \"" + smode + cCurrent.Name + "\"");
                        }

                        //InstType.
                        if (cCurrent.InstallType != null)
                        {
                            string temp = "SectionIn ";
                            foreach (int index in InstallType.ConvertIndex(cCurrent.InstallType))
                            {
                                temp += index.ToString() + " ";
                            }
                            nsi.AppendLine(temp);
                        }
                        AddFilesToSections(cCurrent.Directories);
                        ComponentSpecificCustomActions(cCurrent.ComponentSpecificActions);
                        nsi.AppendLine("SectionEnd");

                        foreach (string s in ComponentGroupEndList)
                        {
                            if (cCurrent.Name == s)
                            {
                                nsi.AppendLine("SectionGroupEnd");
                            }
                        }
                    }
                }
            }

            nsi.AppendLine("");


            ComponentDescriptions_insertmacro.AppendLine("!insertmacro MUI_FUNCTION_DESCRIPTION_END");

            /* ------------------------------------------------------------------------------------------------------------------------
             * Add the component descriptions to the main string builder, so that it is included with the script that will be created 
             * ------------------------------------------------------------------------------------------------------------------------ */
            if (ComponentDescriptions.Length > 0 && ComponentDescriptions_insertmacro.Length > 0)
            {
                nsi.AppendLine(ComponentDescriptions.ToString());
                nsi.AppendLine(ComponentDescriptions_insertmacro.ToString());

                nsi.AppendLine("");
            }
        }

        private static void AddFilesToSections(List<Dir> DirListCollection)
        {
            /* ---------------------------------
             * Add files to sections/component; Component page only, currently.
             * --------------------------------- */
            nsi.AppendLine(" " + Project.SetOverWrite(OverWriteMode.Ifnewer));
            foreach (Dir dir in DirListCollection)
            {
                nsi.AppendLine(" " + Project.SetOutPath(dir.Path));
                foreach (File file in dir.Files)
                {
                    //Check if the recursive flag has been set.
                    if (file.recursive)
                    {
                        nsi.AppendLine(" File /r \"" + file.sourcepath + "\"");
                    }
                    else
                    {
                        nsi.AppendLine(" File \"" + file.sourcepath + "\"");
                    }
                }
            }
        }

        private static void CreateComponentDescription(string Description, int CountSectionId)
        {
            ComponentDescriptions.AppendLine("!define DESC_SEC" + CountSectionId + " \"" + Description + "\"");
            ComponentDescriptions_insertmacro.AppendLine("!insertmacro MUI_DESCRIPTION_TEXT ${SEC" + CountSectionId + "} \"${DESC_SEC" + CountSectionId + "}\"");
        }

        private static void ComponentSpecificCustomActions(List<object> ComponentSpecificActions)
        {
            if (ComponentSpecificActions.Count > 0)
            {
                List<x64> x64ActionList = new List<x64>();
                List<x86> x86ActionList = new List<x86>();
                foreach (object o in ComponentSpecificActions)
                {
                    //The option that o = an object array could be possible, so do some magic to workaround this.
                    List<object> CSAList = new List<object>(); //CSA = Component Specific Actions. 
                    if (o is object[])
                    {
                        foreach (object tempobj in o as object[])
                        {
                            CSAList.Add(tempobj);
                        }
                    }
                    else
                    {
                        CSAList.Add(o);
                    }

                    foreach (object CSA in CSAList)
                    {
                        if (CSA is RegValue)
                        {
                            //Adding action to the list collection that is in the NSISProject object. This is necessary for the Uninstall method which only looks at the collections from the NSISProject object.
                            Project.RegistryValues.Add(CSA as RegValue);
                            nsi.AppendLine((CSA as RegValue).GetValue());
                        }
                        else if (o is x64)
                        {
                            Project.x64Actions.Add(CSA as x64);
                            x64ActionList.Add(CSA as x64);
                        }
                        else if (CSA is x86)
                        {
                            Project.x86Actions.Add(CSA as x86);
                            x86ActionList.Add(CSA as x86);
                        }
                    }
                }

                if (x64ActionList.Count > 0 || x86ActionList.Count > 0)
                {
                    nsi.AppendLine("${If} ${RunningX64}"); //x64.
                    if (x64ActionList.Count > 0)
                    {
                        foreach (x64 x64actions in x64ActionList)
                        {
                            foreach (object action in x64actions.x64Actions)
                            {
                                if (action is RegValue) //For now only regvalue is supported as custom action, hence this check only.
                                {
                                    nsi.AppendLine((action as RegValue).GetValue());
                                }
                            }
                        }
                    }
                    nsi.AppendLine("${Else}"); //x86.
                    if (x86ActionList.Count > 0)
                    {
                        foreach (x86 x86actions in x86ActionList)
                        {
                            foreach (object action in x86actions.x86Actions)
                            {
                                if (action is RegValue)
                                {
                                    nsi.AppendLine((action as RegValue).GetValue());
                                }
                            }
                        }
                    }
                    nsi.AppendLine("${EndIf}");
                }
            }
        }

        private static void DotNetActions()
        {
            /* For now it checks for a specific .NET version only. */
            if (Project.GetDotNetFrameworkVersion || Project.CheckDotNetFrameworkVersion.Capacity > 0)
            {
                nsi.AppendLine("Section \"-DotNet\"");

                if (Project.CheckDotNetFrameworkVersion.Capacity > 0)
                {   
                    string HasDotNetVar = "${HasDotNet4.0}";
                    
                    if (Project.CheckDotNetFrameworkVersion[0]._Version == "3.5")
                    {
                        HasDotNetVar = "${HasDotNet3.5}";
                    }
                    else if (Project.CheckDotNetFrameworkVersion[0]._Version == "3.0")
                    {
                        HasDotNetVar = "${HasDotNet3.0}";
                    }
                    else if(Project.CheckDotNetFrameworkVersion[0]._Version == "2.0")
                    {
                        HasDotNetVar = "${HasDotNet2.0}";
                    }

                    nsi.AppendLine(" ${If} "+ HasDotNetVar);
                    nsi.AppendLine("  DetailPrint \"The required .NET Framework version is already installed.\"");
                    nsi.AppendLine(" ${Else}");
                    nsi.AppendLine("  MessageBox MB_OK \"It appears that version: "+Project.CheckDotNetFrameworkVersion[0]._Version + " of the .NET Framework is not yet installed, please install this first.\"");
                    nsi.AppendLine("  Abort");
                    nsi.AppendLine(" ${EndIf}");
                    nsi.AppendLine("");
                }
                if (Project.GetDotNetFrameworkVersion)
                {
                    nsi.AppendLine(" !insertmacro GetAllVersionNumbers");
                }

                nsi.AppendLine("SectionEnd");
                nsi.AppendLine("");
            }
        }

        private static void MainInstallActions()
        {
            /* Contains all, predefined, main installation actions. */

            //Section start.
            nsi.AppendLine("Section \"MainSection\" SEC01");

            nsi.AppendLine(" " + Project.SetOutPath(Project.InstallDirectory));
            nsi.AppendLine(" " + Project.SetOverWrite(OverWriteMode.Ifnewer));
            foreach(KeyValuePair<string, Dir> kvp in Project.dir.dirinstances.instances)
            {
                //Looping through all files that needs to to installed.
                foreach (File f in kvp.Value.Files)
                {
                    if (kvp.Key != "root")
                    {
                        nsi.AppendLine(" " + Project.SetOutPath(kvp.Value.Path));
                    }

                    //Check if the recursive flag has been set.
                    if (f.recursive)
                    {
                        nsi.AppendLine(" File /r \"" + f.sourcepath + "\"");
                    }
                    else
                    {
                        nsi.AppendLine(" File \"" + f.sourcepath + "\"");
                    }
                    //Check if there needs to be any shortcut created.
                    if (f.createshortcut != CreateShortCut.None)
                    {
                        string ShortcutNameWithoutExtension = f.shortcutname;
                        if (string.IsNullOrEmpty(f.shortcutname))
                        {
                            //Remove extension from filename.
                            ShortcutNameWithoutExtension = RemoveExtensionFromFileName(f.filename);
                        }

                        //Creating shortcuts.
                        if (f.createshortcut == CreateShortCut.StartMenu)
                        {
                            //Create a startmenu shortcut.
                            CreateStartMenuShortcut(ShortcutNameWithoutExtension, f);
                        }
                        else if (f.createshortcut == CreateShortCut.Desktop)
                        {
                            //Create a desktop shortcut.
                            CreateDesktopShortcut(ShortcutNameWithoutExtension, f);
                        }
                        else
                        {
                            //Create all shortcuts.
                            CreateStartMenuShortcut(ShortcutNameWithoutExtension, f);
                            CreateDesktopShortcut(ShortcutNameWithoutExtension, f);
                        }
                    }
                }
            }


            //Section end.
            nsi.AppendLine("SectionEnd");
            nsi.AppendLine("");
        }

        private static void PostInstallActions()
        {
            /* Contains all, predefined, post install actions (install Actions that happens after the main installation part). */

            //Section start
            nsi.AppendLine("Section -Post");

            if (!(string.IsNullOrEmpty(Project.product_web_site)) && (Project.CreateStartMenuShortCut_Website))
            {
                /* Create an internet shortcut of the product website. */

                //Create a file object of the url file.
                File ProductWebsiteUrlFile = new File(Project.InstallDirectory + "\\" + Project.product_name + ".url");

                //Create url file in the installation directory of the product.
                nsi.AppendLine(" WriteIniStr \"" + ProductWebsiteUrlFile.sourcepath + "\" \"InternetShortcut\" \"URL\" \"" + Project.product_web_site + "\"");
                
                //Create shortcut of the url file in startmenu.
                CreateStartMenuShortcut("Website", ProductWebsiteUrlFile);
            }

            if (Project.CreateUninstaller)
            {
                //Write uninstaller.
                File uninstaller = new File(Project.InstallDirectory + "\\uninst.exe");
                nsi.AppendLine(" WriteUninstaller \"" + uninstaller.sourcepath + "\"");

                //Startmenu shortcut only when below condition is true.
                if (Project.CreateStartMenuShortCut_Uninstaller)
                {
                    CreateStartMenuShortcut("Uninstall", uninstaller);
                }

                //Uninstall registry keys.
                if (Project.write_product_uninst_key)
                {
                    RegistryKey subkey = Registry.LocalMachine.CreateSubKey(Project.product_uninst_key);
                    subkey.SetValue("DisplayName", @"$(^Name)", false);
                    subkey.SetValue("UninstallString", Project.InstallDirectory + "\\uninst.exe", false);
                    subkey.SetValue("DisplayIcon", Project.InstallDirectory + "\\" + Project.MainAppName, false);
                    subkey.SetValue("DisplayVersion", Project.product_version, false);
                    subkey.SetValue("URLInfoAbout", Project.product_web_site, false);
                    subkey.SetValue("Publisher", Project.product_publisher, false);
                }
            }


            //Writing registry keys.
            if (Project.write_product_dir_regkey)
            {
                Registry.LocalMachine.CreateSubKey(Project.product_dir_regkey).SetValue("", Project.InstallDirectory + "\\" + Project.MainAppName, false);
            }

            //Section end.
            nsi.AppendLine("SectionEnd");
            nsi.AppendLine("");
        }

        private static void CustomActions()
        {
            /* Contains all install actions that are specified by the user. Will only be used when there is no component page used. */

            nsi.AppendLine("Section \"-CustomActions\"");
            //Looping through the registry values.
            if (Project.RegistryValues.Capacity != 0)
            {
                foreach (RegValue value in Project.RegistryValues)
                {
                    nsi.AppendLine(value.GetValue());
                }
            }

            //Checking for specific x64/x86 actions.
            if (Project.x64Actions.Capacity > 0 || Project.x86Actions.Capacity > 0)
            {
                //Check if system is running x64.
                nsi.AppendLine("${If} ${RunningX64}"); //x64.
                if (Project.x64Actions.Capacity > 0)
                {
                    //x64 actions.
                    foreach (x64 x64ActionObject in Project.x64Actions)
                    {
                        foreach (object action in x64ActionObject.x64Actions)
                        {
                            if (action is RegValue)
                            {
                                nsi.AppendLine((action as RegValue).GetValue());
                            }
                        }
                    }
                }
                nsi.AppendLine("${Else}"); //x86.
                if (Project.x86Actions.Capacity > 0)
                {
                    //x86 actions.
                    foreach(x86 x86ActionObject in Project.x86Actions)
                    {
                        foreach (object action in x86ActionObject.x86Actions)
                        {
                            if (action is RegValue)
                            {
                                nsi.AppendLine((action as RegValue).GetValue());
                            }
                        }
                    }
                }
                nsi.AppendLine("${EndIf}");
            }

            nsi.AppendLine("SectionEnd");
            nsi.AppendLine("");
        }

        private static void Uninstall()
        {
            /* Contains all, predefined, uninstall actions. */

            //Some uninstall callbacks.
            nsi.AppendLine("Function un.onUninstSuccess");
            nsi.AppendLine(" HideWindow");
            nsi.AppendLine(" MessageBox MB_ICONINFORMATION|MB_OK \""+Project.product_name+" was successfully removed from your computer.\"");
            nsi.AppendLine("FunctionEnd");
            nsi.AppendLine("");

            nsi.AppendLine("Function un.onInit");
            nsi.AppendLine(" MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 \"Are you sure you want to completely remove "+ Project.product_name +" and all of its components?\" IDYES +2");
            nsi.AppendLine(" Abort");
            nsi.AppendLine("FunctionEnd");
            nsi.AppendLine("");


            //Create an uninstall section; this should remove all files, registry keys etc that were installed by the installer.
            nsi.AppendLine("Section Uninstall");

            if (Project.GetDotNetFrameworkVersion)
            {
                nsi.AppendLine(" !insertmacro GetAllVersionNumbers");
            }
            
            /*
            //Removing files. 
            foreach(KeyValuePair<string, Dir> kvp in Project.dir.dirinstances.instances)
            {
                foreach (File f in kvp.Value.Files)
                {
                    //If the recursive flag is not set, delete file, else skip.
                    //When the recursive flag has been set, RMDIR /r will do the job of removing all files, (sub)directories, etc.
                    if (!f.recursive)
                    {
                        f.Delete(kvp.Value.Path);
                    }

                    //Check if one of more shortcuts were created by the installer.
                    if (f.createshortcut != CreateShortCut.None)
                    {
                        string ShortcutNameWithoutExtention = f.shortcutname;
                        if (string.IsNullOrEmpty(f.shortcutname))
                        {
                            ShortcutNameWithoutExtention = RemoveExtensionFromFileName(f.filename);
                        }

                        //If there were any shortcuts created, the following code will delete them all.
                        DeleteShortCut(Project, ShortcutNameWithoutExtention, f.createshortcut);
                    }
                }
            }
            */


            //Removing directories.
            Stack<Dir> tempstack = new Stack<Dir>();
            Stack<Dir> tempstack_RecursiveDirectories = new Stack<Dir>();
            foreach (KeyValuePair<string, Dir> kvp in Project.dir.dirinstances.instances)
            {
                bool recursive = false;
                //Removing files.
                foreach (File f in kvp.Value.Files)
                {
                    //If the recursive flag is not set, delete file, else skip.
                    //When the recursive flag has been set, RMDIR /r will do the job of removing all files, (sub)directories, etc.
                    if (f.recursive)
                    {
                        recursive = true;
                    }
                    else
                    {
                        f.Delete(kvp.Value.Path);
                    }

                    //Check if one of more shortcuts were created by the installer.
                    if (f.createshortcut != CreateShortCut.None)
                    {
                        string ShortcutNameWithoutExtention = f.shortcutname;
                        if (string.IsNullOrEmpty(f.shortcutname))
                        {
                            ShortcutNameWithoutExtention = RemoveExtensionFromFileName(f.filename);
                        }

                        //If there were any shortcuts created, the following code will delete them all.
                        DeleteShortCut(ShortcutNameWithoutExtention, f.createshortcut);
                    }
                }

                if (recursive)
                {
                    tempstack_RecursiveDirectories.Push(kvp.Value);
                }
                else
                {
                    tempstack.Push(kvp.Value);
                }
                /*
                //Skip root dir. Root dir will be deleted after all subdirectories are deleted.
                if (kvp.Key != "root")
                {
                    kvp.Value.Delete();
                }
                */
            }


            //Delete uninstaller.
            if (Project.CreateUninstaller)
            {
                File uninstaller = new File(Project.InstallDirectory + "\\uninst.exe");
                uninstaller.Delete(Project.InstallDirectory);
            }
            //Delete Startmenu shortcut of uninstaller, only when following condition is true.
            if (Project.CreateStartMenuShortCut_Uninstaller && Project.CreateUninstaller)
            {

                DeleteShortCut("Uninstall", CreateShortCut.StartMenu);
            }

            //Delete internet shortcut of the product website, if the following condition is true.
            if (!(string.IsNullOrEmpty(Project.product_web_site)) && (Project.CreateStartMenuShortCut_Website))
            {
                //Create a file object of the url file.
                File ProductWebsiteUrlFile = new File(Project.InstallDirectory + "\\" + Project.product_name + ".url");
                ProductWebsiteUrlFile.Delete(Project.InstallDirectory);

                //Delete shortcut of the url file in startmenu.
                DeleteShortCut("Website", CreateShortCut.StartMenu);
            }

            //Delete directories, which contains files that need to be deleted recursively.
            while (tempstack_RecursiveDirectories.Count > 0)
            {
                try
                {
                    Dir dir = tempstack_RecursiveDirectories.Pop();
                    dir.Delete(true);
                }
                catch (Exception)
                {
                    continue;
                }
            }
            //Delete directories.
            while (tempstack.Count > 0)
            {
                try
                {
                    Dir dir = tempstack.Pop();
                    dir.Delete();
                }
                catch(Exception)
                {
                    continue;
                }
            }

            new Dir(@"$SMPROGRAMS\" + Project.product_name).Delete();
            //new Dir(Project.InstallDirectory).Delete();

            //Deleting registry keys that were created by the installer.
            if (Project.write_product_dir_regkey)
            {
                Registry.LocalMachine.OpenSubKey(Project.product_dir_regkey).Delete();
            }
            if (Project.write_product_uninst_key)
            {
                Registry.LocalMachine.OpenSubKey(Project.product_uninst_key).Delete();
            }

            //Deleting registry keys that were specified by the user, when necessary.
            if (Project.RegistryValues.Capacity != 0)
            {
                foreach (RegValue value in Project.RegistryValues)
                {
                    RegistryKey basekey = value.GetBaseKey();
                    basekey.Delete();
                }
            }

            //Checking for specific x64/x86 actions.
            if (Project.x64Actions.Count > 0 || Project.x86Actions.Count > 0)
            {
                //Check if system is running x64.
                nsi.AppendLine("${If} ${RunningX64}"); //x64.
                if (Project.x64Actions.Capacity > 0)
                {
                    //x64 actions.
                    foreach (x64 x64ActionObject in Project.x64Actions)
                    {
                        foreach (object action in x64ActionObject.x64Actions)
                        {
                            if (action is RegValue)
                            {
                                (action as RegValue).GetBaseKey().Delete();                                
                            }
                        }
                    }
                }
                nsi.AppendLine("${Else}"); //x86.
                if (Project.x86Actions.Count > 0)
                {
                    //x86 actions.
                    foreach (x86 x86ActionObject in Project.x86Actions)
                    {
                        foreach (object action in x86ActionObject.x86Actions)
                        {
                            if (action is RegValue)
                            {
                                (action as RegValue).GetBaseKey().Delete();
                            }
                        }
                    }
                }
                nsi.AppendLine("${EndIf}");
            }

            /* -----------------------------------------------------------
             * // Auto close uninstaller. //
             * 
             * TO DO: might give the user the option to set this on/off.
             * ----------------------------------------------------------- */
            nsi.AppendLine(" SetAutoClose true");

            //Close NSIS section.
            nsi.AppendLine("SectionEnd");
            nsi.AppendLine("");
        }

        private static string RemoveExtensionFromFileName(string filename)
        {
            /* -------------------------------------------------------------------------------------------
             * Removes the extension of a filename. 
             * TO DO: Multiple periods in filename support.
             * ------------------------------------------------------------------------------------------- */
            string[] Arr = filename.Split(new char[] { '.' });
            string FileNameWithoutExtension = Arr[0];

            return FileNameWithoutExtension;
        }

        private static void CreateStartMenuShortcut(string ShortCutNameWithoutExtension, File f)
        {
            /* -------------------------------------------------------------------
             * Create directory in startmenu if it doesn't exist yet.
             * TO DO: Support for sub directories in startmenu\application dir\
             * ------------------------------------------------------------------- */

            if (!StartMenuDirectoryCreated)
            {
                nsi.AppendLine(" CreateDirectory \"$SMPROGRAMS\\" + Project.product_name + "\"");
                StartMenuDirectoryCreated = true;
            }
            nsi.AppendLine(" CreateShortCut \"$SMPROGRAMS\\" + Project.product_name + "\\" + ShortCutNameWithoutExtension + ".lnk\" \"" + Project.InstallDirectory + "\\" + f.filename + "\"");
        }

        private static void CreateDesktopShortcut(string ShortCutNameWithoutExtension, File f)
        {
            /* Creates a desktop shortcut. */
            nsi.AppendLine(" CreateShortCut \"$DESKTOP\\" + ShortCutNameWithoutExtension + ".lnk\" \"" + Project.InstallDirectory + "\\" + f.filename + "\"");   
        }

        private static void DeleteShortCut(string ShortCutWithoutExtension, CreateShortCut TypeShortcut)
        {
            /* ----------------------------------------------------------------------------
             * Delete a shortcut of any type supported by NSISSharp.
             * TO DO: Support for deleting sub directories in startmenu\application dir\
             * ---------------------------------------------------------------------------- */

            string DeleteStartMenuShortCut = " Delete \"$SMPROGRAMS\\" + Project.product_name + "\\" + ShortCutWithoutExtension + ".lnk\"";
            string DeleteDesktopShortCut = " Delete \"$DESKTOP\\" + ShortCutWithoutExtension + ".lnk\"";

            if (TypeShortcut == CreateShortCut.StartMenu)
            {
                nsi.AppendLine(DeleteStartMenuShortCut);
            }
            else if (TypeShortcut == CreateShortCut.Desktop)
            {
                nsi.AppendLine(DeleteDesktopShortCut);
            }
            else //TypeShortcut == All.
            {
                nsi.AppendLine(DeleteStartMenuShortCut);
                nsi.AppendLine(DeleteDesktopShortCut);
            }
        }

        private static void OutputNSISScriptFile()
        {
            using (StreamWriter sw = new StreamWriter("script.nsi"))
            {
                sw.WriteLine(nsi.ToString());
            }
            
            //Console.WriteLine(nsi);
            //Console.ReadLine();
        }

        private static void RunProcess(string process, string command)
        {
            OutputNSISScriptFile();

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(process, command)
            {
                UseShellExecute = false
            };

            p.Start();
            p.WaitForExit();

            //Delete script file if the ssf flag has been set to true.
            if (!ssf)
            {
                System.IO.File.Delete("script.nsi");
            }

            if (!CloseConsole)
            {
                //Exit console after a key by the user is pressed.
                Console.WriteLine("");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }
        }
    }
}
