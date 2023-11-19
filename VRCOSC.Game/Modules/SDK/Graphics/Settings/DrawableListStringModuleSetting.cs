// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules.SDK.Attributes.Settings;

namespace VRCOSC.Game.Modules.SDK.Graphics.Settings;

public partial class DrawableListStringModuleSetting : DrawableListModuleSetting<ListStringModuleSetting>
{
    public DrawableListStringModuleSetting(ListStringModuleSetting moduleSetting)
        : base(moduleSetting)
    {
    }
}
