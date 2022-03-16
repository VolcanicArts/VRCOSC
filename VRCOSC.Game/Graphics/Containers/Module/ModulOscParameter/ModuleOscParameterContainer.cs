// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI.TextBox;
using VRCOSC.Game.Graphics.Drawables.Triangles;

namespace VRCOSC.Game.Graphics.Containers.Module.ModulOscParameter;

public class ModuleOscParameterContainer : Container
{
    public string Key { get; init; }
    public Modules.Module SourceModule { get; init; }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        Height = 100;
        Masking = true;
        CornerRadius = 10;
        BorderThickness = 3;
        EdgeEffect = VRCOSCEdgeEffects.BasicShadow;

        VRCOSCTextBox textBox;
        Children = new Drawable[]
        {
            new TrianglesBackground
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ColourLight = VRCOSCColour.Gray7,
                ColourDark = VRCOSCColour.Gray7.Darken(0.25f),
                Velocity = 0.5f,
                TriangleScale = 3
            },
            new Box
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.5f, 1),
                Colour = ColourInfo.GradientHorizontal(Colour4.Black.Opacity(0.75f), VRCOSCColour.Invisible)
            },
            new Container
            {
                Name = "Content",
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding
                {
                    Vertical = 10,
                    Horizontal = 15,
                },
                Children = new Drawable[]
                {
                    new SpriteText
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        Font = FrameworkFont.Regular.With(size: 30),
                        Text = SourceModule.Metadata.Parameters[Key].DisplayName
                    },
                    new SpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Colour = VRCOSCColour.Gray9,
                        Font = FrameworkFont.Regular.With(size: 20),
                        Text = SourceModule.Metadata.Parameters[Key].Description
                    },
                    textBox = new VRCOSCTextBox
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Both,
                        BorderThickness = 3,
                        Size = new Vector2(0.5f, 0.8f),
                        Text = SourceModule.Data.Parameters[Key]
                    }
                }
            }
        };

        textBox.OnCommit += (_, _) =>
        {
            SourceModule.UpdateParameter(Key, textBox.Text);
        };
    }
}
