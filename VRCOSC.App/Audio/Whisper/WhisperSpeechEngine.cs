// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Audio.Whisper;

public partial class WhisperSpeechEngine : SpeechEngine
{
    private AudioProcessor? audioProcessor;
    private Repeater? repeater;

    public override void Initialise()
    {
        var captureDeviceId = SettingsManager.GetInstance().GetValue<string>(VRCOSCSetting.SelectedInputDeviceID);
        var captureDevice = AudioHelper.GetDeviceByID(captureDeviceId);

        if (captureDevice is null) throw new InvalidOperationException();

        audioProcessor = new AudioProcessor(captureDevice);
        audioProcessor.Start();

        repeater = new Repeater(processResult);
        repeater.Start(TimeSpan.FromSeconds(1));
    }

    private async void processResult()
    {
        var result = await GetResult();
        if (result is null) return;

        var text = removeBracketsAndExtraSpaces(result.Text);

        // TODO: Find a better way to detect when the user has stopped talking

        if (result.Text.EndsWith("[BLANK_AUDIO]"))
        {
            audioProcessor?.ClearBuffer();
            OnFinalResult?.Invoke(new SpeechResult(text, result.Confidence));
        }
        else
        {
            OnPartialResult?.Invoke(new SpeechResult(text, result.Confidence));
        }
    }

    private string removeBracketsAndExtraSpaces(string input)
    {
        var output = Regex.Replace(input, @"[\[\{\(][^\[\]\{\}\(\)]*[\]\}\)]", "");
        return Regex.Replace(output, @"\s{2,}", " ").Trim();
    }

    public async Task<SpeechResult?> GetResult()
    {
        if (audioProcessor is null || !audioProcessor.IsProcessing) return null;

        var result = await audioProcessor.GetResult();
        return result;
    }

    public override void Teardown()
    {
        audioProcessor?.Stop();
    }
}
