// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Graphics;
using VRCOSC.Graphics.UI;
using VRCOSC.Screens.Main.Modules.Settings;
using VRCOSC.SDK.Attributes.Settings;

namespace VRCOSC.SDK.Graphics.Settings.Lists;

public abstract partial class DrawableListModuleSetting<TSetting, TItem> : DrawableModuleSetting<TSetting> where TSetting : ListModuleSetting<TItem>
{
    protected virtual Drawable? Header => null;

    protected DrawableListModuleSetting(TSetting moduleSetting)
        : base(moduleSetting)
    {
        FillFlowContainer flowWrapper;

        Child = flowWrapper = new FillFlowContainer
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(0, 5)
        };

        var header = Header;

        if (header is not null)
        {
            flowWrapper.Add(new Container
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding
                {
                    Right = 35
                },
                Child = header
            });
        }

        FillFlowContainer listContentFlow;

        flowWrapper.Add(listContentFlow = new FillFlowContainer
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(0, 5)
        });

        flowWrapper.Add(new Container
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Padding = new MarginPadding
            {
                Right = 35
            },
            Child = new IconButton
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Size = new Vector2(30),
                Icon = FontAwesome.Solid.Plus,
                IconSize = 17,
                Masking = true,
                CornerRadius = 15,
                BackgroundColour = Colours.GREEN0,
                Action = () => ModuleSetting.AddItem()
            }
        });

        ModuleSetting.Attribute.BindCollectionChanged((_, e) =>
        {
            if (e.NewItems is not null)
            {
                if (ModuleSetting.IsDefault())
                    listContentFlow.Clear();

                foreach (TItem newItem in e.NewItems)
                {
                    GridContainer gridInstance;

                    listContentFlow.Add(gridInstance = new GridContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        ColumnDimensions = new[]
                        {
                            new Dimension(),
                            new Dimension(GridSizeMode.Absolute, 5),
                            new Dimension(GridSizeMode.Absolute, 30)
                        },
                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize)
                        }
                    });

                    gridInstance.Content = new[]
                    {
                        new[]
                        {
                            ModuleSetting.GetItemDrawable(newItem),
                            null,
                            new IconButton
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                FillMode = FillMode.Fit,
                                Scale = new Vector2(0.9f),
                                CornerRadius = 5,
                                BackgroundColour = Colours.RED0,
                                Icon = FontAwesome.Solid.Get(0xf00d),
                                IconSize = 17,
                                Action = () =>
                                {
                                    ModuleSetting.Attribute.Remove(newItem);
                                    gridInstance.RemoveAndDisposeImmediately();
                                }
                            }
                        }
                    };
                }
            }
        }, true);
    }
}

public abstract partial class DrawableListModuleSettingItem<T> : Container
{
    protected readonly T Item;

    protected DrawableListModuleSettingItem(T item)
    {
        Item = item;

        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
    }
}
