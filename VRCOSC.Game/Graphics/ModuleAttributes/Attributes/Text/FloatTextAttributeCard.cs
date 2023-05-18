// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Text;
using VRCOSC.Game.Modules.Attributes;

namespace VRCOSC.Game.Graphics.ModuleAttributes.Attributes.Text;

public partial class FloatTextAttributeCard : AttributeCard<ModuleFloatAttribute>
{
    public FloatTextAttributeCard(ModuleFloatAttribute attributeData)
        : base(attributeData)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new FloatTextBox
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 30,
            Masking = true,
            CornerRadius = 5,
            BorderColour = ThemeManager.Current[ThemeAttribute.Border],
            BorderThickness = 2,
            Text = AttributeData.Attribute.Value.ToString(CultureInfo.CurrentCulture),
            ValidCurrent = AttributeData.Attribute.GetBoundCopy()
        });
    }
}
