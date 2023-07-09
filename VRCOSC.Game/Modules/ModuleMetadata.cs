// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using JetBrains.Annotations;

namespace VRCOSC.Game.Modules;

[AttributeUsage(AttributeTargets.Class)]
public class ModuleTitleAttribute : Attribute
{
    public readonly string Title;

    /// <inheritdoc />
    /// <param name="title">The human-readable name of your module</param>
    public ModuleTitleAttribute(string title)
    {
        Title = title;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class ModuleDescriptionAttribute : Attribute
{
    public readonly string ShortDescription;
    public readonly string LongDescription;

    /// <inheritdoc />
    /// <param name="shortDescription">Used in places where there isn't as much space</param>
    /// <param name="longDescription">Used in places where there is room for a detailed description</param>
    /// <remarks>If <paramref name="longDescription" /> is left blank, <paramref name="shortDescription" /> is used in its place</remarks>
    public ModuleDescriptionAttribute(string shortDescription, string longDescription = "")
    {
        ShortDescription = shortDescription;
        LongDescription = string.IsNullOrEmpty(longDescription) ? shortDescription : longDescription;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class ModuleAuthorAttribute : Attribute
{
    public readonly string Name;
    public readonly string? Url;
    public readonly string? IconUrl;

    /// <inheritdoc />
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

[AttributeUsage(AttributeTargets.Class)]
public class ModuleGroupAttribute : Attribute
{
    public readonly Module.ModuleType Type;

    /// <inheritdoc />
    /// <param name="type">The group this module belongs to</param>
    public ModuleGroupAttribute(Module.ModuleType type)
    {
        Type = type;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class ModulePrefabAttribute : Attribute
{
    public readonly string Name;
    public readonly string Url;

    /// <inheritdoc />
    /// <param name="name">The name of the prefab</param>
    /// <param name="url">A download URL for the latest version</param>
    public ModulePrefabAttribute(string name, string url)
    {
        Name = name;
        Url = url;
    }
}

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
    /// <param name="updateImmediately">Whether this method should be called immediately after <see cref="Module.OnModuleStart"/></param>
    /// <param name="deltaMilliseconds">The time between this method being called in milliseconds. This is only used when <paramref name="mode"/> is <see cref="ModuleUpdateMode.Custom"/></param>
    /// <remarks><paramref name="deltaMilliseconds"/> defaults to the fastest update rate you should need for sending parameters</remarks>
    public ModuleUpdateAttribute(ModuleUpdateMode mode, bool updateImmediately = true, int deltaMilliseconds = 50)
    {
        Mode = mode;
        UpdateImmediately = updateImmediately;
        DeltaMilliseconds = deltaMilliseconds;
    }
}

public enum ModuleUpdateMode
{
    /// <summary>
    /// A fixed update that is called every 60th of a second
    /// </summary>
    Fixed,

    /// <summary>
    /// Updates with the speed of the ChatBox's update settings
    /// </summary>
    ChatBox,

    /// <summary>
    /// A custom update rate as marked by <see cref="ModuleUpdateAttribute.DeltaMilliseconds"/>
    /// </summary>
    Custom
}
