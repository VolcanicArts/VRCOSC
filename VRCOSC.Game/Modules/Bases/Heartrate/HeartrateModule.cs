// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using System.Threading;
using osu.Framework.Utils;
using VRCOSC.Game.Modules.ChatBox;

namespace VRCOSC.Game.Modules.Bases.Heartrate;

[ModuleGroup(ModuleType.Health)]
[ModulePrefab("VRCOSC-Heartrate", "https://github.com/VolcanicArts/VRCOSC/releases/download/latest/VRCOSC-Heartrate.unitypackage")]
public abstract class HeartrateModule<T> : ChatBoxModule where T : HeartrateProvider
{
    protected T? HeartrateProvider;

    private float currentHeartrate;
    private int targetHeartrate;
    private int connectionCount;

    protected abstract T CreateProvider();

    protected override void CreateAttributes()
    {
        CreateSetting(HeartrateSetting.Smoothed, @"Smoothed", @"Whether the current heartrate should jump or smoothly converge to the target heartrate", false);
        CreateSetting(HeartrateSetting.SmoothingLength, @"Smoothing Length", @"The length of time (in milliseconds) the current heartrate should take to converge to the target heartrate", 1000, () => GetSetting<bool>(HeartrateSetting.Smoothed));
        CreateSetting(HeartrateSetting.NormalisedLowerbound, @"Normalised Lowerbound", @"The lower bound BPM the normalised parameter should use", 0);
        CreateSetting(HeartrateSetting.NormalisedUpperbound, @"Normalised Upperbound", @"The upper bound BPM the normalised parameter should use", 240);

        CreateParameter<bool>(HeartrateParameter.Enabled, ParameterMode.Write, @"VRCOSC/Heartrate/Enabled", @"Enabled", @"Whether this module is connected and receiving values");
        CreateParameter<float>(HeartrateParameter.Normalised, ParameterMode.Write, @"VRCOSC/Heartrate/Normalised", @"Normalised", @"The heartrate value normalised to the set bounds");
        CreateParameter<float>(HeartrateParameter.Units, ParameterMode.Write, @"VRCOSC/Heartrate/Units", @"Units", @"The units digit 0-9 mapped to a float");
        CreateParameter<float>(HeartrateParameter.Tens, ParameterMode.Write, @"VRCOSC/Heartrate/Tens", @"Tens", @"The tens digit 0-9 mapped to a float");
        CreateParameter<float>(HeartrateParameter.Hundreds, ParameterMode.Write, @"VRCOSC/Heartrate/Hundreds", @"Hundreds", @"The hundreds digit 0-9 mapped to a float");

        CreateVariable(HeartrateVariable.Heartrate, @"Heartrate", @"hr");

        CreateState(HeartrateState.Default, @"Default", $@"Heartrate/v{GetVariableFormat(HeartrateVariable.Heartrate)} bpm");
    }

    protected override void OnModuleStart()
    {
        currentHeartrate = 0;
        targetHeartrate = 0;
        connectionCount = 0;
        ChangeStateTo(HeartrateState.Default);
        attemptConnection();
    }

    private void attemptConnection()
    {
        if (connectionCount >= 3)
        {
            Log(@"Connection cannot be established");
            return;
        }

        connectionCount++;
        HeartrateProvider = CreateProvider();
        HeartrateProvider.OnHeartrateUpdate += newHeartrate => targetHeartrate = newHeartrate;
        HeartrateProvider.OnConnected += () => connectionCount = 0;
        HeartrateProvider.OnDisconnected += attemptReconnection;
        HeartrateProvider.OnLog += Log;
        HeartrateProvider.Initialise();
    }

    private void attemptReconnection()
    {
        Log("Attempting reconnection...");
        Thread.Sleep(2000);
        attemptConnection();
    }

    protected override void OnModuleStop()
    {
        HeartrateProvider?.Teardown();
        HeartrateProvider = null;

        SendParameter(HeartrateParameter.Enabled, false);
    }

    [ModuleUpdate(ModuleUpdateMode.Fixed)]
    private void updateParameters()
    {
        if (GetSetting<bool>(HeartrateSetting.Smoothed))
        {
            currentHeartrate = (float)Interpolation.DampContinuously(currentHeartrate, targetHeartrate, GetSetting<int>(HeartrateSetting.SmoothingLength) / 2d, FIXED_UPDATE_DELTA);
        }
        else
        {
            currentHeartrate = targetHeartrate;
        }

        sendParameters();
    }

    private void sendParameters()
    {
        var isReceiving = HeartrateProvider?.IsReceiving ?? false;

        SendParameter(HeartrateParameter.Enabled, isReceiving);

        if (isReceiving)
        {
            var normalisedHeartRate = Map(currentHeartrate, GetSetting<int>(HeartrateSetting.NormalisedLowerbound), GetSetting<int>(HeartrateSetting.NormalisedUpperbound), 0, 1);

            var intHeartrate = (int)Math.Round(currentHeartrate);
            var individualValues = toDigitArray(intHeartrate, 3);

            SendParameter(HeartrateParameter.Normalised, normalisedHeartRate);
            SendParameter(HeartrateParameter.Units, individualValues[2] / 10f);
            SendParameter(HeartrateParameter.Tens, individualValues[1] / 10f);
            SendParameter(HeartrateParameter.Hundreds, individualValues[0] / 10f);
            SetVariableValue(HeartrateVariable.Heartrate, intHeartrate.ToString());
        }
        else
        {
            SendParameter(HeartrateParameter.Normalised, 0);
            SendParameter(HeartrateParameter.Units, 0);
            SendParameter(HeartrateParameter.Tens, 0);
            SendParameter(HeartrateParameter.Hundreds, 0);
            SetVariableValue(HeartrateVariable.Heartrate, @"0");
        }
    }

    private static int[] toDigitArray(int num, int totalWidth)
    {
        return num.ToString().PadLeft(totalWidth, '0').Select(digit => int.Parse(digit.ToString())).ToArray();
    }

    private enum HeartrateSetting
    {
        Smoothed,
        SmoothingLength,
        NormalisedLowerbound,
        NormalisedUpperbound
    }

    private enum HeartrateParameter
    {
        Enabled,
        Normalised,
        Units,
        Tens,
        Hundreds
    }

    private enum HeartrateState
    {
        Default
    }

    private enum HeartrateVariable
    {
        Heartrate
    }
}
