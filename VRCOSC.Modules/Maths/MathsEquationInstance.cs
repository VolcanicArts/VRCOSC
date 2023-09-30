// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osuTK;
using VRCOSC.Game.Graphics.ModuleAttributes.Attributes;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Graphics.UI.Text;
using VRCOSC.Game.Modules.Attributes;

namespace VRCOSC.Modules.Maths;

public class MathsEquationInstance : IEquatable<MathsEquationInstance>
{
    [JsonProperty("input_parameter")]
    public Bindable<string> TriggerParameter = new(string.Empty);

    [JsonProperty("input_type")]
    public Bindable<MathsEquationValueType> InputType = new();

    [JsonProperty("equation")]
    public Bindable<string> Equation = new(string.Empty);

    [JsonProperty("output_parameter")]
    public Bindable<string> OutputParameter = new(string.Empty);

    [JsonProperty("output_type")]
    public Bindable<MathsEquationValueType> OutputType = new(MathsEquationValueType.Int);

    public bool Equals(MathsEquationInstance? other)
    {
        if (ReferenceEquals(other, null)) return false;

        return TriggerParameter.Value == other.TriggerParameter.Value && InputType.Value == other.InputType.Value && Equation.Value == other.Equation.Value && OutputParameter.Value == other.OutputParameter.Value && OutputType.Value == other.OutputType.Value;
    }

    [JsonConstructor]
    public MathsEquationInstance()
    {
    }

    public MathsEquationInstance(MathsEquationInstance other)
    {
        TriggerParameter.Value = other.TriggerParameter.Value;
        InputType.Value = other.InputType.Value;
        Equation.Value = other.Equation.Value;
        OutputParameter.Value = other.OutputParameter.Value;
        OutputType.Value = other.OutputType.Value;
    }
}

public class MathsEquationInstanceListAttribute : ModuleAttributeList<MathsEquationInstance>
{
    public override Drawable GetAssociatedCard() => new MathsEquationInstanceAttributeCardList(this);

    protected override IEnumerable<MathsEquationInstance> JArrayToType(JArray array) => array.Select(value => new MathsEquationInstance(value.ToObject<MathsEquationInstance>()!)).ToList();
    protected override IEnumerable<MathsEquationInstance> GetClonedDefaults() => Default.Select(defaultValue => new MathsEquationInstance(defaultValue)).ToList();
}

public partial class MathsEquationInstanceAttributeCardList : AttributeCardList<MathsEquationInstanceListAttribute, MathsEquationInstance>
{
    [Resolved]
    private GameHost host { get; set; } = null!;

    public MathsEquationInstanceAttributeCardList(MathsEquationInstanceListAttribute attributeData)
        : base(attributeData)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new Container
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 30,
            Width = 0.75f,
            Child = new TextButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Text = "See built-in functions and variables",
                CornerRadius = 5,
                FontSize = 22,
                Action = () => host.OpenUrlExternally("https://mathparser.org/mxparser-math-collection/"),
                BackgroundColour = ThemeManager.Current[ThemeAttribute.Action]
            }
        });

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
                    new Dimension(maxSize: 150),
                    new Dimension(GridSizeMode.Absolute, 5),
                    new Dimension(),
                    new Dimension(GridSizeMode.Absolute, 5),
                    new Dimension(maxSize: 150)
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
                                Text = "Trigger Parameter",
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
                                Text = "Equation",
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
                                Text = "Output Parameter",
                                Font = FrameworkFont.Regular.With(size: 20)
                            }
                        }
                    }
                }
            }
        }, float.MinValue);
    }

    protected override void OnInstanceAdd(MathsEquationInstance instance)
    {
        AddToList(new GridContainer
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            ColumnDimensions = new[]
            {
                new Dimension(maxSize: 150),
                new Dimension(GridSizeMode.Absolute, 5),
                new Dimension(),
                new Dimension(GridSizeMode.Absolute, 5),
                new Dimension(maxSize: 150)
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
                        ValidCurrent = instance.TriggerParameter.GetBoundCopy(),
                        PlaceholderText = "Trigger Parameter"
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
                        ValidCurrent = instance.Equation.GetBoundCopy(),
                        PlaceholderText = "Equation"
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
                        ValidCurrent = instance.OutputParameter.GetBoundCopy(),
                        PlaceholderText = "Output Parameter"
                    }
                }
            }
        });
    }

    protected override MathsEquationInstance CreateInstance() => new();
}

public partial class MathsValueTypeInstanceDropdown<T> : VRCOSCDropdown<T>
{
    protected override DropdownHeader CreateHeader() => new MathsValueTypeDropdownHeader();

    public partial class MathsValueTypeDropdownHeader : DropdownHeader
    {
        protected override LocalisableString Label
        {
            get => Text.Text;
            set => Text.Text = value;
        }

        protected readonly SpriteText Text;
        public readonly SpriteIcon Icon;

        public MathsValueTypeDropdownHeader()
        {
            Foreground.Padding = new MarginPadding(10);

            AutoSizeAxes = Axes.None;
            CornerRadius = 5;
            Masking = true;
            BorderColour = ThemeManager.Current[ThemeAttribute.Border];
            BorderThickness = 2;
            Height = 30;

            Foreground.Child = new GridContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize)
                },
                ColumnDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(GridSizeMode.AutoSize)
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        Text = new SpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.X,
                            Truncate = true,
                            Font = FrameworkFont.Regular.With(size: 20),
                            Colour = ThemeManager.Current[ThemeAttribute.Text]
                        },
                        Icon = new SpriteIcon
                        {
                            Icon = FontAwesome.Solid.ChevronDown,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Size = new Vector2(10),
                            Colour = ThemeManager.Current[ThemeAttribute.Text]
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            BackgroundColour = ThemeManager.Current[ThemeAttribute.Dark];
            BackgroundColourHover = ThemeManager.Current[ThemeAttribute.Mid];
        }
    }
}

public enum MathsEquationValueType
{
    Bool,
    Int,
    Float
}
