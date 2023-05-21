// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osuTK;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Modules.Attributes;

namespace VRCOSC.Game.Graphics.ModuleAttributes.Attributes.Toggle;

public sealed partial class BoolAttributeCard : AttributeCard<ModuleBoolAttribute>
{
    public BoolAttributeCard(ModuleBoolAttribute attributeData)
        : base(attributeData)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new ToggleButton
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            Size = new Vector2(35),
            CornerRadius = 10,
            BorderThickness = 2,
            ShouldAnimate = false,
            State = AttributeData.Attribute.GetBoundCopy()
        });
    }
}
