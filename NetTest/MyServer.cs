using NewLife.Log;
using NewLife.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTest
{
    class MyServer : NetServer<MySession>
    {
    }

    class MySession : NetSession
    {
        protected override void OnConnected()
        {
            Send($"欢迎{Remote}光临{Environment.MachineName}的嵌入式NETCore直播间！");

            base.OnConnected();
        }

        protected override void OnDisconnected()
        {
            XTrace.WriteLine("客户端{0}已经断开连接啦", Remote);

            base.OnDisconnected();
        }

        protected override void OnReceive(ReceivedEventArgs e)
        {
            base.OnReceive(e);

            XTrace.WriteLine("收到[{0}]:{1}", Remote, e.Packet.ToStr());

            Send("我收到啦！");
        }
    }
}