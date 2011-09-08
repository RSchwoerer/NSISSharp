using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSISSharp
{
    /// <summary>
    /// Represents a page that will be used for your installer.
    /// </summary>
    public class Page
    {
        public string name { get; set; }
        public string licensepath { get; set; }
        public string languagefile { get; set; }
        public bool smalldescription { get; set; }

        /* MUI Pages.
         * ---------------------------------------------------
         * TO DO: using a different approach. 
         * ---------------------------------------------------
         */

        public static readonly string PAGE_WELCOME = "MUI_PAGE_WELCOME";
        public static readonly string PAGE_LICENSE = "MUI_PAGE_LICENSE";
        public static readonly string PAGE_DIRECTORY = "MUI_PAGE_DIRECTORY";
        public static readonly string PAGE_INSTFILES = "MUI_PAGE_INSTFILES";
        public static readonly string PAGE_FINISH = "MUI_PAGE_FINISH";
        public static readonly string PAGE_UNINSTALL = "MUI_UNPAGE_INSTFILES";
        public static readonly string PAGE_LANGUAGE = "MUI_LANGUAGE";
        public static readonly string PAGE_COMPONENTS = "MUI_PAGE_COMPONENTS";

        public static string LANGUAGE_FILE_ENGLISH = "English";

        /// <summary>
        /// Creates a Page object. A page object equals to a page that is used in NSIS.
        /// </summary>
        /// <param name="Name">Name of the page. Use one of the static readonly strings that is provided by the Page class (e.g. Page.PAGE_WELCOME).</param>
        public Page(string Name)
        {
            this.name = Name;
            this.smalldescription = false;
        }
        /// <summary>
        /// Creates a Page object. A page object equals to a page that is used in NSIS.
        /// </summary>
        /// <param name="Name">Name of the page. Use one of the static readonly strings that is provided by the Page class (e.g. Page.PAGE_WELCOME).</param>
        /// <param name="SmallDescription">Use a small textarea for the description of a component. This is used by the component page, only (PAGE_COMPONENTS).</param>
        public Page(string Name, bool SmallDescription)
        {
            this.name = Name;
            this.smalldescription = SmallDescription;
        }
        /// <summary>
        /// Creates a Page object. A page object equals to a page that is used in NSIS.
        /// </summary>
        /// <param name="Name">Name of the page. Use one of the static readonly strings that is provided by the Page class (e.g. Page.PAGE_WELCOME).</param>
        /// <param name="LicensePath">Full path to the license txt file.</param>
        public Page(string Name, string LicensePath)
        {
            this.name = Name;
            this.licensepath = LicensePath;
            this.smalldescription = false;
        }
        /// <summary>
        /// Creates a Page object. A page object equals to a page that is used in NSIS.
        /// </summary>
        /// <param name="Name">Name of the page. Use one of the static readonly strings that is provided by the Page class (e.g. Page.PAGE_WELCOME).</param>
        /// <param name="LicensePath">Full path to the license txt file.</param>
        /// <param name="LanguageFile">Specify which language to use for the installer. For now English is supported only; use Page.LANGUAGE_FILE_ENGLISH.</param>
        public Page(string Name, string LicensePath, string LanguageFile)
        {
            this.name = Name;
            this.licensepath = LicensePath;
            this.languagefile = LanguageFile;
            this.smalldescription = false;
        }
    }
}
