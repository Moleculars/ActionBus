using System;
using System.Collections.Generic;

namespace Bb.ActionBus
{
    public class ActionModelDesciptor
    {

        public ActionModelDesciptor()
        {

        }

        public string Name { get; set; }
        public List<KeyValuePair<string, Type>> Arguments { get; internal set; }
    }
}