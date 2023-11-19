// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.UI.Text;
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
        Add(new GridContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            ColumnDimensions = new[]
            {
                new Dimension(),
                new Dimension(GridSizeMode.Absolute, 5),
                new Dimension()
            },
            RowDimensions = new[]
            {
                new Dimension(GridSizeMode.AutoSize)
            },
            Content = new[]
            {
                new Drawable?[]
                {
                    new StringTextBox
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = 35,
                        ValidCurrent = Item.Name.GetBoundCopy()
                    },
                    null,
                    new StringTextBox
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = 35,
                        ValidCurrent = Item.Sharecode.GetBoundCopy()
                    }
                }
            }
        });
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

    protected override Drawable Header => new GridContainer
    {
        RelativeSizeAxes = Axes.X,
        AutoSizeAxes = Axes.Y,
        ColumnDimensions = new[]
        {
            new Dimension(),
            new Dimension(GridSizeMode.Absolute, 5),
            new Dimension()
        },
        RowDimensions = new[]
        {
            new Dimension(GridSizeMode.AutoSize)
        },
        Content = new[]
        {
            new Drawable?[]
            {
                new TextFlowContainer(t =>
                {
                    t.Font = Fonts.BOLD.With(size: 25);
                    t.Colour = Colours.WHITE2;
                })
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    TextAnchor = Anchor.Centre,
                    Text = "Name"
                },
                null,
                new TextFlowContainer(t =>
                {
                    t.Font = Fonts.BOLD.With(size: 25);
                    t.Colour = Colours.WHITE2;
                })
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    TextAnchor = Anchor.Centre,
                    Text = "Sharecode"
                },
            }
        }
    };
}
