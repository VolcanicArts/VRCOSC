// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Audio.Whisper;

public class WhisperSpeechEngine : SpeechEngine
{
    private AudioProcessor? audioProcessor;
    private Repeater? repeater;

    public override void Initialise()
    {
        var captureDeviceId = SettingsManager.GetInstance().GetObservable<string>(VRCOSCSetting.SelectedInputDeviceID);
        captureDeviceId.Subscribe(onCaptureDeviceIdChanged, true);
    }

    private async void onCaptureDeviceIdChanged(string newDeviceId)
    {
        audioProcessor?.Stop();

        if (repeater is not null)
            await repeater.StopAsync();

        var captureDevice = AudioDeviceHelper.GetDeviceByID(newDeviceId);

        if (captureDevice is null)
        {
            ExceptionHandler.Handle("Chosen microphone isn't available");
            return;
        }

        Logger.Log($"Switching microphone to {captureDevice.FriendlyName}");

        audioProcessor = new AudioProcessor(captureDevice);
        audioProcessor.Start();

        repeater = new Repeater(processResult);
        // Do not change this from 1.5
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

    public override async Task Teardown()
    {
        var captureDeviceId = SettingsManager.GetInstance().GetObservable<string>(VRCOSCSetting.SelectedInputDeviceID);
        captureDeviceId.Unsubscribe(onCaptureDeviceIdChanged);

        audioProcessor?.Stop();

        if (repeater is not null)
            await repeater.StopAsync();

        audioProcessor = null;
        repeater = null;
    }
}
