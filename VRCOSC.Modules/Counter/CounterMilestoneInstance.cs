// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Graphics.ModuleAttributes.Attributes;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Text;
using VRCOSC.Game.Modules.Attributes;

namespace VRCOSC.Modules.Counter;

public class CounterMilestoneInstance : IEquatable<CounterMilestoneInstance>
{
    [JsonProperty("parameter_name")]
    public Bindable<string> ParameterName = new(string.Empty);

    [JsonProperty("counter_key")]
    public Bindable<string> CounterKey = new(string.Empty);

    [JsonProperty("required_count")]
    public Bindable<int> RequiredCount = new();

    [JsonConstructor]
    public CounterMilestoneInstance()
    {
    }

    public CounterMilestoneInstance(CounterMilestoneInstance other)
    {
        ParameterName.Value = other.ParameterName.Value;
        CounterKey.Value = other.CounterKey.Value;
        RequiredCount.Value = other.RequiredCount.Value;
    }

    public bool Equals(CounterMilestoneInstance? other)
    {
        if (ReferenceEquals(null, other)) return false;

        return ParameterName.Value.Equals(other.ParameterName.Value) && CounterKey.Value.Equals(other.CounterKey.Value) && RequiredCount.Value.Equals(other.RequiredCount.Value);
    }
}

public class CounterMilestoneInstanceListAttribute : ModuleAttributeList<CounterMilestoneInstance>
{
    public override Drawable GetAssociatedCard() => new CounterInstanceAttributeCardList(this);

    protected override IEnumerable<CounterMilestoneInstance> JArrayToType(JArray array) => array.Select(value => new CounterMilestoneInstance(value.ToObject<CounterMilestoneInstance>()!)).ToList();
    protected override IEnumerable<CounterMilestoneInstance> GetClonedDefaults() => Default.Select(defaultValue => new CounterMilestoneInstance(defaultValue)).ToList();
}

public partial class CounterInstanceAttributeCardList : AttributeCardList<CounterMilestoneInstanceListAttribute, CounterMilestoneInstance>
{
    public CounterInstanceAttributeCardList(CounterMilestoneInstanceListAttribute attributeData)
        : base(attributeData)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        AddToContent(new Container
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 30,
            Padding = new MarginPadding
            {
                Right = 35
            },
            Child = new GridContainer
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                ColumnDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(GridSizeMode.Absolute, 5),
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
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Child = new SpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Parameter Name",
                                Font = FrameworkFont.Regular.With(size: 20)
                            }
                        },
                        null,
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Child = new SpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Counter Key",
                                Font = FrameworkFont.Regular.With(size: 20)
                            }
                        },
                        null,
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Child = new SpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Required Count",
                                Font = FrameworkFont.Regular.With(size: 20)
                            }
                        }
                    }
                }
            }
        }, float.MinValue);
    }

    protected override void OnInstanceAdd(CounterMilestoneInstance instance)
    {
        AddToList(new GridContainer
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            ColumnDimensions = new[]
            {
                new Dimension(),
                new Dimension(GridSizeMode.Absolute, 5),
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
                        Height = 30,
                        Masking = true,
                        CornerRadius = 5,
                        BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                        BorderThickness = 2,
                        ValidCurrent = instance.ParameterName.GetBoundCopy(),
                        PlaceholderText = "Parameter Name",
                        EmptyIsValid = false
                    },
                    null,
                    new StringTextBox
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = 30,
                        Masking = true,
                        CornerRadius = 5,
                        BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                        BorderThickness = 2,
                        ValidCurrent = instance.CounterKey.GetBoundCopy(),
                        PlaceholderText = "Counter Key",
                        EmptyIsValid = false
                    },
                    null,
                    new IntTextBox
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = 30,
                        Masking = true,
                        CornerRadius = 5,
                        BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                        BorderThickness = 2,
                        ValidCurrent = instance.RequiredCount.GetBoundCopy(),
                        PlaceholderText = "Required Count",
                        EmptyIsValid = false
                    }
                }
            }
        });
    }

    protected override CounterMilestoneInstance CreateInstance() => new();
}
