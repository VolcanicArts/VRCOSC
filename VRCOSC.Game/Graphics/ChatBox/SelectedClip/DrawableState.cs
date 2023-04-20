// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.ChatBox;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Graphics.UI.Button;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip;

public partial class DrawableState : Container
{
    [Resolved]
    private ChatBoxManager chatBoxManager { get; set; } = null!;

    private readonly ClipState state;

    public DrawableState(ClipState state)
    {
        this.state = state;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        var stateNameList = string.Empty;

        state.States.ForEach(pair =>
        {
            var (moduleName, lookup) = pair;
            var stateMetadata = chatBoxManager.StateMetadata[moduleName][lookup];
            stateNameList += moduleName.Replace("Module", string.Empty);
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
                        Height = 30,
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
                                    State = state.Enabled
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
                                    Font = FrameworkFont.Regular.With(size: 20),
                                    Text = stateNameList
                                }
                            }
                        }
                    },
                    new Container
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Child = new VRCOSCTextBox
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 30,
                            Current = state.Format,
                            Masking = true,
                            CornerRadius = 5
                        }
                    }
                }
            }
        };
    }
}
