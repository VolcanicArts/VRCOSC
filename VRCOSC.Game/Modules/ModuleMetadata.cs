// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using JetBrains.Annotations;
using VRCOSC.Game.OSC.VRChat;

namespace VRCOSC.Game.Modules;

[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
[AttributeUsage(AttributeTargets.Class)]
public class ModuleTitleAttribute : Attribute
{
    public readonly string Title;

    /// <summary>
    /// Defines a title for the <see cref="Module"/>
    /// </summary>
    /// <param name="title">The name of your <see cref="Module"/></param>
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
    public readonly string? LongDescription;

    /// <summary>
    /// Allows for providing descriptions of the <see cref="Module"/>
    /// </summary>
    /// <param name="shortDescription">Used in places where there isn't as much space</param>
    /// <param name="longDescription">Used in places where there is room for a detailed description</param>
    /// <remarks>If <paramref name="longDescription" /> is left blank, <paramref name="shortDescription" /> is used in its place</remarks>
    public ModuleDescriptionAttribute(string shortDescription, string? longDescription = null)
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

    /// <summary>
    /// Defines metadata for the author of the <see cref="Module"/>
    /// </summary>
    /// <param name="name">Your name</param>
    /// <param name="url">A link to any page you'd like to promote</param>
    /// <param name="iconUrl">A link to a PNG or JPG of your icon</param>
    public ModuleAuthorAttribute(string name, string? url = null, string? iconUrl = null)
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
    public readonly ModuleType Type;

    /// <summary>
    /// Puts a <see cref="Module"/> into a specific group
    /// </summary>
    /// <param name="type">The group this module belongs to</param>
    public ModuleGroupAttribute(ModuleType type)
    {
        Type = type;
    }
}

[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
[AttributeUsage(AttributeTargets.Class)]
public class ModulePrefabAttribute : Attribute
{
    public readonly string Name;
    public readonly string? Url;

    /// <summary>
    /// Adds a reference to a prefab for this <see cref="Module"/>
    /// </summary>
    /// <param name="name">The name of the prefab</param>
    /// <param name="url">A download URL for the latest version</param>
    public ModulePrefabAttribute(string name, string? url = null)
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
    public readonly string? Url;

    /// <summary>
    /// Adds a line of info to appear in the info menu for a <see cref="Module"/>
    /// </summary>
    /// <param name="description">The description of the info</param>
    /// <param name="url">An optional URL to link to a website</param>
    public ModuleInfoAttribute(string description, string? url = null)
    {
        Description = description;
        Url = url;
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
    /// <param name="updateImmediately">Whether this method should be called immediately after <see cref="M:VRCOSC.Game.Modules.Module.OnModuleStart" />. This is only used when <paramref name="mode" /> is <see cref="F:VRCOSC.Game.Modules.ModuleUpdateMode.Custom" /></param>
    /// <param name="deltaMilliseconds">The time between this method being called in milliseconds. This is only used when <paramref name="mode" /> is <see cref="F:VRCOSC.Game.Modules.ModuleUpdateMode.Custom" /></param>
    /// <remarks><paramref name="deltaMilliseconds" /> defaults to the fastest update rate you should need for sending parameters</remarks>
    public ModuleUpdateAttribute(ModuleUpdateMode mode, bool updateImmediately = true, double deltaMilliseconds = VRChatOscConstants.UPDATE_DELTA_MILLISECONDS)
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
    /// Used to mark a field for being automatically loaded and saved when the <see cref="Module"/> starts and stops
    /// </summary>
    /// <param name="serialisedName">The name to serialise this property as</param>
    /// <param name="legacySerialisedName">Support for migration from a legacy name to the <paramref name="serialisedName" /></param>
    public ModulePersistentAttribute(string serialisedName, string? legacySerialisedName = null)
    {
        SerialisedName = serialisedName;
        LegacySerialisedName = legacySerialisedName;
    }
}

[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
[AttributeUsage(AttributeTargets.Class)]
public class ModuleLegacyAttribute : Attribute
{
    public string? LegacySerialisedName { get; }

    /// <summary>
    /// Marks a <see cref="Module"/> with legacy fields to allow for migration
    /// </summary>
    /// <param name="legacySerialisedName">Allows migration from a legacy serialised name to the current serialised name in the case that the class is renamed</param>
    public ModuleLegacyAttribute(string? legacySerialisedName = null)
    {
        LegacySerialisedName = legacySerialisedName;
    }
}

public enum ModuleUpdateMode
{
    /// <summary>
    /// Updates before the ChatBox is evaluated and text is sent. This is useful for setting ChatBox variables and updating states/events
    /// </summary>
    ChatBox,

    /// <summary>
    /// A custom update rate as marked by <see cref="ModuleUpdateAttribute.DeltaMilliseconds"/>
    /// </summary>
    Custom
}
