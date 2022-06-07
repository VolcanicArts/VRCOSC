﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osuTK;
using osuTK.Input;
using VRCOSC.Game.Graphics.Containers.UI;

namespace VRCOSC.Game.Graphics.Containers.Screens.TerminalScreen;

public sealed class TerminalContainer : Container
{
    [Resolved]
    private ScreenManager ScreenManager { get; set; }

    private VRCOSCScrollContainer<TerminalEntry> terminalScroll;

    public TerminalContainer()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
        Padding = new MarginPadding(40);
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Child = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            CornerRadius = 20,
            EdgeEffect = VRCOSCEdgeEffects.DispersedShadow,
            BorderThickness = 3,
            Masking = true,
            Children = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = VRCOSCColour.Gray3
                },
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(15),
                    Child = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        BorderThickness = 3,
                        Masking = true,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = VRCOSCColour.Gray2,
                            },
                            new Container
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                Size = new Vector2(80),
                                Padding = new MarginPadding(10),
                                Depth = float.MinValue,
                                Child = new IconButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Icon = { Value = FontAwesome.Solid.Get(0xf00d) },
                                    Action = ScreenManager.HideTerminal
                                },
                            },
                            new Container
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding(1.5f),
                                Child = terminalScroll = new VRCOSCScrollContainer<TerminalEntry>
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    ContentSpacing = new Vector2(0),
                                    ContentPadding = new MarginPadding(0),
                                    Padding = new MarginPadding
                                    {
                                        Horizontal = 3
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        Logger.NewEntry += logEntry =>
        {
            if (logEntry.LoggerName == "terminal")
                log(logEntry.Message);
        };
    }

    protected override void UpdateAfterChildren()
    {
        base.UpdateAfterChildren();

        while (terminalScroll.ScrollContent.Count > 50) terminalScroll.ScrollContent[0].RemoveAndDisposeImmediately();
        terminalScroll.ScrollToEnd();
    }

    private void log(string text)
    {
        Scheduler.Add(() =>
        {
            var formattedText = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {text}";
            terminalScroll.Add(new TerminalEntry(formattedText));
        });
    }

    public void ClearTerminal()
    {
        terminalScroll.Clear(true);
    }

    protected override bool OnKeyDown(KeyDownEvent e)
    {
        if (e.Key != Key.Escape) return base.OnKeyDown(e);

        ScreenManager.HideTerminal();
        return true;
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        return true;
    }

    protected override bool OnClick(ClickEvent e)
    {
        return true;
    }

    protected override bool OnHover(HoverEvent e)
    {
        return true;
    }
}
