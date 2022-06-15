// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.Containers.UI.Checkbox;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing.Settings;

public class BoolAttributeCard : AttributeCard
{
    private Checkbox checkBox;

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
            Padding = new MarginPadding(15),
            Child = checkBox = new Checkbox
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                CornerRadius = 10,
                State = { Value = (bool)AttributeData.Attribute.Value }
            }
        });

        checkBox.State.ValueChanged += e => AttributeData.Attribute.Value = e.NewValue;

        AttributeData.Attribute.ValueChanged += updateCheckBox;
    }

    private void updateCheckBox(ValueChangedEvent<object> e)
    {
        checkBox.State.Value = (bool)e.NewValue;
    }

    protected override void Dispose(bool isDisposing)
    {
        AttributeData.Attribute.ValueChanged -= updateCheckBox;
        base.Dispose(isDisposing);
    }
}
