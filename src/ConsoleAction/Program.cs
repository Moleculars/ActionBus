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

            var reps = new ActionRepositories(null, null, AcquitmentQueue, DeadQueue, 10)
                .Register<ClassCustom1>();

            var method = reps.GetMethods();

            if (reps.Execute(order, 0))
            {

            }

            Console.WriteLine("Hello World!");

        }

        private static void AcquitmentQueue(object sender, ActionOrderEventArgs e)
        {

        }

        private static void DeadQueue(object sender, ActionOrderEventArgs e)
        {

        }

    }
}
