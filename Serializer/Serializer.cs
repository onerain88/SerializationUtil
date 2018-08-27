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

        String,
        Class,

        Array,

        Dictionary,
    }

    public static class Serializer
    {
        static byte[] intBytes = new byte[4];

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

        public static void Encode(MemoryStream stream, object obj) {
            Type type = obj.GetType();
            TypeCode typeCode = GetCodeByType(type);
            switch (typeCode) {
                case TypeCode.Byte:
                    EncodeByte(stream, (byte)obj);
                    break;
                case TypeCode.Int:
                    EncodeInt(stream, (int)obj);
                    break;
                case TypeCode.String:
                    EncodeString(stream, obj as string);
                    break;
                //case TypeCode.Object:
                    //EncodeObject(stream, obj);
                    //break;
                case TypeCode.Array:
                    EncodeArray(stream, obj as IList);
                    break;
                case TypeCode.Dictionary:
                    EncodeDictionary(stream, obj as IDictionary);
                    break;
                default:
                    if (typeDict.TryGetValue(type, out CustomClass customClass)) {
                        EncodeTypeCode(stream, TypeCode.Class);
                        stream.WriteByte(customClass.TypeCode);
                        customClass.SerializationFunc.Invoke(stream, obj);
                    } else {
                        throw new Exception(string.Format("Unsupport type : {0}", type));
                    }
                    break;
            }
        }

        public static void EncodeByte(MemoryStream stream, byte b) {
            EncodeTypeCode(stream, TypeCode.Byte);
            stream.WriteByte(b);
        }

        public static void EncodeInt(MemoryStream stream, int i) {
            EncodeTypeCode(stream, TypeCode.Int);
            var bytes = BitConverter.GetBytes(i);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void EncodeString(MemoryStream stream, string s) {
            EncodeTypeCode(stream, TypeCode.String);
            var bytes = Encoding.UTF8.GetBytes(s);
            // 字符串长度
            EncodeLength(stream, bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void EncodeArray(MemoryStream stream, IList list) {
            EncodeTypeCode(stream, TypeCode.Array);
            EncodeLength(stream, list.Count);
            //Type eType = list.GetType().GetGenericArguments()[0];
            //TypeCode typeCode = GetCodeByType(eType);
            //EncodeTypeCode(stream, typeCode);
            for (int i = 0; i < list.Count; i++) {
                Encode(stream, list[i]);
            }
        }

        public static void EncodeDictionary(MemoryStream stream, IDictionary dict) {
            EncodeTypeCode(stream, TypeCode.Dictionary);
            EncodeLength(stream, dict.Count);
            //Type type = dict.GetType();
            //Type kType = type.GetGenericArguments()[0];
            //Type vType = type.GetGenericArguments()[1];
            //TypeCode kTypeCode = GetCodeByType(kType);
            //TypeCode vTypeCode = GetCodeByType(vType);
            //EncodeTypeCode(stream, kTypeCode);
            //EncodeTypeCode(stream, vTypeCode);
            IDictionaryEnumerator enumerator = dict.GetEnumerator();
            while (enumerator.MoveNext()) {
                DictionaryEntry entry = (DictionaryEntry)enumerator.Current;
                Encode(stream, entry.Key);
                Encode(stream, entry.Value);
            }
        }

#endregion

#region Decode

        public static object Decode(MemoryStream stream) {
            TypeCode typeCode = DecodeTypeCode(stream);
            switch (typeCode) {
                case TypeCode.Byte:
                    return DecodeByte(stream);
                case TypeCode.Int:
                    return DecodeInt(stream);
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

        public static byte DecodeByte(MemoryStream stream) {
            return (byte)stream.ReadByte();
        }

        public static int DecodeInt(MemoryStream stream) {
            stream.Read(intBytes, 0, intBytes.Length);
            return (int)BitConverter.ToInt32(intBytes, 0);
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
            if (type == typeof(int))
                return TypeCode.Int;
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
