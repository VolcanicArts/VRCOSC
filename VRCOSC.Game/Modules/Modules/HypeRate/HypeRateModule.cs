// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;

namespace VRCOSC.Game.Modules.Modules.HypeRate;

public class HypeRateModule : Module
{
    public override string Title => "HypeRate";
    public override string Description => "Sends HypeRate.io heartrate values";
    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Heartrate";
    public override Colour4 Colour => Colour4.OrangeRed.Darken(0.5f);
    public override ModuleType ModuleType => ModuleType.Health;
    public override IEnumerable<string> Tags => new[] { "heartrate" };

    private HypeRateProvider? hypeRateProvider;
    private bool receivedHeartrate;

    public override void CreateAttributes()
    {
        CreateSetting(HypeRateSetting.Id, "HypeRate ID", "Your HypeRate ID given on your device", string.Empty);

        CreateOutputParameter(HypeRateOutputParameter.HeartrateEnabled, "Heartrate Enabled", "Whether this module is attempting to emit values", "/avatar/parameters/HeartrateEnabled");
        CreateOutputParameter(HypeRateOutputParameter.HeartrateNormalised, "Heartrate Normalised", "The heartrate value normalised to 60bpm", "/avatar/parameters/HeartrateNormalised");
        CreateOutputParameter(HypeRateOutputParameter.HeartrateUnits, "Heartrate Units", "The units digit 0-9 mapped to a float", "/avatar/parameters/HeartrateUnits");
        CreateOutputParameter(HypeRateOutputParameter.HeartrateTens, "Heartrate Tens", "The tens digit 0-9 mapped to a float", "/avatar/parameters/HeartrateTens");
        CreateOutputParameter(HypeRateOutputParameter.HeartrateHundreds, "Heartrate Hundreds", "The hundreds digit 0-9 mapped to a float", "/avatar/parameters/HeartrateHundreds");
    }

    protected override void OnStart()
    {
        SendParameter(HypeRateOutputParameter.HeartrateEnabled, false);

        var hypeRateId = GetSetting<string>(HypeRateSetting.Id);

        if (string.IsNullOrEmpty(hypeRateId))
        {
            Terminal.Log("Cannot connect to HypeRate. Please enter an Id");
            return;
        }

        hypeRateProvider = new HypeRateProvider(hypeRateId, VRCOSCSecrets.KEYS_HYPERATE);
        hypeRateProvider.OnHeartRateUpdate += handleHeartRateUpdate;
        hypeRateProvider.OnWsConnected += () => SendParameter(HypeRateOutputParameter.HeartrateEnabled, true);
        hypeRateProvider.OnWsDisconnected += () => SendParameter(HypeRateOutputParameter.HeartrateEnabled, false);
        hypeRateProvider.OnWsHeartbeat += handleWsHeartbeat;
        hypeRateProvider.Connect();
    }

    private void handleHeartRateUpdate(int heartrate)
    {
        receivedHeartrate = true;
        var normalisedHeartRate = heartrate / 60.0f;
        var individualValues = toDigitArray(heartrate, 3);

        SendParameter(HypeRateOutputParameter.HeartrateEnabled, true);
        SendParameter(HypeRateOutputParameter.HeartrateNormalised, normalisedHeartRate);
        SendParameter(HypeRateOutputParameter.HeartrateUnits, individualValues[2] / 10f);
        SendParameter(HypeRateOutputParameter.HeartrateTens, individualValues[1] / 10f);
        SendParameter(HypeRateOutputParameter.HeartrateHundreds, individualValues[0] / 10f);
    }

    private void handleWsHeartbeat()
    {
        if (!receivedHeartrate) SendParameter(HypeRateOutputParameter.HeartrateEnabled, false);
        receivedHeartrate = false;
    }

    private static int[] toDigitArray(int num, int totalWidth)
    {
        return num.ToString().PadLeft(totalWidth, '0').Select(digit => int.Parse(digit.ToString())).ToArray();
    }

    protected override void OnStop()
    {
        hypeRateProvider?.Disconnect();
    }

    private enum HypeRateSetting
    {
        Id
    }

    private enum HypeRateOutputParameter
    {
        HeartrateEnabled,
        HeartrateNormalised,
        HeartrateUnits,
        HeartrateTens,
        HeartrateHundreds
    }
}
