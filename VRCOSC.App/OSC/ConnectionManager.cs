﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
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
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.OSC;

public class ConnectionManager
{
    private const int refresh_interval = 2500;
    private const string osc_service_name = "_osc._udp";
    private const string osc_query_service_name = "_oscjson._tcp";
    private const string vrchat_client_name = "VRChat-Client";

    public bool IsConnected => VRChatIP is not null && VRChatQueryPort is not null && VRChatReceivePort is not null;

    public IPAddress? VRChatIP { get; private set; }
    public int? VRChatQueryPort { get; private set; }
    public int? VRChatReceivePort { get; private set; }

    public IPAddress? VRCOSCIP { get; private set; }
    public int? VRCOSCQueryPort { get; private set; }
    public int? VRCOSCReceivePort { get; private set; }

    private MulticastService? mdns;
    private ServiceDiscovery? serviceDiscovery;
    private HttpListener? queryServer;

    private Repeater? refreshTask;

    public void Start() => Task.Run(() =>
    {
        try
        {
            var oscMode = SettingsManager.GetInstance().GetValue<ConnectionMode>(VRCOSCSetting.ConnectionMode);

            refreshTask = new Repeater($"{nameof(ConnectionManager)}-{nameof(refreshServices)}", refreshServices);
            refreshTask.Start(TimeSpan.FromMilliseconds(refresh_interval));

            VRCOSCIP = oscMode == ConnectionMode.LAN ? getLocalIpAddress() : IPAddress.Loopback;
            VRCOSCReceivePort = getAvailableUDPPort();
            VRCOSCQueryPort = getAvailableTCPPort();
            queryServer = new HttpListener();

            queryServer.Prefixes.Add($"http://{VRCOSCIP}:{VRCOSCQueryPort}/");
            queryServer.Start();
            queryServer.BeginGetContext(httpListenerLoop, queryServer);

            mdns = new MulticastService
            {
                UseIpv6 = false
            };

            serviceDiscovery = new ServiceDiscovery(mdns);

            mdns.AnswerReceived += onRemoteServiceInfo;
            mdns.Start();

            var serviceOscQ = new ServiceProfile(AppManager.APP_NAME, osc_query_service_name, (ushort)VRCOSCQueryPort, [VRCOSCIP]);
            serviceDiscovery.Advertise(serviceOscQ);

            Logger.Log($"Hosting IP address: {VRCOSCIP}");
            Logger.Log($"Hosting OSC port: {VRCOSCReceivePort}");
            Logger.Log($"Hosting OSCQuery port: {VRCOSCQueryPort}");
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"{nameof(ConnectionManager)} has experienced an exception when starting OSCQuery");
        }
    }).ConfigureAwait(false);

    public async Task Stop()
    {
        if (refreshTask is not null)
            await refreshTask.StopAsync();

        queryServer?.Stop();
        serviceDiscovery?.Unadvertise();
        mdns?.Stop();

        refreshTask = null;
        queryServer = null;
        serviceDiscovery = null;
        mdns = null;

        VRChatIP = null;
        VRChatQueryPort = null;
        VRChatReceivePort = null;

        VRCOSCIP = null;
        VRCOSCQueryPort = null;
        VRCOSCReceivePort = null;
    }

    #region OSCQuery Server

    private async void httpListenerLoop(IAsyncResult result)
    {
        try
        {
            var context = queryServer!.EndGetContext(result);
            queryServer.BeginGetContext(httpListenerLoop, queryServer);

            await hostInfoResponse(context);
            await rootNodeResponse(context);
        }
        catch (Exception)
        {
            // ignore anything from ending the context
        }
    }

    private async Task hostInfoResponse(HttpListenerContext context)
    {
        if (!context.Request.RawUrl!.Contains("HOST_INFO")) return;

        try
        {
            var serialisedHostInfo = JsonConvert.SerializeObject(new HostInfo(VRCOSCIP!.ToString(), VRCOSCReceivePort!.Value));
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
        if (IsConnected || mdns is null) return;

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
                handleMatchedService(eventArgs.RemoteEndPoint.Address, record);
            }
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"Could not parse answer from {eventArgs.RemoteEndPoint}: {e.Message}");
        }
    }

    private void handleMatchedService(IPAddress sourceIpAddress, SRVRecord srvRecord)
    {
        var port = srvRecord.Port;
        var domainName = srvRecord.Name.Labels;
        var instanceName = domainName[0];
        var serviceName = string.Join(".", domainName.Skip(1));

        if (!instanceName.Contains(vrchat_client_name)) return;

        var oscMode = SettingsManager.GetInstance().GetValue<ConnectionMode>(VRCOSCSetting.ConnectionMode);

        if (serviceName.Contains(osc_service_name))
        {
            // TODO: When this changes, send event for modules to restart?
            if (port == VRChatReceivePort) return;

            VRChatReceivePort = port;
            Logger.Log($"Found VRChat's OSC port: {VRChatReceivePort}");

            if (oscMode == ConnectionMode.LAN)
            {
                VRChatIP = sourceIpAddress;
                Logger.Log($"Found VRChat's LAN OSC IP address: {VRChatIP}");
            }
            else
            {
                VRChatIP = IPAddress.Loopback;
                Logger.Log($"Using {IPAddress.Loopback} as VRChat's local OSC IP address");
            }
        }

        if (serviceName.Contains(osc_query_service_name))
        {
            // TODO: When this changes, send event for modules to restart?
            if (port == VRChatQueryPort) return;

            VRChatQueryPort = port;
            Logger.Log($"Found VRChat's OSCQuery port: {VRChatQueryPort}");
        }
    }

    private static IPAddress getLocalIpAddress()
    {
        var hostName = Dns.GetHostName();
        var hostEntry = Dns.GetHostEntry(hostName);

        foreach (var ip in hostEntry.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork) return ip;
        }

        throw new Exception("No network adapters with an IPv4 address in the system");
    }
}