using Bb;
using Bb.ActionBus;
using Bb.ComponentModel.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ActionBusUnitTest
{

    [TestClass]
    public class UnitTest1
    {


        [TestMethod]
        public void UnserializeTest()
        {

            var o = new ActionOrder()
            {
                Uuid = Guid.NewGuid().ToString(),
                Name = "actionTest",
                ExecutedAt = ClockActionBus.Now(),
                PushedAt = ClockActionBus.Now(),
                Result = "ok",
                Success = false,
            }.Argument("arg1", "{ \"test\": \"ok\" }")
            ;

            var txt = o.ToString();
            var p = ActionOrder.Unserialize(txt);

            Assert.AreEqual(o.ExecutedAt, p.ExecutedAt);
            Assert.AreEqual(o.Name, p.Name);
            Assert.AreEqual(o.PushedAt, p.PushedAt);
            Assert.AreEqual(o.Result, p.Result);
            Assert.AreEqual(o.Success, p.Success);
            Assert.AreEqual(o.Uuid, p.Uuid);

            Assert.AreEqual(o.Arguments.Count, 1);
            Assert.AreEqual(o.Arguments.Count, p.Arguments.Count);

            var txt2 = p.ToString();
            var q = ActionOrder.Unserialize(txt2);

            Assert.AreEqual(p.Arguments["arg1"].Value, q.Arguments["arg1"].Value);

            Assert.AreEqual(txt, txt2);

        }

        [TestMethod]
        public void CallMethodTest()
        {

            var instance = new MethodTest();
            var ioc = new Ioc()
                .Add(typeof(MethodTest), () => instance)
                ;

            var repository = new ActionRepositories<ActionBusContext>()
                .Inject(ioc)
            ;

            repository.Register(typeof(MethodTest));

            var actionC = repository.Execute(new ActionOrder()
            {
                Name = "c1.run1",
                PushedAt = DateTimeOffset.Now,
                Uuid = Guid.NewGuid().ToString(),
            }.Argument("arg1", "test2")
             .Argument("arg2", "2")
             .Argument("arg3", "2")
             .Argument("arg4", "2")
             .Argument("arg5", "2")
             .Argument("arg6", "2")
             .Argument("arg7", "true")
            );

            Assert.AreEqual(actionC.Result, null);

        }
    }

    [ExposeClass(Context = ActionBusContants.BusinessActionBus, Name = "c1")]
    public class MethodTest
    {

        public string Value { get; set; }

        [ExposeMethod(Context = ActionBusContants.BusinessActionBus, DisplayName = "run1")]
        public void Run1(ActionBusContext ctx, string arg1, short arg2, long arg3, ushort arg4, uint arg5, ulong arg6, bool arg7)
        {

        }

    }


}
