// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Audio.Whisper;

public class WhisperSpeechEngine : SpeechEngine
{
    private AudioProcessor? audioProcessor;
    private Repeater? repeater;

    public override void Initialise()
    {
        var captureDeviceId = SettingsManager.GetInstance().GetValue<string>(VRCOSCSetting.SelectedInputDeviceID);
        var captureDevice = AudioDeviceHelper.GetDeviceByID(captureDeviceId);

        if (captureDevice is null)
        {
            ExceptionHandler.Handle("Chosen microphone isn't available");
            return;
        }

        audioProcessor = new AudioProcessor(captureDevice);
        audioProcessor.Start();

        repeater = new Repeater(processResult);
        // Do not change this from 1.5. It's tuned
        repeater.Start(TimeSpan.FromSeconds(1.5f));
    }

    private async void processResult()
    {
        Debug.Assert(audioProcessor is not null);

        var result = await audioProcessor.GetResultAsync();
        if (result is null || result.Text.Contains('*')) return;

        // filter out things like [BLANK AUDIO]
        if (result.Text.StartsWith('[')) return;

        var requiredConfidence = SettingsManager.GetInstance().GetValue<float>(VRCOSCSetting.SpeechConfidence);
        if (result.Confidence < requiredConfidence) return;

        var text = result.Text;

        if (result.IsFinal)
            OnFinalResult?.Invoke(text);
        else
            OnPartialResult?.Invoke(text);
    }

    public override void Teardown()
    {
        audioProcessor?.Stop();
        _ = repeater?.StopAsync();
    }
}
