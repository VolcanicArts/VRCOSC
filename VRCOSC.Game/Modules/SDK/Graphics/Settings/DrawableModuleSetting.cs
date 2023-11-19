// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Modules.SDK.Attributes.Settings;

namespace VRCOSC.Game.Modules.SDK.Graphics.Settings;

public abstract partial class DrawableModuleSetting<T> : Container where T : ModuleSetting
{
    protected T ModuleSetting;

    protected override Container<Drawable> Content { get; }

    protected override bool ShouldBeConsideredForInput(Drawable child) => ModuleSetting.IsEnabled.Invoke();

    protected readonly Container SideContainer;

    private readonly FillFlowContainer addonFlow;

    protected DrawableModuleSetting(T moduleSetting)
    {
        ModuleSetting = moduleSetting;

        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Masking = true;
        CornerRadius = 5;
        InternalChildren = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY4
            },
            new FillFlowContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10),
                Padding = new MarginPadding(7),
                Children = new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Children = new Drawable[]
                                {
                                    new SpriteText
                                    {
                                        Font = Fonts.REGULAR.With(size: 25),
                                        Colour = Colours.WHITE0,
                                        Text = ModuleSetting.Metadata.Title
                                    },
                                    new TextFlowContainer(t =>
                                    {
                                        t.Font = Fonts.REGULAR.With(size: 20);
                                        t.Colour = Colours.WHITE2;
                                    })
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Text = ModuleSetting.Metadata.Description
                                    },
                                    Content = new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Margin = new MarginPadding
                                        {
                                            Top = 7
                                        }
                                    }
                                }
                            },
                            SideContainer = new Container
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y
                            }
                        }
                    },
                    addonFlow = new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 5)
                    }
                }
            }
        };
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        addonFlow.Alpha = ModuleSetting.Addons.Any() ? 1 : 0;
        ModuleSetting.Addons.ForEach(moduleSettingAddon => addonFlow.Add(moduleSettingAddon.GetDrawableModuleSettingAddon()));
    }

    protected override void Update()
    {
        this.FadeTo(ModuleSetting.IsEnabled.Invoke() ? 1 : 0.25f, 150, Easing.OutQuart);
    }
}

public abstract partial class DrawableValueModuleSetting<T> : DrawableModuleSetting<T> where T : ModuleSetting
{
    protected DrawableValueModuleSetting(T moduleAttribute)
        : base(moduleAttribute)
    {
    }

    protected internal void AddSide(Drawable drawable) => SideContainer.Add(drawable);
}

public abstract partial class DrawableListModuleSetting<T> : DrawableModuleSetting<ListModuleSetting<T>>
{
    protected virtual Container? Header => null;

    protected DrawableListModuleSetting(ListModuleSetting<T> moduleSetting)
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
        if (header is not null) flowWrapper.Add(header);

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

        flowWrapper.Add(new IconButton
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            Size = new Vector2(30),
            Icon = FontAwesome.Solid.Plus,
            Masking = true,
            CornerRadius = 15,
            BackgroundColour = Colours.GREEN0,
            Action = () => ModuleSetting.AddItem()
        });

        ModuleSetting.Attribute.BindCollectionChanged((_, e) =>
        {
            if (e.NewItems is not null)
            {
                if (ModuleSetting.IsDefault())
                    listContentFlow.Clear();

                foreach (T newItem in e.NewItems)
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
