// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Bindables;
using VRCOSC.Game.OSC.VRChat;

namespace VRCOSC.Game.Modules;

public class ModuleAttribute
{
    public readonly ModuleAttributeMetadata Metadata;
    public readonly Bindable<object> Attribute;
    public readonly Type Type;
    private readonly Func<bool>? dependsOn;

    public ModuleAttribute(ModuleAttributeMetadata metadata, object defaultValue, Func<bool>? dependsOn)
    {
        Metadata = metadata;
        Type = defaultValue.GetType();

        Attribute = new Bindable<object>
        {
            Value = defaultValue,
            Default = defaultValue
        };

        this.dependsOn = dependsOn;
    }

    public bool Enabled => dependsOn?.Invoke() ?? true;
}

public sealed class ParameterAttribute : ModuleAttribute
{
    public readonly ParameterMode Mode;
    public readonly Type ExpectedType;

    public string Name => (string)Attribute.Value;
    public string FormattedAddress => $"{VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}/{Attribute.Value}";

    public ParameterAttribute(ParameterMode mode, ModuleAttributeMetadata metadata, string defaultName, Type expectedType, Func<bool>? dependsOn)
        : base(metadata, defaultName, dependsOn)
    {
        Mode = mode;
        ExpectedType = expectedType;
    }
}

public sealed class ModuleAttributeWithButton : ModuleAttribute
{
    public readonly Action ButtonAction;
    public readonly string ButtonText;

    public ModuleAttributeWithButton(ModuleAttributeMetadata metadata, object defaultValue, string buttonText, Action buttonAction, Func<bool>? dependsOn)
        : base(metadata, defaultValue, dependsOn)
    {
        ButtonText = buttonText;
        ButtonAction = buttonAction;
    }
}

public sealed class ModuleAttributeWithBounds : ModuleAttribute
{
    public readonly object MinValue;
    public readonly object MaxValue;

    public ModuleAttributeWithBounds(ModuleAttributeMetadata metadata, object defaultValue, object minValue, object maxValue, Func<bool>? dependsOn)
        : base(metadata, defaultValue, dependsOn)
    {
        MinValue = minValue;
        MaxValue = maxValue;
    }
}

public sealed class ModuleAttributeMetadata
{
    public readonly string DisplayName;
    public readonly string Description;

    public ModuleAttributeMetadata(string displayName, string description)
    {
        DisplayName = displayName;
        Description = description;
    }
}
