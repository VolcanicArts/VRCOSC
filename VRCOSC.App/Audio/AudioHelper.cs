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
    public static void RegisterCallbackClient(IMMNotificationClient client)
    {
        using var deviceEnumerator = new MMDeviceEnumerator();
        deviceEnumerator.RegisterEndpointNotificationCallback(client);
    }

    public static List<MMDevice> GetAllInputDevices()
    {
        using var deviceEnumerator = new MMDeviceEnumerator();
        var inputDevices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
        return inputDevices.ToList();
    }

    public static MMDevice? GetDeviceByID(string id)
    {
        try
        {
            return GetAllInputDevices().SingleOrDefault(device => device.ID == id);
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e);
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
