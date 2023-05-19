// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.ModuleAttributes.Attributes;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Text;
using VRCOSC.Game.Modules.Attributes;

namespace VRCOSC.Modules.Counter;

public class NameParameterPair : IEquatable<NameParameterPair>
{
    [JsonProperty("key")]
    public Bindable<string> Key = new(string.Empty);

    [JsonProperty("parameter")]
    public Bindable<string> Parameter = new(string.Empty);

    [JsonConstructor]
    public NameParameterPair()
    {
    }

    public NameParameterPair(NameParameterPair other)
    {
        Key.Value = other.Key.Value;
        Parameter.Value = other.Parameter.Value;
    }

    public bool Equals(NameParameterPair? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Key.Value.Equals(other.Key.Value)
               && Parameter.Value.Equals(other.Parameter.Value);
    }
}

public class ModuleNameParameterPairAttribute : ModuleAttributeList<NameParameterPair>
{
    public override Drawable GetAssociatedCard() => new NameParameterPairListCard(this);
    public override bool IsDefault() => Attribute.Count == Default.Count && !Attribute.Where((t, i) => !t.Equals(Default.ElementAt(i))).Any();

    protected override BindableList<NameParameterPair> CreateBindableList() => new(Default);
    protected override IEnumerable<NameParameterPair> JArrayToType(JArray array) => array.Select(value => new NameParameterPair(value.ToObject<NameParameterPair>()!)).ToList();
    protected override IEnumerable<NameParameterPair> GetClonedDefaults() => Default.Select(defaultValue => new NameParameterPair(defaultValue)).ToList();
}

public partial class NameParameterPairListCard : AttributeCardList<NameParameterPair>
{
    public NameParameterPairListCard(ModuleNameParameterPairAttribute attributeData)
        : base(attributeData)
    {
    }

    protected override void OnInstanceAdd(NameParameterPair instance)
    {
        AddToFlow(new DrawableNameParameterPair(instance));
    }

    protected override NameParameterPair CreateInstance() => new();
}

public partial class DrawableNameParameterPair : Container
{
    private readonly NameParameterPair attribute;

    public DrawableNameParameterPair(NameParameterPair attribute)
    {
        this.attribute = attribute;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        Child = new GridContainer
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            ColumnDimensions = new[]
            {
                new Dimension(GridSizeMode.Relative, 0.25f),
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
                        Text = attribute.Key.Value,
                        ValidCurrent = attribute.Key.GetBoundCopy(),
                        PlaceholderText = "Key"
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
                        Text = attribute.Parameter.Value,
                        ValidCurrent = attribute.Parameter.GetBoundCopy(),
                        PlaceholderText = "Parameter Name"
                    }
                }
            }
        };
    }
}
