// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using Octokit;
using VRCOSC.Packages;

namespace VRCOSC.Actions.Packages;

public class SearchRepositoriesAction : ResultableProgressAction<SearchRepositoryResult>
{
    public override string Title => "Finding remote repositories";

    private readonly string tag;

    public SearchRepositoriesAction(string tag)
    {
        this.tag = tag;
    }

    protected override async Task Perform()
    {
        var repos = await GitHubProxy.Client.Search.SearchRepo(new SearchRepositoriesRequest
        {
            Topic = tag
        });

        Result = repos;
    }

    public override float GetProgress() => 0f;
}
