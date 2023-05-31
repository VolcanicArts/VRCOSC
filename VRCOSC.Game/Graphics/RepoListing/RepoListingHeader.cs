// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Graphics.Screen;

namespace VRCOSC.Game.Graphics.RepoListing;

public partial class RepoListingHeader : BaseHeader
{
    protected override string Title => "Module Repositories";
    protected override string SubTitle => "Here you can add repositories that publish module DLLs to be automatically updated";
}
