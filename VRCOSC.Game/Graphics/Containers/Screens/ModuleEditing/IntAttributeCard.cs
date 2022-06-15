// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing;

public class IntAttributeCard : StringAttributeCard
{
    public IntAttributeCard(ModuleAttributeData attributeData)
        : base(attributeData)
    {
    }

    protected override void OnCommit(string text)
    {
        if (int.TryParse(text, out var newValue))
            AttributeData.Attribute.Value = newValue;
    }
}
