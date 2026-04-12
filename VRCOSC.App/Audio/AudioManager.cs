// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.IO;
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

    private MiniAudioEngine _audioEngine = null!;
    public readonly List<AudioPlaybackDevice> PlaybackDevices = [];

    public Task Init()
    {
        try
        {
            _audioEngine = new();
            _audioEngine.UpdateAudioDevicesInfo();

            foreach (var deviceInfo in _audioEngine.PlaybackDevices)
            {
                var device = _audioEngine.InitializePlaybackDevice(deviceInfo, audio_format);
                device.Start();
                PlaybackDevices.Add(device);
            }

            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Unable to initialise {nameof(AudioManager)}");
            return Task.CompletedTask;
        }
    }

    public Task Stop()
    {
        _audioEngine.Dispose();
        PlaybackDevices.Clear();
        return Task.CompletedTask;
    }

    public Utils.Result<ISoundPlayer> CreatePlayer(AudioPlaybackDevice playbackDevice, string filePath)
    {
        try
        {
            var dataProvider = new StreamDataProvider(_audioEngine, audio_format, File.OpenRead(filePath));
            var player = new SoundPlayer(_audioEngine, audio_format, dataProvider);
            playbackDevice.MasterMixer.AddComponent(player);
            return player;
        }
        catch (Exception e)
        {
            return e;
        }
    }
}