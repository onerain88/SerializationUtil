using System;
using System.IO;

namespace SerializationUtil
{
    public delegate void SerializationFunc(MemoryStream stream, object obj);
    public delegate object DeserializationFunc(MemoryStream stream);

    public class CustomType
    {
        public Type Type {
            get; private set;
        }

        public byte TypeCode {
            get; private set;
        }

        public SerializationFunc SerializationFunc {
            get; private set;
        }

        public DeserializationFunc DeserializationFunc {
            get; private set;
        }

        public CustomType(Type type, byte typeCode, SerializationFunc serializationFunc, DeserializationFunc deserializationFunc)
        {
            this.Type = type;
            this.TypeCode = typeCode;
            this.SerializationFunc = serializationFunc;
            this.DeserializationFunc = deserializationFunc;
        }
    }
}
