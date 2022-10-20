// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osuTK;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.UI.Button;

namespace VRCOSC.Game.Graphics.About;

public sealed class AboutScreen : Container
{
    [Resolved]
    private GameHost host { get; set; } = null!;

    [Resolved]
    private VRCOSCConfigManager configManager { get; set; } = null!;

    private readonly FillFlowContainer buttonFlow;
    private readonly TextFlowContainer text;

    public AboutScreen()
    {
        RelativeSizeAxes = Axes.Both;

        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray5
            },
            new Container
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
                        Direction = FillDirection.Full,
                        Spacing = new Vector2(5)
                    },
                    text = new TextFlowContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        TextAnchor = Anchor.BottomCentre
                    }
                }
            }
        };
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        buttonFlow.AddRange(new[]
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
            }
        });

        text.AddText($"VRCOSC {configManager.Get<string>(VRCOSCSetting.Version)}");
        text.AddParagraph("Copyright VolcanicArts 2022. See license file in repository root for more information");
    }

    private sealed class AboutButton : Container
    {
        public IconUsage Icon { get; init; }
        public Colour4 BackgroundColour { get; init; }
        public Action Action { get; init; }

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
                Masking = true,
                CornerRadius = 10,
                Icon = Icon,
                BackgroundColour = BackgroundColour,
                Action = Action
            };
        }
    }
}
