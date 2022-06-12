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
    public string Key { get; init; }
    public Module SourceModule { get; init; }

    [BackgroundDependencyLoader]
    private void load()
    {
        Height = 120;

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
                Text = SourceModule.DataManager.GetParameter(Key)
            }
        });

        TextFlow.AddText(SourceModule.DataManager.Parameters[Key].DisplayName, t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 30);
        });
        TextFlow.AddText("\n");
        TextFlow.AddText(SourceModule.DataManager.Parameters[Key].Description, t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 20);
            t.Colour = VRCOSCColour.Gray9;
        });

        textBox.OnCommit += updateParameter;

        ResetToDefault.Action += () =>
        {
            var defaultAddress = SourceModule.GetDefaultOutputParameter(Key);
            updateParameter(defaultAddress);
            textBox.Text = defaultAddress;
        };

        updateParameter(textBox.Text);
    }

    private void updateParameter(string newAddress)
    {
        SourceModule.DataManager.UpdateParameter(Key, newAddress);

        if (!SourceModule.GetDefaultOutputParameter(Key).Equals(SourceModule.DataManager.GetParameter(Key)))
        {
            ResetToDefault.Show();
        }
        else
        {
            ResetToDefault.Hide();
        }
    }
}
