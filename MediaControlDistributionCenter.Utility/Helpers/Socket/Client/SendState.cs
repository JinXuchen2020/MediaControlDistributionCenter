using MediaControlDistributionCenter.Helpers.SocketClient.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;  

namespace MediaControlDistributionCenter.Helpers.SocketClient
{
    public class SendState
    {
        public SendState(byte[] data, object state, Action<NetSockSendCompletedResult> sendCompleted = null)
        {
            this.data = data;
            this.state = state;
            this.sendCompleted = sendCompleted;
        }

        public byte[] data;

        public object state;

        public Action<NetSockSendCompletedResult> sendCompleted;
    }
}
