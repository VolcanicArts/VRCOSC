﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.ModuleListing;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing.Attributes.Toggle;

public class ToggleAttributeCard : AttributeCard
{
    protected ToggleButton ToggleButton;

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
            Padding = new MarginPadding(10),
            Child = ToggleButton = new ToggleButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                CornerRadius = 10,
                State = { Value = (bool)AttributeData.Attribute.Value }
            }
        });

        AttributeData.Attribute.ValueChanged += updateValues;
        ToggleButton.State.ValueChanged += e => updateValues(OnToggleChange(e));
    }

    private void updateValues(ValueChangedEvent<object> e)
    {
        updateValues((bool)e.NewValue);
    }

    private void updateValues(bool value)
    {
        AttributeData.Attribute.Value = value;
        ToggleButton.State.Value = value;
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
