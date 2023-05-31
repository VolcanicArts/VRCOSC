// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using Octokit;

namespace VRCOSC.Game.Github;

public class GitHubProvider
{
    private readonly GitHubClient client;

    public GitHubProvider(string appName)
    {
        client = new GitHubClient(new ProductHeaderValue(appName));
    }

    public Task<Release> GetLatestReleaseFor(Uri repoUrl)
    {
        var userName = repoUrl.Segments[^2].TrimEnd(new[] { '/' });
        var repoName = repoUrl.Segments[^1].TrimEnd(new[] { '/' });

        return client.Repository.Release.GetLatest(userName, repoName);
    }
}
