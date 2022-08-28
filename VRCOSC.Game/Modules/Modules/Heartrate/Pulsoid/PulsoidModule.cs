// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Game.Modules.Modules.Heartrate.Pulsoid;

public class PulsoidModule : HeartRateModule
{
    public override string Title => "Pulsoid";
    public override string Description => "Connects to Pulsoid and sends your heartrate to VRChat";

    protected override HeartRateProvider CreateHeartRateProvider() => new PulsoidProvider(GetSetting<string>(PulsoidSetting.AccessToken));

    protected override void CreateAttributes()
    {
        base.CreateAttributes();
        CreateSetting(PulsoidSetting.AccessToken, "Access Token", "Your Pulsoid access token", string.Empty, () => OpenUrlExternally("https://google.com"));
    }

    protected override void OnStart()
    {
        var accessToken = GetSetting<string>(PulsoidSetting.AccessToken);

        if (string.IsNullOrEmpty(accessToken))
        {
            Terminal.Log("Cannot connect to Pulsoid. Please obtain an access token");
            return;
        }

        base.OnStart();
    }

    private enum PulsoidSetting
    {
        AccessToken
    }
}
