// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Graphics.ChatBox.SelectedClip;
using VRCOSC.Game.Graphics.ChatBox.Timeline;
using VRCOSC.Game.Graphics.ChatBox.Timeline.Menu.Clip;
using VRCOSC.Game.Graphics.ChatBox.Timeline.Menu.Layer;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Button;
using osu.Framework.Platform;
using VRCOSC.Game.Managers;
using VRCOSC.Game.Processes;

namespace VRCOSC.Game.Graphics.ChatBox;

[Cached]
public partial class ChatBoxScreen : Container
{
    [Cached]
    private TimelineLayerMenu layerMenu = new();

    [Cached]
    private TimelineClipMenu clipMenu = new();

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;

        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = ThemeManager.Current[ThemeAttribute.Light]
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(10),
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 5),
                        new Dimension(GridSizeMode.Absolute, 40),
                        new Dimension(GridSizeMode.Absolute, 5),
                        new Dimension()
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new SelectedClipEditorWrapper
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.Both
                            }
                        },
                        null,
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        RelativeSizeAxes = Axes.Y,
                                        AutoSizeAxes = Axes.X,
                                        Spacing = new Vector2(5, 0),
                                        Direction = FillDirection.Horizontal,
                                        Children = new Drawable[]
                                        {
                                            new ImportButton(),
                                            new ExportButton()
                                        }
                                    },
                                    new TimelineLengthContainer
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        RelativeSizeAxes = Axes.Y,
                                        Width = 300,
                                    }
                                }
                            }
                        },
                        null,
                        new Drawable[]
                        {
                            new TimelineWrapper
                            {
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                RelativeSizeAxes = Axes.Both
                            }
                        }
                    }
                }
            },
            layerMenu,
            clipMenu
        };
    }

    private partial class ImportButton : TextButton
    {
        [Resolved]
        private ChatBoxManager chatBoxManager { get; set; } = null!;

        public ImportButton()
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;
            Size = new Vector2(110, 30);
            Text = "Import Config";
            FontSize = 18;
            CornerRadius = 5;
            BackgroundColour = ThemeManager.Current[ThemeAttribute.Action];
            BorderThickness = 2;
        }

        protected override void LoadComplete()
        {
            Action += () => WinForms.OpenFileDialog(@"chatbox.json|*.json", fileName => Schedule(() => chatBoxManager.Import(fileName)));
        }
    }

    private partial class ExportButton : TextButton
    {
        [Resolved]
        private GameHost host { get; set; } = null!;

        [Resolved]
        private Storage storage { get; set; } = null!;

        public ExportButton()
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;
            Size = new Vector2(110, 30);
            Text = "Export Config";
            FontSize = 18;
            CornerRadius = 5;
            BackgroundColour = ThemeManager.Current[ThemeAttribute.Action];
            BorderThickness = 2;
        }

        protected override void LoadComplete()
        {
            Action += () => host.PresentFileExternally(storage.GetFullPath(@"chatbox.json"));
        }
    }
}
