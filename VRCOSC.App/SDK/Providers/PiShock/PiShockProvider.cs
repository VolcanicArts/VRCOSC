// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Utils;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Providers.PiShock;

public class PiShockProvider
{
    private const string broker_endpoint = "wss://broker.pishock.com/v2";

    private readonly JsonSerializerOptions serialiserOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient httpClient = new();
    private readonly Lock sharedShockersLock = new();
    private readonly string username;
    private readonly string apiKey;

    private WebSocketClient? webSocket;
    private TokenSourceTask? serialTask;
    private PiShockSerialInstance? serialInstance;
    private List<PiShockShocker> availableShockers { get; } = [];
    private bool initialised;
    private int userId = -1;
    private int clientId = -1;

    public PiShockProvider(string username, string apiKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        this.username = username;
        this.apiKey = apiKey;
    }

    public async Task<bool> Initialise()
    {
        if (initialised) throw new InvalidOperationException("Cannot initialise whilst already initialised");

        try
        {
            var authResult = await authenticateUser();
            if (!authResult) return false;

            var refreshClientResult = await populateClientId();
            if (!refreshClientResult) return false;

            var refreshShockersResult = await populateAvailableShockers();
            if (!refreshShockersResult) return false;

            webSocket = new WebSocketClient($"{broker_endpoint}?Username={username}&ApiKey={apiKey}", 2000, 3);
            webSocket.OnWsDisconnected += () => initialised = false;
            await webSocket.ConnectAsync();

            var serialConnectionSource = new CancellationTokenSource();
            var serialConnectionTask = Task.Run(scanSerialPorts, serialConnectionSource.Token);
            serialTask = new TokenSourceTask(serialConnectionSource, serialConnectionTask);

            initialised = true;
            return true;
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"{nameof(PiShockProvider)} has experienced an exception when initialising");
            initialised = false;
            return false;
        }
    }

    public async Task Teardown()
    {
        try
        {
            if (webSocket is not null)
            {
                await webSocket.DisconnectAsync();
                webSocket.Dispose();
            }

            if (serialTask is not null)
                await serialTask.CancelAndWaitAsync();

            serialInstance?.Serial.Close();
        }
        catch (Exception e)
        {
            Logger.Error(e, $"{nameof(PiShockProvider)} has experienced an exception when tearing down");
        }
        finally
        {
            webSocket = null;
            serialTask = null;
            serialInstance = null;
            userId = -1;
            clientId = -1;
            initialised = false;
        }
    }

    private async Task<bool> authenticateUser()
    {
        var authResult = await PiShockRequestFactory.AuthenticateUser(username, apiKey);

        if (!authResult)
        {
            Logger.Error(authResult.Exception, $"Error in {nameof(PiShockProvider)}");
            userId = -1;
            return false;
        }

        var user = authResult.Value;
        userId = user.UserId;
        return true;
    }

    private async Task<bool> populateClientId()
    {
        var devicesResult = await PiShockRequestFactory.GetUserDevices(userId, apiKey);

        if (!devicesResult)
        {
            Logger.Error(devicesResult.Exception, $"Error in {nameof(PiShockProvider)}");
            clientId = -1;
            return false;
        }

        var devices = devicesResult.Value;
        clientId = devices[0].ClientId;
        return true;
    }

    private async Task<bool> populateAvailableShockers()
    {
        var shareIDResult = await PiShockRequestFactory.GetShareCodesByOwner(userId, apiKey);

        if (!shareIDResult)
        {
            Logger.Error(shareIDResult.Exception, $"Error in {nameof(PiShockProvider)}");
            return false;
        }

        var shareIDs = shareIDResult.Value;

        // if we cannot get any shockers someone might be generating their first sharecode so return true
        if (shareIDs.Count == 0) return true;

        var localAvailableShockersResult = await getShockersFromShareIds(shareIDs.SelectMany(pair => pair.Value));

        if (!localAvailableShockersResult)
        {
            Logger.Error(localAvailableShockersResult.Exception, $"Error in {nameof(PiShockProvider)}");
            return false;
        }

        var localAvailableShockers = localAvailableShockersResult.Value;

        lock (sharedShockersLock)
        {
            availableShockers.Clear();
            availableShockers.AddRange(localAvailableShockers);
        }

        return true;
    }

    private async Task<Result<PiShockShocker[]>> getShockersFromShareIds(IEnumerable<int> shareIds)
    {
        var devicesResult = await PiShockRequestFactory.GetShockersByShareIDs(userId, apiKey, shareIds);

        if (!devicesResult)
        {
            Logger.Error(devicesResult.Exception, $"Error in {nameof(PiShockProvider)}");
            return devicesResult.Exception;
        }

        var devices = devicesResult.Value;
        return devices.SelectMany(p => p.Value).ToArray();
    }

    private bool disableSerialScan;

    private async Task scanSerialPorts()
    {
        Debug.Assert(serialTask is not null);

        try
        {
            while (!serialTask.Source.IsCancellationRequested && !disableSerialScan)
            {
                if (serialInstance is not null) continue;

                var ports = SerialPort.GetPortNames();

                foreach (var port in ports)
                {
                    var serial = new SerialPort(port, 115200);
                    serial.WriteTimeout = 2000;
                    serial.ReadTimeout = 2000;
                    serial.WriteBufferSize = 4096;
                    serial.ReadBufferSize = 4096;
                    serial.NewLine = "\n";

                    try
                    {
                        var command = JsonSerializer.Serialize(new PiShockSerialCommand
                        {
                            Command = "info"
                        }, serialiserOptions);

                        serial.Open();
                        serial.WriteLine(command);
                        await Task.Delay(200);

                        var response = serial.ReadExisting();

                        foreach (var line in response.Split('\n'))
                        {
                            if (!line.StartsWith("TERMINALINFO:")) continue;

                            var terminalInfo = line["TERMINALINFO:".Length..];

                            var serialInfo = JsonSerializer.Deserialize<PiShockSerialTerminalInfoResponse>(terminalInfo);
                            if (serialInfo is null) continue;

                            serialInstance = new PiShockSerialInstance(serialInfo, serial);
                            break;
                        }

                        if (serialInstance is null)
                        {
                            serial.Close();
                            serial = null;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, $"{nameof(PiShockProvider)} has experienced an error when scanning serial port {port}");

                        if (serial!.IsOpen)
                            serial.Close();

                        serialInstance = null;
                        disableSerialScan = true;
                    }
                }

                await Task.Delay(5000);
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, $"{nameof(PiShockProvider)} has experienced an error when scanning serial ports");
        }
    }

    public Task<PiShockResult> ExecuteSerialAsync(PiShockMode mode, int intensity, int duration, int? shockerId)
    {
        if (!initialised) return Task.FromResult(new PiShockResult(false, "Provider not initialised"));
        if (serialInstance is null) return Task.FromResult(new PiShockResult(false, "Serial has not initialised"));

        if (mode == PiShockMode.End)
        {
            intensity = 0;
            duration = 0;
        }

        if (mode == PiShockMode.Beep)
        {
            intensity = 0;
        }

        try
        {
            var commands = new List<string>();

            if (shockerId.HasValue)
            {
                commands.Add(JsonSerializer.Serialize(new PiShockSerialCommand
                    {
                        Command = "operate",
                        Body = new PiShockSerialBody
                        {
                            ShockerId = shockerId.Value,
                            Op = mode.ToCode(),
                            Duration = duration,
                            Intensity = intensity
                        }
                    }, serialiserOptions
                ));
            }
            else
            {
                commands.AddRange(serialInstance.Info.Shockers.Select(shocker => JsonSerializer.Serialize(new PiShockSerialCommand
                {
                    Command = "operate",
                    Body = new PiShockSerialBody
                    {
                        ShockerId = shocker.ShockerId,
                        Op = mode.ToCode(),
                        Duration = duration,
                        Intensity = intensity
                    }
                }, serialiserOptions)));
            }

            foreach (var command in commands)
            {
                serialInstance.Serial.WriteLine(command);
            }

            return Task.FromResult(new PiShockResult(true, "Success"));
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"{nameof(PiShockProvider)} has experienced an error when executing serial");
            return Task.FromResult(new PiShockResult(false, "An error has occured writing to serial"));
        }
    }

    public async Task<PiShockResult> ExecuteAsync(int shockerId, PiShockMode mode, int intensity, int duration)
    {
        if (!initialised) return new PiShockResult(false, "Provider not initialised");

        await executeAsync($"c{clientId}-ops", [shockerId], mode, intensity, duration);
        return new PiShockResult(true, "Success");
    }

    public async Task<PiShockResult> ExecuteAsync(IEnumerable<string> shareCodes, PiShockMode mode, int intensity, int duration)
    {
        if (!initialised) return new PiShockResult(false, "Provider not initialised");

        var shareCodeArray = shareCodes.ToArray();
        var missingShareCodes = shareCodeArray.Where(code => availableShockers.All(shocker => shocker.ShareCode != code)).ToArray();

        foreach (var code in missingShareCodes)
        {
            var claimed = await claimShareCode(code);
            if (!claimed.Success) return claimed;
        }

        if (missingShareCodes.Length != 0)
        {
            var refreshResult = await populateAvailableShockers();
            if (!refreshResult) return new PiShockResult(false, $"{nameof(PiShockProvider)} cannot execute due to an error when refreshing shockers");
        }

        foreach (var shareCode in shareCodeArray)
        {
            if (availableShockers.All(shocker => shocker.ShareCode != shareCode)) return new PiShockResult(false, $"Shocker for sharecode '{shareCode}' does not exist");
        }

        var tasks = shareCodeArray.Select(shareCode => availableShockers.Single(shocker => shocker.ShareCode == shareCode))
                                  .GroupBy(shocker => shocker.ClientId)
                                  .Select(group => executeAsync($"c{group.Key}-ops", group.Select(shocker => shocker.ShockerId), mode, intensity, duration));

        await Task.WhenAll(tasks);
        return new PiShockResult(true, "Success");
    }

    private async Task executeAsync(string channel, IEnumerable<int> shockerIds, PiShockMode mode, int intensity, int duration)
    {
        if (mode == PiShockMode.End)
        {
            intensity = 0;
            duration = 0;
        }

        if (mode == PiShockMode.Beep)
        {
            intensity = 0;
        }

        var content = JsonSerializer.Serialize(new PiShockPublishOperation
        {
            Commands = shockerIds.Select(shockerId => new PiShockPublishCommand
            {
                Target = channel,
                Body = new PiShockPublishCommandBody
                {
                    ShockerId = shockerId,
                    Mode = mode.ToCode(),
                    Intensity = intensity,
                    Duration = duration,
                    Repeating = false,
                    LogData = new PiShockPublishCommandLogData
                    {
                        User = userId,
                        Type = "sc",
                        Warning = false,
                        Hold = false,
                        Origin = $"{AppManager.APP_NAME}-{username}"
                    }
                }
            }).ToArray()
        }, serialiserOptions);

        if (!webSocket!.IsConnected)
        {
            ExceptionHandler.Handle("User authenticated but websocket disconnected!\nLog out and log back in on PiShock's website");
            return;
        }

        await webSocket!.SendAsync(content);
    }

    private async Task<PiShockResult> claimShareCode(string shareCode)
    {
        try
        {
            var shocker = await legacyRetrieveShockerInfo(shareCode);
            if (shocker is null) return new PiShockResult(false, $"Shocker for sharecode '{shareCode}' does not exist");

            var request = new LegacyPiShockVibrateActionRequest
            {
                Duration = "1",
                Intensity = "1",
                AppName = $"{AppManager.APP_NAME}-{username}",
                Username = username,
                ApiKey = apiKey,
                ShareCode = shareCode
            };

            var response = await httpClient.PostAsync("https://ps.pishock.com/pishock/operate", new StringContent(JsonSerializer.Serialize(request, serialiserOptions), Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            return new PiShockResult(responseString.Contains("Succeeded") || responseString.Contains("Attempted"), responseString);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"{nameof(PiShockProvider)} has experienced an exception when claiming sharecode '{shareCode}'");
            return new PiShockResult(false, $"{nameof(PiShockProvider)} has experienced an exception when claiming sharecode '{shareCode}'");
        }
    }

    private async Task<LegacyPiShockShocker?> legacyRetrieveShockerInfo(string shareCode)
    {
        try
        {
            var request = new LegacyPiShockShockerInfoRequest
            {
                Username = username,
                ApiKey = apiKey,
                ShareCode = shareCode
            };

            var response = await httpClient.PostAsync("https://do.pishock.com/api/GetShockerInfo", new StringContent(JsonSerializer.Serialize(request, serialiserOptions), Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<LegacyPiShockShocker>(responseString);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"{nameof(PiShockProvider)} has experienced an exception when retrieving legacy shocker info for sharecode '{shareCode}'");
            return null;
        }
    }
}