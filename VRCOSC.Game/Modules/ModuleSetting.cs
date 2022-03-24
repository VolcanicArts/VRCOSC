// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.Game.Modules;

public abstract class ModuleSetting
{
    public string DisplayName { get; set; }
    public string Description { get; set; }
}

public class StringModuleSetting : ModuleSetting
{
    public string Value { get; set; }
}

public class IntModuleSetting : ModuleSetting
{
    public int Value { get; set; }
}

public class BoolModuleSetting : ModuleSetting
{
    public bool Value { get; set; }
}

public class EnumModuleSetting : ModuleSetting
{
    public Enum Value { get; set; }
}
