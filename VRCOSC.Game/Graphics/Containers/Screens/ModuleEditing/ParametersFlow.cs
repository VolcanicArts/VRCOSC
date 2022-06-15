// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Extensions.IEnumerableExtensions;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing;

public class ParametersFlow : AttributeFlow
{
    protected override string Title => "Output Parameters";

    protected override void GenerateCards(Module source)
    {
        source.OutputParameters.Values.ForEach(attributeData => AddAttributeCard(new StringAttributeCard(attributeData)));
    }
}
