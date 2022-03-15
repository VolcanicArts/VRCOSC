using osu.Framework.Graphics;

namespace VRCOSC.Game.Modules.Modules;

public class HypeRateModule : Module
{
    public override string Title => "HypeRate";
    public override string Description => "Sends heartrate data taken from HypeRate.io into VRChat";
    public override Colour4 Colour => Colour4.OrangeRed;

    private HypeRateProvider hypeRateProvider;

    public HypeRateModule()
    {
        CreateSetting("id", "HypeRate ID", "Your HypeRate ID given on your device", string.Empty);
        CreateSetting("apikey", "Api Key", "Your API key from HypeRate", string.Empty);

        CreateParameter("heartrate", "Heartrate", "The raw Heartrate value", "/avatar/parameters/Heartrate");
    }

    public override void Start()
    {
        base.Start();
        hypeRateProvider = new HypeRateProvider(GetSettingValue<string>("id"), GetSettingValue<string>("apikey"));
        hypeRateProvider.OnHeartRateUpdate += handleHeartRateUpdate;
        hypeRateProvider.Connect();
    }

    private void handleHeartRateUpdate(int heartrate)
    {
        SendParameter("heartrate", heartrate);
    }

    public override void Stop()
    {
        base.Stop();
        hypeRateProvider.Disconnect();
    }
}
