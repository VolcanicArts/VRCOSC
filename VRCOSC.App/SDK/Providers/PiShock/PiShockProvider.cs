// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
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

    private readonly Lock sharedShockersLock = new();
    private readonly PiShockCredentials credentials;

    private WebSocketClient? webSocket;
    private TokenSourceTask? serialTask;
    private PiShockSerialInstance? serialInstance;
    private List<PiShockShocker> availableShockers { get; } = [];
    private bool initialised;
    private int userId = -1;
    private int hubId = -1;

    public PiShockProvider(string username, string apiKey)
    {
        credentials = new PiShockCredentials(username, apiKey);
    }

    public async Task<bool> Initialise()
    {
        if (initialised) throw new InvalidOperationException("Cannot initialise whilst already initialised");

        try
        {
            var userPopulated = await populateUser();
            if (!userPopulated) return false;

            // TODO: Waiting for GET /Hub to go live
            //var hubPopulated = await populateHub();
            //if (!hubPopulated) return false;

            var shockersPopulated = await populateShockers();
            if (!shockersPopulated.IsSuccess) return false;

            webSocket = new WebSocketClient($"{broker_endpoint}?Username={credentials.Username}&ApiKey={credentials.ApiKey}", 2000, 3);
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
            hubId = -1;
            initialised = false;
        }
    }

    private async Task<bool> populateUser()
    {
        var userResult = await PiShockRequestFactory.GetUser(credentials);

        if (!userResult.IsSuccess)
        {
            Logger.Error(userResult.Exception, $"Error in {nameof(PiShockProvider)}");
            userId = -1;
            return false;
        }

        var user = userResult.Value;
        userId = user.Id;
        return true;
    }

    private async Task<bool> populateHub()
    {
        var hubsResult = await PiShockRequestFactory.GetHubs(credentials);

        if (!hubsResult.IsSuccess)
        {
            Logger.Error(hubsResult.Exception, $"Error in {nameof(PiShockProvider)}");
            hubId = -1;
            return false;
        }

        var hubs = hubsResult.Value;
        hubId = hubs[0].Id;
        return true;
    }

    private async Task<Result> populateShockers()
    {
        var shockersResult = await PiShockRequestFactory.GetSharedShockers(credentials);

        if (!shockersResult.IsSuccess)
        {
            Logger.Error(shockersResult.Exception, $"Error in {nameof(PiShockProvider)}");
            return shockersResult.Exception;
        }

        var shockers = shockersResult.Value;

        // if we cannot get any shockers someone might be generating their first sharecode so return true
        if (shockers.Length == 0) return true;

        // TODO: Remove when populateHubId is migrated
        hubId = shockers.FirstOrDefault(s => s.OwnerId == userId)?.HubId ?? -1;

        lock (sharedShockersLock)
        {
            availableShockers.Clear();
            availableShockers.AddRange(shockers);
        }

        return true;
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

        await executeAsync($"c{hubId}-ops", [shockerId], mode, intensity, duration);
        return new PiShockResult(true, "Success");
    }

    public async Task<PiShockResult> ExecuteAsync(IEnumerable<string> shareCodes, PiShockMode mode, int intensity, int duration)
    {
        if (!initialised) return new PiShockResult(false, "Provider not initialised");

        var shareCodeArray = shareCodes.ToArray();
        var missingShareCodes = shareCodeArray.Where(code => availableShockers.All(shocker => shocker.ShareCode != code)).ToArray();

        if (missingShareCodes.Length != 0)
        {
            var claimed = await PiShockRequestFactory.ClaimSharecodes(credentials, missingShareCodes);
            if (!claimed.IsSuccess) return new PiShockResult(false, claimed.Exception.ToString());

            var refreshResult = await populateShockers();
            if (!refreshResult.IsSuccess) return new PiShockResult(false, $"{nameof(PiShockProvider)} cannot execute due to an error when refreshing shockers\n{refreshResult.Exception}");
        }

        foreach (var shareCode in shareCodeArray)
        {
            if (availableShockers.All(shocker => shocker.ShareCode != shareCode)) return new PiShockResult(false, $"Shocker for sharecode '{shareCode}' does not exist");
        }

        var tasks = shareCodeArray.Select(shareCode => availableShockers.Single(shocker => shocker.ShareCode == shareCode))
                                  .GroupBy(shocker => shocker.HubId)
                                  .Select(group => executeAsync($"c{group.Key}-ops", group.Select(shocker => shocker.Id), mode, intensity, duration));

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
                        Origin = $"{AppManager.APP_NAME}-{credentials.Username}"
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
}