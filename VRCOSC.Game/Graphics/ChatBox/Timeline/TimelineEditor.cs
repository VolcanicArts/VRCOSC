// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Input;
using VRCOSC.Game.ChatBox;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Graphics.ChatBox.Timeline.Menu.Clip;
using VRCOSC.Game.Graphics.ChatBox.Timeline.Menu.Layer;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline;

[Cached]
public partial class TimelineEditor : Container
{
    [Resolved]
    private Bindable<Clip?> selectedClip { get; set; } = null!;

    [Resolved]
    private ChatBoxManager chatBoxManager { get; set; } = null!;

    [Resolved]
    private TimelineLayerMenu layerMenu { get; set; } = null!;

    [Resolved]
    private TimelineClipMenu clipMenu { get; set; } = null!;

    private RectangularPositionSnapGrid snapping = null!;

    private TimelineLayer layer5 = null!;
    private TimelineLayer layer4 = null!;
    private TimelineLayer layer3 = null!;
    private TimelineLayer layer2 = null!;
    private TimelineLayer layer1 = null!;
    private TimelineLayer layer0 = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new Box
            {
                Colour = ThemeManager.Current[ThemeAttribute.Mid],
                RelativeSizeAxes = Axes.Both
            },
            snapping = new RectangularPositionSnapGrid(Vector2.Zero)
            {
                RelativeSizeAxes = Axes.Both
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(2),
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 2),
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 2),
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 2),
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 2),
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 2),
                        new Dimension(),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            layer5 = new TimelineLayer
                            {
                                Priority = 5
                            }
                        },
                        null,
                        new Drawable[]
                        {
                            layer4 = new TimelineLayer
                            {
                                Priority = 4
                            }
                        },
                        null,
                        new Drawable[]
                        {
                            layer3 = new TimelineLayer
                            {
                                Priority = 3
                            }
                        },
                        null,
                        new Drawable[]
                        {
                            layer2 = new TimelineLayer
                            {
                                Priority = 2
                            }
                        },
                        null,
                        new Drawable[]
                        {
                            layer1 = new TimelineLayer
                            {
                                Priority = 1
                            }
                        },
                        null,
                        new Drawable[]
                        {
                            layer0 = new TimelineLayer
                            {
                                Priority = 0
                            }
                        }
                    }
                }
            }
        };

        chatBoxManager.Clips.BindCollectionChanged((_, e) =>
        {
            if (e.NewItems == null) return;

            foreach (Clip newClip in e.NewItems)
            {
                switch (newClip.Priority.Value)
                {
                    case 0:
                        layer0.Add(newClip);
                        break;

                    case 1:
                        layer1.Add(newClip);
                        break;

                    case 2:
                        layer2.Add(newClip);
                        break;

                    case 3:
                        layer3.Add(newClip);
                        break;

                    case 4:
                        layer4.Add(newClip);
                        break;

                    case 5:
                        layer5.Add(newClip);
                        break;

                    default:
                        throw new InvalidProgramException("Invalid priority");
                }

                newClip.Priority.BindValueChanged(e =>
                {
                    getLayer(e.OldValue).Remove(newClip);
                    getLayer(e.NewValue).Add(newClip);
                });
            }
        }, true);
    }

    private TimelineLayer getLayer(int priority)
    {
        return priority switch
        {
            0 => layer0,
            1 => layer1,
            2 => layer2,
            3 => layer3,
            4 => layer4,
            5 => layer5,
            _ => throw new InvalidOperationException($"No layer with priority {priority}")
        };
    }

    public void ShowLayerMenu(MouseDownEvent e, int xPos, TimelineLayer layer)
    {
        clipMenu.Hide();
        layerMenu.Hide();
        layerMenu.SetPosition(e);
        layerMenu.XPos = xPos;
        layerMenu.Layer = layer;
        layerMenu.Show();
    }

    public void HideClipMenu()
    {
        clipMenu.Hide();
    }

    public void ShowClipMenu(Clip clip, MouseDownEvent e)
    {
        layerMenu.Hide();
        clipMenu.Hide();
        clipMenu.SetClip(clip);
        clipMenu.SetPosition(e);
        clipMenu.Show();
    }

    protected override void LoadComplete()
    {
        chatBoxManager.TimelineLength.BindValueChanged(e => snapping.Spacing = new Vector2(DrawWidth / (float)e.NewValue.TotalSeconds, DrawHeight / 6), true);
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if (e.Button == MouseButton.Left)
        {
            selectedClip.Value = null;
            clipMenu.Hide();
            layerMenu.Hide();
        }

        return true;
    }
}
