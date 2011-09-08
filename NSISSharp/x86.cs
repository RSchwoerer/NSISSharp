﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSISSharp
{
    public class x86
    {
        public List<object> x86Actions = new List<object>();
        public x86(params object[] actions)
        {
            foreach (object action in actions)
            {
                if (action is object[])
                {
                    foreach (object o in action as object[])
                    {
                        x86Actions.Add(o);
                    }
                }
                x86Actions.Add(action);
            }
        }
    }
}
