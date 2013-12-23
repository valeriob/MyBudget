using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyBudget.Infrastructure;
using MyBudget.Budgets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace MyBudget.Tests
{
    [TestFixture]
    public class serialization_should
    {
        [Test]
        public void serialize_userAdded()
        {
            var evnt = new UserCreated(UserId.CreateNew(), new Budgets.ValueObjects.UserLoginInfo("prov", "key"));


            var settings = new JsonSerializerSettings 
            {
                TypeNameHandling = TypeNameHandling.None,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                ConstructorHandling = Newtonsoft.Json.ConstructorHandling.Default,
                Formatting = Newtonsoft.Json.Formatting.Indented,
                ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
                {
                    DefaultMembersSearchFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                },
                //ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            };

            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(evnt, settings));

            var result = (UserCreated)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), typeof(UserCreated));

            Assert.AreEqual(evnt.Id, result.Id);
            Assert.AreEqual(evnt.Timestamp, result.Timestamp);
            Assert.AreEqual(evnt.LoginInfo, result.LoginInfo);
            Assert.AreEqual(evnt.UserId, result.UserId);
        }

    }
}
