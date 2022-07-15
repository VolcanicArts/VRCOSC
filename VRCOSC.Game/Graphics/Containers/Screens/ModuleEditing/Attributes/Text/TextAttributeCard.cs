// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.Containers.UI;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing.Attributes.Text;

public class TextAttributeCard : AttributeCard
{
    protected VRCOSCTextBox TextBox = null!;

    public TextAttributeCard(ModuleAttributeData attributeData)
        : base(attributeData)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        AddToFlow(TextBox = new VRCOSCTextBox
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 40,
            Text = AttributeData.Attribute.Value.ToString()
        });

        AttributeData.Attribute.ValueChanged += e => updateValues(e.NewValue);
        TextBox.Current.ValueChanged += e => updateValues(OnTextWrite(e));
    }

    private void updateValues(object value)
    {
        AttributeData.Attribute.Value = value;
        TextBox.Current.Value = value.ToString();
    }

    protected virtual object OnTextWrite(ValueChangedEvent<string> e)
    {
        return e.NewValue;
    }

    protected override void Dispose(bool isDisposing)
    {
        AttributeData.Attribute.ValueChanged -= updateValues;
        base.Dispose(isDisposing);
    }
}
