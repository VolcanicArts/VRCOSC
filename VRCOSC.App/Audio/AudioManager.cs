// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Interfaces;
using SoundFlow.Providers;
using SoundFlow.Structs;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Audio;

public class AudioManager
{
    private static AudioManager? instance;
    internal static AudioManager GetInstance() => instance ??= new AudioManager();

    private static readonly AudioFormat audio_format = AudioFormat.DvdHq;

    private MiniAudioEngine? _audioEngine;
    private readonly ConcurrentDictionary<string, AudioPlaybackDevice> _playbackDeviceCache = [];

    public Task Init()
    {
        try
        {
            _audioEngine = new();
            InvalidateCache();
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Unable to initialise {nameof(AudioManager)}");
        }

        return Task.CompletedTask;
    }

    public Task Stop()
    {
        if (_audioEngine is not null)
        {
            InvalidateCache(false);
            _audioEngine.Dispose();
            _audioEngine = null;
        }

        return Task.CompletedTask;
    }

    public void InvalidateCache(bool refreshDevices = true)
    {
        Logger.Log($"Invalidating {nameof(AudioManager)} cache");

        if (_audioEngine is null)
            throw new InvalidOperationException($"Please call {nameof(Init)} before attempting to invalidate the cache");

        foreach (var (_, device) in _playbackDeviceCache)
        {
            device.Stop();
            device.Dispose();
        }

        _playbackDeviceCache.Clear();

        if (refreshDevices)
        {
            Logger.Log($"Refreshing {nameof(AudioManager)} devices");
            _audioEngine.UpdateAudioDevicesInfo();
        }
    }

    public AudioPlaybackDevice? GetDefaultPlaybackDevice()
        => _playbackDeviceCache.Values.FirstOrDefault(d => d.Info!.Value.IsDefault) ?? GetPlaybackDevice(i => i.IsDefault);

    public AudioPlaybackDevice? GetPlaybackDeviceByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;

        return _playbackDeviceCache.TryGetValue(name, out var device) ? device : GetPlaybackDevice(i => i.Name == name);
    }

    public AudioPlaybackDevice? GetPlaybackDevice(Func<DeviceInfo, bool> predicate)
    {
        if (_audioEngine is null)
            throw new InvalidOperationException($"Please call {nameof(Init)} before attempting to get an audio playback device");

        try
        {
            foreach (var info in _audioEngine.PlaybackDevices)
            {
                if (!predicate(info)) continue;

                var device = _audioEngine.InitializePlaybackDevice(info, audio_format);
                device.Start();
                _playbackDeviceCache.TryAdd(device.Info!.Value.Name, device);
                Logger.Log($"Added device {info.Name} to cache");
                return device;
            }

            return null;
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error in {nameof(AudioManager)}");
            return null;
        }
    }

    public Utils.Result<ISoundPlayer?> CreatePlayer(AudioPlaybackDevice playbackDevice, string filePath)
    {
        Debug.Assert(_audioEngine is not null);

        try
        {
            var dataProvider = new StreamDataProvider(_audioEngine, audio_format, File.OpenRead(filePath));
            var player = new SoundPlayer(_audioEngine, audio_format, dataProvider);
            playbackDevice.MasterMixer.AddComponent(player);
            return player;
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error in {nameof(AudioManager)}");
            return e;
        }
    }
}