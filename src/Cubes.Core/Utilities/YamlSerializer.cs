using System;
using Cubes.Core.Base;
using YamlDotNet.Serialization;

namespace Cubes.Core.Utilities
{
    public class YamlSerializer : ISerializer
    {
        private static YamlDotNet.Serialization.ISerializer Serializer { get; } = new SerializerBuilder()
            .Build();
        private static IDeserializer Deserializer { get; } = new DeserializerBuilder()
            .Build();

        public string Serialize(object objToSerialize)
            => Serializer.Serialize(objToSerialize);

        public object Deserialize(string objAsString, Type targetType)
            => Deserializer.Deserialize(objAsString, targetType);

        public string Format => CubesConstants.Serializer_YAML;
    }
}

//public class MultilineStringEmitter : ChainedEventEmitter
//{
//    public MultilineStringEmitter(IEventEmitter nextEmitter) : base(nextEmitter) { }
//    public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
//    {
//        if (eventInfo.Source.Type == typeof(string))
//        {
//            var value = (string)eventInfo.Source.Value;
//            if (!String.IsNullOrEmpty(value) && value.Contains('\n'))
//            {
//                var ev = new Scalar(null, null, value, ScalarStyle.Literal, true, false);
//                emitter.Emit(ev);
//                return;
//            }
//        }
//        base.Emit(eventInfo, emitter);
//    }
//}
