using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Utilities
{
    public static class SerializationExtensions
    {
        public static void Write(this BinaryWriter writer, Vector3 value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
        }
        
        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static void Write(this BinaryWriter writer, Vector2 value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
        }
        
        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }
        
        public static void WriteList(this BinaryWriter writer, List<string> list)
        {
            writer.Write(list.Count);
            foreach (var element in list) writer.Write(element);
        }

        public static void ReadList(this BinaryReader reader, List<string> list)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++) list.Add(reader.ReadString());
        }
        
        public static void WriteList(this BinaryWriter writer, List<int> list)
        {
            writer.Write(list.Count);
            foreach (var element in list) writer.Write(element);
        }
        
        public static void ReadList(this BinaryReader reader, List<int> list)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++) list.Add(reader.ReadInt32());
        }
        
        public static void WriteList<T>(this BinaryWriter writer, List<T> list, System.Func<T, string> toData)
        {
            writer.Write(list.Count);
            foreach (var element in list) writer.Write(toData(element));
        }
        
        public static void ReadList<T>(this BinaryReader reader, List<T> list, System.Func<string, T> fromData)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++) list.Add(fromData(reader.ReadString()));
        }
        
        public static void WriteSet(this BinaryWriter writer, HashSet<string> set)
        {
            writer.Write(set.Count);
            foreach (var element in set) writer.Write(element);
        }
        
        public static void ReadSet(this BinaryReader reader, HashSet<string> set)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++) set.Add(reader.ReadString());
        }
    }
}