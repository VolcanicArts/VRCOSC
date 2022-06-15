// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI.TextBox;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing;

public class StringAttributeCard : AttributeCard
{
    private VRCOSCTextBox textBox;

    public StringAttributeCard(ModuleAttributeData attributeData)
        : base(attributeData)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
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
                Text = attributeData.Attribute.Value.ToString()
            }
        });

        attributeData.Attribute.ValueChanged += updateTextBox;

        textBox.OnCommit += (_, _) => OnCommit(textBox.Text!);
    }

    private void updateTextBox(ValueChangedEvent<object> e)
    {
        textBox.Text = e.NewValue.ToString();
    }

    protected virtual void OnCommit(string text)
    {
        attributeData.Attribute.Value = text;
    }

    protected override void Dispose(bool isDisposing)
    {
        attributeData.Attribute.ValueChanged -= updateTextBox;
        base.Dispose(isDisposing);
    }
}
