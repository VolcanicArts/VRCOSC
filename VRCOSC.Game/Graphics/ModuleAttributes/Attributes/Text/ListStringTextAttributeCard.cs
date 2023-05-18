// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Text;
using VRCOSC.Game.Modules.Attributes;

namespace VRCOSC.Game.Graphics.ModuleAttributes.Attributes.Text;

public partial class ListStringTextAttributeCard : AttributeCardList<Bindable<string>>
{
    public ListStringTextAttributeCard(ModuleAttributeList<Bindable<string>> attributeData)
        : base(attributeData)
    {
    }

    protected override void OnInstanceAdd(Bindable<string> instance)
    {
        AddToFlow(new StringTextBox
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 30,
            Masking = true,
            CornerRadius = 5,
            BorderColour = ThemeManager.Current[ThemeAttribute.Border],
            BorderThickness = 2,
            Text = instance.Value,
            ValidCurrent = instance.GetBoundCopy()
        });
    }

    protected override Bindable<string> CreateInstance() => new(string.Empty);
}
