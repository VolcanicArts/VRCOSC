// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;

namespace VRCOSC.Game.Modules.Modules.Heartrate.HypeRate;

public class HypeRateModule : HeartrateModule
{
    public override string Title => "HypeRate";
    public override string Description => "Sends HypeRate.io heartrate values";
    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Heartrate";
    public override ColourInfo Colour => Colour4.Red;
    public override ModuleType ModuleType => ModuleType.Health;

    private HypeRateProvider? hypeRateProvider;
    private bool receivedHeartRate;

    protected override void CreateAttributes()
    {
        CreateSetting(HypeRateSetting.Id, "HypeRate ID", "Your HypeRate ID given on your device", string.Empty);

        base.CreateAttributes();
    }

    protected override void OnStart()
    {
        SendParameter(HeartrateOutputParameter.HeartrateEnabled, false);

        var hypeRateId = GetSetting<string>(HypeRateSetting.Id);

        if (string.IsNullOrEmpty(hypeRateId))
        {
            Terminal.Log("Cannot connect to HypeRate. Please enter an Id");
            return;
        }

        hypeRateProvider = new HypeRateProvider(hypeRateId, VRCOSCSecrets.KEYS_HYPERATE);
        hypeRateProvider.OnHeartRateUpdate += HandleHeartRateUpdate;
        hypeRateProvider.OnConnected += () => SendParameter(HeartrateOutputParameter.HeartrateEnabled, true);
        hypeRateProvider.OnDisconnected += () => SendParameter(HeartrateOutputParameter.HeartrateEnabled, false);
        hypeRateProvider.OnWsHeartBeat += handleWsHeartBeat;

        hypeRateProvider.Initialise();
        hypeRateProvider.Connect();
    }

    private void handleWsHeartBeat()
    {
        if (!receivedHeartRate) SendParameter(HeartrateOutputParameter.HeartrateEnabled, false);
        receivedHeartRate = false;
    }

    protected override void OnStop()
    {
        SendParameter(HeartrateOutputParameter.HeartrateEnabled, false);
        hypeRateProvider?.Disconnect();
    }

    private enum HypeRateSetting
    {
        Id
    }
}
