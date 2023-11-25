// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using Microsoft.Win32;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Screens.Main.Home;

public partial class HomeTab : Container
{
    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;

        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY1
            },
            new Container
            {
                Name = "Top Bar",
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding
                {
                    Horizontal = 10,
                    Vertical = 5
                },
                Children = new Drawable[]
                {
                    new SpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = $"Welcome {getUserName()}!",
                        Font = FrameworkFont.Regular.With(size: 60, weight: "Bold"),
                        Colour = Colours.WHITE2
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(5, 0),
                        Padding = new MarginPadding
                        {
                            Vertical = 5
                        },
                        Children = new Drawable[]
                        {
                            new LinkButton("https://github.com/VolcanicArts/VRCOSC", FontAwesome.Brands.Github, Colour4.FromHex("272b33").Lighten(0.25f)),
                            new LinkButton("https://discord.gg/vj4brHyvT5", FontAwesome.Brands.Discord, Colour4.FromHex("7289DA")),
                            new LinkButton("https://ko-fi.com/volcanicarts", FontAwesome.Solid.Coffee, Colour4.FromHex("ff5f5f"))
                        }
                    }
                }
            }
        };
    }

    private static string getUserName()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Office\Common\UserInfo");
            if (key is null) return Environment.UserName;

            var userNameValue = key.GetValue("UserName");
            return userNameValue is not null ? userNameValue.ToString()! : Environment.UserName;
        }
        catch (Exception)
        {
            return Environment.UserName;
        }
    }
}
