using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLRiptide.Networks
{
    public interface INetworkStartInfo
    {
        ushort Port { get; set; }

        Action OnTick { get; set; }
    }
}
