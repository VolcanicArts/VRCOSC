// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Extensions.IEnumerableExtensions;
using VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing.Settings;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing;

public class SettingsFlow : AttributeFlow
{
    protected override string Title => "Settings";

    protected override void GenerateCards(Module source)
    {
        source.Settings.Values.ForEach(attributeData =>
        {
            switch (Type.GetTypeCode(attributeData.Attribute.Value.GetType()))
            {
                case TypeCode.String:
                    AddAttributeCard(new StringAttributeCard(attributeData));
                    break;

                case TypeCode.Int32:
                    AddAttributeCard(new IntAttributeCard(attributeData));
                    break;

                case TypeCode.Boolean:
                    AddAttributeCard(new BoolAttributeCard(attributeData));
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        });
    }
}
