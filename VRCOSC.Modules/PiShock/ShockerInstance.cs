// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu.Framework.Bindables;
using VRCOSC.Game.Modules.SDK.Attributes.Settings;
using VRCOSC.Game.Modules.SDK.Graphics.Settings;

namespace VRCOSC.Modules.PiShock;

public class ShockerInstance : IEquatable<ShockerInstance>
{
    [JsonProperty("name")]
    public Bindable<string> Name = new(string.Empty);

    [JsonProperty("sharecode")]
    public Bindable<string> Sharecode = new(string.Empty);

    [JsonConstructor]
    public ShockerInstance()
    {
    }

    public ShockerInstance(ShockerInstance other)
    {
        Name.Value = other.Name.Value;
        Sharecode.Value = other.Sharecode.Value;
    }

    public bool Equals(ShockerInstance? other)
    {
        if (ReferenceEquals(null, other)) return false;

        return Name.Value.Equals(other.Name.Value) && Sharecode.Value.Equals(other.Sharecode.Value);
    }
}

public partial class DrawableShockerInstance : DrawableListModuleSettingItem<ShockerInstance>
{
    public DrawableShockerInstance(ShockerInstance item)
        : base(item)
    {
    }
}

public class ShockerListModuleSetting : ListModuleSetting<ShockerInstance>
{
    public ShockerListModuleSetting(ListModuleSettingMetadata metadata, IEnumerable<ShockerInstance> defaultValues)
        : base(metadata, defaultValues)
    {
    }

    protected override ShockerInstance CloneValue(ShockerInstance value) => new(value);
    protected override ShockerInstance ConstructValue(JToken token) => token.ToObject<ShockerInstance>()!;
}

public partial class DrawableShockerListModuleSetting : DrawableListModuleSetting<ShockerListModuleSetting, ShockerInstance>
{
    public DrawableShockerListModuleSetting(ShockerListModuleSetting moduleSetting)
        : base(moduleSetting)
    {
    }
}
