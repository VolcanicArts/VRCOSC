// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Audio;

public static class AudioHelper
{
    private static readonly MMDeviceEnumerator device_enumerator = new();

    public static void RegisterCallbackClient(IMMNotificationClient client)
    {
        device_enumerator.RegisterEndpointNotificationCallback(client);
    }

    public static List<MMDevice> GetAllInputDevices()
    {
        var inputDevices = device_enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
        return inputDevices.ToList();
    }

    public static MMDevice GetDeviceByID(string id)
    {
        try
        {
            return device_enumerator.GetDevice(id);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"{nameof(AudioHelper)} experienced an exception");
            return null;
        }
    }
}

public class AudioEndpointNotificationClient : IMMNotificationClient
{
    public Action? DeviceListChanged;
    public Action<DataFlow, Role, string>? DeviceChanged;

    public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
    {
        DeviceChanged?.Invoke(flow, role, defaultDeviceId);
    }

    public void OnDeviceAdded(string pwstrDeviceId)
    {
        DeviceListChanged?.Invoke();
    }

    public void OnDeviceRemoved(string deviceId)
    {
        DeviceListChanged?.Invoke();
    }

    public void OnDeviceStateChanged(string deviceId, DeviceState newState)
    {
        DeviceListChanged?.Invoke();
    }

    public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
    {
    }
}
