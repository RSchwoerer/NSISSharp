using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSISSharp
{
     public class Dir
    {
        /// <summary>
        /// The specified directory path.
        /// </summary>
        public string Path { get; private set; }
        /// <summary>
        /// A collection of file objects. These are the files that will be installed in the specified directory.
        /// </summary>
        public List<File> Files { get; set; }
        internal DirInstance dirinstances;
        private bool root = true;

        /// <summary>
        /// Creates/Open a directory. This will be the installation directory.
        /// </summary>
        public Dir()
        {
            //Setting path to the NSIS variable '$INSTDIR'. 
            this.Path = "$INSTDIR"; 

            this.Files = new List<File>();

            //Root.
            this.dirinstances = new DirInstance(this);
            //this.instances = new Dictionary<string, Dir>();
            //this.instances.Add("root", this);
        }
        /// <summary>
        /// Creates/Open a directory by specifying a name or path.
        /// </summary>
        /// <param name="path"></param>
        public Dir(string path)
        {
            //If path is null or empty, set its value to the NSIS variable '$INSTDIR'.
            if (string.IsNullOrEmpty(path))
            {
                this.Path = "$INSTDIR";
            }
            else
            {
                this.Path = path;
            }
            this.Files = new List<File>();



            /*
            if (this.instances != null)
            {
                if (this.instances.Count == 0)
                {
                    //Root.
                    this.instances.Add("root", this);    
                }
                else
                {
                    //Subdirectory.
                    this.instances.Add(path, this);
                }
            }
            else
            {
                //root.
                this.instances = new Dictionary<string, Dir>();
                this.instances.Add("root", this);
            }
            */
        }

        private void CheckIfCurrentDirectoryIsNotRoot()
        {
            if (!this.root)
            {
                //Checking if an instance of 'instances' exist. 
                //This is not necessary, since root/subdir is already determined, but this will make sure that no weird stuff is happening.
                if (this.dirinstances != null)
                {
                    if (this.dirinstances.instances.Count == 0)
                    {
                        //Root.
                        this.dirinstances.AddDirectoryInstance(this, true);
                    }
                    else
                    {
                        //Subdirectory.
                        this.dirinstances.AddDirectoryInstance(this, false);
                    }
                }
                else
                {
                    //Root.
                    this.dirinstances = new DirInstance(this);
                }
            }
        }
       
        /// <summary>
        /// Creates a new subdirectory.
        /// </summary>
        /// <param name="path">Name or path of the subdirectory.</param>
        /// <returns>The created subdirectory.</returns>
        public Dir CreateSubDirectory(string path)
        {
            Dir subdir = new Dir(path)
            {
                Path = this.Path + "\\" + path,
                dirinstances = this.dirinstances,
                root = false,
            };

            subdir.CheckIfCurrentDirectoryIsNotRoot();

            return subdir;
        }

        internal void Delete(bool Recursive = false)
        {
            if (!Recursive)
            {
                //Delete dir of the current instance.
                Compiler.nsi.AppendLine(" RMDir \"" + this.Path + "\"");
            }
            else
            {
                //Delete dir of the current instance including all of its subdirectories and files.
                Compiler.nsi.AppendLine(" RMDir /r \"" + this.Path + "\"");
            }
        }
    }

    internal class DirInstance
    {
        internal Dictionary<string, Dir> instances { get; private set; }
        /// <summary>
        /// Create a new 'DirInstance' instance which will contain created 'Dir' instances.
        /// </summary>
        /// <param name="currentinstance"></param>
        public DirInstance(Dir currentinstance)
        {
            this.instances = new Dictionary<string, Dir>();
            this.instances.Add("root", currentinstance);
        }

        internal void AddDirectoryInstance(Dir dirinstance, bool root)
        {
            if (root)
            {
                //Root.
                instances.Add("root", dirinstance);
            }
            else
            {
                //Subdirectory.
                instances.Add(dirinstance.Path, dirinstance);
            }
        }
    }
};