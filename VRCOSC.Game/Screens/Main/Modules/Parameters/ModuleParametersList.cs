// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.UI.List;

namespace VRCOSC.Game.Screens.Main.Modules.Parameters;

public partial class ModuleParametersList : HeightLimitedScrollableList<ModuleParameterInstance>
{
    protected override Colour4 BackgroundColourEven => Colours.GRAY4;
    protected override Colour4 BackgroundColourOdd => Colours.GRAY2;

    protected override Drawable CreateHeader() => new ModuleParametersListHeader();
}
