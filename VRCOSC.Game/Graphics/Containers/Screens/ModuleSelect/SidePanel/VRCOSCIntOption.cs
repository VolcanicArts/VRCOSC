// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.Containers.UI;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleSelect.SidePanel;

public class VRCOSCIntOption : VRCOSCOption
{
    public Bindable<int> Value { get; init; }

    [BackgroundDependencyLoader]
    private void load()
    {
        VRCOSCTextBox textBox;

        Add(new Container
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            RelativeSizeAxes = Axes.Both,
            Width = 0.5f,
            Padding = new MarginPadding(5),
            Child = textBox = new VRCOSCTextBox
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                CornerRadius = 7,
                Text = Value.Value.ToString()
            },
        });

        textBox.Current.ValueChanged += e =>
        {
            if (string.IsNullOrEmpty(e.NewValue))
            {
                Value.Value = 0;
                textBox.Current.Value = "0";
                return;
            }

            if (int.TryParse(e.NewValue, out var intValue))
            {
                Value.Value = intValue;
                textBox.Current.Value = intValue.ToString();
            }
            else
            {
                textBox.Current.Value = e.OldValue;
            }
        };
    }
}
