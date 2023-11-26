// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Octokit;

namespace VRCOSC.Game.Packages;

public static class GitHubProxy
{
    private const string client_id = "70c1b7af05288131463c";
    private const string client_secret = "";

    public static GitHubClient Client = new(new ProductHeaderValue("VRCOSC"))
    {
        Credentials = new Credentials(client_id, client_secret, AuthenticationType.Basic)
    };
}
