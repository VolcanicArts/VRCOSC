// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Audio.Whisper;

public class WhisperSpeechEngine : SpeechEngine
{
    private AudioProcessor? audioProcessor;
    private Repeater? repeater;
    private SpeechResult? result;
    private AudioEndpointNotificationClient? audioNotificationClient;
    private bool initialised;

    public override void Initialise()
    {
        var captureDeviceId = SettingsManager.GetInstance().GetObservable<string>(VRCOSCSetting.SelectedMicrophoneID);
        captureDeviceId.Subscribe(onCaptureDeviceIdChanged, true);

        audioNotificationClient = new AudioEndpointNotificationClient();

        audioNotificationClient.DeviceChanged += (flow, role, _) =>
        {
            try
            {
                if (flow != DataFlow.Capture) return;
                if (role != Role.Multimedia) return;

                if (string.IsNullOrEmpty(SettingsManager.GetInstance().GetValue<string>(VRCOSCSetting.SelectedMicrophoneID)))
                {
                    Logger.Log("Default microphone change detected. Switching to new microphone");
                    onCaptureDeviceIdChanged(string.Empty);
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e);
            }
        };

        AudioDeviceHelper.RegisterCallbackClient(audioNotificationClient);

        initialised = true;
    }

    private void onCaptureDeviceIdChanged(string newDeviceId)
    {
        run().Forget();
        return;

        async Task run()
        {
            if (audioProcessor is not null)
                await audioProcessor.Stop();

            if (repeater is not null)
                await repeater.StopAsync();

            var captureDevice = string.IsNullOrEmpty(newDeviceId) ? WasapiCapture.GetDefaultCaptureDevice() : AudioDeviceHelper.GetDeviceByID(newDeviceId);

            if (captureDevice is null)
            {
                ExceptionHandler.Handle("Chosen microphone isn't available");
                return;
            }

            Logger.Log($"Switching microphone to {captureDevice.FriendlyName}");

            audioProcessor = new AudioProcessor(captureDevice);
            audioProcessor.Start();

            repeater = new Repeater($"{nameof(WhisperSpeechEngine)}-{nameof(processResult)}", processResult);
            // Do not change this from 1.5
            repeater.Start(TimeSpan.FromSeconds(1.5f));
        }
    }

    private async Task processResult()
    {
        Debug.Assert(audioProcessor is not null);

        var newResult = await audioProcessor.GetResultAsync();
        if (newResult is not null) result = newResult;

        if (result is null) return;

        // filter out things like [BLANK AUDIO]
        if (result.Text.StartsWith('[')) return;

        var requiredConfidence = SettingsManager.GetInstance().GetValue<float>(VRCOSCSetting.SpeechConfidence);
        if (result.Confidence < requiredConfidence) return;

        var text = result.Text;

        if (text.Length >= 144)
        {
            audioProcessor.ClearBuffer();
            // trigger final result so anything waiting for text considers this block
            OnFinalResult?.Invoke(text);
            result = null;
            return;
        }

        if (result.IsFinal)
        {
            OnFinalResult?.Invoke(text);
            result = null;
        }
        else
        {
            OnPartialResult?.Invoke(text);
        }
    }

    public override async Task Teardown()
    {
        if (!initialised) return;

        var captureDeviceId = SettingsManager.GetInstance().GetObservable<string>(VRCOSCSetting.SelectedMicrophoneID);
        captureDeviceId.Unsubscribe(onCaptureDeviceIdChanged);

        if (audioNotificationClient is not null)
            AudioDeviceHelper.UnRegisterCallbackClient(audioNotificationClient);

        audioNotificationClient = null;

        if (audioProcessor is not null)
            await audioProcessor.Stop();

        if (repeater is not null)
            await repeater.StopAsync();

        audioProcessor = null;
        repeater = null;
        result = null;

        initialised = false;
    }
}