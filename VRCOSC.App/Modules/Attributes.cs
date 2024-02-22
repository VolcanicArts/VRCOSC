// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using JetBrains.Annotations;

namespace VRCOSC.App.Modules;

[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
[AttributeUsage(AttributeTargets.Class)]
public class ModuleTitleAttribute : Attribute
{
    internal readonly string Title;

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
    internal readonly string ShortDescription;
    internal readonly string LongDescription;

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
public class ModuleTypeAttribute : Attribute
{
    internal readonly ModuleType Type;

    /// <summary>
    /// Puts a <see cref="Module"/> into a specific type
    /// </summary>
    /// <param name="type">The type this module belongs to</param>
    public ModuleTypeAttribute(ModuleType type)
    {
        Type = type;
    }
}

[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ModulePrefabAttribute : Attribute
{
    internal readonly string Name;
    internal readonly string? Url;

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
[AttributeUsage(AttributeTargets.Method)]
public class ModuleUpdateAttribute : Attribute
{
    internal readonly ModuleUpdateMode Mode;
    internal readonly bool UpdateImmediately;
    internal readonly double DeltaMilliseconds;

    /// <param name="mode">The mode this update is defined as</param>
    /// <param name="updateImmediately">Whether this method should be called immediately after <see cref="Module.OnModuleStart" />. This is only used when <paramref name="mode" /> is <see cref="ModuleUpdateMode.Custom" /></param>
    /// <param name="deltaMilliseconds">The time between this method being called in milliseconds. This is only used when <paramref name="mode" /> is <see cref="ModuleUpdateMode.Custom" /></param>
    /// <remarks><paramref name="deltaMilliseconds" /> defaults to the fastest update rate you should need for sending parameters. If multiple of this attribute that have the same <see cref="ModuleUpdateMode"/> are defined in a class they will execute top to bottom</remarks>
    public ModuleUpdateAttribute(ModuleUpdateMode mode, bool updateImmediately = true, double deltaMilliseconds = 50)
    {
        Mode = mode;
        UpdateImmediately = updateImmediately;
        DeltaMilliseconds = deltaMilliseconds;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class ModulePersistentAttribute : Attribute
{
    internal readonly string SerialisedName;

    /// <summary>
    /// Used to mark a field for being automatically loaded and saved when the <see cref="Module"/> starts and stops
    /// </summary>
    /// <param name="serialisedName">The name to serialise this property as</param>
    public ModulePersistentAttribute(string serialisedName)
    {
        SerialisedName = serialisedName;
    }
}

public enum ModuleUpdateMode
{
    /// <summary>
    /// A custom update rate as marked by <see cref="ModuleUpdateAttribute.DeltaMilliseconds"/>
    /// </summary>
    Custom
}
