// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
    private const string osc_service_name = "_osc._udp";
    private const string osc_query_service_name = "_oscjson._tcp";
    private const string vrchat_client_name = "VRChat-Client";

    public bool IsConnected => VRChatQueryPort is not null && VRChatReceivePort is not null;

    public int? VRChatQueryPort { get; private set; }
    public int? VRChatReceivePort { get; private set; }

    public int VRCOSCQueryPort { get; private set; }
    public int VRCOSCReceivePort { get; private set; }

    private MulticastService mdns = null!;
    private ServiceDiscovery serviceDiscovery = null!;
    private HttpListener queryServer = null!;

    private Repeater? refreshTask;

    public void Init() => Task.Run(() =>
    {
        refreshTask = new Repeater($"{nameof(ConnectionManager)}-{nameof(refreshServices)}", refreshServices);
        refreshTask.Start(TimeSpan.FromMilliseconds(refresh_interval));

        VRCOSCReceivePort = getAvailableUDPPort();
        VRCOSCQueryPort = getAvailableTCPPort();
        queryServer = new HttpListener();

        queryServer.Prefixes.Add($"http://{IPAddress.Loopback}:{VRCOSCQueryPort}/");
        queryServer.Start();
        queryServer.BeginGetContext(httpListenerLoop, queryServer);

        mdns = new MulticastService
        {
            UseIpv6 = false
        };

        serviceDiscovery = new ServiceDiscovery(mdns);

        mdns.AnswerReceived += onRemoteServiceInfo;
        mdns.Start();

        var serviceOscQ = new ServiceProfile(AppManager.APP_NAME, osc_query_service_name, (ushort)VRCOSCQueryPort, new[] { IPAddress.Loopback });
        serviceDiscovery.Advertise(serviceOscQ);

        Logger.Log($"Receiving OSC on {VRCOSCReceivePort}");
        Logger.Log($"Hosting OSCQuery on {VRCOSCQueryPort}");
    }).ConfigureAwait(false);

    public void Reset()
    {
        refreshTask = null;
        VRChatQueryPort = null;
        VRChatReceivePort = null;
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

            using var sw = new StreamWriter(context.Response.OutputStream);
            await sw.WriteAsync(serialisedHostInfo);
            await sw.FlushAsync();
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, "Error handling host info response");
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

            using var sw = new StreamWriter(context.Response.OutputStream);

            await sw.WriteAsync(stringResponse);
            await sw.FlushAsync();
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, "Error handling root node response");
        }
    }

    #endregion

    private void refreshServices()
    {
        if (IsConnected) return;

        mdns.SendQuery($"{osc_service_name}.local");
        mdns.SendQuery($"{osc_query_service_name}.local");
    }

    private static readonly IPEndPoint default_loopback_endpoint = new(IPAddress.Loopback, 0);

    private static int getAvailableUDPPort()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(default_loopback_endpoint);
        return ((IPEndPoint)socket.LocalEndPoint!).Port;
    }

    private static int getAvailableTCPPort()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(default_loopback_endpoint);
        return ((IPEndPoint)socket.LocalEndPoint!).Port;
    }

    private void onRemoteServiceInfo(object? sender, MessageEventArgs eventArgs)
    {
        var response = eventArgs.Message;

        if (response.Answers.All(a => !a.CanonicalName.Contains(osc_service_name) && !a.CanonicalName.Contains(osc_query_service_name))) return;

        try
        {
            foreach (var record in response.AdditionalRecords.OfType<SRVRecord>())
            {
                handleMatchedService(record);
            }
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"Could not parse answer from {eventArgs.RemoteEndPoint}: {e.Message}");
        }
    }

    private void handleMatchedService(SRVRecord srvRecord)
    {
        var port = srvRecord.Port;
        var domainName = srvRecord.Name.Labels;
        var instanceName = domainName[0];
        var serviceName = string.Join(".", domainName.Skip(1));

        // TODO: If there's multiple instances of the VRChat client on the network, prioritise the one with the same LAN IP as the PC that we're running on

        if (instanceName.Contains(vrchat_client_name) && serviceName.Contains(osc_service_name))
        {
            if (port == VRChatReceivePort) return;

            VRChatReceivePort = port;
            Logger.Log($"Found VRChat's OSC port: {VRChatReceivePort}");
        }

        if (instanceName.Contains(vrchat_client_name) && serviceName.Contains(osc_query_service_name))
        {
            if (port == VRChatQueryPort) return;

            VRChatQueryPort = port;
            Logger.Log($"Found VRChat's OSCQuery port: {VRChatQueryPort}");
        }
    }
}