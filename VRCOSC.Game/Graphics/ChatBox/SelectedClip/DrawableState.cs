// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip;

public partial class DrawableState : Container
{
    [Resolved]
    private ChatBoxManager chatBoxManager { get; set; } = null!;

    [Resolved]
    private GameManager gameManager { get; set; } = null!;

    public readonly ClipState ClipState;

    public DrawableState(ClipState clipState)
    {
        ClipState = clipState;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        var stateNameList = string.Empty;

        ClipState.States.ForEach(pair =>
        {
            var stateMetadata = chatBoxManager.StateMetadata[pair.Item1][pair.Item2];
            stateNameList += gameManager.ModuleManager.GetModuleName(pair.Item1);
            if (stateMetadata.Name != "Default") stateNameList += " - " + stateMetadata.Name;
            stateNameList += " & ";
        });

        stateNameList = stateNameList.TrimEnd(' ', '&');
        stateNameList += ":";

        Children = new Drawable[]
        {
            new Box
            {
                Colour = ThemeManager.Current[ThemeAttribute.Light],
                RelativeSizeAxes = Axes.Both
            },
            new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Padding = new MarginPadding(3),
                Spacing = new Vector2(0, 2),
                Children = new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = 25,
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                RelativeSizeAxes = Axes.Both,
                                FillMode = FillMode.Fit,
                                Child = new ToggleButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    State = ClipState.Enabled.GetBoundCopy()
                                }
                            },
                            new Container
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding(3),
                                Child = new SpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Font = FrameworkFont.Regular.With(size: 18),
                                    Text = stateNameList,
                                    Colour = ThemeManager.Current[ThemeAttribute.Text]
                                }
                            }
                        }
                    },
                    new VRCOSCTextBox
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = 25,
                        Current = ClipState.Format.GetBoundCopy(),
                        Masking = true,
                        CornerRadius = 5
                    }
                }
            }
        };
    }
}
