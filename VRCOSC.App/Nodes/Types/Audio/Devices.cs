// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using SoundFlow.Abstracts.Devices;
using VRCOSC.App.Audio;

namespace VRCOSC.App.Nodes.Types.Audio;

[Node("Default Playback Device", "Audio/Devices")]
[NodeCollapsed]
public sealed class AudioDefaultPlaybackDeviceNode() : SimpleValueSourceNode<AudioPlaybackDevice?>(() => AudioManager.GetInstance().PlaybackDevices.FirstOrDefault(d => d.Info!.Value.IsDefault));

[Node("Playback Device Source", "Audio/Devices")]
public sealed class AudioPlaybackDeviceSourceNode() : ValueSourceNode<AudioPlaybackDevice?>("Device"), IHasTextProperty
{
    [NodeProperty("device_name")]
    public string Text { get; set; } = string.Empty;

    protected override AudioPlaybackDevice? ComputeValue(PulseContext c) => AudioManager.GetInstance().PlaybackDevices.FirstOrDefault(d => d.Info!.Value.Name == Text);
}