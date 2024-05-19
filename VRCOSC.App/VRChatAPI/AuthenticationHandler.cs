// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Text.Encodings.Web;
using Newtonsoft.Json;
using VRChat.API.Api;
using VRChat.API.Client;
using VRChat.API.Model;
using VRCOSC.App.Utils;

namespace VRCOSC.App.VRChatAPI;

public class AuthenticationHandler
{
    private const string base_path = "https://api.vrchat.cloud/api/1";
    private const string api_key = "";
    private const string user_agent = $"{AppManager.APP_NAME} volcanicarts";

    public Observable<AuthenticationState> State { get; } = new();

    public IAuthenticationApi? AuthAPI { get; private set; }
    public Configuration? Configuration { get; private set; }
    public string? AuthToken { get; private set; }

    public void Logout()
    {
        if (AuthAPI?.GetCurrentUser() is null)
        {
            throw new InvalidOperationException("Cannot logout while already logged out");
        }

        AuthAPI.Logout();
        AuthAPI = null;
        Configuration = null;
        AuthToken = null;
        State.Value = AuthenticationState.LoggedOut;
    }

    public void LoginWithAuthToken(string authToken)
    {
        if (AuthAPI?.GetCurrentUser() is not null)
        {
            throw new InvalidOperationException("Cannot login while already logged in");
        }

        Configuration = makeConfigurationWithAuthToken(authToken);
        AuthAPI = new AuthenticationApi(Configuration);

        if (!AuthAPI.VerifyAuthToken().Ok)
        {
            State.Value = AuthenticationState.InvalidCredentials;
            return;
        }

        AuthToken = authToken;
        State.Value = AuthenticationState.LoggedIn;
    }

    public void LoginWithCredentials(string username, string password)
    {
        if (AuthAPI?.GetCurrentUser() is not null)
        {
            throw new InvalidOperationException("Cannot login while already logged in");
        }

        Configuration = makeConfigurationWithCredentials(username, password);
        AuthAPI = new AuthenticationApi(Configuration);

        var httpInfo = JsonConvert.DeserializeObject<UserHttpInfo>(AuthAPI.GetCurrentUserWithHttpInfo().RawContent);

        if (httpInfo is null)
        {
            State.Value = AuthenticationState.InvalidCredentials;
            return;
        }

        if (httpInfo.TwoFactorAuthTypes.Contains("totp"))
        {
            State.Value = AuthenticationState.Requires2FA;
            return;
        }

        if (httpInfo.TwoFactorAuthTypes.Contains("otp"))
        {
            State.Value = AuthenticationState.Requires2FAEmail;
            return;
        }

        AuthToken = AuthAPI.VerifyAuthToken().Token;
        State.Value = AuthenticationState.LoggedIn;
    }

    public void Verify2FACode(string code, bool isEmail)
    {
        if (AuthAPI is null || Configuration is null)
        {
            throw new InvalidOperationException("Cannot verify 2FA code before a login attempt");
        }

        if (isEmail)
        {
            var result = AuthAPI.Verify2FAEmailCode(new TwoFactorEmailCode(code));

            if (!result.Verified)
            {
                State.Value = AuthenticationState.Invalid2FA;
                return;
            }
        }
        else
        {
            var result = AuthAPI.Verify2FA(new TwoFactorAuthCode(code));

            if (!result.Verified)
            {
                State.Value = AuthenticationState.Invalid2FA;
                return;
            }
        }

        AuthToken = AuthAPI.VerifyAuthToken().Token;
        State.Value = AuthenticationState.LoggedIn;
    }

    private Configuration makeConfigurationWithAuthToken(string authKey)
    {
        return new Configuration
        {
            BasePath = base_path,
            UserAgent = user_agent,
            Timeout = 5000,
            DefaultHeaders =
            {
                ["Cookie"] = $"apiKey={api_key}; auth={authKey}"
            }
        };
    }

    private Configuration makeConfigurationWithCredentials(string username, string password)
    {
        return new Configuration
        {
            BasePath = base_path,
            UserAgent = user_agent,
            Username = UrlEncoder.Default.Encode(username),
            Password = UrlEncoder.Default.Encode(password),
            Timeout = 5000,
            DefaultHeaders =
            {
                ["Cookie"] = $"apiKey={api_key}"
            }
        };
    }
}

public enum AuthenticationState
{
    LoggedOut,
    InvalidCredentials,
    Requires2FA,
    Requires2FAEmail,
    Invalid2FA,
    LoggedIn
}
