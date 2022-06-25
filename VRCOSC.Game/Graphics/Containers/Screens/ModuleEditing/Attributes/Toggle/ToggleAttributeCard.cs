// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.Containers.UI.Dynamic;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing.Attributes.Toggle;

public class ToggleAttributeCard : AttributeCard
{
    protected StatefulIconButton IconButton;

    public ToggleAttributeCard(ModuleAttributeData attributeData)
        : base(attributeData)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new Container
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            RelativeSizeAxes = Axes.Both,
            FillMode = FillMode.Fit,
            Padding = new MarginPadding(20),
            Child = IconButton = new StatefulIconButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                CornerRadius = 10,
                State = { Value = (bool)AttributeData.Attribute.Value }
            }
        });

        AttributeData.Attribute.ValueChanged += updateValues;
        IconButton.State.ValueChanged += e => updateValues(OnToggleChange(e));
    }

    private void updateValues(ValueChangedEvent<object> e)
    {
        updateValues((bool)e.NewValue);
    }

    private void updateValues(bool value)
    {
        AttributeData.Attribute.Value = value;
        IconButton.State.Value = value;
    }

    protected virtual bool OnToggleChange(ValueChangedEvent<bool> e)
    {
        return e.NewValue;
    }

    protected override void Dispose(bool isDisposing)
    {
        AttributeData.Attribute.ValueChanged -= updateValues;
        base.Dispose(isDisposing);
    }
}
