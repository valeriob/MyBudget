using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyBudget.Infrastructure;
using MyBudget.Projections;

namespace MyBudget.Tests
{
    [TestFixture]
    public class projection_should
    {
        [Test]
        public void apply_event()
        {
            var p = new MyProjection();

            dynamic evnt = new MyEvnt();
            p.FakeDispatch(evnt);

            Assert.AreEqual(evnt, p.EventAppened);
        }

    }


    class MyEvnt : Event
    {

    }
    class MyProjection : InMemoryProjection
    {
        public MyEvnt EventAppened;

        public MyProjection() : base(null, null)
        {

        }
        public void FakeDispatch(dynamic evnt)
        {
            Dispatch(evnt);
        }
        protected override void Dispatch(dynamic evnt)
        {
            dynamic p = this;
            try
            {
                p.When(evnt);
            }
            catch { }

        }
        public void When(MyEvnt evnt)
        {
            EventAppened = evnt;
        }
        public override void Reset()
        {
            base.Reset();
        }
    }
}
