// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing.Attributes.Text;

public partial class TextAttributeCard : AttributeCardSingle
{
    protected VRCOSCTextBox TextBox = null!;

    public TextAttributeCard(ModuleAttributeSingle attributeData)
        : base(attributeData)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        ContentFlow.Add(CreateContent());
    }

    protected virtual Drawable CreateContent()
    {
        return TextBox = CreateTextBox().With(t => t.Text = AttributeData.Attribute.Value.ToString());
    }

    protected override void SetDefault()
    {
        base.SetDefault();
        TextBox.Current.Value = AttributeData.Attribute.Value.ToString();
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        TextBox.Current.ValueChanged += OnTextBoxUpdate;
    }

    protected virtual void OnTextBoxUpdate(ValueChangedEvent<string> e) => UpdateAttribute(e.NewValue);
}
