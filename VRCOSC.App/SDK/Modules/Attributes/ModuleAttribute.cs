// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.SDK.Modules.Attributes;

public abstract class ModuleAttribute
{
    /// <summary>
    /// The metadata for this <see cref="ModuleAttribute"/>
    /// </summary>
    public ModuleAttributeMetadata Metadata;

    /// <summary>
    /// Called before this <see cref="ModuleAttribute"/> deserialises
    /// </summary>
    public virtual void PreDeserialise()
    {
    }

    /// <summary>
    /// Called after this <see cref="ModuleAttribute"/> deserialises
    /// </summary>
    public virtual void PostDeserialise()
    {
    }

    /// <summary>
    /// Resets this <see cref="ModuleAttribute"/>'s value to its default value
    /// </summary>
    public abstract void SetDefault();

    /// <summary>
    /// If this <see cref="ModuleAttribute"/>'s value is currently the default value
    /// </summary>
    /// <returns></returns>
    public abstract bool IsDefault();

    /// <summary>
    /// Attempts to deserialise an object into this <see cref="ModuleAttribute"/>'s value's type
    /// </summary>
    /// <param name="ingestValue">The value to attempt to deserialise</param>
    /// <returns>True if the deserialisation was successful, otherwise false</returns>
    public abstract bool Deserialise(object ingestValue);

    /// <summary>
    /// Retrieves the value for this <see cref="ModuleAttribute"/> using a provided expected type
    /// </summary>
    /// <typeparam name="TValueType">The type to attempt to convert the value to</typeparam>
    /// <returns>True if the value was converted successfully, otherwise false</returns>
    public virtual bool GetValue<TValueType>(out TValueType? outValue)
    {
        var value = GetRawValue();

        if (value is TValueType valueAsType)
        {
            outValue = valueAsType;
            return true;
        }

        outValue = default;
        return false;
    }

    /// <summary>
    /// Retrieves the unknown raw typed value for this <see cref="ModuleAttribute"/>.
    /// </summary>
    public abstract object GetRawValue();

    /// <summary>
    /// Call to request serialisation of this <see cref="ModuleAttribute"/>
    /// </summary>
    public Action? RequestSerialisation;

    protected ModuleAttribute(ModuleAttributeMetadata metadata)
    {
        Metadata = metadata;
    }
}
