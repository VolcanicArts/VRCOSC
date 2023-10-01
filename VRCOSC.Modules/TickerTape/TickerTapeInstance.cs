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
using osuTK;
using VRCOSC.Game.Graphics.ModuleAttributes.Attributes;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Graphics.UI.Text;
using VRCOSC.Game.Modules.Attributes;

namespace VRCOSC.Modules.TickerTape;

public class TickerTapeInstance : IEquatable<TickerTapeInstance>
{
    [JsonProperty("key")]
    public Bindable<string> Name = new(string.Empty);

    [JsonProperty("text")]
    public Bindable<string> Text = new(string.Empty);

    [JsonProperty("direction")]
    public Bindable<TickerTapeDirection> Direction = new();

    [JsonProperty("scroll_speed")]
    public Bindable<int> ScrollSpeed = new();

    [JsonProperty("max_length")]
    public Bindable<int> MaxLength = new();

    public bool Equals(TickerTapeInstance? other)
    {
        if (ReferenceEquals(other, null)) return false;

        return Name.Value == other.Name.Value && Text.Value == other.Text.Value && Direction.Value == other.Direction.Value && ScrollSpeed.Value == other.ScrollSpeed.Value && MaxLength.Value == other.MaxLength.Value;
    }

    [JsonConstructor]
    public TickerTapeInstance()
    {
    }

    public TickerTapeInstance(TickerTapeInstance other)
    {
        Name.Value = other.Name.Value;
        Text.Value = other.Text.Value;
        Direction.Value = other.Direction.Value;
        ScrollSpeed.Value = other.ScrollSpeed.Value;
        MaxLength.Value = other.MaxLength.Value;
    }
}

public class TickerTapeInstanceListAttribute : ModuleAttributeList<TickerTapeInstance>
{
    public override Drawable GetAssociatedCard() => new TickerTapeInstanceAttributeCardList(this);

    protected override IEnumerable<TickerTapeInstance> JArrayToType(JArray array) => array.Select(value => new TickerTapeInstance(value.ToObject<TickerTapeInstance>()!)).ToList();
    protected override IEnumerable<TickerTapeInstance> GetClonedDefaults() => Default.Select(defaultValue => new TickerTapeInstance(defaultValue)).ToList();
}

public partial class TickerTapeInstanceAttributeCardList : AttributeCardList<TickerTapeInstanceListAttribute, TickerTapeInstance>
{
    public TickerTapeInstanceAttributeCardList(TickerTapeInstanceListAttribute attributeData)
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
                    new Dimension(maxSize: 150),
                    new Dimension(GridSizeMode.Absolute, 5),
                    new Dimension(),
                    new Dimension(GridSizeMode.Absolute, 5),
                    new Dimension(maxSize: 100),
                    new Dimension(GridSizeMode.Absolute, 5),
                    new Dimension(maxSize: 100),
                    new Dimension(GridSizeMode.Absolute, 5),
                    new Dimension(maxSize: 100)
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
                                Text = "Name",
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
                                Text = "Text",
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
                                Text = "Direction",
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
                                Text = "Scroll Speed",
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
                                Text = "Max Length",
                                Font = FrameworkFont.Regular.With(size: 20)
                            }
                        }
                    }
                }
            }
        }, float.MinValue);
    }

    protected override void OnInstanceAdd(TickerTapeInstance instance)
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
                new Dimension(maxSize: 100),
                new Dimension(GridSizeMode.Absolute, 5),
                new Dimension(maxSize: 100),
                new Dimension(GridSizeMode.Absolute, 5),
                new Dimension(maxSize: 100)
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
                        ValidCurrent = instance.Name.GetBoundCopy(),
                        PlaceholderText = "Name"
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
                        ValidCurrent = instance.Text.GetBoundCopy(),
                        PlaceholderText = "Text"
                    },
                    null,
                    new TickerTapeInstanceDropdown<TickerTapeDirection>
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Items = Enum.GetValues(typeof(TickerTapeDirection)).Cast<TickerTapeDirection>(),
                        Current = instance.Direction.GetBoundCopy()
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
                        ValidCurrent = instance.ScrollSpeed.GetBoundCopy(),
                        PlaceholderText = "Scroll Speed"
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
                        ValidCurrent = instance.MaxLength.GetBoundCopy(),
                        PlaceholderText = "Max Length"
                    },
                }
            }
        });
    }

    protected override TickerTapeInstance CreateInstance() => new();
}

public partial class TickerTapeInstanceDropdown<T> : VRCOSCDropdown<T>
{
    protected override DropdownHeader CreateHeader() => new TickerTapeDropdownHeader();

    public partial class TickerTapeDropdownHeader : DropdownHeader
    {
        protected override LocalisableString Label
        {
            get => Text.Text;
            set => Text.Text = value;
        }

        protected readonly SpriteText Text;
        public readonly SpriteIcon Icon;

        public TickerTapeDropdownHeader()
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

public enum TickerTapeDirection
{
    Right,
    Left
}
