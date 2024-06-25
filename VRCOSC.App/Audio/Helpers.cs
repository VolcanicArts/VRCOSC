// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Audio;

public static class AudioDeviceHelper
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

public static class WaveHelper
{
    public static void ApplyBandPassFilter(float[] waveStream, float sampleRate, float lowCutoffFrequency = 1000f, float highCutoffFrequency = 18000f)
    {
        if (waveStream.Length == 0)
        {
            throw new ArgumentException("The waveStream array must not be empty");
        }

        ApplyHighPassFilter(waveStream, lowCutoffFrequency, sampleRate);
        ApplyLowPassFilter(waveStream, highCutoffFrequency, sampleRate);
    }

    public static void ApplyHighPassFilter(float[] waveStream, float sampleRate, float cutoffFrequency = 1000f)
    {
        float rc = 1.0f / (cutoffFrequency * 2 * MathF.PI);
        float dt = 1.0f / sampleRate;
        float alpha = rc / (rc + dt);

        float previousInput = waveStream[0];
        float previousOutput = waveStream[0];

        for (int i = 1; i < waveStream.Length; i++)
        {
            float currentInput = waveStream[i];
            waveStream[i] = alpha * (previousOutput + currentInput - previousInput);
            previousInput = currentInput;
            previousOutput = waveStream[i];
        }
    }

    public static void ApplyLowPassFilter(float[] waveStream, float sampleRate, float cutoffFrequency = 18000f)
    {
        float rc = 1.0f / (cutoffFrequency * 2 * MathF.PI);
        float dt = 1.0f / sampleRate;
        float alpha = dt / (rc + dt);

        float previous = waveStream[0];

        for (int i = 1; i < waveStream.Length; i++)
        {
            waveStream[i] = previous + (alpha * (waveStream[i] - previous));
            previous = waveStream[i];
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
