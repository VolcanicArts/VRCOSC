// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Modules.Attributes.Parameters;

public class ModuleParameterMetadata : ModuleAttributeMetadata
{
    public override string Title => Legacy ? $"Legacy: {base.Title}" : base.Title;

    /// <summary>
    /// The mode for this <see cref="ModuleParameter"/>
    /// </summary>
    public ParameterMode Mode { get; }

    /// <summary>
    /// The expected type for this <see cref="ModuleParameter"/>
    /// </summary>
    public Type Type { get; }

    public string UIMode => Mode switch
    {
        ParameterMode.Read => "Receive",
        ParameterMode.Write => "Send",
        ParameterMode.ReadWrite => "Send/Receive",
        _ => throw new ArgumentOutOfRangeException()
    };

    public string ReadableType => Type.ToReadableName();

    /// <summary>
    /// Whether this <see cref="ModuleParameter"/> should be marked as legacy
    /// </summary>
    public bool Legacy { get; }

    public ModuleParameterMetadata(string title, string description, ParameterMode mode, Type type, bool legacy)
        : base(title, description)
    {
        Mode = mode;
        Type = type;
        Legacy = legacy;
    }
}
