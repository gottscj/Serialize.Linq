using System;
using System.Reflection;
using Newtonsoft.Json;
using Serialize.Linq.Nodes;

namespace Serialize.Linq
{
   public class ExpressionNodeJsonConverter : JsonConverter
    {
        private readonly JsonSerializer _serializer = new JsonSerializer
        {
            TypeNameHandling = TypeNameHandling.All,
            NullValueHandling = NullValueHandling.Ignore
        };

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            _serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return _serializer.Deserialize(reader);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ExpressionNode).GetTypeInfo().IsAssignableFrom(objectType);
        }
    }
}