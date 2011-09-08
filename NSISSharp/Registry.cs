using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSISSharp
{
    /// <summary>
    /// Provides NSISSharp.RegistryKey objects that represent the root keys in the Windows registry, and static methods to access key/value pairs.
    /// </summary>
    public static class Registry
    {
        //Root keys.
        /// <summary>
        /// HKEY_CURRENT_USER.
        /// </summary>
        public static readonly RegistryKey CurrentUser = RegistryKey.GetRootKey("HKEY_CURRENT_USER");
        /// <summary>
        /// HKEY_LOCAL_MACHINE.
        /// </summary>
        public static readonly RegistryKey LocalMachine = RegistryKey.GetRootKey("HKEY_LOCAL_MACHINE");
        /// <summary>
        /// HKEY_CLASSES_ROOT.
        /// </summary>
        public static readonly RegistryKey ClassesRoot = RegistryKey.GetRootKey("HKEY_CLASSES_ROOT");
        /// <summary>
        /// HKEY_USERS.
        /// </summary>
        public static readonly RegistryKey Users = RegistryKey.GetRootKey("HKEY_USERS");
        /// <summary>
        /// HKEY_PERFORMANCE_DATA.
        /// </summary>
        public static readonly RegistryKey PerformanceData = RegistryKey.GetRootKey("HKEY_PERFORMANCE_DATA");
        /// <summary>
        /// HKEY_CURRENT_CONFIG.
        /// </summary>
        public static readonly RegistryKey CurrentConfig = RegistryKey.GetRootKey("HKEY_CURRENT_CONFIG");
        /// <summary>
        /// HKEY_DYN_DATA.
        /// </summary>
        public static readonly RegistryKey DynData = RegistryKey.GetRootKey("HKEY_DYN_DATA");
    }

    /// <summary>
    /// Represents a key-level node in the Windows registry. This class is a registry encapsulation.
    /// </summary>
    public class RegistryKey
    {
        /*
        private string keyName;
        public string Name 
        {
            get { return this.keyName; } 
        }
        */

        /// <summary>
        /// Registrykey name.
        /// </summary>
        public string Name { get; private set; } //Root + subkey.
        private string RootKeyName;
        private string NsiStr;
        private string tempNsiStr;
        private string SubKey; //Without root.

        private RegistryKey()
        {
            //Constructor for this class use only.
            
            //Default values:
            this.Name = "";
            this.RootKeyName = "";
            this.NsiStr = "";
            this.SubKey = "";
        }

        internal static RegistryKey GetRootKey(string rootkey)
        {
            return new RegistryKey()
            {
                Name = rootkey,
                RootKeyName = rootkey
            };
        }

        /// <summary>
        /// Creates a new subkey.
        /// </summary>
        /// <param name="subkey">The name or path of the subkey to create or open.</param>
        /// <returns>The newly created subkey.</returns>
        public RegistryKey CreateSubKey(string subkey)
        {
            try
            {
                //When needed: remove the root key and merge old subkey with the new one.
                int i = this.Name.IndexOf("\\");
                if (i != -1)
                {
                    subkey = this.Name.Substring(i + 1) + "\\" + subkey;
                }
     
                //Create subkey nsi string (partly).
                string tempstr = " WriteRegStr " + RootKeyName + " \"" + subkey + "\" ";

                //Open subkey.
                return this.OpenSubKey(subkey, tempstr);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieves a subkey.
        /// </summary>
        /// <param name="subkey">The name or path of the subkey to open.</param>
        /// <returns>The subkey requested.</returns>
        public RegistryKey OpenSubKey(string subkey)
        {
            /* --------------------------------------------------------------------------------------------------------------
             * TO DO:
             * Method needs to be expanded, since it really doesn't serve any purpose now except for when deleting a subkey.
             * -------------------------------------------------------------------------------------------------------------- */
            return new RegistryKey()
            {
                RootKeyName = RootKeyName,
                SubKey = subkey,
                Name = RootKeyName + "\\" + subkey
            };
        }
        internal RegistryKey OpenSubKey(string subkey, string nsistr)
        {
            return new RegistryKey()
            {
                RootKeyName = RootKeyName,
                SubKey = subkey,
                Name = RootKeyName + "\\" + subkey,
                NsiStr = nsistr,
                tempNsiStr = nsistr
            };
        }

        /// <summary>
        /// Sets the specified name/value pair.
        /// </summary>
        /// <param name="name">The name of the value to store.</param>
        /// <param name="value">The data to be stored.</param>
        public object SetValue(string name, object value)
        {
            return this.SetValue(name, value, true);
        }

        internal RegValue SetValue(string name, object value, bool CustomAction = false)
        {
            /* ------------------------------------------------------------------------------------------------------------------
             * TO DO: 
             * 
             * - Needs a check if a subkey has been opened. If no subkey have been opened it shouldn't be able to set a value.
             * 
             * - DWORD and other registry types support is planned for future versions. 
             *      - RegStr is supported in v0.2 only.
             * ------------------------------------------------------------------------------------------------------------------ */

            try
            {
                //Converting object to string.
                string ValueRegStr = (string)value;
                this.NsiStr = this.tempNsiStr + "\"" + name + "\" \"" + ValueRegStr + "\"";

                if (CustomAction)
                {
                   //Registry key value is set/specified by the user.
                    return new RegValue(this.NsiStr, this);
                }
                else
                {
                    //Pass the nsistr to the compiler.
                    Compiler.nsi.AppendLine(this.NsiStr);
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Deletes the current subkey from the registry.
        /// </summary>
        public void Delete()
        {
            Compiler.nsi.AppendLine(" DeleteRegKey " + RootKeyName + " \"" + this.SubKey + "\"");
        }
    }

    internal class RegValue
    {
        private string Value;
        private RegistryKey basekey;
        public RegValue(string value,  RegistryKey baseKey)
        {
            this.Value = value;
            this.basekey = baseKey;
        }

        public string GetValue()
        {
            return this.Value;
        }

        public RegistryKey GetBaseKey()
        {
            return this.basekey;
        }
    }
}
