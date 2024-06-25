// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Audio.Whisper;

public partial class WhisperSpeechEngine : SpeechEngine
{
    private AudioProcessor? audioProcessor;
    private Repeater? repeater;
    private string? previousText;

    public override void Initialise()
    {
        var captureDeviceId = SettingsManager.GetInstance().GetValue<string>(VRCOSCSetting.SelectedInputDeviceID);
        var captureDevice = AudioDeviceHelper.GetDeviceByID(captureDeviceId);

        if (captureDevice is null) throw new InvalidOperationException();

        audioProcessor = new AudioProcessor(captureDevice);
        audioProcessor.Start();

        repeater = new Repeater(processResult);
        repeater.Start(TimeSpan.FromSeconds(1.5f));

        previousText = null;
    }

    private async void processResult()
    {
        Debug.Assert(audioProcessor is not null);

        var result = await audioProcessor.GetResult();
        if (result is null) return;

        var isTextBlank = ((result.Text.StartsWith('[') || result.Text.StartsWith('{') || result.Text.StartsWith('(')) && (result.Text.EndsWith(']') || result.Text.EndsWith('}') || result.Text.EndsWith(')')))
                          || result.Text == "*"
                          || result.Text == "("
                          || result.Text.Contains(">>");

        if (previousText is null && isTextBlank)
        {
            audioProcessor.ClearBuffer();
        }

        if (!isTextBlank && result.Confidence >= 0.4f)
        {
            var text = result.Text;
            OnResult?.Invoke(text);
            previousText = text;
        }
        else
        {
            previousText = null;
        }
    }

    public override void Teardown()
    {
        audioProcessor?.Stop();
    }
}
