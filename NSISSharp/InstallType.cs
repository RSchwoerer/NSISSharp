using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSISSharp
{
    public class InstallType
    {
        public string Name { get; set; }
        public InstallType(string name)
        {
            this.Name = name;
        }

        internal static int[] ConvertIndex(int[] InstallTypeIndexArray)
        {
            int[] array = (int[])InstallTypeIndexArray.Clone();
            //Convert index numbers so that it is compatible with NSIS. 
            //First element for InstType in NSIS is 1; thus every value will be increased by one.
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = array[i] + 1;
            }

            return array;
        }
    }
}
