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
    #region Band Pass Filter

    public static void ApplyBandPassFilter(float[] waveStream, float sampleRate, float lowCutoffFrequency, float highCutoffFrequency)
    {
        if (waveStream.Length == 0)
        {
            throw new ArgumentException("The waveStream array must not be empty");
        }

        ApplyHighPassFilter(waveStream, lowCutoffFrequency, sampleRate);
        ApplyLowPassFilter(waveStream, highCutoffFrequency, sampleRate);
    }

    public static void ApplyHighPassFilter(float[] waveStream, float sampleRate, float cutoffFrequency)
    {
        var rc = 1.0f / (cutoffFrequency * 2 * MathF.PI);
        var dt = 1.0f / sampleRate;
        var alpha = rc / (rc + dt);

        var previousInput = waveStream[0];
        var previousOutput = waveStream[0];

        for (var i = 1; i < waveStream.Length; i++)
        {
            var currentInput = waveStream[i];
            waveStream[i] = alpha * (previousOutput + currentInput - previousInput);
            previousInput = currentInput;
            previousOutput = waveStream[i];
        }
    }

    public static void ApplyLowPassFilter(float[] waveStream, float sampleRate, float cutoffFrequency)
    {
        var rc = 1.0f / (cutoffFrequency * 2 * MathF.PI);
        var dt = 1.0f / sampleRate;
        var alpha = dt / (rc + dt);

        var previous = waveStream[0];

        for (var i = 1; i < waveStream.Length; i++)
        {
            waveStream[i] = previous + (alpha * (waveStream[i] - previous));
            previous = waveStream[i];
        }
    }

    #endregion

    public static void ApplyCompression(float[] buffer, float threshold, float ratio)
    {
        var linearThreshold = (float)Math.Pow(10, threshold / 20.0f); // Convert dB to linear scale

        for (var i = 0; i < buffer.Length; i++)
        {
            if (buffer[i] > linearThreshold)
            {
                buffer[i] = linearThreshold + (buffer[i] - linearThreshold) / ratio;
            }
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
