// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Specialized;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Modules.Attributes;

namespace VRCOSC.Game.Graphics.ModuleAttributes.Attributes;

public abstract partial class AttributeCardList<TAttribute, TInstance> : AttributeCard<TAttribute> where TAttribute : ModuleAttributeList<TInstance>
{
    private FillFlowContainer contentFlow = null!;
    private FillFlowContainer listFlow = null!;

    protected AttributeCardList(TAttribute attributeData)
        : base(attributeData)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Content.Add(contentFlow = new FillFlowContainer
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Spacing = new Vector2(0, 5),
            AutoSizeEasing = Easing.OutQuint,
            AutoSizeDuration = 150,
            LayoutEasing = Easing.OutQuint,
            LayoutDuration = 150,
            Child = listFlow = new FillFlowContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(0, 5),
                AutoSizeEasing = Easing.OutQuint,
                AutoSizeDuration = 150,
                LayoutEasing = Easing.OutQuint,
                LayoutDuration = 150
            }
        });

        contentFlow.SetLayoutPosition(listFlow, float.MaxValue);
    }

    protected override void LoadComplete()
    {
        AttributeData.Attribute.CollectionChanged += attributeOnCollectionChanged;
        addAdditionIcon();
        AttributeData.Attribute.ForEach(OnInstanceAdd);
    }

    private void attributeOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (TInstance newInstance in e.NewItems)
            {
                OnInstanceAdd(newInstance);
            }
        }

        OnCollectionChanged(e);
    }

    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e) { }

    protected override void OnSetDefault()
    {
        listFlow.Clear();
        AttributeData.Attribute.ForEach(OnInstanceAdd);
    }

    private void addInstance()
    {
        AttributeData.Attribute.Add(CreateInstance());
    }

    protected void AddToContent(Drawable drawable, float depth)
    {
        contentFlow.Add(drawable);
        contentFlow.SetLayoutPosition(drawable, depth);
    }

    protected void AddToList(Drawable drawable)
    {
        GridContainer gridInstance;

        var position = listFlow.Count > 0 ? listFlow[^1].Position : Vector2.Zero;

        listFlow.Add(gridInstance = new GridContainer
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Position = position,
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
                drawable,
                null,
                new IconButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                    Scale = new Vector2(0.9f),
                    CornerRadius = 5,
                    BackgroundColour = ThemeManager.Current[ThemeAttribute.Failure],
                    Icon = FontAwesome.Solid.Get(0xf00d),
                    IconPadding = 4,
                    IconShadow = true,
                    Action = () =>
                    {
                        AttributeData.Attribute.RemoveAt(listFlow.IndexOf(gridInstance));
                        gridInstance.RemoveAndDisposeImmediately();
                    }
                }
            }
        };
    }

    private void addAdditionIcon()
    {
        var iconWrapper = new Container
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 30,
            Width = 0.8f,
            Child = new IconButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                Icon = FontAwesome.Solid.Plus,
                BackgroundColour = ThemeManager.Current[ThemeAttribute.Success],
                Circular = true,
                IconShadow = true,
                IconPadding = 6,
                Action = addInstance
            }
        };

        contentFlow.Add(iconWrapper);
        contentFlow.SetLayoutPosition(iconWrapper, float.MaxValue);
    }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);
        AttributeData.Attribute.CollectionChanged -= attributeOnCollectionChanged;
    }

    protected abstract void OnInstanceAdd(TInstance instance);
    protected abstract TInstance CreateInstance();
}
