using System;
using System.Collections.Generic;
using UnityEngine;

namespace HLRiptide.Util.ContainerUtil
{
    public class Container<T> where T : IId
    {
        public Dictionary<uint, T> ContainerDict { get; private set; }

        System.Random random = new System.Random();

        public Container()
        {
            ContainerDict = new Dictionary<uint, T>();
        }

        public void RegisterValue(T value, uint overrideId)
        {
            RegisterValue(value);

            OverrideId(value.Id, overrideId);
        }

        public void RegisterValue(T value)
        {
            IId networkedId = value;

            //id was already assigned, so we don't assign it a random Id
            if (networkedId.Id != 0)
            {
                ContainerDict.Add(networkedId.Id, value);
            }
            else
            {
                uint randomId = GenerateRandomUint();

                if (ContainerDict.ContainsKey(randomId))
                {
                    RegisterValue(value);
                    return;
                }

                networkedId.Id = randomId;

                ContainerDict.Add(networkedId.Id, value);
            }
        }

        public void RemoveValue(IId id)
        {
            ContainerDict.Remove(id.Id);
        }

        public void RemoveValue(uint id)
        {
            ContainerDict.Remove(id);
        }

        public T GetValue(IId id)
        {
            return GetValue(id.Id);
        }

        public T GetValue(uint id)
        {
            if (ContainerDict.TryGetValue(id, out T value))
            {
                return value;
            }
            else
            {
                return default;
            }
        }

        public void OverrideId(uint oldId, uint newId)
        {
            IId iid = GetValue(oldId);

            if (iid == null) throw new KeyNotFoundException();

            RemoveValue(iid);

            iid.Id = newId;

            ContainerDict.Add(iid.Id, (T)iid);
        }

        private uint GenerateRandomUint()
        {
            uint num = (uint)random.Next(int.MinValue, int.MaxValue);

            return num;
        }
    }
}