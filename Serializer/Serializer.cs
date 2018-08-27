using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Serializer
{
    public enum TypeCode : byte{
        Unknown,

        Byte,
        Bool,
        Short,
        Int,
        Long,
        Float,
        Double,

        String,
        Class,

        Array,
        Dictionary,
    }

    public static class Serializer
    {
        static byte[] shortBytes = new byte[2];
        static byte[] intBytes = new byte[4];
        static byte[] longBytes = new byte[8];
        static byte[] floatBytes = new byte[4];
        static byte[] doubleBytes = new byte[8];

        private static Dictionary<Type, CustomClass> typeDict = new Dictionary<Type, CustomClass>();
        private static Dictionary<byte, CustomClass> typeCodeDict = new Dictionary<byte, CustomClass>();

        public static void RegisterCustomType(Type type, byte typeCode, SerializationFunc serializationFunc, DeserializationFunc deserializationFunc)
        {
            if (typeDict.ContainsKey(type))
                return;
            var customClass = new CustomClass(type, typeCode, serializationFunc, deserializationFunc);
            typeDict.Add(type, customClass);
            typeCodeDict.Add(typeCode, customClass);
        }

        #region Encode

        public static void Encode(MemoryStream stream, object obj)
        {
            Type type = obj.GetType();
            TypeCode typeCode = GetCodeByType(type);
            switch (typeCode)
            {
                case TypeCode.Byte:
                    EncodeByte(stream, (byte)obj);
                    break;
                case TypeCode.Bool:
                    EncodeBool(stream, (bool)obj);
                    break;
                case TypeCode.Short:
                    EncodeShort(stream, (short)obj);
                    break;
                case TypeCode.Int:
                    EncodeInt(stream, (int)obj);
                    break;
                case TypeCode.Long:
                    EncodeLong(stream, (long)obj);
                    break;
                case TypeCode.Float:
                    EncodeFloat(stream, (float)obj);
                    break;
                case TypeCode.Double:
                    EncodeDouble(stream, (double)obj);
                    break;
                case TypeCode.String:
                    EncodeString(stream, obj as string);
                    break;
                case TypeCode.Array:
                    EncodeArray(stream, obj as IList);
                    break;
                case TypeCode.Dictionary:
                    EncodeDictionary(stream, obj as IDictionary);
                    break;
                default:
                    if (typeDict.TryGetValue(type, out CustomClass customClass))
                    {
                        EncodeTypeCode(stream, TypeCode.Class);
                        stream.WriteByte(customClass.TypeCode);
                        customClass.SerializationFunc.Invoke(stream, obj);
                    }
                    else
                    {
                        throw new Exception(string.Format("Unsupport type : {0}", type));
                    }
                    break;
            }
        }

        public static void EncodeByte(MemoryStream stream, byte b)
        {
            EncodeTypeCode(stream, TypeCode.Byte);
            stream.WriteByte(b);
        }

        public static void EncodeBool(MemoryStream stream, bool b)
        {
            EncodeTypeCode(stream, TypeCode.Bool);
            var bytes = BitConverter.GetBytes(b);
            if (b)
            {
                stream.WriteByte((byte)1);
            }
            else
            {
                stream.WriteByte((byte)0);
            }
        }

        public static void EncodeShort(MemoryStream stream, short s)
        {
            EncodeTypeCode(stream, TypeCode.Short);
            var bytes = BitConverter.GetBytes(s);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void EncodeInt(MemoryStream stream, int i)
        {
            EncodeTypeCode(stream, TypeCode.Int);
            var bytes = BitConverter.GetBytes(i);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void EncodeLong(MemoryStream stream, long l) {
            EncodeTypeCode(stream, TypeCode.Long);
            var bytes = BitConverter.GetBytes(l);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void EncodeFloat(MemoryStream stream, float f) {
            EncodeTypeCode(stream, TypeCode.Float);
            var bytes = BitConverter.GetBytes(f);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void EncodeDouble(MemoryStream stream, double d) {
            EncodeTypeCode(stream, TypeCode.Double);
            var bytes = BitConverter.GetBytes(d);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void EncodeString(MemoryStream stream, string s)
        {
            EncodeTypeCode(stream, TypeCode.String);
            var bytes = Encoding.UTF8.GetBytes(s);
            // 字符串长度
            EncodeLength(stream, bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void EncodeArray(MemoryStream stream, IList list)
        {
            EncodeTypeCode(stream, TypeCode.Array);
            EncodeLength(stream, list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                Encode(stream, list[i]);
            }
        }

        public static void EncodeDictionary(MemoryStream stream, IDictionary dict)
        {
            EncodeTypeCode(stream, TypeCode.Dictionary);
            EncodeLength(stream, dict.Count);
            IDictionaryEnumerator enumerator = dict.GetEnumerator();
            while (enumerator.MoveNext())
            {
                DictionaryEntry entry = (DictionaryEntry)enumerator.Current;
                Encode(stream, entry.Key);
                Encode(stream, entry.Value);
            }
        }

        #endregion

        #region Decode

        public static object Decode(MemoryStream stream)
        {
            TypeCode typeCode = DecodeTypeCode(stream);
            switch (typeCode)
            {
                case TypeCode.Byte:
                    return DecodeByte(stream);
                case TypeCode.Bool:
                    return DecodeBool(stream);
                case TypeCode.Short:
                    return DecodeShort(stream);
                case TypeCode.Int:
                    return DecodeInt(stream);
                case TypeCode.Long:
                    return DecodeLong(stream);
                case TypeCode.Float:
                    return DecodeFloat(stream);
                case TypeCode.Double:
                    return DecodeDouble(stream);
                case TypeCode.String:
                    return DecodeString(stream);
                case TypeCode.Class:
                    return DecodeCustomClass(stream);
                case TypeCode.Array:
                    return DecodeArray(stream);
                case TypeCode.Dictionary:
                    return DecodeDictionary(stream);
                default:
                    return null;
            }
        }

        public static byte DecodeByte(MemoryStream stream)
        {
            return (byte)stream.ReadByte();
        }

        public static bool DecodeBool(MemoryStream stream) {
            byte b = (byte)stream.ReadByte();
            if (b == 1)
            {
                return true;
            }
            else {
                return false;
            }
        }

        public static short DecodeShort(MemoryStream stream) {
            stream.Read(shortBytes, 0, shortBytes.Length);
            return BitConverter.ToInt16(shortBytes, 0);
        }

        public static int DecodeInt(MemoryStream stream) {
            stream.Read(intBytes, 0, intBytes.Length);
            return (int)BitConverter.ToInt32(intBytes, 0);
        }

        public static long DecodeLong(MemoryStream stream) {
            stream.Read(longBytes, 0, longBytes.Length);
            return BitConverter.ToInt64(longBytes, 0);
        }

        public static float DecodeFloat(MemoryStream stream) {
            stream.Read(floatBytes, 0, floatBytes.Length);
            return BitConverter.ToSingle(floatBytes, 0);
        }

        public static double DecodeDouble(MemoryStream stream) {
            stream.Read(doubleBytes, 0, doubleBytes.Length);
            return BitConverter.ToDouble(doubleBytes, 0);
        }

        public static string DecodeString(MemoryStream stream) {
            int length = DecodeLength(stream);
            byte[] bytes = new byte[length];
            stream.Read(bytes, 0, bytes.Length);
            return Encoding.UTF8.GetString(bytes);
        }

        public static object DecodeCustomClass(MemoryStream stream)
        {
            byte typeCode = (byte)stream.ReadByte();
            if (typeCodeDict.TryGetValue(typeCode, out CustomClass customClass)) {
                return customClass.DeserializationFunc(stream);
            } else {
                throw new Exception(string.Format("Unknow class code : {0}", typeCode));
            }
        }

        public static List<object> DecodeArray(MemoryStream stream) {
            int length = DecodeLength(stream);
            List<object> list = new List<object>(length);
            for (int i = 0; i < length; i++) {
                object obj = Decode(stream);
                list.Add(obj);
            }
            return list;
        }

        public static Dictionary<object, object> DecodeDictionary(MemoryStream stream) {
            int length = DecodeLength(stream);
            Dictionary<object, object> dict = new Dictionary<object, object>(length);
            for (int i = 0; i < length; i++) {
                object k = Decode(stream);
                object v = Decode(stream);
                dict.Add(k, v);
            }
            return dict;
        }

#endregion



        static void EncodeTypeCode(MemoryStream stream, TypeCode typeCode) {
            stream.WriteByte((byte)typeCode);
        }

        static void EncodeLength(MemoryStream stream, int length) {
            var bytes = BitConverter.GetBytes(length);
            stream.Write(bytes, 0, bytes.Length);
        }

        static TypeCode DecodeTypeCode(MemoryStream stream) {
            return (TypeCode)stream.ReadByte();
        }

        static int DecodeLength(MemoryStream stream) {
            stream.Read(intBytes, 0, intBytes.Length);
            return BitConverter.ToInt32(intBytes, 0);
        }

        static TypeCode GetCodeByType(Type type) {
            if (type == typeof(byte))
                return TypeCode.Byte;
            if (type == typeof(bool))
                return TypeCode.Bool;
            if (type == typeof(short))
                return TypeCode.Short;
            if (type == typeof(int))
                return TypeCode.Int;
            if (type == typeof(long))
                return TypeCode.Long;
            if (type == typeof(float))
                return TypeCode.Float;
            if (type == typeof(double))
                return TypeCode.Double;
            if (type == typeof(string))
                return TypeCode.String;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return TypeCode.Array;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                return TypeCode.Dictionary;
            return TypeCode.Unknown;
        }
    }
}
