// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.SDK.Parameters;

namespace VRCOSC.SDK.Attributes.Parameters;

public class ModuleParameterMetadata : ModuleAttributeMetadata
{
    /// <summary>
    /// The mode for this <see cref="ModuleParameter"/>
    /// </summary>
    public readonly ParameterMode Mode;

    /// <summary>
    /// The expected type for this <see cref="ModuleParameter"/>
    /// </summary>
    public readonly Type Type;

    /// <summary>
    /// Whether this <see cref="ModuleParameter"/> should be marked as legacy
    /// </summary>
    public readonly bool Legacy;

    public ModuleParameterMetadata(string title, string description, ParameterMode mode, Type type, bool legacy)
        : base(title, description)
    {
        Mode = mode;
        Type = type;
        Legacy = legacy;
    }
}
