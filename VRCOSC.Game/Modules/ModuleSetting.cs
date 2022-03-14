using osu.Framework.Bindables;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace VRCOSC.Game.Modules;

public class ModuleSetting
{
    public string Reference { get; }
    public string DisplayName { get; }
    public string Description { get; }

    public ModuleSetting(string reference, string displayName, string description)
    {
        Reference = reference;
        DisplayName = displayName;
        Description = description;
    }
}

public class ModuleSettingString : ModuleSetting
{
    public Bindable<string> Value { get; }

    public ModuleSettingString(string reference, string displayName, string description, string startingValue = "")
        : base(reference, displayName, description)
    {
        Value = new Bindable<string>(startingValue);
    }
}

public class ModuleSettingInt : ModuleSetting
{
    public BindableInt Value { get; }

    public ModuleSettingInt(string reference, string displayName, string description, int startingValue = 0)
        : base(reference, displayName, description)
    {
        Value = new BindableInt(startingValue);
    }
}

public class ModuleSettingBool : ModuleSetting
{
    public BindableBool Value { get; }

    public ModuleSettingBool(string reference, string displayName, string description, bool startingValue = false)
        : base(reference, displayName, description)
    {
        Value = new BindableBool(startingValue);
    }
}
