// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Modules.Attributes.Parameters;

public class ModuleParameter : ModuleAttribute
{
    public Observable<string> Name { get; private set; } = null!;

    private readonly string defaultName;

    public override void PreDeserialise() => Name = new Observable<string>(defaultName);
    public override bool IsDefault() => Name.IsDefault;
    public override void SetDefault() => Name.SetDefault();
    public override object GetRawValue() => Name.Value;
    public override object GetSerialisableValue() => GetRawValue();

    public override bool Deserialise(object ingestValue)
    {
        if (ingestValue is not string stringIngestValue) return false;

        Name.Value = stringIngestValue;
        return true;
    }

    public override string Title => Legacy ? $"Legacy: {base.Title}" : base.Title;

    /// <summary>
    /// The mode for this <see cref="ModuleParameter"/>
    /// </summary>
    public ParameterMode Mode { get; }

    /// <summary>
    /// The expected type for this <see cref="ModuleParameter"/>
    /// </summary>
    public Type ExpectedType { get; }

    /// <summary>
    /// Whether this <see cref="ModuleParameter"/> should be marked as legacy
    /// </summary>
    public bool Legacy { get; }

    public ModuleParameter(string title, string description, string defaultName, ParameterMode mode, Type expectedType, bool legacy)
        : base(title, description)
    {
        this.defaultName = defaultName;
        Mode = mode;
        ExpectedType = expectedType;
        Legacy = legacy;
    }
}
