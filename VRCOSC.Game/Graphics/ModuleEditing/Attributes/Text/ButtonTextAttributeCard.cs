// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing.Attributes.Text;

public sealed class ButtonTextAttributeCard : TextAttributeCard
{
    private readonly ModuleAttributeSingleWithButton attributeSingleWithButton;

    public ButtonTextAttributeCard(ModuleAttributeSingleWithButton attributeData)
        : base(attributeData)
    {
        attributeSingleWithButton = attributeData;
    }

    protected override Drawable CreateContent()
    {
        return new GridContainer
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 40,
            ColumnDimensions = new[]
            {
                new Dimension(GridSizeMode.Relative, 0.75f),
                new Dimension()
            },
            Content = new[]
            {
                new[]
                {
                    base.CreateContent(),
                    new TextButton
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.9f),
                        Text = attributeSingleWithButton.ButtonText,
                        Masking = true,
                        CornerRadius = 5,
                        Action = attributeSingleWithButton.ButtonAction,
                        BackgroundColour = VRCOSCColour.Blue
                    }
                }
            }
        };
    }
}
