// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing.Attributes.Text;

public class TextAttributeCardList : AttributeCardList
{
    public TextAttributeCardList(ModuleAttributeList attributeData)
        : base(attributeData)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        AttributeData.AttributeList.ForEach(addTextBox);
    }

    private void addTextBox(Bindable<object> item)
    {
        var textBox = new VRCOSCTextBox
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 40,
            Masking = true,
            CornerRadius = 5,
            Text = item.Value.ToString()
        };

        textBox.Current.ValueChanged += e => item.Value = OnTextWrite(e);

        AddContent(textBox);
    }

    protected override void SetDefault()
    {
        base.SetDefault();
        ContentFlow.Clear();
        AttributeData.AttributeList.ForEach(addTextBox);
    }

    protected override void AddItem(Bindable<object> item)
    {
        base.AddItem(item);
        addTextBox(item);
    }

    protected override void RemoveItem(int index)
    {
        base.RemoveItem(index);
        ContentFlow.Children[index].RemoveAndDisposeImmediately();
    }

    protected virtual object OnTextWrite(ValueChangedEvent<string> e)
    {
        return e.NewValue;
    }
}
