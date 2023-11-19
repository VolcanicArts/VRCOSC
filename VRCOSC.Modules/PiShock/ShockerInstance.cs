// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu.Framework.Bindables;
using VRCOSC.Game.Modules.SDK.Attributes.Settings;
using VRCOSC.Game.Modules.SDK.Graphics.Settings;

namespace VRCOSC.Modules.PiShock;

public class Shocker : IEquatable<Shocker>
{
    [JsonProperty("name")]
    public Bindable<string> Name = new(string.Empty);

    [JsonProperty("sharecode")]
    public Bindable<string> Sharecode = new(string.Empty);

    [JsonConstructor]
    public Shocker()
    {
    }

    public Shocker(Shocker other)
    {
        Name.Value = other.Name.Value;
        Sharecode.Value = other.Sharecode.Value;
    }

    public bool Equals(Shocker? other)
    {
        if (ReferenceEquals(null, other)) return false;

        return Name.Value.Equals(other.Name.Value) && Sharecode.Value.Equals(other.Sharecode.Value);
    }
}

public partial class DrawableShocker : DrawableListModuleSettingItem<Shocker>
{
    public DrawableShocker(Shocker item)
        : base(item)
    {
    }
}

public class ShockerListModuleSetting : ListModuleSetting<Shocker>
{
    public ShockerListModuleSetting(ListModuleSettingMetadata metadata, IEnumerable<Shocker> defaultValues)
        : base(metadata, defaultValues)
    {
    }

    protected override Shocker CloneValue(Shocker value) => new(value);
    protected override Shocker ConstructValue(JToken token) => token.ToObject<Shocker>()!;
    protected override Shocker CreateNewItem() => new();
}

public partial class DrawableShockerListModuleSetting : DrawableListModuleSetting<Shocker>
{
    public DrawableShockerListModuleSetting(ShockerListModuleSetting moduleSetting)
        : base(moduleSetting)
    {
    }
}
