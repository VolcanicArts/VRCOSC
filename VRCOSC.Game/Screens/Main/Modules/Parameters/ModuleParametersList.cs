// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using VRCOSC.Graphics.UI.List;

namespace VRCOSC.Screens.Main.Modules.Parameters;

public partial class ModuleParametersList : HeightLimitedScrollableList<ModuleParameterInstance>
{
    protected override Drawable CreateHeader() => new ModuleParametersListHeader();
}
