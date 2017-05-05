using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR.Json;
using Newtonsoft.Json;

namespace WebApp.AspNetCore
{
    public class SerializeLinqHubParameterResolver : DefaultParameterResolver
    {
        private readonly JsonSerializer _serializer;

        public SerializeLinqHubParameterResolver(JsonSerializer serializer)
        {
            _serializer = serializer;
        }
        private FieldInfo _valueField;

        public override object ResolveParameter(ParameterDescriptor descriptor, IJsonValue value)
        {
            if (value.GetType() == descriptor.ParameterType)
            {
                return value;
            }

            if (_valueField == null)
            {
                _valueField = value.GetType().GetField("_value", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            var json = (string)_valueField.GetValue(value);
            using (var reader = new StringReader(json))
            {
                return _serializer.Deserialize(reader, descriptor.ParameterType);
            }
        }
    }
}
