// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Utils;
using VRCOSC.SDK.Avatars;
using VRCOSC.SDK.Parameters;

namespace VRCOSC.SDK.Modules.Heartrate;

[ModuleType(ModuleType.Health)]
[ModulePrefab("VRCOSC-Heartrate", "https://github.com/VolcanicArts/VRCOSC/releases/download/latest/VRCOSC-Heartrate.unitypackage")]
public abstract class HeartrateModule<T> : AvatarModule where T : HeartrateProvider
{
    protected T? HeartrateProvider;

    protected virtual int ReconnectionDelay => 2000;
    protected virtual int ReconnectionLimit => 5;
    private int connectionCount;

    private float currentValue;
    private int targetValue;

    private float currentAverage;
    private int targetAverage;

    private bool beatParameterValue;
    private CancellationTokenSource? beatParameterSource;
    private Task? beatParameterTask;

    private readonly object valuesLock = new();
    private readonly Dictionary<DateTimeOffset, int> values = new();

    protected abstract T CreateProvider();

    private bool isReceiving => HeartrateProvider?.IsReceiving ?? false;

    protected override void OnLoad()
    {
        CreateToggle(HeartrateSetting.SmoothValue, "Smoothing", "Whether the Value parameter should jump or smoothly converge to the received heartrate", true);
        CreateTextBox(HeartrateSetting.SmoothValueLength, "Smoothing Length", "The length of time (in milliseconds) the Value parameter should take to converge to the received heartrate", 1000);

        CreateTextBox(HeartrateSetting.AveragePeriod, "Average Period", "The period of time (in milliseconds) that should be used to calculate your average heartrate", 10000);
        CreateToggle(HeartrateSetting.SmoothAverage, "Smoothing", "Whether the Average parameter should jump or smoothly converge to the average heartrate", true);
        CreateTextBox(HeartrateSetting.SmoothAverageLength, "Smoothing Length", "The length of time (in milliseconds) the Average parameter should take to converge to the average heartrate", 1000);

        CreateTextBox(HeartrateSetting.NormalisedLowerbound, "Normalised Lowerbound", "The lower bound BPM the normalised parameter should use", 0);
        CreateTextBox(HeartrateSetting.NormalisedUpperbound, "Normalised Upperbound", "The upper bound BPM the normalised parameter should use", 240);

        CreateToggle(HeartrateSetting.BeatMode, "Beat Mode", "Whether the Beat parameter should toggle its value or become true for 1 update\nFalse for toggle. True for become true", false);

        RegisterParameter<bool>(HeartrateParameter.Connected, "VRCOSC/Heartrate/Connected", ParameterMode.Write, "Connected", "Whether this module is connected and receiving values");
        RegisterParameter<int>(HeartrateParameter.Value, "VRCOSC/Heartrate/Value", ParameterMode.Write, "Value", "The value of your heartrate");
        RegisterParameter<float>(HeartrateParameter.Normalised, "VRCOSC/Heartrate/Normalised", ParameterMode.Write, "Normalised", "The heartrate value normalised from the set bounds to 0-1");
        RegisterParameter<int>(HeartrateParameter.Average, "VRCOSC/Heartrate/Average", ParameterMode.Write, "Average", "The average of your heartrate");
        RegisterParameter<bool>(HeartrateParameter.Beat, "VRCOSC/Heartrate/Beat", ParameterMode.ReadWrite, "Beat", "Toggles value OR becomes true for 1 update (depending on the setting) when your heart beats");

        RegisterParameter<bool>(HeartrateParameter.LegacyEnabled, "VRCOSC/Heartrate/Enabled", ParameterMode.Write, "Enabled", "Whether this module is connected and receiving values", true);
        RegisterParameter<float>(HeartrateParameter.LegacyUnits, "VRCOSC/Heartrate/Units", ParameterMode.Write, "Units", "The units digit 0-9 mapped to a float", true);
        RegisterParameter<float>(HeartrateParameter.LegacyTens, "VRCOSC/Heartrate/Tens", ParameterMode.Write, "Tens", "The tens digit 0-9 mapped to a float", true);
        RegisterParameter<float>(HeartrateParameter.LegacyHundreds, "VRCOSC/Heartrate/Hundreds", ParameterMode.Write, "Hundreds", "The hundreds digit 0-9 mapped to a float", true);

        CreateGroup("Value", HeartrateSetting.SmoothValue, HeartrateSetting.SmoothValueLength);
        CreateGroup("Average", HeartrateSetting.AveragePeriod, HeartrateSetting.SmoothAverage, HeartrateSetting.SmoothAverageLength);
        CreateGroup("Beat", HeartrateSetting.BeatMode);
        CreateGroup("Normalised Parameter", HeartrateSetting.NormalisedLowerbound, HeartrateSetting.NormalisedUpperbound);
    }

    protected override void OnPostLoad()
    {
        GetSetting(HeartrateSetting.SmoothValueLength)!.IsEnabled = () => GetSettingValue<bool>(HeartrateSetting.SmoothValue);
        GetSetting(HeartrateSetting.SmoothAverageLength)!.IsEnabled = () => GetSettingValue<bool>(HeartrateSetting.SmoothAverage);
    }

    protected override async Task<bool> OnModuleStart()
    {
        currentValue = 0;
        targetValue = 0;
        currentAverage = 0;
        targetAverage = 0;
        connectionCount = 1;
        beatParameterValue = false;
        values.Clear();

        beatParameterSource = new CancellationTokenSource();
        beatParameterTask = Task.Run(handleBeatParameter);

        HeartrateProvider = CreateProvider();
        HeartrateProvider.OnHeartrateUpdate += newHeartrate =>
        {
            connectionCount = 0;
            targetValue = newHeartrate;

            lock (valuesLock)
            {
                values.Add(DateTimeOffset.Now, newHeartrate);
            }
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
        beatParameterSource?.Cancel();
        await (beatParameterTask ?? Task.CompletedTask);
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
        SendParameter(HeartrateParameter.LegacyEnabled, false);
    }

    private async Task handleBeatParameter()
    {
        while (!beatParameterSource!.IsCancellationRequested)
        {
            if (targetValue == 0f) continue;

            var delay = (int)MathF.Round(60f * 1000f / targetValue);
            await Task.Delay(delay);

            if (GetSettingValue<bool>(HeartrateSetting.BeatMode))
            {
                beatParameterValue = true;
                SendParameter(HeartrateParameter.Beat, beatParameterValue);
            }
            else
            {
                beatParameterValue = !beatParameterValue;
                SendParameter(HeartrateParameter.Beat, beatParameterValue);
            }
        }
    }

    protected override async void OnRegisteredParameterReceived(AvatarParameter avatarParameter)
    {
        switch (avatarParameter.Lookup)
        {
            case HeartrateParameter.Beat:
                if (GetSettingValue<bool>(HeartrateSetting.BeatMode) && avatarParameter.GetValue<bool>())
                {
                    await Task.Delay(50);
                    beatParameterValue = false;
                    SendParameter(HeartrateParameter.Beat, beatParameterValue);
                }

                break;
        }
    }

    [ModuleUpdate(ModuleUpdateMode.Custom)]
    private void updateValue()
    {
        if (GetSettingValue<bool>(HeartrateSetting.SmoothValue))
        {
            currentValue = (float)Interpolation.DampContinuously(currentValue, targetValue, GetSettingValue<int>(HeartrateSetting.SmoothValueLength) / 2d, 50d);
        }
        else
        {
            currentValue = targetValue;
        }
    }

    [ModuleUpdate(ModuleUpdateMode.Custom)]
    private void updateAverage()
    {
        var averageLength = TimeSpan.FromMilliseconds(GetSettingValue<int>(HeartrateSetting.AveragePeriod));

        lock (valuesLock)
        {
            var valuesToRemove = values.Where(pair => pair.Key + averageLength <= DateTimeOffset.Now);
            valuesToRemove.ForEach(pair => values.Remove(pair.Key));

            if (!values.Any())
            {
                targetAverage = 0;
                return;
            }

            targetAverage = (int)MathF.Round(values.Values.Sum() / (float)values.Count);
        }

        if (GetSettingValue<bool>(HeartrateSetting.SmoothAverage))
        {
            currentAverage = (float)Interpolation.DampContinuously(currentAverage, targetAverage, GetSettingValue<int>(HeartrateSetting.SmoothAverageLength) / 2d, 50d);
        }
        else
        {
            currentAverage = targetAverage;
        }
    }

    [ModuleUpdate(ModuleUpdateMode.Custom)]
    private void updateParameters()
    {
        SendParameter(HeartrateParameter.Connected, isReceiving);

        if (isReceiving)
        {
            var normalisedHeartRate = Map(currentValue, GetSettingValue<int>(HeartrateSetting.NormalisedLowerbound), GetSettingValue<int>(HeartrateSetting.NormalisedUpperbound), 0, 1);

            SendParameter(HeartrateParameter.Normalised, normalisedHeartRate);
            SendParameter(HeartrateParameter.Value, (int)MathF.Round(currentValue));
            SendParameter(HeartrateParameter.Average, (int)MathF.Round(currentAverage));
        }
        else
        {
            SendParameter(HeartrateParameter.Normalised, 0f);
            SendParameter(HeartrateParameter.Value, 0);
            SendParameter(HeartrateParameter.Average, 0);
        }
    }

    [ModuleUpdate(ModuleUpdateMode.Custom)]
    private void updateLegacyParameters()
    {
        SendParameter(HeartrateParameter.LegacyEnabled, isReceiving);

        if (isReceiving)
        {
            var individualValues = toDigitArray((int)Math.Round(currentValue), 3);

            SendParameter(HeartrateParameter.LegacyUnits, individualValues[2] / 10f);
            SendParameter(HeartrateParameter.LegacyTens, individualValues[1] / 10f);
            SendParameter(HeartrateParameter.LegacyHundreds, individualValues[0] / 10f);
        }
        else
        {
            SendParameter(HeartrateParameter.LegacyUnits, 0f);
            SendParameter(HeartrateParameter.LegacyTens, 0f);
            SendParameter(HeartrateParameter.LegacyHundreds, 0f);
        }
    }

    private static int[] toDigitArray(int num, int totalWidth) => num.ToString().PadLeft(totalWidth, '0').Select(digit => int.Parse(digit.ToString())).ToArray();

    private enum HeartrateSetting
    {
        SmoothValue,
        SmoothValueLength,
        AveragePeriod,
        SmoothAverage,
        SmoothAverageLength,
        NormalisedLowerbound,
        NormalisedUpperbound,
        BeatMode
    }

    private enum HeartrateParameter
    {
        Connected,
        Normalised,
        Value,
        Average,
        Beat,
        LegacyEnabled,
        LegacyUnits,
        LegacyTens,
        LegacyHundreds
    }
}
