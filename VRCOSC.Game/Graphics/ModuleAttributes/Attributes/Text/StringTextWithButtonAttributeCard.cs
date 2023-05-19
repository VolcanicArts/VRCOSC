// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Graphics.UI.Text;
using VRCOSC.Game.Modules.Attributes;

namespace VRCOSC.Game.Graphics.ModuleAttributes.Attributes.Text;

public partial class StringTextWithButtonAttributeCard : AttributeCard<ModuleStringWithButtonAttribute>
{
    public StringTextWithButtonAttributeCard(ModuleStringWithButtonAttribute attributeData)
        : base(attributeData)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new StringTextBox
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 30,
            Masking = true,
            CornerRadius = 5,
            BorderColour = ThemeManager.Current[ThemeAttribute.Border],
            BorderThickness = 2,
            Text = AttributeData.Attribute.Value,
            ValidCurrent = AttributeData.Attribute.GetBoundCopy()
        });

        Add(new Container
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 30,
            Width = 0.75f,
            Child = new TextButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Text = AttributeData.ButtonText,
                Masking = true,
                CornerRadius = 5,
                FontSize = 22,
                Action = AttributeData.ButtonCallback,
                BackgroundColour = ThemeManager.Current[ThemeAttribute.Action]
            }
        });
    }
}
