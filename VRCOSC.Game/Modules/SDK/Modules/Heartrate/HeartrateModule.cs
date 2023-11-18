﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using osu.Framework.Utils;
using VRCOSC.Game.Modules.SDK.Parameters;

namespace VRCOSC.Game.Modules.SDK.Modules.Heartrate;

[ModuleType(ModuleType.Health)]
[ModulePrefab("VRCOSC-Heartrate", "https://github.com/VolcanicArts/VRCOSC/releases/download/latest/VRCOSC-Heartrate.unitypackage")]
public abstract class HeartrateModule<T> : Module where T : HeartrateProvider
{
    protected T? HeartrateProvider;

    protected virtual int ReconnectionDelay => 2000;
    protected virtual int ReconnectionLimit => 30;

    private float currentHeartrate;
    private int targetHeartrate;
    private int connectionCount;

    protected abstract T CreateProvider();

    protected override void OnLoad()
    {
        CreateToggle(HeartrateSetting.Smoothing, "Smoothing", "Whether the current heartrate should jump or smoothly converge to the received heartrate", true);
        CreateTextBox(HeartrateSetting.SmoothingLength, "Smoothing Length", "The length of time (in milliseconds) the current heartrate should take to converge to the received heartrate", 1000);
        CreateTextBox(HeartrateSetting.NormalisedLowerbound, "Normalised Lowerbound", "The lower bound BPM the normalised parameter should use", 0);
        CreateTextBox(HeartrateSetting.NormalisedUpperbound, "Normalised Upperbound", "The upper bound BPM the normalised parameter should use", 240);

        RegisterParameter<bool>(HeartrateParameter.Enabled, "VRCOSC/Heartrate/Enabled", ParameterMode.Write, "Enabled", "Whether this module is connected and receiving values");
        RegisterParameter<int>(HeartrateParameter.Value, "VRCOSC/Heartrate/Value", ParameterMode.Write, "Value", "The raw value of your heartrate");
        RegisterParameter<float>(HeartrateParameter.Normalised, "VRCOSC/Heartrate/Normalised", ParameterMode.Write, "Normalised", "The heartrate value normalised to the set bounds");
    }

    protected override void OnPostLoad()
    {
        GetSetting(HeartrateSetting.SmoothingLength)!.IsEnabled = () => GetSettingValue<bool>(HeartrateSetting.Smoothing);
    }

    protected override Task<bool> OnModuleStart()
    {
        currentHeartrate = 0;
        targetHeartrate = 0;
        connectionCount = 1;

        HeartrateProvider = CreateProvider();
        HeartrateProvider.OnHeartrateUpdate += newHeartrate => targetHeartrate = newHeartrate;
        HeartrateProvider.OnConnected += () => connectionCount = 0;
        HeartrateProvider.OnDisconnected += async () => await attemptReconnection();
        HeartrateProvider.OnLog += Log;
        HeartrateProvider.Initialise();

        return Task.FromResult(true);
    }

    private async Task attemptReconnection()
    {
        await Task.Delay(ReconnectionDelay);

        if (connectionCount >= ReconnectionLimit)
        {
            Log("Connection cannot be established");
            return;
        }

        Log("Attempting reconnection...");

        if (HeartrateProvider is null) return;

        await HeartrateProvider.Teardown();
        await Task.Delay(100);
        await HeartrateProvider.Initialise();

        connectionCount++;
    }

    protected override Task OnModuleStop()
    {
        if (HeartrateProvider is not null)
        {
            HeartrateProvider.Teardown();
            HeartrateProvider = null;
        }

        SendParameter(HeartrateParameter.Enabled, false);

        return Task.CompletedTask;
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
        var isReceiving = HeartrateProvider?.IsReceiving ?? false;

        SendParameter(HeartrateParameter.Enabled, isReceiving);

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

    private enum HeartrateSetting
    {
        Smoothing,
        SmoothingLength,
        NormalisedLowerbound,
        NormalisedUpperbound
    }

    private enum HeartrateParameter
    {
        Enabled,
        Normalised,
        Value
    }
}
