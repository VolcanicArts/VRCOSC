// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Graphics;
using VRCOSC.Game.Modules.SDK;
using VRCOSC.Game.Modules.SDK.Attributes;
using VRCOSC.Game.Modules.SDK.Modules.Heartrate;

namespace VRCOSC.Modules.Pulsoid;

[ModuleTitle("Pulsoid")]
[ModuleDescription("Connects to Pulsoid and sends your heartrate to VRChat")]
public sealed class PulsoidModule : HeartrateModule<PulsoidProvider>
{
    private const string pulsoid_access_token_url = "https://pulsoid.net/oauth2/authorize?response_type=token&client_id=a31caa68-b6ac-4680-976a-9761b915a1e3&redirect_uri=&scope=data:heart_rate:read&state=a52beaeb-c491-4cd3-b915-16fed71e17a8&response_mode=web_page";

    protected override PulsoidProvider CreateProvider() => new(GetSettingValue<string>(PulsoidSetting.AccessToken)!);

    protected override void OnLoad()
    {
        CreateTextBox(PulsoidSetting.AccessToken, "Access Token", "Your Pulsoid access token", string.Empty, false);

        CreateGroup("Access", PulsoidSetting.AccessToken);

        base.OnLoad();
    }

    protected override void OnPostLoad()
    {
        GetSetting(PulsoidSetting.AccessToken)!
            .AddAddon(new ButtonModuleSettingAddon("Obtain Access Token", Colours.BLUE0, () => OpenUrlExternally(pulsoid_access_token_url)));

        base.OnPostLoad();
    }

    protected override Task<bool> OnModuleStart()
    {
        if (string.IsNullOrEmpty(GetSettingValue<string>(PulsoidSetting.AccessToken)))
        {
            Log("Please enter a valid access token in the settings");
            return Task.FromResult(false);
        }

        return base.OnModuleStart();
    }

    private enum PulsoidSetting
    {
        AccessToken
    }
}
