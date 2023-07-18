// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using JetBrains.Annotations;

namespace VRCOSC.Game.Modules;

[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
[AttributeUsage(AttributeTargets.Class)]
public class ModuleTitleAttribute : Attribute
{
    public readonly string Title;

    /// <param name="title">The human-readable name of your module</param>
    public ModuleTitleAttribute(string title)
    {
        Title = title;
    }
}

[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
[AttributeUsage(AttributeTargets.Class)]
public class ModuleDescriptionAttribute : Attribute
{
    public readonly string ShortDescription;
    public readonly string LongDescription;

    /// <param name="shortDescription">Used in places where there isn't as much space</param>
    /// <param name="longDescription">Used in places where there is room for a detailed description</param>
    /// <remarks>If <paramref name="longDescription" /> is left blank, <paramref name="shortDescription" /> is used in its place</remarks>
    public ModuleDescriptionAttribute(string shortDescription, string longDescription = "")
    {
        ShortDescription = shortDescription;
        LongDescription = string.IsNullOrEmpty(longDescription) ? shortDescription : longDescription;
    }
}

[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
[AttributeUsage(AttributeTargets.Class)]
public class ModuleAuthorAttribute : Attribute
{
    public readonly string Name;
    public readonly string? Url;
    public readonly string? IconUrl;

    /// <param name="name">Your name</param>
    /// <param name="url">A link to any page you'd like to promote</param>
    /// <param name="iconUrl">A link to a PNG or JPG of your icon</param>
    public ModuleAuthorAttribute(string name, string? url, string? iconUrl)
    {
        Name = name;
        Url = url;
        IconUrl = iconUrl;
    }
}

[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
[AttributeUsage(AttributeTargets.Class)]
public class ModuleGroupAttribute : Attribute
{
    public readonly Module.ModuleType Type;

    /// <param name="type">The group this module belongs to</param>
    public ModuleGroupAttribute(Module.ModuleType type)
    {
        Type = type;
    }
}

[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
[AttributeUsage(AttributeTargets.Class)]
public class ModulePrefabAttribute : Attribute
{
    public readonly string Name;
    public readonly string Url;

    /// <param name="name">The name of the prefab</param>
    /// <param name="url">A download URL for the latest version</param>
    public ModulePrefabAttribute(string name, string url)
    {
        Name = name;
        Url = url;
    }
}

[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ModuleInfoAttribute : Attribute
{
    public readonly string Description;

    public ModuleInfoAttribute(string description)
    {
        Description = description;
    }
}

[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
[AttributeUsage(AttributeTargets.Method)]
public class ModuleUpdateAttribute : Attribute
{
    public readonly ModuleUpdateMode Mode;
    public readonly bool UpdateImmediately;
    public readonly double DeltaMilliseconds;

    /// <param name="mode">The mode this update is defined as</param>
    /// <param name="updateImmediately">Whether this method should be called immediately after <see cref="M:VRCOSC.Game.Modules.Module.OnModuleStart" /></param>
    /// <param name="deltaMilliseconds">The time between this method being called in milliseconds. This is only used when <paramref name="mode" /> is <see cref="F:VRCOSC.Game.Modules.ModuleUpdateMode.Custom" /></param>
    /// <remarks><paramref name="deltaMilliseconds" /> defaults to the fastest update rate you should need for sending parameters</remarks>
    public ModuleUpdateAttribute(ModuleUpdateMode mode, bool updateImmediately = true, int deltaMilliseconds = 50)
    {
        Mode = mode;
        UpdateImmediately = updateImmediately;
        DeltaMilliseconds = deltaMilliseconds;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class ModulePersistentAttribute : Attribute
{
    public string SerialisedName { get; }
    public string? LegacySerialisedName { get; }

    /// <summary>
    /// Used to mark a field for being automatically loaded and saved when the module starts and stops
    /// </summary>
    /// <param name="serialisedName">The name to serialise this property as</param>
    /// <param name="legacySerialisedName">Support for migration from a legacy name to the <paramref name="serialisedName" /></param>
    public ModulePersistentAttribute(string serialisedName, string? legacySerialisedName = null)
    {
        SerialisedName = serialisedName;
        LegacySerialisedName = legacySerialisedName;
    }
}

public enum ModuleUpdateMode
{
    /// <summary>
    /// A fixed update that is called 60 times per second
    /// </summary>
    Fixed,

    /// <summary>
    /// Updates before the ChatBox is evaluated and text is sent. This is useful for setting ChatBox variables and updating states/events
    /// </summary>
    ChatBox,

    /// <summary>
    /// A custom update rate as marked by <see cref="ModuleUpdateAttribute.DeltaMilliseconds"/>
    /// </summary>
    Custom
}
