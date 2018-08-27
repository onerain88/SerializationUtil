using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace SerializationUtil
{
    public class Hero
    {
        public string Name {
            get; set;
        }

        public int Gold {
            get; set;
        }

        public List<int> ItemIdList {
            get; set;
        }

        public Hero() {
            this.ItemIdList = new List<int>();
        }

        public static void Serialize(MemoryStream stream, object obj) {
            Hero hero = obj as Hero;
            SerializationUtil.SerializeString(stream, hero.Name);
            SerializationUtil.SerializeInt(stream, hero.Gold);
            // 不考虑为 null 的情况
            SerializationUtil.SerializePrimaryList(stream, hero.ItemIdList, SerializationUtil.GetCodeByType(typeof(int)));
        }

        public static object Deserialize(MemoryStream stream) {
            Hero hero = new Hero();
            hero.Name = SerializationUtil.DeserializeString(stream);
            hero.Gold = SerializationUtil.DeserializeInt(stream);
            hero.ItemIdList = SerializationUtil.DeserializeList(stream) as List<int>;
            return hero;
        }

        public static void Encode(MemoryStream stream, object obj) {
            Hero hero = obj as Hero;
            Serializer.Serializer.Encode(stream, hero.Name);
            Serializer.Serializer.Encode(stream, hero.Gold);
            Serializer.Serializer.Encode(stream, hero.ItemIdList);
        }

        public static object Decode(MemoryStream stream) {
            Hero hero = new Hero();
            hero.Name = Serializer.Serializer.Decode(stream) as string;
            hero.Gold = (int)Serializer.Serializer.Decode(stream);
            var idObjList = Serializer.Serializer.Decode(stream) as List<object>;
            hero.ItemIdList = idObjList.Cast<int>().ToList();
            return hero;
        }
    }
}
