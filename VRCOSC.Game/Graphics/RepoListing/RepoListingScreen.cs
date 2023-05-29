// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics.Screen;

namespace VRCOSC.Game.Graphics.RepoListing;

public partial class RepoListingScreen : BaseScreen
{
    protected override BaseHeader CreateHeader() => new RepoListingHeader();

    protected override Drawable CreateBody() => new Box();
}
