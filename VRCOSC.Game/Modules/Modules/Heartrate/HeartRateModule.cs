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
        CreateOutgoingParameter(HeartrateOutputParameter.HeartrateEnabled, "Heartrate Enabled", "Whether this module is attempting to emit values", "/avatar/parameters/HeartrateEnabled");
        CreateOutgoingParameter(HeartrateOutputParameter.HeartrateNormalised, "Heartrate Normalised", "The heartrate value normalised to 60bpm", "/avatar/parameters/HeartrateNormalised");
        CreateOutgoingParameter(HeartrateOutputParameter.HeartrateUnits, "Heartrate Units", "The units digit 0-9 mapped to a float", "/avatar/parameters/HeartrateUnits");
        CreateOutgoingParameter(HeartrateOutputParameter.HeartrateTens, "Heartrate Tens", "The tens digit 0-9 mapped to a float", "/avatar/parameters/HeartrateTens");
        CreateOutgoingParameter(HeartrateOutputParameter.HeartrateHundreds, "Heartrate Hundreds", "The hundreds digit 0-9 mapped to a float", "/avatar/parameters/HeartrateHundreds");
    }

    protected override void OnStart()
    {
        SendParameter(HeartrateOutputParameter.HeartrateEnabled, false);

        heartRateProvider = CreateHeartRateProvider();
        heartRateProvider.OnHeartRateUpdate += HandleHeartRateUpdate;
        heartRateProvider.OnConnected += () => SendParameter(HeartrateOutputParameter.HeartrateEnabled, true);
        heartRateProvider.OnDisconnected += () => SendParameter(HeartrateOutputParameter.HeartrateEnabled, false);
        heartRateProvider.Initialise();
        heartRateProvider.Connect();
    }

    protected override async void OnStop()
    {
        if (heartRateProvider is null) return;

        await heartRateProvider.Disconnect();
        SendParameter(HeartrateOutputParameter.HeartrateEnabled, false);
    }

    protected virtual void HandleHeartRateUpdate(int heartrate)
    {
        var normalisedHeartRate = heartrate / 60.0f;
        var individualValues = toDigitArray(heartrate, 3);

        SendParameter(HeartrateOutputParameter.HeartrateEnabled, true);
        SendParameter(HeartrateOutputParameter.HeartrateNormalised, normalisedHeartRate);
        SendParameter(HeartrateOutputParameter.HeartrateUnits, individualValues[2] / 10f);
        SendParameter(HeartrateOutputParameter.HeartrateTens, individualValues[1] / 10f);
        SendParameter(HeartrateOutputParameter.HeartrateHundreds, individualValues[0] / 10f);
    }

    private static int[] toDigitArray(int num, int totalWidth)
    {
        return num.ToString().PadLeft(totalWidth, '0').Select(digit => int.Parse(digit.ToString())).ToArray();
    }

    protected enum HeartrateOutputParameter
    {
        HeartrateEnabled,
        HeartrateNormalised,
        HeartrateUnits,
        HeartrateTens,
        HeartrateHundreds
    }
}
