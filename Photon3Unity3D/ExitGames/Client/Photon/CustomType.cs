using System;

namespace ExitGames.Client.Photon
{
    internal class CustomType
    {
        public readonly byte Code;
        public readonly DeserializeMethod DeserializeFunction;
        public readonly SerializeMethod SerializeFunction;
        public readonly Type Type;

        public CustomType(Type type, byte code, SerializeMethod serializeFunction, DeserializeMethod deserializeFunction)
        {
            this.Type = type;
            this.Code = code;
            this.SerializeFunction = serializeFunction;
            this.DeserializeFunction = deserializeFunction;
        }
    }
}