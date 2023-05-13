// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Text;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleAttributes.Attributes.Text;

public partial class TextAttributeCard<TTextBox, TType> : AttributeCard where TTextBox : ValidationTextBox<TType>, new()
{
    private TTextBox textBox = null!;

    public TextAttributeCard(ModuleAttribute attributeData)
        : base(attributeData)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(CreateContent());
    }

    protected virtual Drawable CreateContent() => textBox = new TTextBox
    {
        Anchor = Anchor.TopCentre,
        Origin = Anchor.TopCentre,
        RelativeSizeAxes = Axes.X,
        Height = 30,
        Masking = true,
        CornerRadius = 5,
        BorderColour = ThemeManager.Current[ThemeAttribute.Border],
        BorderThickness = 2,
        Text = AttributeData.Attribute.Value.ToString()
    };

    protected override void LoadComplete()
    {
        base.LoadComplete();
        textBox.OnValidEntry += entry => UpdateAttribute(entry!);
    }

    protected override void SetDefault()
    {
        base.SetDefault();
        textBox.Current.Value = AttributeData.Attribute.Value.ToString();
    }
}
