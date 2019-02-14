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
        public void Post([FromBody] string value)
        {

            var order = ActionOrder.Unserialize(value);

            if (!this._repositories.Execute(order))
                throw (Exception)order.Result;

        }

        private readonly ActionRepositories _repositories;

    }
}