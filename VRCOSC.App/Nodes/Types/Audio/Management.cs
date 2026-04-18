// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Interfaces;
using VRCOSC.App.Audio;

namespace VRCOSC.App.Nodes.Types.Audio;

[Node("Create Player", "Audio")]
public sealed class AudioCreatePlayerNode : Node, IFlowInput
{
    public FlowContinuation OnSuccess = new();
    public FlowContinuation OnFail = new();

    public ValueInput<AudioPlaybackDevice> Device = new();
    public ValueInput<string?> FilePath = new();

    public ValueOutput<ISoundPlayer?> Player = new();

    protected override Task Process(PulseContext c)
    {
        var device = Device.Read(c);
        var filePath = FilePath.Read(c);

        if (device is null || string.IsNullOrWhiteSpace(filePath)) return OnFail.Execute(c);

        var result = AudioManager.GetInstance().CreatePlayer(device, filePath);

        if (result.IsSuccess)
        {
            Player.Write(result.Value, c);
            return OnSuccess.Execute(c);
        }

        Player.Write(null, c);
        return OnFail.Execute(c);
    }
}