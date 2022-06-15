// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI.TextBox;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing.Parameters;

public class ParameterCard : AttributeCard
{
    public ParameterCard(ModuleAttributeData attributeData)
        : base(attributeData)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        VRCOSCTextBox textBox;

        Add(new Container
        {
            Anchor = Anchor.BottomCentre,
            Origin = Anchor.BottomCentre,
            RelativeSizeAxes = Axes.Both,
            Size = new Vector2(1.0f, 0.5f),
            Padding = new MarginPadding(5),
            Child = textBox = new VRCOSCTextBox
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                BorderThickness = 3,
                Text = (string)attributeData.Attribute.Value
            }
        });

        textBox.OnCommit += (_, _) => attributeData.Attribute.Value = textBox.Text;

        ResetToDefault.Action += () => textBox.Text = (string)attributeData.Attribute.Default;
    }
}
