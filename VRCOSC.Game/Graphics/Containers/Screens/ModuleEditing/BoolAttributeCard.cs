// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.Containers.UI.Dynamic;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing;

public class BoolAttributeCard : AttributeCard
{
    private StatefulIconButton iconButton;

    public BoolAttributeCard(ModuleAttributeData attributeData)
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
            Child = iconButton = new StatefulIconButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                CornerRadius = 10,
                State = { Value = (bool)AttributeData.Attribute.Value }
            }
        });

        iconButton.State.ValueChanged += e => AttributeData.Attribute.Value = e.NewValue;

        AttributeData.Attribute.ValueChanged += updateCheckBox;
    }

    private void updateCheckBox(ValueChangedEvent<object> e)
    {
        iconButton.State.Value = (bool)e.NewValue;
    }

    protected override void Dispose(bool isDisposing)
    {
        AttributeData.Attribute.ValueChanged -= updateCheckBox;
        base.Dispose(isDisposing);
    }
}
