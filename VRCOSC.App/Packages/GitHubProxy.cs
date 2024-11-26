// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using Octokit;

namespace VRCOSC.App.Packages;

public static class GitHubProxy
{
    private static readonly string client_id = "-";
    private static readonly string client_secret = "-";

    public static GitHubClient Client = new(new ProductHeaderValue("VRCOSC"))
    {
        Credentials = new Credentials(client_id, client_secret, AuthenticationType.Basic)
    };
}

