// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Modules.Modules.Random;

public sealed class RandomBoolModule : RandomModule<bool>
{
    protected override bool GetRandomValue()
    {
        return ModuleMaths.RandomBool();
    }
}
