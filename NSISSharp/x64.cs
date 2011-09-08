using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSISSharp
{
    public class x64
    {
        public List<object> x64Actions = new List<object>();
        public x64(params object[] actions)
        {
            foreach (object action in actions)
            {
                if (action is object[])
                {
                    foreach (object o in action as object[])
                    {
                        x64Actions.Add(o);
                    }
                }
                x64Actions.Add(action);
            }
        }
    }
}
