﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Test.Common;

using Xunit;
using Xunit.Abstractions;

namespace System.Net.Sockets.Tests
{
    /// <summary>
    /// Summary description for AgnosticListenerTest
    /// </summary>
    public class AgnosticListenerTest
    {
        public AgnosticListenerTest(ITestOutputHelper _log)
        {
            Assert.True(Capability.IPv4Support() && Capability.IPv6Support());
        }

        [Fact]
        public void Create_Success()
        {
            // NOTE: the '0' below will cause the TcpListener to bind to an anonymous port.
            TcpListener listener = TcpListener.Create(0);
            listener.Start();
            listener.Stop();
        }

        [Fact]
        public void ConnectWithV4_Success()
        {
            int port;
            TcpListener listener = SocketTestExtensions.CreateAndStartTcpListenerOnAnonymousPort(out port);
            IAsyncResult asyncResult = listener.BeginAcceptTcpClient(null, null);

            TcpClient client = new TcpClient(AddressFamily.InterNetwork);
            client.Connect(new IPEndPoint(IPAddress.Loopback, port));

            TcpClient acceptedClient = listener.EndAcceptTcpClient(asyncResult);
            client.Dispose();
            acceptedClient.Dispose();
            listener.Stop();
        }

        [Fact]
        public void ConnectWithV6_Success()
        {
            int port;
            TcpListener listener = SocketTestExtensions.CreateAndStartTcpListenerOnAnonymousPort(out port);
            IAsyncResult asyncResult = listener.BeginAcceptTcpClient(null, null);

            TcpClient client = new TcpClient(AddressFamily.InterNetworkV6);
            client.Connect(new IPEndPoint(IPAddress.IPv6Loopback, port));

            TcpClient acceptedClient = listener.EndAcceptTcpClient(asyncResult);
            client.Dispose();
            acceptedClient.Dispose();
            listener.Stop();
        }
        
        [Fact]
        public void ConnectWithV4AndV6_Success()
        {
            int port;
            TcpListener listener = SocketTestExtensions.CreateAndStartTcpListenerOnAnonymousPort(out port);
            IAsyncResult asyncResult = listener.BeginAcceptTcpClient(null, null);

            TcpClient v6Client = new TcpClient(AddressFamily.InterNetworkV6);
            v6Client.Connect(new IPEndPoint(IPAddress.IPv6Loopback, port));

            TcpClient acceptedV6Client = listener.EndAcceptTcpClient(asyncResult);
            Assert.Equal(AddressFamily.InterNetworkV6, acceptedV6Client.Client.RemoteEndPoint.AddressFamily);
            Assert.Equal(AddressFamily.InterNetworkV6, v6Client.Client.RemoteEndPoint.AddressFamily);

            asyncResult = listener.BeginAcceptTcpClient(null, null);

            TcpClient v4Client = new TcpClient(AddressFamily.InterNetwork);
            v4Client.Connect(new IPEndPoint(IPAddress.Loopback, port));

            TcpClient acceptedV4Client = listener.EndAcceptTcpClient(asyncResult);
            Assert.Equal(AddressFamily.InterNetworkV6, acceptedV4Client.Client.RemoteEndPoint.AddressFamily);
            Assert.Equal(AddressFamily.InterNetwork, v4Client.Client.RemoteEndPoint.AddressFamily);
            
            v6Client.Dispose();
            acceptedV6Client.Dispose();

            v4Client.Dispose();
            acceptedV4Client.Dispose();

            listener.Stop();
        }

        #region GC Finalizer test
        // This test assumes sequential execution of tests and that it is going to be executed after other tests
        // that used Sockets. 
        [Fact]
        public void TestFinalizers()
        {
            // Making several passes through the FReachable list.
            for (int i = 0; i < 3; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        #endregion 
    }
}
