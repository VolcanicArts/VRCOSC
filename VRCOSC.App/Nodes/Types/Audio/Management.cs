// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using SoundFlow.Abstracts.Devices;
using SoundFlow.Interfaces;
using VRCOSC.App.Audio;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Audio;

[Node("Create Player", "Audio")]
public sealed class AudioCreatePlayerNode() : TryValueComputeNode<ISoundPlayer?>("Player")
{
    public ValueInput<AudioPlaybackDevice> Device = new();
    public ValueInput<string?> FilePath = new();

    protected override Result<ISoundPlayer?> TryComputeValue(PulseContext c)
    {
        var device = Device.Read(c);
        var filePath = FilePath.Read(c);

        if (device is null || string.IsNullOrWhiteSpace(filePath)) return Result<ISoundPlayer?>.Fail();

        return AudioManager.GetInstance().CreatePlayer(device, filePath);
    }
}