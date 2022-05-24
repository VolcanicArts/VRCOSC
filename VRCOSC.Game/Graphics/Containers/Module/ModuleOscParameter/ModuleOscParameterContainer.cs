// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI.TextBox;
using VRCOSC.Game.Graphics.Drawables.Triangles;

namespace VRCOSC.Game.Graphics.Containers.Module.ModuleOscParameter;

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
        Height = 120;
        Masking = true;
        CornerRadius = 10;
        BorderThickness = 3;
        EdgeEffect = VRCOSCEdgeEffects.BasicShadow;

        VRCOSCTextBox textBox;
        TextFlowContainer textFlow;

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
                    Vertical = 8,
                    Horizontal = 12,
                },
                Children = new Drawable[]
                {
                    textFlow = new TextFlowContainer
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        AutoSizeAxes = Axes.Both,
                        Padding = new MarginPadding
                        {
                            Horizontal = 5
                        }
                    },
                    textBox = new VRCOSCTextBox
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        RelativeSizeAxes = Axes.Both,
                        BorderThickness = 3,
                        Size = new Vector2(1, 0.45f),
                        Text = SourceModule.DataManager.GetParameter(Key)
                    }
                }
            }
        };

        textFlow.AddText(SourceModule.DataManager.Parameters[Key].DisplayName, t => t.Font = FrameworkFont.Regular.With(size: 30));
        textFlow.AddText("\n");
        textFlow.AddText(SourceModule.DataManager.Parameters[Key].Description, t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 20);
            t.Colour = VRCOSCColour.Gray9;
        });

        textBox.OnCommit += (_, _) =>
        {
            SourceModule.DataManager.UpdateParameter(Key, textBox.Text);
        };
    }
}
