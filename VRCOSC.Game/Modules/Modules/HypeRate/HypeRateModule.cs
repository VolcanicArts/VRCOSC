using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Platform;

namespace VRCOSC.Game.Modules.Modules;

public class HypeRateModule : Module
{
    public override string Title => "HypeRate";
    public override string Description => "Sends heartrate data taken from HypeRate.io into VRChat";
    public override string Author => "VolcanicArts";
    public override Colour4 Colour => Colour4.OrangeRed;
    public override ModuleType Type => ModuleType.Health;

    private HypeRateProvider hypeRateProvider;

    public HypeRateModule(Storage storage)
        : base(storage)
    {
        CreateSetting("id", "HypeRate ID", "Your HypeRate ID given on your device", string.Empty);
        CreateSetting("apikey", "Api Key", "Your API key from HypeRate", string.Empty);

        CreateParameter(HypeRateParameter.HeartrateEnabled, "Heartrate Enabled", "Whether this module is attepting to emit values", "/avatar/parameters/HeartrateEnabled");
        CreateParameter(HypeRateParameter.HeartrateNormalised, "Heartrate Normalised", "The heartrate value normalised to 60bpm", "/avatar/parameters/HeartrateNormalised");
        CreateParameter(HypeRateParameter.HeartrateUnits, "Heartrate Units", "The units value of the heartrate value", "/avatar/parameters/HeartrateUnits");
        CreateParameter(HypeRateParameter.HeartrateTens, "Heartrate Tens", "The tens value of the heartate value", "/avatar/parameters/HeartrateTens");
        CreateParameter(HypeRateParameter.HeartrateHundreds, "Heartrate Hundreds", "The hundreds value of the heartrate value", "/avatar/parameters/HeartrateHundreds");

        LoadData();
    }

    public override void Start()
    {
        base.Start();
        SendParameter(HypeRateParameter.HeartrateEnabled, false);
        hypeRateProvider = new HypeRateProvider(GetSettingValue<string>("id"), GetSettingValue<string>("apikey"));
        hypeRateProvider.OnHeartRateUpdate += handleHeartRateUpdate;
        hypeRateProvider.OnConnected += () => SendParameter(HypeRateParameter.HeartrateEnabled, true);
        hypeRateProvider.OnDisconnected += () => SendParameter(HypeRateParameter.HeartrateEnabled, false);
        hypeRateProvider.Connect();
    }

    private void handleHeartRateUpdate(int heartrate)
    {
        SendParameter(HypeRateParameter.HeartrateEnabled, true);

        var normalisedHeartRate = heartrate / 60.0f;
        SendParameter(HypeRateParameter.HeartrateNormalised, normalisedHeartRate);

        var individualValues = toDigitArray(heartrate);
        SendParameter(HypeRateParameter.HeartrateUnits, individualValues[2]);
        SendParameter(HypeRateParameter.HeartrateTens, individualValues[1]);
        SendParameter(HypeRateParameter.HeartrateHundreds, individualValues[0]);
    }

    private static int[] toDigitArray(int num)
    {
        var numStr = num.ToString().PadLeft(3, '0');
        return numStr.Select(digit => int.Parse(digit.ToString())).ToArray();
    }

    public override void Stop()
    {
        base.Stop();
        hypeRateProvider.Disconnect();
    }
}

public enum HypeRateParameter
{
    HeartrateEnabled,
    HeartrateNormalised,
    HeartrateUnits,
    HeartrateTens,
    HeartrateHundreds
}
