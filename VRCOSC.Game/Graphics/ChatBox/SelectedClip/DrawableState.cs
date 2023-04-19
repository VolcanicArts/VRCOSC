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
            if (stateMetadata.Name != "Default") stateNameList += ":" + stateMetadata.Name;
            stateNameList += " + ";
        });

        stateNameList = stateNameList.TrimEnd(' ', '+');

        Children = new Drawable[]
        {
            new Box
            {
                Colour = ThemeManager.Current[ThemeAttribute.Light],
                RelativeSizeAxes = Axes.Both
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(5),
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Relative, 0.25f),
                        new Dimension()
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                RelativeSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(5, 0),
                                Children = new Drawable[]
                                {
                                    new Container
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
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
                                    new SpriteText
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Font = FrameworkFont.Regular.With(size: 20),
                                        Text = stateNameList
                                    }
                                }
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Child = new VRCOSCTextBox
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Current = state.Format,
                                    Masking = true,
                                    CornerRadius = 5
                                }
                            }
                        }
                    }
                }
            }
        };
    }
}
