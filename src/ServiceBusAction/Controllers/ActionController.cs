using Bb.ActionBus;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace ServiceBusAction.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActionController : ControllerBase
    {

        private readonly ActionRepositories _repositories;

        public ActionController(Bb.ActionBus.ActionRepositories repositories)
        {
            this._repositories = repositories;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<ActionModelDesciptor>> Get()
        {
            return Ok(this._repositories.GetMethods());
        }


        // POST api/values
        [HttpPost]
        public void Post([FromBody] ActionOrder value)
        {

            int count = 0;
            var _count = this.Request.Headers["count"].ToArray();
            if (_count != null && _count.Length > 0)
            {
                string c = _count[0];
                count = int.Parse(c);
            }

            if (!this._repositories.Execute(value, count))
                throw (Exception)value.Result;

        }

    }
}