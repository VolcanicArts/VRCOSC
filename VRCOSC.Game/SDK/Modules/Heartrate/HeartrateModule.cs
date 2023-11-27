// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Utils;
using VRCOSC.Game.SDK.Parameters;

namespace VRCOSC.Game.SDK.Modules.Heartrate;

[ModuleType(ModuleType.Health)]
[ModulePrefab("VRCOSC-Heartrate", "https://github.com/VolcanicArts/VRCOSC/releases/download/latest/VRCOSC-Heartrate.unitypackage")]
public abstract class HeartrateModule<T> : Module where T : HeartrateProvider
{
    protected T? HeartrateProvider;

    protected virtual int ReconnectionDelay => 2000;
    protected virtual int ReconnectionLimit => 5;

    private float currentHeartrate;
    private int targetHeartrate;
    private int connectionCount;

    protected abstract T CreateProvider();

    private bool isReceiving => HeartrateProvider?.IsReceiving ?? false;

    protected override void OnLoad()
    {
        CreateToggle(HeartrateSetting.Smoothing, "Smoothing", "Whether the current heartrate should jump or smoothly converge to the received heartrate", true);
        CreateTextBox(HeartrateSetting.SmoothingLength, "Smoothing Length", "The length of time (in milliseconds) the current heartrate should take to converge to the received heartrate", 1000);
        CreateTextBox(HeartrateSetting.NormalisedLowerbound, "Normalised Lowerbound", "The lower bound BPM the normalised parameter should use", 0);
        CreateTextBox(HeartrateSetting.NormalisedUpperbound, "Normalised Upperbound", "The upper bound BPM the normalised parameter should use", 240);

        RegisterParameter<bool>(HeartrateParameter.Connected, "VRCOSC/Heartrate/Connected", ParameterMode.Write, "Connected", "Whether this module is connected and receiving values");
        RegisterParameter<int>(HeartrateParameter.Value, "VRCOSC/Heartrate/Value", ParameterMode.Write, "Value", "The value of your heartrate");
        RegisterParameter<float>(HeartrateParameter.Normalised, "VRCOSC/Heartrate/Normalised", ParameterMode.Write, "Normalised", "The heartrate value normalised to the set bounds");

        RegisterParameter<bool>(LegacyHeartrateParameter.Enabled, "VRCOSC/Heartrate/Enabled", ParameterMode.Write, "Legacy: Enabled", "Whether this module is connected and receiving values");
        RegisterParameter<float>(LegacyHeartrateParameter.Units, "VRCOSC/Heartrate/Units", ParameterMode.Write, "Legacy: Units", "The units digit 0-9 mapped to a float");
        RegisterParameter<float>(LegacyHeartrateParameter.Tens, "VRCOSC/Heartrate/Tens", ParameterMode.Write, "Legacy: Tens", "The tens digit 0-9 mapped to a float");
        RegisterParameter<float>(LegacyHeartrateParameter.Hundreds, "VRCOSC/Heartrate/Hundreds", ParameterMode.Write, "Legacy: Hundreds", "The hundreds digit 0-9 mapped to a float");

        CreateGroup("Smoothing", HeartrateSetting.Smoothing, HeartrateSetting.SmoothingLength);
        CreateGroup("Normalised Parameter", HeartrateSetting.NormalisedLowerbound, HeartrateSetting.NormalisedUpperbound);
    }

    protected override void OnPostLoad()
    {
        GetSetting(HeartrateSetting.SmoothingLength)!.IsEnabled = () => GetSettingValue<bool>(HeartrateSetting.Smoothing);
    }

    protected override async Task<bool> OnModuleStart()
    {
        currentHeartrate = 0;
        targetHeartrate = 0;
        connectionCount = 1;

        HeartrateProvider = CreateProvider();
        HeartrateProvider.OnHeartrateUpdate += newHeartrate =>
        {
            connectionCount = 0;
            targetHeartrate = newHeartrate;
        };
        HeartrateProvider.OnDisconnected += attemptReconnection;
        HeartrateProvider.OnLog += Log;
        await HeartrateProvider.Initialise();

        return true;
    }

    private async void attemptReconnection()
    {
        Debug.Assert(HeartrateProvider is not null);

        Log($"{typeof(T).Name} disconnected");
        Log("Attempting reconnection...");

        await Task.Delay(ReconnectionDelay);

        if (connectionCount >= ReconnectionLimit)
        {
            Log("Connection cannot be established");
            Log("Restart the module to attempt a full reconnection");
            await teardownProvider();
            return;
        }

        connectionCount++;

        await HeartrateProvider.Teardown();
        await Task.Delay(100);
        await HeartrateProvider.Initialise();
    }

    protected override async Task OnModuleStop()
    {
        await teardownProvider();
    }

    private async Task teardownProvider()
    {
        if (HeartrateProvider is not null)
        {
            HeartrateProvider.OnDisconnected = null;
            await HeartrateProvider.Teardown();
            HeartrateProvider = null;
        }

        SendParameter(HeartrateParameter.Connected, false);
        SendParameter(LegacyHeartrateParameter.Enabled, false);
    }

    [ModuleUpdate(ModuleUpdateMode.Custom)]
    private void updateCurrentHeartrate()
    {
        if (GetSettingValue<bool>(HeartrateSetting.Smoothing))
        {
            currentHeartrate = (float)Interpolation.DampContinuously(currentHeartrate, targetHeartrate, GetSettingValue<int>(HeartrateSetting.SmoothingLength) / 2d, 50d);
        }
        else
        {
            currentHeartrate = targetHeartrate;
        }
    }

    [ModuleUpdate(ModuleUpdateMode.Custom)]
    private void updateParameters()
    {
        SendParameter(HeartrateParameter.Connected, isReceiving);

        if (isReceiving)
        {
            var normalisedHeartRate = Map(currentHeartrate, GetSettingValue<int>(HeartrateSetting.NormalisedLowerbound), GetSettingValue<int>(HeartrateSetting.NormalisedUpperbound), 0, 1);

            SendParameter(HeartrateParameter.Normalised, normalisedHeartRate);
            SendParameter(HeartrateParameter.Value, (int)Math.Round(currentHeartrate));
        }
        else
        {
            SendParameter(HeartrateParameter.Normalised, 0f);
            SendParameter(HeartrateParameter.Value, 0);
        }
    }

    [ModuleUpdate(ModuleUpdateMode.Custom)]
    private void updateLegacyParameters()
    {
        SendParameter(LegacyHeartrateParameter.Enabled, isReceiving);

        if (isReceiving)
        {
            var individualValues = toDigitArray((int)Math.Round(currentHeartrate), 3);

            SendParameter(LegacyHeartrateParameter.Units, individualValues[2] / 10f);
            SendParameter(LegacyHeartrateParameter.Tens, individualValues[1] / 10f);
            SendParameter(LegacyHeartrateParameter.Hundreds, individualValues[0] / 10f);
        }
        else
        {
            SendParameter(LegacyHeartrateParameter.Units, 0f);
            SendParameter(LegacyHeartrateParameter.Tens, 0f);
            SendParameter(LegacyHeartrateParameter.Hundreds, 0f);
        }
    }

    private static int[] toDigitArray(int num, int totalWidth) => num.ToString().PadLeft(totalWidth, '0').Select(digit => int.Parse(digit.ToString())).ToArray();

    private enum HeartrateSetting
    {
        Smoothing,
        SmoothingLength,
        NormalisedLowerbound,
        NormalisedUpperbound
    }

    private enum HeartrateParameter
    {
        Connected,
        Normalised,
        Value
    }

    private enum LegacyHeartrateParameter
    {
        Enabled,
        Units,
        Tens,
        Hundreds
    }
}
