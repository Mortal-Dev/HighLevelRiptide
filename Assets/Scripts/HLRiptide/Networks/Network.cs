using HLRiptide.Util.MessageUtil;
using System;
using UnityEngine;
using RiptideNetworking.Utils;

namespace HLRiptide.Networks
{
    internal enum UniversalMessageId
    {
        message = 1,
    }

    public abstract class Network
    {
        public uint NetworkTick { get; private set; } = 0;

        internal static MessageGenerator messageGenerator;

        internal static MessageHandler messageHandler;

        internal Action<ushort> clientJoinAction;
        internal Action<ushort> clientLeaveAction;

        internal NetworkSceneManager networkSceneManager;

        public Network()
        {
            messageGenerator = new MessageGenerator();
            messageHandler = new MessageHandler();

            RiptideLogger.Initialize(Debug.Log, true);
        }

        public abstract void InternalTick();
        
        public abstract void Start(INetworkStartInfo networkStartInfo);

        public abstract void Stop();

        public void Tick()
        {
            NetworkTick++;

            InternalTick();
        }
    }
}