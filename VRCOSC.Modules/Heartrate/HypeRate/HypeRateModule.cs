// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules;

namespace VRCOSC.Modules.Heartrate.HypeRate;

public sealed class HypeRateModule : HeartRateModule
{
    public override string Title => @"HypeRate";
    public override string Description => @"Connects to HypeRate.io and sends your heartrate to VRChat";
    protected override TimeSpan DeltaUpdate => TimeSpan.FromSeconds(10);

    protected override HeartRateProvider CreateHeartRateProvider() => new HypeRateProvider(GetSetting<string>(HypeRateSetting.Id), OfficialModuleSecrets.GetSecret(OfficialModuleSecretsKeys.Hyperate), new TerminalLogger(Title));

    protected override void CreateAttributes()
    {
        CreateSetting(HypeRateSetting.Id, @"HypeRate ID", @"Your HypeRate ID given on your device", string.Empty);
        base.CreateAttributes();
    }

    protected override void OnModuleStart()
    {
        var hypeRateId = GetSetting<string>(HypeRateSetting.Id);

        if (string.IsNullOrEmpty(hypeRateId))
        {
            Log(@"Cannot connect to HypeRate. Please enter an Id");
            return;
        }

        base.OnModuleStart();
    }

    protected override void OnModuleUpdate()
    {
        if (HeartRateProvider is null || !HeartRateProvider.IsConnected) return;

        ((HypeRateProvider)HeartRateProvider).SendWsHeartBeat();
    }

    private enum HypeRateSetting
    {
        Id
    }
}
