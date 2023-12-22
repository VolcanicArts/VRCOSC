// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using VRCOSC.Graphics.UI;
using VRCOSC.Screens.Main.Modules.Settings;
using VRCOSC.SDK.Attributes.Settings;

namespace VRCOSC.SDK.Graphics.Settings.Values;

public partial class DrawableEnumModuleSetting<T> : DrawableModuleSetting<EnumModuleSetting<T>> where T : Enum
{
    public DrawableEnumModuleSetting(EnumModuleSetting<T> moduleSetting)
        : base(moduleSetting)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new VRCOSCDropdown<T>
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Items = Enum.GetValues(typeof(T)).Cast<T>(),
            Current = ModuleSetting.Attribute.GetBoundCopy()
        });
    }
}
