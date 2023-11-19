// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Modules.SDK.Attributes;
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

public class ShockerListModuleSetting : ListModuleSetting<ShockerInstance>
{
    public ShockerListModuleSetting(ModuleSettingMetadata metadata, IEnumerable<ShockerInstance> defaultValues)
        : base(metadata, defaultValues)
    {
    }

    protected override ShockerInstance CloneValue(ShockerInstance value) => new(value);
    protected override ShockerInstance ConstructValue(JToken token) => token.ToObject<ShockerInstance>()!;
}

public partial class DrawableShockerListModuleSetting : DrawableListModuleSetting<ShockerListModuleSetting>
{
    public DrawableShockerListModuleSetting(ShockerListModuleSetting moduleSetting)
        : base(moduleSetting)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        ModuleSetting.Attribute.ForEach(shockerInstance =>
        {
            Add(new SpriteText
            {
                Font = Fonts.REGULAR.With(size: 25),
                Colour = Colours.WHITE0,
                Text = shockerInstance.Name.Value
            });
        });
    }
}
