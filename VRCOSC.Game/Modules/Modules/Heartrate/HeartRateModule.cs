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
        CreateOutgoingParameter(HeartrateOutgoingParameter.HeartrateEnabled, "Heartrate Enabled", "Whether this module is attempting to emit values", "/avatar/parameters/HeartrateEnabled");
        CreateOutgoingParameter(HeartrateOutgoingParameter.HeartrateNormalised, "Heartrate Normalised", "The heartrate value normalised to 60bpm", "/avatar/parameters/HeartrateNormalised");
        CreateOutgoingParameter(HeartrateOutgoingParameter.HeartrateUnits, "Heartrate Units", "The units digit 0-9 mapped to a float", "/avatar/parameters/HeartrateUnits");
        CreateOutgoingParameter(HeartrateOutgoingParameter.HeartrateTens, "Heartrate Tens", "The tens digit 0-9 mapped to a float", "/avatar/parameters/HeartrateTens");
        CreateOutgoingParameter(HeartrateOutgoingParameter.HeartrateHundreds, "Heartrate Hundreds", "The hundreds digit 0-9 mapped to a float", "/avatar/parameters/HeartrateHundreds");
    }

    protected override void OnStart()
    {
        heartRateProvider = CreateHeartRateProvider();
        heartRateProvider.OnHeartRateUpdate += HandleHeartRateUpdate;
        heartRateProvider.OnDisconnected += () => SendParameter(HeartrateOutgoingParameter.HeartrateEnabled, false);
        heartRateProvider.Initialise();
        heartRateProvider.Connect();
    }

    protected override async void OnStop()
    {
        if (heartRateProvider is null) return;

        await heartRateProvider.Disconnect();
        SendParameter(HeartrateOutgoingParameter.HeartrateEnabled, false);
    }

    protected virtual void HandleHeartRateUpdate(int heartrate)
    {
        var normalisedHeartRate = heartrate / 60.0f;
        var individualValues = toDigitArray(heartrate, 3);

        SendParameter(HeartrateOutgoingParameter.HeartrateEnabled, true);
        SendParameter(HeartrateOutgoingParameter.HeartrateNormalised, normalisedHeartRate);
        SendParameter(HeartrateOutgoingParameter.HeartrateUnits, individualValues[2] / 10f);
        SendParameter(HeartrateOutgoingParameter.HeartrateTens, individualValues[1] / 10f);
        SendParameter(HeartrateOutgoingParameter.HeartrateHundreds, individualValues[0] / 10f);
    }

    private static int[] toDigitArray(int num, int totalWidth)
    {
        return num.ToString().PadLeft(totalWidth, '0').Select(digit => int.Parse(digit.ToString())).ToArray();
    }

    protected enum HeartrateOutgoingParameter
    {
        HeartrateEnabled,
        HeartrateNormalised,
        HeartrateUnits,
        HeartrateTens,
        HeartrateHundreds
    }
}
