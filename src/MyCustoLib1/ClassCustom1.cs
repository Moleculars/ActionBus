using Bb.ComponentModel.Attributes;
using Microsoft.Extensions.Configuration;
using System;

namespace MyCustoLib1
{

    [ExposeClass("business1", Context = "BusinessAction")]
    public class ClassCustom1
    {

        public ClassCustom1(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [RegisterMethod("CancelScan", Context = "BusinessAction")]
        public bool MyCustomMethodWithBigName(Guid uuid)
        {

            return true;

        }


        [RegisterMethod("PushScan", Context = "BusinessAction")]
        public Guid PushMessagePudo(string internationalSiteId, string scan)
        {

            return Guid.NewGuid();

        }

        private readonly IConfiguration _configuration;

    }

}
