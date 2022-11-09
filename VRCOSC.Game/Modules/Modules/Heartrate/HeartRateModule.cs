// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;

namespace VRCOSC.Game.Modules.Modules.Heartrate;

public abstract class HeartRateModule : Module
{
    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Heartrate";
    public override ModuleType ModuleType => ModuleType.Health;

    private HeartRateProvider? heartRateProvider;

    protected abstract HeartRateProvider CreateHeartRateProvider();

    protected override void CreateAttributes()
    {
        CreateParameter<bool>(HeartrateOutgoingParameter.Enabled, ParameterMode.Write, "VRCOSC/Heartrate/Enabled", "Whether this module is attempting to emit values");
        CreateParameter<float>(HeartrateOutgoingParameter.Normalised, ParameterMode.Write, "VRCOSC/Heartrate/Normalised", "The heartrate value normalised to 60bpm");
        CreateParameter<float>(HeartrateOutgoingParameter.Units, ParameterMode.Write, "VRCOSC/Heartrate/Units", "The units digit 0-9 mapped to a float");
        CreateParameter<float>(HeartrateOutgoingParameter.Tens, ParameterMode.Write, "VRCOSC/Heartrate/Tens", "The tens digit 0-9 mapped to a float");
        CreateParameter<float>(HeartrateOutgoingParameter.Hundreds, ParameterMode.Write, "VRCOSC/Heartrate/Hundreds", "The hundreds digit 0-9 mapped to a float");
    }

    protected override void OnStart()
    {
        heartRateProvider = CreateHeartRateProvider();
        heartRateProvider.OnHeartRateUpdate += HandleHeartRateUpdate;
        heartRateProvider.OnDisconnected += () => SendParameter(HeartrateOutgoingParameter.Enabled, false);
        heartRateProvider.Initialise();
        heartRateProvider.Connect();
    }

    protected override async void OnStop()
    {
        if (heartRateProvider is null) return;

        await heartRateProvider.Disconnect();
        SendParameter(HeartrateOutgoingParameter.Enabled, false);
    }

    protected virtual void HandleHeartRateUpdate(int heartrate)
    {
        var normalisedHeartRate = heartrate / 60.0f;
        var individualValues = toDigitArray(heartrate, 3);

        SendParameter(HeartrateOutgoingParameter.Enabled, true);
        SendParameter(HeartrateOutgoingParameter.Normalised, normalisedHeartRate);
        SendParameter(HeartrateOutgoingParameter.Units, individualValues[2] / 10f);
        SendParameter(HeartrateOutgoingParameter.Tens, individualValues[1] / 10f);
        SendParameter(HeartrateOutgoingParameter.Hundreds, individualValues[0] / 10f);
    }

    private static int[] toDigitArray(int num, int totalWidth)
    {
        return num.ToString().PadLeft(totalWidth, '0').Select(digit => int.Parse(digit.ToString())).ToArray();
    }

    protected enum HeartrateOutgoingParameter
    {
        Enabled,
        Normalised,
        Units,
        Tens,
        Hundreds
    }
}
