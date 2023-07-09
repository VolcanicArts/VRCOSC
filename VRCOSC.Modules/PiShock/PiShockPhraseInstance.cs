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
using VRCOSC.Game.Providers.PiShock;

namespace VRCOSC.Modules.PiShock;

public class PiShockPhraseInstance : IEquatable<PiShockPhraseInstance>
{
    [JsonProperty("phrase")]
    public Bindable<string> Phrase = new(string.Empty);

    [JsonProperty("shocker_key")]
    public Bindable<string> ShockerKey = new(string.Empty);

    [JsonProperty("mode")]
    public Bindable<PiShockMode> Mode = new();

    [JsonProperty("duration")]
    public Bindable<int> Duration = new(1);

    [JsonProperty("intensity")]
    public Bindable<int> Intensity = new();

    [JsonConstructor]
    public PiShockPhraseInstance()
    {
    }

    public PiShockPhraseInstance(PiShockPhraseInstance other)
    {
        Phrase.Value = other.Phrase.Value;
        ShockerKey.Value = other.ShockerKey.Value;
        Mode.Value = other.Mode.Value;
        Duration.Value = other.Duration.Value;
        Intensity.Value = other.Intensity.Value;
    }

    public bool Equals(PiShockPhraseInstance? other)
    {
        if (ReferenceEquals(null, other)) return false;

        return Phrase.Value.Equals(other.Phrase.Value) && ShockerKey.Value.Equals(other.ShockerKey.Value) && Mode.Value.Equals(other.Mode.Value) && Duration.Value.Equals(other.Duration.Value) && Intensity.Value.Equals(other.Intensity.Value);
    }
}

public class PiShockPhraseInstanceListAttribute : ModuleAttributeList<PiShockPhraseInstance>
{
    public override Drawable GetAssociatedCard() => new PiShockPhraseInstanceAttributeCardList(this);

    protected override IEnumerable<PiShockPhraseInstance> JArrayToType(JArray array) => array.Select(value => new PiShockPhraseInstance(value.ToObject<PiShockPhraseInstance>()!)).ToList();
    protected override IEnumerable<PiShockPhraseInstance> GetClonedDefaults() => Default.Select(defaultValue => new PiShockPhraseInstance(defaultValue)).ToList();
}

public partial class PiShockPhraseInstanceAttributeCardList : AttributeCardList<PiShockPhraseInstanceListAttribute, PiShockPhraseInstance>
{
    public PiShockPhraseInstanceAttributeCardList(PiShockPhraseInstanceListAttribute attributeData)
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
                    new Dimension(maxSize: 175),
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
                                Text = "Phrase",
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
                                Text = "Key",
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
                                Text = "Mode",
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
                                Text = "Duration",
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
                                Text = "Intensity",
                                Font = FrameworkFont.Regular.With(size: 20)
                            }
                        }
                    }
                }
            }
        }, float.MinValue);
    }

    protected override void OnInstanceAdd(PiShockPhraseInstance instance)
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
                new Dimension(maxSize: 175),
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
                        ValidCurrent = instance.Phrase.GetBoundCopy(),
                        PlaceholderText = "Phrase",
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
                        ValidCurrent = instance.ShockerKey.GetBoundCopy(),
                        PlaceholderText = "Key",
                        EmptyIsValid = false
                    },
                    null,
                    new PiShockPhraseInstanceDropdown<PiShockMode>
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Items = Enum.GetValues(typeof(PiShockMode)).Cast<PiShockMode>(),
                        Current = instance.Mode.GetBoundCopy()
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
                        ValidCurrent = instance.Duration.GetBoundCopy(),
                        PlaceholderText = "Duration",
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
                        ValidCurrent = instance.Intensity.GetBoundCopy(),
                        PlaceholderText = "Intensity",
                        EmptyIsValid = false
                    }
                }
            }
        });
    }

    protected override PiShockPhraseInstance CreateInstance() => new();
}

public partial class PiShockPhraseInstanceDropdown<T> : VRCOSCDropdown<T>
{
    protected override DropdownHeader CreateHeader() => new PiShockPhraseInstanceDropdownHeader();

    public partial class PiShockPhraseInstanceDropdownHeader : DropdownHeader
    {
        protected override LocalisableString Label
        {
            get => Text.Text;
            set => Text.Text = value;
        }

        protected readonly SpriteText Text;
        public readonly SpriteIcon Icon;

        public PiShockPhraseInstanceDropdownHeader()
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
