// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Game.Modules;

public class ModuleAttributeData
{
    public string DisplayName { get; }
    public string Description { get; }
    public object Value { get; set; }
    public object DefaultValue { get; }

    public ModuleAttributeData(string displayName, string description, object defaultValue)
    {
        DisplayName = displayName;
        Description = description;
        Value = defaultValue;
        DefaultValue = defaultValue;
    }

    public bool IsDefault()
    {
        return Value.Equals(DefaultValue);
    }
}
