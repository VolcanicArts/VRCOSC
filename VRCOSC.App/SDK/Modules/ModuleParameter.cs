// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Modules;

public class ModuleParameter
{
    public Observable<bool> Enabled { get; } = new(true);
    public Observable<string> Name { get; }

    private readonly string title;

    public string Title => Legacy ? $"Legacy: {title}" : title;

    public string Description { get; }

    /// <summary>
    /// The mode for this <see cref="ModuleParameter"/>
    /// </summary>
    public ParameterMode Mode { get; }

    /// <summary>
    /// The expected type for this <see cref="ModuleParameter"/>
    /// </summary>
    public ParameterType ExpectedType { get; }

    /// <summary>
    /// Whether this <see cref="ModuleParameter"/> should be marked as legacy
    /// </summary>
    public bool Legacy { get; }

    public ModuleParameter(string title, string description, string defaultName, ParameterMode mode, ParameterType expectedType, bool legacy)
    {
        Name = new Observable<string>(defaultName);

        this.title = title;
        Description = description;
        Mode = mode;
        ExpectedType = expectedType;
        Legacy = legacy;
    }

    public bool IsDefault() => Enabled.IsDefault && Name.IsDefault;
}