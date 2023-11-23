// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Specialized;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Graphics.UI.Text;
using VRCOSC.Game.Profiles;

namespace VRCOSC.Game.Screens.Main.Settings.Profiles;

public partial class AvatarLinkContainer : Container
{
    private readonly Profile profile;

    private FillFlowContainer linkFlow = null!;

    public AvatarLinkContainer(Profile profile)
    {
        this.profile = profile;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Masking = true;
        CornerRadius = 5;

        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY0
            },
            new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(0, 5),
                Padding = new MarginPadding(7),
                Children = new Drawable[]
                {
                    new SpriteText
                    {
                        Font = Fonts.BOLD.With(size: 25),
                        Colour = Colours.WHITE2,
                        Text = "Link To Avatars",
                        X = 2
                    },
                    linkFlow = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(0, 5),
                        Direction = FillDirection.Vertical
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Child = new IconButton
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(30),
                            Icon = FontAwesome.Solid.Plus,
                            BackgroundColour = Colours.GREEN0,
                            CornerRadius = 15,
                            Action = () => profile.LinkedAvatars.Add(new Bindable<string>(string.Empty))
                        }
                    }
                }
            }
        };

        profile.LinkedAvatars.BindCollectionChanged(onLinkedAvatarsChange, true);
    }

    private void onLinkedAvatarsChange(object? sender, NotifyCollectionChangedEventArgs e)
    {
        linkFlow.Clear();

        profile.LinkedAvatars.ForEach(linkedAvatarID =>
        {
            linkFlow.Add(new StringTextBox
            {
                RelativeSizeAxes = Axes.X,
                Width = 0.5f,
                Height = 30,
                ValidCurrent = linkedAvatarID.GetBoundCopy(),
                EmptyIsValid = true
            });
        });
    }
}
