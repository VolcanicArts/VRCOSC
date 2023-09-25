// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Specialized;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using VRCOSC.Game.Graphics.ModuleAttributes.Attributes;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Graphics.UI.Text;
using VRCOSC.Game.Modules.Attributes;

namespace VRCOSC.Modules.SpeechToText;

public class SpeechToTextModelInstance : IEquatable<SpeechToTextModelInstance>
{
    [JsonProperty("path")]
    public Bindable<string> Path = new(string.Empty);

    [JsonConstructor]
    public SpeechToTextModelInstance()
    {
    }

    public SpeechToTextModelInstance(SpeechToTextModelInstance other)
    {
        Path.Value = other.Path.Value;
    }

    public bool Equals(SpeechToTextModelInstance? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (!ReferenceEquals(this, other)) return false;

        return Path.Value.Equals(other.Path.Value);
    }
}

public class SpeechToTextModelInstanceListAttribute : ModuleAttributeList<SpeechToTextModelInstance>
{
    public override Drawable GetAssociatedCard() => new SpeechToTextModelInstanceAttributeCardList(this);

    protected override IEnumerable<SpeechToTextModelInstance> JArrayToType(JArray array) => array.Select(value => new SpeechToTextModelInstance(value.ToObject<SpeechToTextModelInstance>()!)).ToList();
    protected override IEnumerable<SpeechToTextModelInstance> GetClonedDefaults() => Default.Select(defaultValue => new SpeechToTextModelInstance(defaultValue)).ToList();
}

public partial class SpeechToTextModelInstanceAttributeCardList : AttributeCardList<SpeechToTextModelInstanceListAttribute, SpeechToTextModelInstance>
{
    [Resolved]
    private GameHost host { get; set; } = null!;

    private readonly List<DrawableSpeechToTextModelInstance> localList = new();

    public SpeechToTextModelInstanceAttributeCardList(SpeechToTextModelInstanceListAttribute attributeData)
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
                Text = "Download a model",
                CornerRadius = 5,
                FontSize = 22,
                Action = () => host.OpenUrlExternally("https://alphacephei.com/vosk/models"),
                BackgroundColour = ThemeManager.Current[ThemeAttribute.Action]
            }
        });
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        localList.RemoveAll(instance => (e.OldItems?.Contains(instance) ?? false) && (!e.NewItems?.Contains(instance) ?? false));
        localList.ForEach(instance => instance.UpdatePositionText());
    }

    protected override void OnInstanceAdd(SpeechToTextModelInstance instance)
    {
        var drawablePiShockGroupInstance = new DrawableSpeechToTextModelInstance(AttributeData, instance);
        localList.Add(drawablePiShockGroupInstance);
        AddToList(drawablePiShockGroupInstance);
    }

    protected override SpeechToTextModelInstance CreateInstance() => new();

    public partial class DrawableSpeechToTextModelInstance : GridContainer
    {
        private readonly SpeechToTextModelInstanceListAttribute attributeData;
        private readonly SpeechToTextModelInstance instance;
        private readonly StringTextBox positionTextBox;

        public DrawableSpeechToTextModelInstance(SpeechToTextModelInstanceListAttribute attributeData, SpeechToTextModelInstance instance)
        {
            this.attributeData = attributeData;
            this.instance = instance;

            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            ColumnDimensions = new[]
            {
                new Dimension(maxSize: 50),
                new Dimension(GridSizeMode.Absolute, 5),
                new Dimension(),
            };

            RowDimensions = new[]
            {
                new Dimension(GridSizeMode.AutoSize)
            };

            Content = new[]
            {
                new Drawable?[]
                {
                    positionTextBox = new StringTextBox
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = 30,
                        Masking = true,
                        CornerRadius = 5,
                        BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                        BorderThickness = 2,
                        ReadOnly = true
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
                        ValidCurrent = instance.Path.GetBoundCopy(),
                        PlaceholderText = "C:/Some/Folder/Path",
                        EmptyIsValid = false
                    }
                }
            };

            UpdatePositionText();
        }

        public void UpdatePositionText()
        {
            var position = attributeData.Attribute.IndexOf(instance);
            positionTextBox.Text = position.ToString();
        }
    }
}
