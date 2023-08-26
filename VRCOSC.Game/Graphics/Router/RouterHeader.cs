// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using VRCOSC.Game.Graphics.Screen;
using VRCOSC.Game.Graphics.UI;

namespace VRCOSC.Game.Graphics.Router;

public partial class RouterHeader : BaseHeader
{
    private const string vrcosc_router_wiki_url = "https://github.com/VolcanicArts/VRCOSC/wiki/VRCOSC-Router";

    [Resolved]
    private GameHost host { get; set; } = null!;

    protected override string Title => "Router";
    protected override string SubTitle => "Define port routing to route other programs through VRCOSC to VRChat and vice versa";

    protected override Drawable CreateRightShoulder() => new Container
    {
        Anchor = Anchor.Centre,
        Origin = Anchor.Centre,
        RelativeSizeAxes = Axes.Both,
        FillMode = FillMode.Fit,
        Padding = new MarginPadding(5),
        Child = UIPrefabs.QuestionButton.With(d =>
        {
            d.Action = () => host.OpenUrlExternally(vrcosc_router_wiki_url);
        })
    };
}
