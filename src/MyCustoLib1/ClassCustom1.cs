using Bb.ComponentModel.Attributes;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace MyCustoLib1
{

    [ExposeClass("business1")]
    public class ClassCustom1
    {

        public ClassCustom1(IConfiguration configuration)
        {
            _configuration = configuration;
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

        private IConfiguration _configuration;

    }

}
