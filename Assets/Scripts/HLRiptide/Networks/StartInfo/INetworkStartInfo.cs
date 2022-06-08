using System;

namespace HLRiptide.Networks
{
    public interface INetworkStartInfo
    {
        ushort Port { get; set; }

        Action OnTick { get; set; }
    }
}
