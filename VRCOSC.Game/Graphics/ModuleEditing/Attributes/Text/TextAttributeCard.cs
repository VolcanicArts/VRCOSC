// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing.Attributes.Text;

public class TextAttributeCard : AttributeCard
{
    private VRCOSCTextBox textBox = null!;

    public TextAttributeCard(ModuleAttributeData attributeData)
        : base(attributeData)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        ContentFlow.Add(textBox = new VRCOSCTextBox
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 40,
            Masking = true,
            CornerRadius = 5,
            Text = AttributeData.Attribute.Value.ToString()
        });
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        textBox.Current.ValueChanged += e => UpdateValues(OnTextWrite(e));
    }

    protected override void UpdateValues(object value)
    {
        base.UpdateValues(value);
        textBox.Current.Value = value.ToString();
    }

    protected virtual object OnTextWrite(ValueChangedEvent<string> e)
    {
        return e.NewValue;
    }
}
