using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSISSharp
{
    /// <summary>
    /// Creates a NSIS project.
    /// </summary>
    public class NSISProject
    {
        /// <summary>
        /// The name of the product.
        /// </summary>
        public string product_name { get; set; }
        /// <summary>
        /// Product version.
        /// </summary>
        public string product_version { get; set; }
        /// <summary>
        /// Publisher of the product.
        /// </summary>
        public string product_publisher { get; set; }
        /// <summary>
        /// Website related to the product.
        /// </summary>
        public string product_web_site { get; set; }

        public bool write_product_dir_regkey { get; set; }
        public bool write_product_uninst_key { get; set; }

        internal string product_dir_regkey { get; private set; }
        internal string product_uninst_key { get; private set; }
        internal string product_uninst_root_key { get; private set; }
   
     
        /// <summary>
        /// Contains all Page objects.
        /// </summary>
        public List<Page> pages { get; private set; }
        /// <summary>
        /// Contains all Component object.
        /// </summary>
        public List<Component> components { get; private set; }

        /// <summary>
        /// Filename of the setup that will be created with this NSIS project.
        /// </summary>
        public string outfile { get; set; }
        /// <summary>
        /// Directories that contains all files that will be installed.
        /// </summary>
        public Dir dir { get; set; }

        internal string MUI_FILENAME { get; private set; }

        internal bool AbortWarning { get; set; } //TO DO: Have to make an option for people to set this property to true or false.
        /// <summary>
        /// The icon for the installer. Needs to be a .ico format.
        /// </summary>
        public string InstallIcon { get; set; }
        /// <summary>
        /// The icon for the uninstaller. Needs to be a .ico format.
        /// </summary>
        public string UnInstallIcon { get; set; }

        internal string HeaderImage;
        internal bool StretchHeaderImage;
        internal bool HeaderImageRight;
        internal string WelcomeFinishImage;
        internal bool StretchWelcomeFinishImage;

        internal string MainAppName { get; private set; }
        /// <summary>
        /// Install directory of the application. This property is deprecated, use InstallDir instead.
        /// </summary>
        public string InstallDirectory { get; private set; } //Deprecated; needs to move to the static property: InstallDir.

        private static string instdir = "$INSTDIR";
        /// <summary>
        /// Install directory of the application.
        /// </summary>
        public static string InstallDir { get { return instdir; } }

        /// <summary>
        /// Create an uninstaller? Default value is: true.
        /// </summary>
        public bool CreateUninstaller { get; set; }

        /// <summary>
        /// Create a startmenu shortcut of the product website.
        /// </summary>
        public bool CreateStartMenuShortCut_Website { get; set; }
        /// <summary>
        /// Create a startmenu shortcut of the uninstaller.
        /// </summary>
        public bool CreateStartMenuShortCut_Uninstaller { get; set; }

        internal List<DotNetFrameworkVersion> CheckDotNetFrameworkVersion = new List<DotNetFrameworkVersion>();
        internal List<RegValue> RegistryValues = new List<RegValue>();
        internal List<x64> x64Actions = new List<x64>();
        internal List<x86> x86Actions = new List<x86>();
        internal List<InstallType> InstallTypes = new List<InstallType>();

        /// <summary>
        /// Retrieve .NET Framework version numbers, necessary if you want to use the .NET Framework version numbers (e.g. DotNetFramework.v40.Version).
        /// </summary>
        public bool GetDotNetFrameworkVersion { get; set; }
        /// <summary>
        /// Run the installed application after installation.
        /// </summary>
        public bool RunAppAfterInstall { get; set; }

        internal string BrandingText;
        internal string BrandingTextTrim;

        /// <summary>
        /// Creates a new NSIS project.
        /// </summary>
        /// <param name="productname">Name of your product/application.</param>
        /// <param name="productversion">Version of your product/application.</param>
        /// <param name="productpublisher">The publisher of your product/application.</param>
        /// <param name="productweb_site">Website of your product/application.</param>
        /// <param name="Outfile">Filename of the setup that will be created with this NSIS project.</param>
        /// <param name="Directory">Directories that contains all files that will be installed.</param>
        public NSISProject(string productname, string productversion, string productpublisher, string productweb_site, string Outfile, Dir Directory)
        {
            this.InitializeProject(productname, productversion, productpublisher, productweb_site, Outfile, Directory, null);
        }
        /// <summary>
        /// Creates a new NSIS project.
        /// </summary>
        /// <param name="productname">Name of your product/application.</param>
        /// <param name="productversion">Version of your product/application.</param>
        /// <param name="productpublisher">The publisher of your product/application.</param>
        /// <param name="productweb_site">Website of your product/application.</param>
        /// <param name="Outfile">Filename of the setup that will be created with this NSIS project.</param>
        /// <param name="Directory">Directories that contains all files that will be installed.</param>
        /// <param name="Actions">All custom actions.</param>
        public NSISProject(string productname, string productversion, string productpublisher, string productweb_site, string Outfile, Dir Directory, params object[] Actions)
        {
            this.InitializeProject(productname, productversion, productpublisher, productweb_site, Outfile, Directory, Actions);
        }

        private void InitializeProject(string productname, string productversion, string productpublisher, string productweb_site, string Outfile, Dir Directory, params object[] Actions)
        {
            this.product_name = productname;
            this.product_version = productversion;
            this.product_publisher = productpublisher;
            this.product_web_site = productweb_site;

            this.CreateUninstaller = true; //Default value = true.
            this.CreateStartMenuShortCut_Website = true; // Default value = true.
            this.CreateStartMenuShortCut_Uninstaller = true; // Default value = true.

            this.MUI_FILENAME = "MUI2.nsh";
            this.InstallDirectory = "$INSTDIR"; //Deprecated.

            this.dir = Directory;
            //this.files = Files;

            /*
            Console.WriteLine(this.GetFileName(this.files[0].sourcepath));
            Console.ReadLine();
            */
            this.MainAppName = this.GetFileName(this.dir.Files[0].sourcepath);

            this.write_product_dir_regkey = true;
            this.write_product_uninst_key = true;

            this.product_dir_regkey = @"Software\Microsoft\Windows\CurrentVersion\App Paths\" + this.MainAppName;
            this.product_uninst_key = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + this.product_name;
            this.product_uninst_root_key = "HKLM";

            this.AbortWarning = true;
            this.InstallIcon = @"${NSISDIR}\Contrib\Graphics\Icons\modern-install.ico";
            this.UnInstallIcon = @"${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico";

            this.pages = new List<Page>();
            this.components = new List<Component>();

            this.outfile = Outfile;

            this.GetDotNetFrameworkVersion = false;
            this.RunAppAfterInstall = true;

            //Checking if there are any custom actions set.
            if (Actions != null)
            {
                //Looping through each action and determine its type.
                for (int i = 0; i < Actions.Length; i++)
                {
                    //Actions[i] could be an object array. Do some magic to workaround this.
                    List<object> tempactionobjectlist = new List<object>();
                    if (Actions[i] is object[])
                    {
                        foreach (object o in Actions[i] as object[])
                        {
                            tempactionobjectlist.Add(o);
                        }
                    }
                    else
                    {
                        tempactionobjectlist.Add(Actions[i]);
                    }

                    foreach (object ActionObject in tempactionobjectlist)
                    {
                        if (ActionObject is DotNetFrameworkVersion)
                        {
                            CheckDotNetFrameworkVersion.Add(ActionObject as DotNetFrameworkVersion);
                        }
                        else
                        {
                            if (ActionObject is Page)
                            {
                                pages.Add(ActionObject as Page);
                            }
                            else
                            {
                                if (ActionObject is Component)
                                {
                                    Component c = ActionObject as Component;
                                    if (!c.IsGroup || c.ComponentList.Count > 0)
                                    {
                                        this.components.Add(ActionObject as Component);
                                    }
                                }
                                else
                                {
                                    if (ActionObject is InstallType)
                                    {
                                        this.InstallTypes.Add(ActionObject as InstallType);
                                    }
                                    else
                                    {
                                        if (ActionObject is RegValue)
                                        {
                                            //Action = RegValue.
                                            //Add the RegValue to the RegistryValues List. This will be used for the compiler that will process each registry value.
                                            this.RegistryValues.Add(ActionObject as RegValue);
                                        }
                                        else
                                        {
                                            if (ActionObject is x64)
                                            {
                                                this.x64Actions.Add(ActionObject as x64);
                                            }
                                            else
                                            {
                                                if (ActionObject is x86)
                                                {
                                                    this.x86Actions.Add(ActionObject as x86);
                                                }
                                                else
                                                {
                                                    throw new Exception("Unexpected object type as among NSISProject constructor arguments is: " + ActionObject.GetType().Name);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Retrieve filename from a full path.
        /// </summary>
        /// <param name="filepath">Path to a file.</param>
        /// <returns>The filename of the specified path will be returned.</returns>
        public string GetFileName(string filepath)
        {
            //Get filename from path.

            string[] splitted = filepath.Split(new char [] {'\\'});
            Array.Reverse(splitted);
            string filename = splitted[0];

            return filename;
        }

        /// <summary>
        /// Bitmap image to display on the header of installers pages (recommended size: 150x57 pixels).
        /// </summary>
        /// <param name="Path">Location of the image. (Bitmap only).</param>
        /// <param name="Stretch">Stretch the installer header bitmap to fit the size of the field. Set this to false, only if you have an image that does not use the whole space. If you have a full size bitmap that fits exactly, you should not set this option to false, because the size of the field will be different if the user has a custom DPI setting.</param>
        /// <param name="LocationRight">Display the header image on the right side instead of the left side.</param>
        public void SetHeaderImage(string Path, bool Stretch = true, bool LocationRight = false)
        {
            HeaderImage = Path;
            StretchHeaderImage = Stretch;
            HeaderImageRight = LocationRight;
        }

        /// <summary>
        /// Bitmap for the Welcome page and the Finish page (recommended size: 164x314 pixels).
        /// </summary>
        /// <param name="Path">Location of the image. (Bitmap only).</param>
        /// <param name="Stretch">Stretch the bitmap for the Welcome and Finish page to fit the size of the field. Set this to false, only if you have an image that does not use the whole space. If you have a full size bitmap that fits exactly, you should not set this option to false, because the size of the field will be different if the user has a custom DPI setting.</param>
        public void SetWelcomeFinishPage(string Path, bool Stretch = true)
        {
            WelcomeFinishImage = Path;
            StretchWelcomeFinishImage = Stretch;
        }

        /// <summary>
        /// Sets the text that is shown (by default it is 'Nullsoft Install System vX.XX') at the bottom of the install window.
        /// </summary>
        /// <param name="Text">Text of your choice. Leaving it empty will make it use the default. A space will remove the branding text.</param>
        /// <param name="Trim">Options: left, right and center. Everything else will be just set to left.</param>
        public void SetBrandingText(string Text, string Trim = null)
        {
            BrandingText = Text;
            BrandingTextTrim = Trim;
        }

        /*
         * ------------------------------------------------------------------------------------------------------------------------------
         * TO DO:
         * [ShowInstDetails, ShowUnInstDetails, SethOutPath and SetOverWrite]
         * These methods needs to be protected from the application that is using the NSISSharp.dll; using a Subclass for example.
         * ------------------------------------------------------------------------------------------------------------------------------
         */
        internal string ShowInstDetails(ShowHide mode)
        {
            return "ShowInstDetails " + mode.ToString().ToLower();
        }

        internal string ShowUnInstDetails(ShowHide mode)
        {
            return "ShowUnInstDetails " + mode.ToString().ToLower();
        }

        /* Set current output path.
         * ----------------------------------------------------------------------------------
         * TO DO: options for setting the current output path to $TEMP and $PLUGINSDIR 
         * ----------------------------------------------------------------------------------
         */
        public string SetOutPath(string path)
        {
            return "SetOutPath \"" + path +"\"";
        }

        /* Set overwrite mode. */
        internal string SetOverWrite(OverWriteMode mode)
        {
            return "SetOverWrite " + mode.ToString().ToLower();
        }
    }
    /* ---------------------------------------------------------------
     * Various enumerators.
     * ---------------------------------------------------------------
     * TO DO:
     *   - enum OverWriteMode, ShowHide are temp internal.
     *   - When custom actions is supported, they should be public.
     * --------------------------------------------------------------- */
    internal enum OverWriteMode
    {
        Ifnewer,
        Ifdiff,
        Lastused,
        On,
        Off,
        Try
    }

    internal enum ShowHide
    {
        Show,
        Hide,
        Nevershow
    }

    /// <summary>
    /// Enumerator to specify which kind of shortcut must be created.
    /// </summary>
    public enum CreateShortCut
    {
        StartMenu,
      //InstallDir,
        Desktop,
      //StartMenuAndInstallDir,
      //StartMenuAndDesktop,
      //InstallDirAndDesktop,
        All,
        None
    }
}
