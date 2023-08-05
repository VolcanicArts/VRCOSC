// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Bases.Heartrate;

namespace VRCOSC.Modules.Heartrate.HypeRate;

[ModuleTitle("HypeRate")]
[ModuleDescription("Connects to HypeRate.io and sends your heartrate to VRChat")]
[ModuleAuthor("VolcanicArts", "https://github.com/VolcanicArts", "https://avatars.githubusercontent.com/u/29819296?v=4")]
public sealed class HypeRateModule : HeartrateModule<HypeRateProvider>
{
    protected override HypeRateProvider CreateProvider() => new(GetSetting<string>(HypeRateSetting.Id), OfficialModuleSecrets.GetSecret(OfficialModuleSecretsKeys.Hyperate));

    protected override void CreateAttributes()
    {
        CreateSetting(HypeRateSetting.Id, "HypeRate ID", "Your HypeRate ID given on your device", string.Empty);
        base.CreateAttributes();
    }

    protected override void OnModuleStart()
    {
        if (string.IsNullOrEmpty(GetSetting<string>(HypeRateSetting.Id)))
        {
            Log("Cannot connect to HypeRate. Please enter an Id");
            return;
        }

        base.OnModuleStart();
    }

    [ModuleUpdate(ModuleUpdateMode.Custom, true, 10000)]
    private void sendWsHeartbeat()
    {
        HeartrateProvider?.SendWsHeartBeat();
    }

    private enum HypeRateSetting
    {
        Id
    }
}
