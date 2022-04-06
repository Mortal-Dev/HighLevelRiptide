using RiptideNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.HLRiptide.Util.SerializeUtil
{
    public static class PrimativeSerializers
    {
        //i have a love-hate relationship with lambdas
        private static readonly Dictionary<Type, Func<Message, object>> getValueMethods = new Dictionary<Type, Func<Message, object>>()
        {
            { typeof(int), (Message messge) => { return messge.GetInt(); } },
            { typeof(uint), (Message messge) => { return messge.GetUInt(); } },
            { typeof(short), (Message messge) => { return messge.GetShort(); } },
            { typeof(ushort), (Message messge) => { return messge.GetUShort(); } },
            { typeof(byte), (Message messge) => { return messge.GetByte(); } },
            { typeof(long), (Message messge) => { return messge.GetLong(); } },
            { typeof(ulong), (Message messge) => { return messge.GetULong(); } },
            { typeof(bool), (Message messge) => { return messge.GetBool(); } },
            { typeof(char), (Message messge) => { return messge.GetUShort(); } },
            { typeof(string), (Message messge) => { return messge.GetString(); } },
            { typeof(string[]), (Message messge) => { return messge.GetStrings(); } },
            { typeof(int[]), (Message messge) => { return messge.GetInts(); } },
            { typeof(uint[]), (Message messge) => { return messge.GetUInts(); } },
            { typeof(short[]), (Message messge) => { return messge.GetShorts(); } },
            { typeof(ushort[]), (Message messge) => { return messge.GetUShorts(); } },
            { typeof(byte[]), (Message messge) => { return messge.GetBytes(); } },
            { typeof(long[]), (Message messge) => { return messge.GetLongs(); } },
            { typeof(ulong[]), (Message messge) => { return messge.GetULongs(); } },
            { typeof(bool[]), (Message messge) => { return messge.GetBools(); } },
        };

        private static readonly Dictionary<Type, Action<object, Message>> addValueMethods = new Dictionary<Type, Action<object, Message>>()
        {
            { typeof(int), (object value, Message message) => message.Add((int)value) },
            { typeof(uint), (object value, Message message) => message.Add((uint)value) },
            { typeof(short), (object value, Message message) => message.Add((short)value) },
            { typeof(ushort), (object value, Message message) => message.Add((ushort)value) },
            { typeof(byte), (object value, Message message) => message.Add((byte)value) },
            { typeof(long), (object value, Message message) => message.Add((long)value) },
            { typeof(ulong), (object value, Message message) => message.Add((ulong)value) },
            { typeof(bool), (object value, Message message) => message.Add((bool)value) },
            { typeof(char), (object value, Message message) => message.Add((char)value) },
            { typeof(string), (object value, Message message) => message.Add((string)value) },
            { typeof(int[]), (object value, Message message) => message.Add((int[])value) },
            { typeof(uint[]), (object value, Message message) => message.Add((uint[])value) },
            { typeof(short[]), (object value, Message message) => message.Add((short[])value) },
            { typeof(ushort[]), (object value, Message message) => message.Add((ushort[])value) },
            { typeof(byte[]), (object value, Message message) => message.Add((byte[])value) },
            { typeof(long[]), (object value, Message message) => message.Add((long[])value) },
            { typeof(ulong[]), (object value, Message message) => message.Add((ulong[])value) },
            { typeof(bool[]), (object value, Message message) => message.Add((bool[])value) },
            { typeof(string[]), (object value, Message message) => message.Add((string[])value) }
        };

        public static void AddPrimativeToMessage<T>(Message message, T value)
        {
            if (addValueMethods.TryGetValue(typeof(T), out Action<object, Message> method))
            {
                method?.Invoke(value, message);
            }
            else
            {
                byte[] byteValue = ConvertStructToByteArr(value);
                message.Add(byteValue);
            }
        }

        public static T GetPrimativeFromMessage<T>(Message message)
        {
            if (getValueMethods.TryGetValue(typeof(T), out Func<Message, object> method))
            {
                object value = method.Invoke(message);
                return (T)value;
            }
            else
            {
                object value = null;

                ConvertByteArrToStruct(message.GetBytes(), ref value);

                return (T)value;
            }
        }

        private static void ConvertByteArrToStruct(byte[] data, ref object obj)
        {
            int len = Marshal.SizeOf(obj);

            IntPtr i = Marshal.AllocHGlobal(len);

            Marshal.Copy(data, 0, i, len);

            obj = Marshal.PtrToStructure(i, obj.GetType());

            Marshal.FreeHGlobal(i);
        }

        private static byte[] ConvertStructToByteArr(object obj)
        {
            int len = Marshal.SizeOf(obj);

            byte[] arr = new byte[len];

            IntPtr ptr = Marshal.AllocHGlobal(len);

            Marshal.StructureToPtr(obj, ptr, true);

            Marshal.Copy(ptr, arr, 0, len);

            Marshal.FreeHGlobal(ptr);

            return arr;
        }
    }
}