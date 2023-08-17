// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osuTK;
using VRCOSC.Game.Graphics.Screen;
using VRCOSC.Game.Graphics.UI.Button;

namespace VRCOSC.Game.Graphics.About;

public sealed partial class AboutScreen : BaseScreen
{
    [Resolved]
    private GameHost host { get; set; } = null!;

    private FillFlowContainer buttonFlow = null!;

    protected override BaseHeader CreateHeader() => new AboutHeader();

    protected override Drawable CreateBody() => new Container
    {
        RelativeSizeAxes = Axes.Both,
        Padding = new MarginPadding(10),
        Children = new Drawable[]
        {
            buttonFlow = new FillFlowContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.5f, 0.9f),
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(5)
            }
        }
    };

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;

        buttonFlow.AddRange(new Drawable[]
        {
            new FillFlowContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(5, 0),
                Children = new Drawable[]
                {
                    new AboutButton
                    {
                        Icon = FontAwesome.Brands.Github,
                        BackgroundColour = Colour4.FromHex("272b33"),
                        Action = () => host.OpenUrlExternally("https://github.com/VolcanicArts/VRCOSC")
                    },
                    new AboutButton
                    {
                        Icon = FontAwesome.Brands.Discord,
                        BackgroundColour = Colour4.FromHex("7289DA"),
                        Action = () => host.OpenUrlExternally("https://discord.gg/vj4brHyvT5")
                    },
                    new AboutButton
                    {
                        Icon = FontAwesome.Solid.Coffee,
                        BackgroundColour = Colour4.FromHex("ff5f5f"),
                        Action = () => host.OpenUrlExternally("https://ko-fi.com/volcanicarts")
                    }
                }
            }
        });
    }

    private sealed partial class AboutButton : Container
    {
        public IconUsage Icon { get; init; }
        public Colour4 BackgroundColour { get; init; }
        public Action? Action { get; init; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            Size = new Vector2(100);

            Child = new IconButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.9f),
                CornerRadius = 10,
                Icon = Icon,
                BackgroundColour = BackgroundColour,
                Action = Action
            };
        }
    }
}
