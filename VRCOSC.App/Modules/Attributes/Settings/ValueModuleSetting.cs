// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.Utils;

namespace VRCOSC.App.Modules.Attributes.Settings;

/// <summary>
/// For use with value types
/// </summary>
public abstract class ValueModuleSetting<T> : ModuleSetting
{
    public virtual Observable<T> Attribute { get; private set; } = null!;

    protected readonly T DefaultValue;

    protected abstract Observable<T> CreateObservable();

    internal override void Load()
    {
        Attribute = CreateObservable();
        Attribute.Subscribe(_ => RequestSerialisation?.Invoke());
    }

    internal override bool IsDefault() => Attribute.IsDefault;
    internal override void SetDefault() => Attribute.SetDefault();

    internal override object GetRawValue() => Attribute.Value!;

    protected ValueModuleSetting(ModuleSettingMetadata metadata, T defaultValue)
        : base(metadata)
    {
        DefaultValue = defaultValue;
    }
}

public class BoolModuleSetting : ValueModuleSetting<bool>
{
    protected override Observable<bool> CreateObservable() => new(DefaultValue);

    internal override bool Deserialise(object ingestValue)
    {
        if (ingestValue is not bool boolIngestValue) return false;

        Attribute.Value = boolIngestValue;
        return true;
    }

    public BoolModuleSetting(ModuleSettingMetadata metadata, bool defaultValue)
        : base(metadata, defaultValue)
    {
    }
}
