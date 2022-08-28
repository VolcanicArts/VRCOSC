using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;

namespace VRCOSC.Game.Modules.Modules.Heartrate.Pulsoid;

public class PulsoidModule : HeartrateModule
{
    public override string Title => "Pulsoid";
    public override string Description => "Sends Pulsoid heartrate values";
    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Heartrate";
    public override ColourInfo Colour => Colour4.Red;
    public override ModuleType ModuleType => ModuleType.Health;

    private PulsoidProvider? pulsoidProvider;

    protected override void CreateAttributes()
    {
        CreateSetting(PulsoidSetting.AccessToken, "Access Token", "Your Pulsoid access token given by Pulsoid", string.Empty, () => OpenUrlExternally("https://google.com"));

        base.CreateAttributes();
    }

    protected override void OnStart()
    {
        SendParameter(HeartrateOutputParameter.HeartrateEnabled, false);

        var accessToken = GetSetting<string>(PulsoidSetting.AccessToken);

        if (string.IsNullOrEmpty(accessToken))
        {
            Terminal.Log("Cannot connect to Pulsoid. Please obtain an access token");
            return;
        }

        pulsoidProvider = new PulsoidProvider(accessToken);
        pulsoidProvider.OnHeartRateUpdate += HandleHeartRateUpdate;
        pulsoidProvider.OnConnected += () => SendParameter(HeartrateOutputParameter.HeartrateEnabled, true);
        pulsoidProvider.OnDisconnected += () => SendParameter(HeartrateOutputParameter.HeartrateEnabled, false);

        pulsoidProvider.Initialise();
        pulsoidProvider.Connect();
    }

    protected override void OnStop()
    {
        SendParameter(HeartrateOutputParameter.HeartrateEnabled, false);
        pulsoidProvider?.Disconnect();
    }

    private enum PulsoidSetting
    {
        AccessToken
    }
}
