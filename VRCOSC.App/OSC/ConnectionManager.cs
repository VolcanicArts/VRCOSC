// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MeaMod.DNS.Model;
using MeaMod.DNS.Multicast;
using Newtonsoft.Json;
using VRCOSC.App.OSC.Query;
using VRCOSC.App.Utils;

namespace VRCOSC.App.OSC;

public class ConnectionManager
{
    private const int refresh_interval = 2500;

    public bool IsConnected => VRChatQueryPort is not null && VRChatSendPort is not null;

    public int? VRChatQueryPort;
    public int? VRChatSendPort;
    public int VRCOSCReceivePort;

    private MulticastService mdns = null!;
    private ServiceDiscovery serviceDiscovery = null!;
    private HttpListener queryServer = null!;
    private int vrcoscQueryPort;

    private CancellationTokenSource? refreshTaskCancellationSource;
    private Task? refreshTask;

    public void Init()
    {
        refreshTaskCancellationSource = new CancellationTokenSource();
        refreshTask = Task.Run(refreshServices);

        VRCOSCReceivePort = getAvailableUdpPort();
        vrcoscQueryPort = getAvailableTcpPort();
        queryServer = new HttpListener();

        string prefix = $"http://{IPAddress.Loopback}:{vrcoscQueryPort}/";
        queryServer.Prefixes.Add(prefix);
        queryServer.Start();
        queryServer.BeginGetContext(httpListenerLoop, queryServer);

        mdns = new MulticastService
        {
            UseIpv6 = false,
        };

        serviceDiscovery = new ServiceDiscovery(mdns);

        mdns.AnswerReceived += onRemoteServiceInfo;
        mdns.Start();

        var serviceOscQ = new ServiceProfile("VRCOSC", "_oscjson._tcp", (ushort)vrcoscQueryPort, new[] { IPAddress.Loopback });
        serviceDiscovery.Advertise(serviceOscQ);

        Logger.Log($"Listening for OSC on {VRCOSCReceivePort}");
        Logger.Log($"Listening for OSCQ on {vrcoscQueryPort}");
    }

    public void Reset()
    {
        refreshTask = null;
        VRChatQueryPort = null;
        VRChatSendPort = null;
    }

    #region OSCQuery Server

    private async void httpListenerLoop(IAsyncResult result)
    {
        var context = queryServer.EndGetContext(result);
        queryServer.BeginGetContext(httpListenerLoop, queryServer);

        await hostInfoResponse(context);
        await rootNodeResponse(context);
    }

    private async Task hostInfoResponse(HttpListenerContext context)
    {
        if (!context.Request.RawUrl!.Contains("HOST_INFO")) return;

        try
        {
            var serialisedHostInfo = JsonConvert.SerializeObject(new HostInfo(VRCOSCReceivePort));
            context.Response.Headers.Add("pragma:no-cache");
            context.Response.ContentType = "application/json";
            context.Response.ContentLength64 = serialisedHostInfo.Length;

            using var sw = new StreamWriter(context.Response.OutputStream);
            await sw.WriteAsync(serialisedHostInfo);
            await sw.FlushAsync();
        }
        catch (Exception)
        {
        }
    }

    private async Task rootNodeResponse(HttpListenerContext context)
    {
        try
        {
            var path = context.Request.Url!.LocalPath;
            if (path != "/") return;

            var rootNode = new OSCQueryRootNode();

            // Register a single child node as VRChat sends everything for some reason but doesn't if you only register the root
            rootNode.AddNode(new OSCQueryNode("/avatar/change")
            {
                OscType = "s",
                Access = OSCQueryNodeAccess.Write,
                Value = new object[] { string.Empty }
            });

            var stringResponse = JsonConvert.SerializeObject(rootNode, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            context.Response.Headers.Add("pragma:no-cache");
            context.Response.ContentType = "application/json";
            context.Response.ContentLength64 = stringResponse.Length;

            using var sw = new StreamWriter(context.Response.OutputStream);

            await sw.WriteAsync(stringResponse);
            await sw.FlushAsync();
        }
        catch (Exception)
        {
        }
    }

    #endregion

    private async Task refreshServices()
    {
        if (IsConnected || refreshTaskCancellationSource!.IsCancellationRequested) return;

        mdns.SendQuery("_osc._udp.local");
        mdns.SendQuery("_oscjson._tcp.local");

        await Task.Delay(refresh_interval);
    }

    private static readonly IPEndPoint default_loopback_endpoint = new(IPAddress.Loopback, 0);

    private static int getAvailableUdpPort()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(default_loopback_endpoint);
        return ((IPEndPoint)socket.LocalEndPoint!).Port;
    }

    private static int getAvailableTcpPort()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(default_loopback_endpoint);
        return ((IPEndPoint)socket.LocalEndPoint!).Port;
    }

    private void onRemoteServiceInfo(object? sender, MessageEventArgs eventArgs)
    {
        var response = eventArgs.Message;

        if (response.Answers.All(a => !a.CanonicalName.Contains("_osc._udp") && !a.CanonicalName.Contains("_oscjson._tcp"))) return;

        try
        {
            foreach (var record in response.AdditionalRecords.OfType<SRVRecord>())
            {
                addMatchedService(record);
            }
        }
        catch (Exception e)
        {
            Logger.Log($"Could not parse answer from {eventArgs.RemoteEndPoint}: {e.Message}");
        }
    }

    private void addMatchedService(SRVRecord srvRecord)
    {
        var port = srvRecord.Port;
        var domainName = srvRecord.Name.Labels;
        var instanceName = domainName[0];
        var serviceName = string.Join(".", domainName.Skip(1));

        if (instanceName.Contains("VRChat-Client") && serviceName.Contains("_osc._udp"))
        {
            VRChatSendPort = port;
        }

        if (instanceName.Contains("VRChat-Client") && serviceName.Contains("_oscjson._tcp"))
        {
            VRChatQueryPort = port;
        }
    }
}
