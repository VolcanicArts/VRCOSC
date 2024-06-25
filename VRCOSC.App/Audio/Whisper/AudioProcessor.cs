// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using Whisper.net;

namespace VRCOSC.App.Audio.Whisper;

internal class AudioProcessor
{
    private readonly AudioCapture audioCapture;
    private readonly WhisperProcessor whisper;

    public bool IsProcessing => audioCapture.IsCapturing;

    public AudioProcessor(MMDevice device)
    {
        // TODO: Replace with runtime/whisper/model.bin file and give the user the option to select a model to download
        var modelFilePath = @"";

        var builder = WhisperFactory.FromPath(modelFilePath);

        whisper = builder.CreateBuilder()
                         .WithProbabilities()
                         .WithNoContext()
                         .Build();

        audioCapture = new AudioCapture(device);
    }

    public void Start()
    {
        audioCapture.ClearBuffer();
        audioCapture.StartCapture();
    }

    public void Stop()
    {
        audioCapture.StopCapture();
    }

    public void SaveAudioBufferToFile(string filePath)
    {
        audioCapture.SaveBufferToFile(filePath);
    }

    public async Task<SpeechResult> GetResult()
    {
        var data = audioCapture.GetBufferedData();
        return data.Length > 0 ? await processWithWhisper(data) : new SpeechResult(string.Empty, 0f);
    }

    private async Task<SpeechResult> processWithWhisper(float[] data)
    {
        var stringBuilder = new StringBuilder();

        var index = 0;
        var confidence = 0f;

        await foreach (var result in whisper.ProcessAsync(data))
        {
            index++;
            stringBuilder.Append(result.Text.Trim());
            confidence += result.Probability;
        }

        confidence = index == 0 ? 0f : confidence / index;

        return new SpeechResult(stringBuilder.ToString(), confidence);
    }

    public void ClearBuffer()
    {
        audioCapture.ClearBuffer();
    }
}
