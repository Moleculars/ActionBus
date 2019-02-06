using Bb.ActionBus;
using Bb.ComponentModel;
using MyCustoLib1;
using System;

namespace ConsoleAction
{
    internal class Program
    {
        private static void Main(string[] args)
        {

            var order = new ActionOrder()
            {
                Name = "business1.Action1",
                PushedAt = DateTimeOffset.Now,
                Uuid = Guid.NewGuid(),
                Arguments = new System.Collections.Generic.List<ArgumentOrder>()
                {
                    new ArgumentOrder() { Name = "firstIdentifier", Value = "1" },
                    new ArgumentOrder() { Name = "secondIdentifier", Value = "Test" } }
            };

            var factory = TypeDiscovery.Initialize();

            var reps = new ActionRepositories()
                .Register(() => new ClassCustom1(), 10);

            var method = reps.GetMethods();

            if (reps.Execute(order))
            {

            }

            Console.WriteLine("Hello World!");

        }
    }
}
