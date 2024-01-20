// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Graphics;
using VRCOSC.Graphics.UI;
using VRCOSC.Graphics.UI.List;
using VRCOSC.Router;

namespace VRCOSC.Screens.Main.Router;

public partial class RouterList : HeightLimitedScrollableList<RouterListInstance>
{
    [Resolved]
    private AppManager appManager { get; set; } = null!;

    protected override Colour4 BackgroundColourOdd => Colours.GRAY5;
    protected override Colour4 BackgroundColourEven => Colours.GRAY2;

    protected override Drawable CreateHeader() => new Container
    {
        Anchor = Anchor.TopCentre,
        Origin = Anchor.TopCentre,
        RelativeSizeAxes = Axes.X,
        Height = 65,
        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY0
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(7),
                Children = new Drawable[]
                {
                    new SpriteText
                    {
                        Text = "Router",
                        Font = Fonts.BOLD,
                        Colour = Colours.WHITE0
                    },
                    new SpriteText
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Text = "Here you can enter an IP and port to forward VRChat’s OSC data to when using multiple applications that do not support OSCQuery",
                        Font = Fonts.REGULAR.With(size: 23),
                        Colour = Colours.WHITE2
                    }
                }
            }
        }
    };

    protected override Drawable CreateFooter() => new Container
    {
        Anchor = Anchor.TopCentre,
        Origin = Anchor.TopCentre,
        RelativeSizeAxes = Axes.X,
        Height = 40,
        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY0
            },
            new IconButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(140, 30),
                Icon = FontAwesome.Solid.Plus,
                IconSize = 20,
                BackgroundColour = Colours.GREEN0,
                CornerRadius = 5,
                Action = () => appManager.RouterManager.Routes.Add(new Route())
            }
        }
    };

    [BackgroundDependencyLoader]
    private void load()
    {
        appManager.RouterManager.Routes.BindCollectionChanged((_, e) =>
        {
            if (e.OldItems is not null)
            {
                foreach (Route oldItem in e.OldItems)
                {
                    RemoveAll(child => child.Route == oldItem, true);
                }
            }

            if (e.NewItems is not null)
            {
                foreach (Route newItem in e.NewItems)
                {
                    Add(new RouterListInstance(newItem));
                }
            }
        }, true);
    }
}
