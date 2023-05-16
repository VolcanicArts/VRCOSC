// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osuTK;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleAttributes.Attributes.Toggle;

public sealed partial class ToggleAttributeCard : AttributeCard
{
    private ToggleButton toggleButton = null!;

    public ToggleAttributeCard(ModuleAttribute attributeData)
        : base(attributeData)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(toggleButton = new ToggleButton
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            Size = new Vector2(35),
            CornerRadius = 10,
            BorderColour = ThemeManager.Current[ThemeAttribute.Border],
            BorderThickness = 2,
            ShouldAnimate = false,
            State = { Value = (bool)AttributeData.Attribute.Value }
        });
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        toggleButton.State.ValueChanged += e => UpdateAttribute(e.NewValue);
    }

    protected override void SetDefault()
    {
        base.SetDefault();
        toggleButton.State.Value = (bool)AttributeData.Attribute.Value;
    }
}
