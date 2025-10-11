// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.IO;
using System.Threading.Tasks;
using SoundFlow.Components;
using SoundFlow.Providers;

namespace VRCOSC.App.Nodes.Types.Audio;

[Node("Audio Play Once", "Audio")]
public sealed class AudioPlayOnceNode : Node, IFlowInput
{
    public FlowContinuation OnFinished = new("On Finished");
    public FlowContinuation OnFailed = new("On Failed");

    public ValueInput<string> FilePath = new("File Path");
    public ValueInput<float> Volume = new("Volume", 1f);

    protected override async Task Process(PulseContext c)
    {
        var filePath = FilePath.Read(c);

        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            await OnFailed.Execute(c);
            return;
        }

        var player = new SoundPlayer(new StreamDataProvider(File.OpenRead(filePath)))
        {
            Volume = Volume.Read(c),
        };

        Mixer.Master.AddComponent(player);
        player.Play();

        while (player.Time < player.Duration)
        {
            if (c.IsCancelled) break;
        }

        player.Stop();
        Mixer.Master.RemoveComponent(player);

        await OnFinished.Execute(c);
    }
}