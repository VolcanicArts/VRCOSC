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

namespace VRCOSC.Modules.VoiceRecognition;

public class VoiceRecognitionPhraseInstance : IEquatable<VoiceRecognitionPhraseInstance>
{
    [JsonProperty("phrase")]
    public Bindable<string> Phrase = new(string.Empty);

    [JsonProperty("parameter_name")]
    public Bindable<string> ParameterName = new(string.Empty);

    [JsonProperty("value")]
    public Bindable<string> Value = new(string.Empty);

    [JsonConstructor]
    public VoiceRecognitionPhraseInstance()
    {
    }

    public VoiceRecognitionPhraseInstance(VoiceRecognitionPhraseInstance other)
    {
        Phrase.Value = other.Phrase.Value;
        ParameterName.Value = other.ParameterName.Value;
        Value.Value = other.Value.Value;
    }

    public bool Equals(VoiceRecognitionPhraseInstance? other)
    {
        if (ReferenceEquals(null, other)) return false;

        return Phrase.Value.Equals(other.Phrase.Value) && ParameterName.Value.Equals(other.ParameterName.Value) && Value.Value.Equals(other.Value.Value);
    }
}

public class VoiceRecognitionPhraseInstanceListAttribute : ModuleAttributeList<VoiceRecognitionPhraseInstance>
{
    public override Drawable GetAssociatedCard() => new PiShockPhraseInstanceAttributeCardList(this);

    protected override IEnumerable<VoiceRecognitionPhraseInstance> JArrayToType(JArray array) => array.Select(value => new VoiceRecognitionPhraseInstance(value.ToObject<VoiceRecognitionPhraseInstance>()!)).ToList();
    protected override IEnumerable<VoiceRecognitionPhraseInstance> GetClonedDefaults() => Default.Select(defaultValue => new VoiceRecognitionPhraseInstance(defaultValue)).ToList();
}

public partial class PiShockPhraseInstanceAttributeCardList : AttributeCardList<VoiceRecognitionPhraseInstanceListAttribute, VoiceRecognitionPhraseInstance>
{
    public PiShockPhraseInstanceAttributeCardList(VoiceRecognitionPhraseInstanceListAttribute attributeData)
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
                                Text = "Value",
                                Font = FrameworkFont.Regular.With(size: 20)
                            }
                        }
                    }
                }
            }
        }, float.MinValue);
    }

    protected override void OnInstanceAdd(VoiceRecognitionPhraseInstance instance)
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
                        ValidCurrent = instance.Value.GetBoundCopy(),
                        PlaceholderText = "Value",
                        EmptyIsValid = false
                    }
                }
            }
        });
    }

    protected override VoiceRecognitionPhraseInstance CreateInstance() => new();
}
