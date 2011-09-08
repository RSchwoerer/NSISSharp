using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSISSharp
{
    /// <summary>
    /// Retrieves .NET framework version numbers etc.
    /// The actual code for retrieving the .NET framework version numbers is handled by NSIS, obviously; see: DotNetVersionNumber.nsh.
    /// This class, along with the DotNetFrameworkVersion class, might get properly documented one day.
    /// </summary>
    public static class DotNetFramework
    {
        public static readonly DotNetFrameworkVersion v40 = DotNetFrameworkVersion.CreateObject("4.0");
        public static readonly DotNetFrameworkVersion v35 = DotNetFrameworkVersion.CreateObject("3.5");
        public static readonly DotNetFrameworkVersion v30 = DotNetFrameworkVersion.CreateObject("3.0");
        public static readonly DotNetFrameworkVersion v20 = DotNetFrameworkVersion.CreateObject("2.0");
        public static readonly DotNetFrameworkVersion v10 = DotNetFrameworkVersion.CreateObject("1.0");

        public static DotNetFrameworkVersion CheckVersion(DotNetFrameworkVersion version)
        {
            //Checking for a specific .NET framework version.
            //NSIS will then determine if the version that was specified is installed on the user's system. 
            version.CheckVersion = true;
            return version;
        }
    }

    public class DotNetFrameworkVersion
    {
        internal string _Version;
        public string Version { get; private set; }
        public string MajorVersion { get; private set; }
        public string MinorVersion { get; private set; }
        public string RevNumber { get; private set; }
        public string BuildNumber { get; private set; }
        internal bool CheckVersion;

        private DotNetFrameworkVersion()
        {
            this._Version = "";
            this.Version = "";
            this.MajorVersion = "";
            this.MinorVersion = "";
            this.RevNumber = "";
            this.BuildNumber = "";
            this.CheckVersion = false;
        }

        internal static DotNetFrameworkVersion CreateObject(string version)
        {
            string[] temp = version.Split(new char[] {'.'});
            string __version = temp[0] + temp[1]; //Version without the period.

            string _version = "$dotNETVersion";
            string _majorversion = "$dotNETMajorVersion";
            string _minorversion = "$dotNETMinorVersion";
            string _revnumber = "";
            string _buildnumber = "dotNETBuildNumber"; 
            if (version != "4.0")
            {
               _version = _version + __version;
               _majorversion = _majorversion + __version;
               _minorversion = _minorversion + __version;
               _revnumber = _revnumber + __version;
               _buildnumber = _buildnumber + __version;
            }

            return new DotNetFrameworkVersion()
            {
               _Version = version, //Format: x.y; e.g. 4.0.
               Version = _version,
               MajorVersion = _majorversion,
               MinorVersion = _minorversion,
               RevNumber = _revnumber,
               BuildNumber = _buildnumber
            };
        }
    }
}
