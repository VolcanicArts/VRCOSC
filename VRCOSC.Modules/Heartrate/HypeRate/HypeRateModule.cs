// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules.Bases.Heartrate;

namespace VRCOSC.Modules.Heartrate.HypeRate;

public sealed class HypeRateModule : HeartrateModule<HypeRateProvider>
{
    public override string Title => @"HypeRate";
    public override string Author => @"VolcanicArts";
    public override string Description => @"Connects to HypeRate.io and sends your heartrate to VRChat";
    protected override TimeSpan DeltaUpdate => TimeSpan.FromSeconds(10);

    protected override HypeRateProvider CreateProvider() => new(GetSetting<string>(HypeRateSetting.Id), OfficialModuleSecrets.GetSecret(OfficialModuleSecretsKeys.Hyperate));

    protected override void CreateAttributes()
    {
        CreateSetting(HypeRateSetting.Id, @"HypeRate ID", @"Your HypeRate ID given on your device", string.Empty);
        base.CreateAttributes();
    }

    protected override void OnModuleStart()
    {
        if (string.IsNullOrEmpty(GetSetting<string>(HypeRateSetting.Id)))
        {
            Log(@"Cannot connect to HypeRate. Please enter an Id");
            return;
        }

        base.OnModuleStart();
    }

    protected override void OnModuleUpdate()
    {
        HeartrateProvider?.SendWsHeartBeat();
    }

    private enum HypeRateSetting
    {
        Id
    }
}
