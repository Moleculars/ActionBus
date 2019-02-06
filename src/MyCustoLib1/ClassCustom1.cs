using Bb.ComponentModel.Attributes;
using System;
using System.Collections.Generic;

namespace MyCustoLib1
{

    [ExposeClass("business1")]
    public class ClassCustom1
    {

        public ClassCustom1()
        {

        }

        [RegisterMethodAttribute("Action1")]
        public List<KeyValuePair<string, string>> MyCustomMethodWithBigName(int firstIdentifier, string secondIdentifier)
        {

            return new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>(firstIdentifier.ToString(), secondIdentifier) };

        }


        [RegisterMethodAttribute("PushScan")]
        public List<KeyValuePair<string, string>> PushMessagePudo(string internationalSiteId, string scan)
        {

            return new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("uuid", Guid.NewGuid().ToString()) };

        }



    }

}
