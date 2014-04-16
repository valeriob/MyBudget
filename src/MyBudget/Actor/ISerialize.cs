using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Actor
{
    public interface ISerialize
    {
        T Deserialize<T>(string str);
        object Deserialize(string str, Type type);
        string Serialize(object obj);
    }

    class JSonNet_Serializer : ISerialize
    {
        static JsonSerializerSettings _config;
        static JSonNet_Serializer()
        {
            _config = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Newtonsoft.Json.Formatting.Indented,
                ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
                {
                    DefaultMembersSearchFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                },
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            };
        }


        T ISerialize.Deserialize<T>(string str)
        {
            return JsonConvert.DeserializeObject<T>(str, _config);
        }

        object ISerialize.Deserialize(string str, Type type)
        {
            return JsonConvert.DeserializeObject(str, type);
        }

        string ISerialize.Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, _config);
        }

    }
}
