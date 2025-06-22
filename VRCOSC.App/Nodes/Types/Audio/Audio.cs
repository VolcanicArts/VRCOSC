// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Audio;

[Node("Audio Play Once", "Audio")]
public sealed class AudioPlayOnce : Node, IFlowInput
{
    public FlowContinuation OnFinished = new("On Finished");

    public ValueInput<string> FilePath = new("File Path");
    public ValueInput<float> Volume = new("Volume", 1f);

    protected override void Process(PulseContext c)
    {
        var filePath = FilePath.Read(c);
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return;

        using var audioFile = new AudioFileReader(filePath);
        audioFile.Volume = Volume.Read(c);

        using var outputDevice = new WaveOutEvent();

        outputDevice.Init(audioFile);
        outputDevice.Play();

        Task.Delay(audioFile.TotalTime, c.Token).Wait(c.Token);
        OnFinished.Execute(c);
    }
}