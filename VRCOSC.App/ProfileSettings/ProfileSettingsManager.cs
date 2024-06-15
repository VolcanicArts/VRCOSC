// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using NAudio.CoreAudioApi;
using VRCOSC.App.Audio;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ProfileSettings;

internal class ProfileSettingsManager
{
    private static ProfileSettingsManager? instance;
    public static ProfileSettingsManager GetInstance() => instance ??= new ProfileSettingsManager();

    public Observable<string> ModelDirectory = new();
    public Observable<string> ChosenInputDeviceID = new();
    public Observable<bool> AutoSwitchWithWindows = new(true);

    private readonly AudioEndpointNotificationClient notificationClient = new();

    private ProfileSettingsManager()
    {
        ChosenInputDeviceID.Value = WasapiCapture.GetDefaultCaptureDevice().ID;

        notificationClient.DeviceChanged += (flow, role, deviceId) =>
        {
            try
            {
                if (!AutoSwitchWithWindows.Value) return;
                if (flow != DataFlow.Capture) return;
                if (role != Role.Multimedia) return;

                Logger.Log("Automatic microphone change detected. Switching to new microphone");
                ChosenInputDeviceID.Value = deviceId;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
            }
        };

        AudioHelper.RegisterCallbackClient(notificationClient);
    }
}
