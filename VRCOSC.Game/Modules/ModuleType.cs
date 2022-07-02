// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Game.Modules;

public enum ModuleType
{
    General = 1 << 0,
    Health = 1 << 1,
    Integrations = 1 << 2,
    Random = 1 << 3
}
