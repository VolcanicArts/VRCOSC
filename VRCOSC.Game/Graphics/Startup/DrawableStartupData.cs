// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Graphics.UI.Text;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Graphics.Startup;

public partial class DrawableStartupData : StartupDataFlowEntry
{
    [Resolved]
    private StartupManager startupManager { get; set; } = null!;

    private readonly Bindable<string> filePath;

    public DrawableStartupData(Bindable<string> filePath)
    {
        this.filePath = filePath;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Masking = true,
                CornerRadius = 10,
                BorderThickness = 2,
                BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = ThemeManager.Current[ThemeAttribute.Darker],
                        RelativeSizeAxes = Axes.Both
                    },
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding(5),
                        Child = new GridContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            ColumnDimensions = new[]
                            {
                                new Dimension(),
                                new Dimension(GridSizeMode.Absolute, 5),
                                new Dimension(GridSizeMode.Absolute, 40)
                            },
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize)
                            },
                            Content = new[]
                            {
                                new Drawable?[]
                                {
                                    new Container
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Child = new StringTextBox
                                        {
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            RelativeSizeAxes = Axes.X,
                                            Height = 35,
                                            Masking = true,
                                            CornerRadius = 5,
                                            BorderThickness = 2,
                                            BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                                            Text = filePath.Value,
                                            OnValidEntry = entryData => filePath.Value = entryData,
                                            PlaceholderText = @"C:\some\file\path.exe"
                                        }
                                    },
                                    null,
                                    new Container
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        RelativeSizeAxes = Axes.Both,
                                        FillMode = FillMode.Fit,
                                        Depth = float.MinValue,
                                        Padding = new MarginPadding(3),
                                        Child = new IconButton
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            RelativeSizeAxes = Axes.Both,
                                            Icon = FontAwesome.Solid.Get(0xf00d),
                                            IconPadding = 6,
                                            Circular = true,
                                            IconShadow = true,
                                            Action = () =>
                                            {
                                                startupManager.FilePaths.Remove(filePath);
                                                this.RemoveAndDisposeImmediately();
                                            }
                                        }
                                    },
                                }
                            }
                        }
                    }
                }
            }
        };
    }
}
