// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Text;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing.Attributes.Text;

public partial class TextAttributeCardList : AttributeCardList
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

    protected override Bindable<object> GetDefaultItem()
    {
        var value = AttributeData.AttributeList.LastOrDefault()?.Value ?? string.Empty;
        return new Bindable<object>(value);
    }

    private void addTextBox(Bindable<object> item)
    {
        var textBox = new StringTextBox
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 40,
            Masking = true,
            CornerRadius = 5,
            BorderColour = ThemeManager.Current[ThemeAttribute.Border],
            BorderThickness = 2,
            Text = item.Value.ToString()
        };
        textBox.OnValidEntry += e => item.Value = e;

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
}
