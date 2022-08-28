using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;

namespace VRCOSC.Game.Modules.Modules.Heartrate.Pulsoid;

public class PulsoidModule : Module
{
    public override string Title => "Pulsoid";
    public override string Description => "Sends Pulsoid heartrate values";
    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Heartrate";
    public override ColourInfo Colour => Colour4.Red;
    public override ModuleType ModuleType => ModuleType.Health;

    private PulsoidProvider? pulsoidProvider;
    private bool receivedHeartRate;

    protected override void CreateAttributes()
    {
        CreateSetting(PulsoidSetting.AccessToken, "Access Token", "Your Pulsoid access token given bu Pulsoid", string.Empty);

        CreateOutputParameter(PulsoidOutputParameter.HeartrateEnabled, "Heartrate Enabled", "Whether this module is attempting to emit values", "/avatar/parameters/HeartrateEnabled");
        CreateOutputParameter(PulsoidOutputParameter.HeartrateNormalised, "Heartrate Normalised", "The heartrate value normalised to 60bpm", "/avatar/parameters/HeartrateNormalised");
        CreateOutputParameter(PulsoidOutputParameter.HeartrateUnits, "Heartrate Units", "The units digit 0-9 mapped to a float", "/avatar/parameters/HeartrateUnits");
        CreateOutputParameter(PulsoidOutputParameter.HeartrateTens, "Heartrate Tens", "The tens digit 0-9 mapped to a float", "/avatar/parameters/HeartrateTens");
        CreateOutputParameter(PulsoidOutputParameter.HeartrateHundreds, "Heartrate Hundreds", "The hundreds digit 0-9 mapped to a float", "/avatar/parameters/HeartrateHundreds");
    }

    protected override void OnStart()
    {
        SendParameter(PulsoidOutputParameter.HeartrateEnabled, false);

        var accessToken = GetSetting<string>(PulsoidSetting.AccessToken);

        if (string.IsNullOrEmpty(accessToken))
        {
            Terminal.Log("Cannot connect to Pulsoid. Please obtain an access token");
            return;
        }

        pulsoidProvider = new PulsoidProvider(accessToken);
        pulsoidProvider.OnHeartRateUpdate += handleHeartRateUpdate;
        pulsoidProvider.OnConnected += () => SendParameter(PulsoidOutputParameter.HeartrateEnabled, true);
        pulsoidProvider.OnDisconnected += () => SendParameter(PulsoidOutputParameter.HeartrateEnabled, false);
        pulsoidProvider.OnWsHeartBeat += handleWsHeartBeat;

        pulsoidProvider.Initialise();
        pulsoidProvider.Connect();
    }

    private void handleHeartRateUpdate(int heartrate)
    {
        receivedHeartRate = true;
        var normalisedHeartRate = heartrate / 60.0f;
        var individualValues = toDigitArray(heartrate, 3);

        SendParameter(PulsoidOutputParameter.HeartrateEnabled, true);
        SendParameter(PulsoidOutputParameter.HeartrateNormalised, normalisedHeartRate);
        SendParameter(PulsoidOutputParameter.HeartrateUnits, individualValues[2] / 10f);
        SendParameter(PulsoidOutputParameter.HeartrateTens, individualValues[1] / 10f);
        SendParameter(PulsoidOutputParameter.HeartrateHundreds, individualValues[0] / 10f);
    }

    private void handleWsHeartBeat()
    {
        if (!receivedHeartRate) SendParameter(PulsoidOutputParameter.HeartrateEnabled, false);
        receivedHeartRate = false;
    }

    private static int[] toDigitArray(int num, int totalWidth)
    {
        return num.ToString().PadLeft(totalWidth, '0').Select(digit => int.Parse(digit.ToString())).ToArray();
    }

    protected override void OnStop()
    {
        SendParameter(PulsoidOutputParameter.HeartrateEnabled, false);
        pulsoidProvider?.Disconnect();
    }

    private enum PulsoidSetting
    {
        AccessToken
    }

    private enum PulsoidOutputParameter
    {
        HeartrateEnabled,
        HeartrateNormalised,
        HeartrateUnits,
        HeartrateTens,
        HeartrateHundreds
    }
}
