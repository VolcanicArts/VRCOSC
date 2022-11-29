// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing.Attributes;

public abstract class AttributeCardList : AttributeCard
{
    protected ModuleAttributeList AttributeData;

    protected AttributeCardList(ModuleAttributeList attributeData)
        : base(attributeData)
    {
        AttributeData = attributeData;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        LayoutFlow.Add(new Container
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 30,
            Children = new Drawable[]
            {
                new IconButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.8f,
                    Icon = FontAwesome.Solid.Plus,
                    BackgroundColour = VRCOSCColour.Gray3,
                    CornerRadius = 5,
                    ShouldAnimate = false,
                    Action = () => AddItem(GetDefaultItem())
                }
            }
        });
    }

    protected void AddContent(Drawable content)
    {
        IconButton removeButton;

        var wrapper = new GridContainer
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 40,
            ColumnDimensions = new[]
            {
                new Dimension(),
                new Dimension(GridSizeMode.Absolute, 36),
            },
            Content = new[]
            {
                new Drawable[]
                {
                    content,
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding
                        {
                            Vertical = 4,
                            Left = 4,
                        },
                        Child = removeButton = new IconButton
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Icon = FontAwesome.Solid.Get(0xf00d),
                            CornerRadius = 5,
                            BackgroundColour = VRCOSCColour.Gray4
                        }
                    }
                }
            }
        };

        ContentFlow.Add(wrapper);

        removeButton.Action += () =>
        {
            if (!AttributeData.CanBeEmpty && ContentFlow.Count == 1) return;

            RemoveItem(ContentFlow.IndexOf(wrapper));
        };
    }

    protected override void LoadComplete()
    {
        AttributeData.AttributeList.ForEach(item => item.BindValueChanged(performAttributeUpdate));
        checkForDefault();
    }

    protected abstract Bindable<object> GetDefaultItem();

    private void performAttributeUpdate(ValueChangedEvent<object> e)
    {
        checkForDefault();
    }

    private void checkForDefault()
    {
        UpdateResetToDefault(!AttributeData.IsDefault());
    }

    protected override void SetDefault()
    {
        base.SetDefault();
        AttributeData.AttributeList.ForEach(item => item.BindValueChanged(performAttributeUpdate));
        checkForDefault();
    }

    protected virtual void AddItem(Bindable<object> item)
    {
        AttributeData.AttributeList.Add(item);
        item.BindValueChanged(performAttributeUpdate, true);
    }

    protected virtual void RemoveItem(int index)
    {
        AttributeData.AttributeList.RemoveAt(index);
        checkForDefault();
    }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);
        AttributeData.AttributeList.ForEach(item => item.ValueChanged -= performAttributeUpdate);
    }
}
