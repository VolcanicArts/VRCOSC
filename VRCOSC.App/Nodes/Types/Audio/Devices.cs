// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using SoundFlow.Abstracts.Devices;
using VRCOSC.App.Audio;

namespace VRCOSC.App.Nodes.Types.Audio;

[Node("Default Playback Device", "Audio/Devices")]
public sealed class AudioDefaultPlaybackDeviceNode() : ValueSourceNode<AudioPlaybackDevice?>("Device")
{
    protected override AudioPlaybackDevice? ComputeValue(IPulseContext c)
    {
        var playbackDevices = AudioManager.GetInstance().PlaybackDevices;
        if (playbackDevices.Count == 0) return null;

        return playbackDevices.FirstOrDefault(d => d.Info!.Value.IsDefault);
    }
}

[Node("Playback Device Source", "Audio/Devices")]
public sealed class AudioPlaybackDeviceSourceNode() : ValueSourceNode<AudioPlaybackDevice?>("Device")
{
    public GlobalStore<string> PrevName = new();
    public GlobalStore<AudioPlaybackDevice?> CurrDevice = new();

    [InputMode(InputModes.Inline)]
    public ValueInput<string> Name = new();

    protected override AudioPlaybackDevice? ComputeValue(IPulseContext c)
    {
        var name = Name.Read(c);
        if (string.IsNullOrWhiteSpace(name)) return null;

        var device = CurrDevice.Read(c);
        if (name == PrevName.Read(c)) return device;

        device = AudioManager.GetInstance().PlaybackDevices.FirstOrDefault(d => d.Info!.Value.Name == name);
        CurrDevice.Write(device, c);
        PrevName.Write(name, c);

        return device;
    }
}