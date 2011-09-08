using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSISSharp
{
    public class File
    {
        /// <summary>
        /// Path to file.
        /// </summary>
        public string sourcepath { get; private set; }
        /// <summary>
        /// Shortcut type.
        /// </summary>
        public CreateShortCut createshortcut { get; private set; }
        /// <summary>
        /// The name of the shortcut that will be created.
        /// </summary>
        public string shortcutname { get; private set; }
        /// <summary>
        /// If set to true: matching files and directories are recursively searched for in subdirectories.
        /// </summary>
        public bool recursive { get; private set; }
        /// <summary>
        /// The file name of the path that has been specified as the 'sourcepath'.
        /// </summary>
        public string filename { get; private set; }

        /// <summary>
        ///  Creates a new file object. 
        /// </summary>
        /// <param name="SourcePath">Path to file. Wildcards are allowed.</param>
        public File(string SourcePath)
        {
            this.sourcepath = SourcePath;
            //this.installpath = InstallPath;
            this.createshortcut = CreateShortCut.None; //Set default value; no shortcut of the file will be created.
            this.shortcutname = "";
            this.filename = this.GetFilenameFromSourcePath();
        }
        /// <summary>
        ///  Creates a new file object. 
        /// </summary>
        /// <param name="SourcePath">Path to file. Wildcards are allowed.</param>
        /// <param name="Recursive">If set to true: matching files and directories are recursively searched for in subdirectories.</param>
        public File(string SourcePath, bool Recursive)
        {
            this.sourcepath = SourcePath;
            //this.installpath = InstallPath;
            this.createshortcut = CreateShortCut.None; //Set default value; no shortcut of the file will be created.
            this.shortcutname = "";
            this.filename = this.GetFilenameFromSourcePath();
            this.recursive = Recursive;
        }
        /// <summary>
        ///  Creates a new file object. 
        /// </summary>
        /// <param name="SourcePath">Path to file. Wildcards are allowed.</param>
        /// <param name="Shortcut">Shortcut type.</param>
        public File(string SourcePath, CreateShortCut Shortcut)
        {
            this.sourcepath = SourcePath;
            //this.installpath = InstallPath;
            this.createshortcut = Shortcut;
            this.shortcutname = "";
            this.filename = this.GetFilenameFromSourcePath();
        }
        /// <summary>
        ///  Creates a new file object. 
        /// </summary>
        /// <param name="SourcePath">Path to file. Wildcards are allowed.</param>
        /// <param name="Shortcut">Shortcut type.</param>
        /// <param name="ShortcutName">The name of the shortcut that will be created.</param>
        public File(string SourcePath, CreateShortCut Shortcut, string ShortcutName)
        {
            this.sourcepath = SourcePath;
            //this.installpath = InstallPath;
            this.createshortcut = Shortcut;
            this.shortcutname = ShortcutName;
            this.filename = this.GetFilenameFromSourcePath();
        }

        private string GetFilenameFromSourcePath()
        {
            String[] Arr = this.sourcepath.Split(new char[] { '\\' });
            Array.Reverse(Arr);
            return Arr[0];
        }

        internal void Delete(string dirpath)
        {
            //Delete dir of the current instance.
            Compiler.nsi.AppendLine(" Delete \"" + dirpath + "\\" + filename + "\"");
        }
    }
}
