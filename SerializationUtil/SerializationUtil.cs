using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace SerializationUtil
{
    /// <summary>
    /// 支持类型
    /// bool
    /// byte
    /// short
    /// int
    /// long
    /// float
    /// double
    /// string
    /// int[]
    /// string[]
    /// ...
    /// List<int>
    /// List<string>
    /// ...
    /// Dictionary<string, int>
    /// Dictionary<string, string>
    /// Dictionary<string, object>
    /// ...
    /// Class
    /// </summary>
    public enum SUType : byte
    {
        Unknown,

        Byte,
        Bool,
        Short,
        Int,
        Long,
        Float,
        Double,
        String,

        CustomClass,

        Array,
        ObjectArray,

        List,
        ObjectList,

        Dictionary,
    }

    public static class SerializationUtil
    {
        private static Dictionary<Type, CustomType> typeDict = new Dictionary<Type, CustomType>();
        private static Dictionary<byte, CustomType> typeCodeDict = new Dictionary<byte, CustomType>();

        public static void RegisterCustomType(Type type, byte typeCode, SerializationFunc serializationFunc, DeserializationFunc deserializationFunc) {
            if (typeDict.ContainsKey(type))
                return;
            CustomType customType = new CustomType(type, typeCode, serializationFunc, deserializationFunc);
            typeDict.Add(type, customType);
            typeCodeDict.Add(typeCode, customType);
        }

        public static void Serialize(MemoryStream stream, object obj, bool setType = false)
        {
            Type type = obj.GetType();
            if (type.IsPrimitive)
            {
                // 基础类型
                if (type == typeof(bool))
                {
                    SerializeBool(stream, (bool)obj, setType);
                }
                else if (type == typeof(byte))
                {
                    SerializeByte(stream, (byte)obj, setType);
                }
                else if (type == typeof(short))
                {
                    SerializeShort(stream, (short)obj, setType);
                }
                else if (type == typeof(int))
                {
                    SerializeInt(stream, (int)obj, setType);
                }
                else if (type == typeof(float)) {
                    SerializeFloat(stream, (float)obj, setType);
                }
                else if (type == typeof(double)) {
                    SerializeDouble(stream, (double)obj, setType);
                }
            }
            else if (type.IsArray)
            {
                SerializeArray(stream, obj as Array);
            }
            else if (type.IsGenericType)
            {
                // 泛型类型

            }
            else
            {
                // 对象类型
                if (type == typeof(string))
                {
                    SerializeString(stream, obj as string, setType);
                } else {
                    // 自定义类型
                    if (typeDict.TryGetValue(type, out CustomType customType)) {
                        SerializeCustomType(stream, customType, obj);
                    } else {
                        throw new Exception(string.Format("not support type: {0}", type));
                    }
                }
            }
        }

        static void SerializeByte(MemoryStream stream, byte b, bool setType = false)
        {
            if (setType) {
                stream.WriteByte((byte)SUType.Byte);   
            }
            stream.WriteByte(b);
        }

        static void SerializeBool(MemoryStream stream, bool b, bool setType = false)
        {
            if (setType) {
                stream.WriteByte((byte)SUType.Bool);
            }
            byte[] bytes = BitConverter.GetBytes(b);
            stream.Write(bytes, 0, bytes.Length);
        }

        static void SerializeShort(MemoryStream stream, short s, bool setType = false)
        {
            if (setType) {
                stream.WriteByte((byte)SUType.Short);
            }
            byte[] bytes = BitConverter.GetBytes(s);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void SerializeInt(MemoryStream stream, int i, bool setType = false)
        {
            if (setType) {
                stream.WriteByte((byte)SUType.Int);
            }
            byte[] bytes = BitConverter.GetBytes(i);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void SerializeLong(MemoryStream stream, int l, bool setType = false) {
            if (setType) {
                stream.WriteByte((byte)SUType.Long);
            }
            byte[] bytes = BitConverter.GetBytes(l);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void SerializeFloat(MemoryStream stream, float f, bool setType = false) {
            if (setType) {
                stream.WriteByte((byte)SUType.Float);
            }
            byte[] bytes = BitConverter.GetBytes(f);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void SerializeDouble(MemoryStream stream, double d, bool setType = false) {
            if (setType) {
                stream.WriteByte((byte)SUType.Double);
            }
            byte[] bytes = BitConverter.GetBytes(d);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void SerializeString(MemoryStream stream, string str, bool setType = false) {
            if (setType) {
                stream.WriteByte((byte)SUType.String);
            }
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            int length = bytes.Length;
            byte[] lengthBytes = BitConverter.GetBytes(length);
            stream.Write(lengthBytes, 0, lengthBytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void SerializeCustomType(MemoryStream stream, CustomType customType, object obj) {
            stream.WriteByte((byte)SUType.CustomClass);
            stream.WriteByte(customType.TypeCode);
            customType.SerializationFunc.Invoke(stream, obj);
        }

        public static void SerializeArray(MemoryStream stream, Array array) {
            Type type = array.GetType();
            Type eType = type.GetElementType();
            if (eType == typeof(object))
            {
                SerializeObjectArray(stream, array);
            }
            else
            {
                // 指定类型的数组
                // 标记数组类型
                stream.WriteByte((byte)SUType.Array);
                SUType eTypeCode = GetCodeByType(eType);
                if (eTypeCode != SUType.Unknown)
                {
                    SerializePrimaryArray(stream, array, eTypeCode);
                }
                else
                {
                    if (typeDict.TryGetValue(eType, out CustomType customType))
                    {
                        SerializeCustomTypeArray(stream, array, customType);                        
                    }
                }
            }
        }

        public static void SerializePrimaryArray(MemoryStream stream, Array array, SUType eTypeCode) {
            // 标记数组元素类型
            SerializeByte(stream, (byte)eTypeCode);
            // 标记数组长度
            SerializeInt(stream, array.Length);
            // 序列化数组元素
            for (int i = 0; i < array.Length; i++)
            {
                object v = array.GetValue(i);
                Serialize(stream, v);
            }
        }

        public static void SerializeCustomTypeArray(MemoryStream stream, Array array, CustomType customType) {
            SerializeByte(stream, (byte)SUType.CustomClass);
            SerializeByte(stream, customType.TypeCode);
            SerializeInt(stream, array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                object v = array.GetValue(i);
                customType.SerializationFunc.Invoke(stream, v);
            }
        }

        public static void SerializeObjectArray(MemoryStream stream, Array array) {
            // object 类型的数组
            stream.WriteByte((byte)SUType.ObjectArray);
            SerializeInt(stream, array.Length);
            // TODO 序列化 Object
            for (int i = 0; i < array.Length; i++)
            {
                object v = array.GetValue(i);
                Serialize(stream, v, true);
            }
        }

        public static object Deserialize(MemoryStream stream) {
            SUType type = (SUType)stream.ReadByte();
            return Deserialize(stream, type);
        }

        public static object Deserialize(MemoryStream stream, SUType type) {
            if (type == SUType.Bool)
            {
                return DeserializeBool(stream);
            }
            else if (type == SUType.Byte)
            {
                return DeserializeByte(stream);
            }
            else if (type == SUType.Short)
            {
                return DeserializeShort(stream);
            }
            else if (type == SUType.Int)
            {
                return DeserializeInt(stream);
            }
            else if (type == SUType.Float)
            {
                return DeserializeFloat(stream);
            }
            else if (type == SUType.Double) {
                return DeserializeDouble(stream);
            }
            else if (type == SUType.String)
            {
                return DeserializeString(stream);
            }
            else if (type == SUType.Array)
            {
                return DeserializeArray(stream);
            } else if (type == SUType.ObjectArray) {
                return DeserializeObjectArray(stream);
            }
            else if (type == SUType.CustomClass) {
                byte typeCode = (byte)stream.ReadByte();
                CustomType customType = null;
                if (typeCodeDict.TryGetValue(typeCode, out customType)) {
                    return customType.DeserializationFunc.Invoke(stream);
                }
                return null;
            }
            return null;
        }

        public static bool DeserializeBool(MemoryStream stream) {
            byte[] bytes = new byte[1];
            stream.Read(bytes, 0, bytes.Length);
            return BitConverter.ToBoolean(bytes, 0);
        }

        public static byte DeserializeByte(MemoryStream stream) {
            return (byte)stream.ReadByte();
        }

        public static short DeserializeShort(MemoryStream stream) {
            byte[] bytes = new byte[2];
            stream.Read(bytes, 0, bytes.Length);
            return BitConverter.ToInt16(bytes, 0);
        }

        public static int DeserializeInt(MemoryStream stream) {
            byte[] bytes = new byte[4];
            stream.Read(bytes, 0, bytes.Length);
            return BitConverter.ToInt32(bytes, 0);
        }

        public static float DeserializeFloat(MemoryStream stream) {
            byte[] bytes = new byte[4];
            stream.Read(bytes, 0, bytes.Length);
            return BitConverter.ToSingle(bytes, 0);
        }

        public static double DeserializeDouble(MemoryStream stream) {
            byte[] bytes = new byte[8];
            stream.Read(bytes, 0, bytes.Length);
            return BitConverter.ToDouble(bytes, 0);
        }

        public static string DeserializeString(MemoryStream stream) {
            byte[] lengthBytes = new byte[4];
            stream.Read(lengthBytes, 0, 4);
            int length = BitConverter.ToInt32(lengthBytes, 0);
            byte[] strBytes = new byte[length];
            stream.Read(strBytes, 0, length);
            return Encoding.UTF8.GetString(strBytes);
        }

        public static Array DeserializeArray(MemoryStream stream) {
            SUType eTypeCode = (SUType)stream.ReadByte();
            if (eTypeCode == SUType.CustomClass) {
                byte customTypeCode = (byte)stream.ReadByte();
                if (typeCodeDict.TryGetValue(customTypeCode, out CustomType customType)) {
                    byte[] lengthBytes = new byte[4];
                    stream.Read(lengthBytes, 0, 4);
                    int length = BitConverter.ToInt32(lengthBytes, 0);
                    Array array = Array.CreateInstance(customType.Type, length);
                    for (int i = 0; i < length; i++) {
                        object v = customType.DeserializationFunc.Invoke(stream);
                        array.SetValue(v, i);
                    }
                    return array;
                } else {
                    throw new Exception(string.Format("not support type: {0}", customTypeCode));
                }
            } else {
                Type eType = GetTypeByCode(eTypeCode);
                byte[] lengthBytes = new byte[4];
                stream.Read(lengthBytes, 0, 4);
                int length = BitConverter.ToInt32(lengthBytes, 0);
                Array array = Array.CreateInstance(eType, length);
                for (int i = 0; i < length; i++)
                {
                    object v = Deserialize(stream, eTypeCode);
                    array.SetValue(v, i);
                }
                return array;
            }
        }

        public static Array DeserializeObjectArray(MemoryStream stream) {
            byte[] lengthBytes = new byte[4];
            stream.Read(lengthBytes, 0, 4);
            int length = BitConverter.ToInt32(lengthBytes, 0);
            object[] array = new object[length];
            for (int i = 0; i < length; i++)
            {
                SUType eType = (SUType)stream.ReadByte();
                //array[i] = Deserialize()
            }
            return array;
        }

        static SUType GetCodeByType(Type type) {
            if (type == typeof(byte))
                return SUType.Byte;
            if (type == typeof(bool))
                return SUType.Bool;
            if (type == typeof(short))
                return SUType.Short;
            if (type == typeof(int))
                return SUType.Int;
            if (type == typeof(float))
                return SUType.Float;
            if (type == typeof(double))
                return SUType.Double;
            if (type == typeof(string))
                return SUType.String;
            return SUType.Unknown;
        }

        static Type GetTypeByCode(SUType code) {
            switch (code) {
                case SUType.Byte:
                    return typeof(byte);
                case SUType.Bool:
                    return typeof(bool);
                case SUType.Short:
                    return typeof(short);
                case SUType.Int:
                    return typeof(int);
                case SUType.Float:
                    return typeof(float);
                case SUType.Double:
                    return typeof(double);
                case SUType.String:
                    return typeof(string);
                default:
                    throw new Exception(string.Format("not support: {0}", code));
            }
        }
    }
}
