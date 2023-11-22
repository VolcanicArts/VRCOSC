// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;

namespace VRCOSC.Game.SDK.Attributes.Parameters;

public class ModuleParameter : ModuleAttribute
{
    /// <summary>
    /// The metadata for this <see cref="ModuleParameter"/>
    /// </summary>
    internal new ModuleParameterMetadata Metadata => (ModuleParameterMetadata)base.Metadata;

    internal Bindable<string> Name = null!;

    private readonly string defaultName;

    internal override void Load()
    {
        Name = new Bindable<string>(defaultName);
        Name.BindValueChanged(_ => RequestSerialisation?.Invoke());
    }

    internal override bool IsDefault() => Name.IsDefault;
    internal override void SetDefault() => Name.SetDefault();
    internal override object GetRawValue() => Name.Value;

    internal override bool Deserialise(object ingestValue)
    {
        if (ingestValue is not string stringIngestValue) return false;

        Name.Value = stringIngestValue;
        return true;
    }

    public ModuleParameter(ModuleParameterMetadata metadata, string defaultName)
        : base(metadata)
    {
        this.defaultName = defaultName;
    }
}
