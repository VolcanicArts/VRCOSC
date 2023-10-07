// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Specialized;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.ModuleAttributes.Attributes;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Text;
using VRCOSC.Game.Modules.Attributes;

namespace VRCOSC.Modules.PiShock;

public class PiShockGroupInstance : IEquatable<PiShockGroupInstance>
{
    [JsonProperty("keys")]
    public Bindable<string> Names = new(string.Empty);

    [JsonConstructor]
    public PiShockGroupInstance()
    {
    }

    public PiShockGroupInstance(PiShockGroupInstance other)
    {
        Names.Value = other.Names.Value;
    }

    public bool Equals(PiShockGroupInstance? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (!ReferenceEquals(this, other)) return false;

        return Names.Value.Equals(other.Names.Value);
    }
}

public class PiShockGroupInstanceListAttribute : ModuleAttributeList<PiShockGroupInstance>
{
    public override Drawable GetAssociatedCard() => new PiShockGroupInstanceAttributeCardList(this);

    protected override IEnumerable<PiShockGroupInstance> JArrayToType(JArray array) => array.Select(value => new PiShockGroupInstance(value.ToObject<PiShockGroupInstance>()!)).ToList();
    protected override IEnumerable<PiShockGroupInstance> GetClonedDefaults() => Default.Select(defaultValue => new PiShockGroupInstance(defaultValue)).ToList();
}

public partial class PiShockGroupInstanceAttributeCardList : AttributeCardList<PiShockGroupInstanceListAttribute, PiShockGroupInstance>
{
    private readonly List<DrawablePiShockGroupInstance> localList = new();

    public PiShockGroupInstanceAttributeCardList(PiShockGroupInstanceListAttribute attributeData)
        : base(attributeData)
    {
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        localList.RemoveAll(instance => (e.OldItems?.Contains(instance) ?? false) && (!e.NewItems?.Contains(instance) ?? false));
        localList.ForEach(instance => instance.UpdatePositionText());
    }

    protected override void OnInstanceAdd(PiShockGroupInstance instance)
    {
        var drawablePiShockGroupInstance = new DrawablePiShockGroupInstance(AttributeData, instance);
        localList.Add(drawablePiShockGroupInstance);
        AddToList(drawablePiShockGroupInstance);
    }

    protected override PiShockGroupInstance CreateInstance() => new();

    public partial class DrawablePiShockGroupInstance : GridContainer
    {
        private readonly PiShockGroupInstanceListAttribute attributeData;
        private readonly PiShockGroupInstance instance;
        private readonly StringTextBox positionTextBox;

        public DrawablePiShockGroupInstance(PiShockGroupInstanceListAttribute attributeData, PiShockGroupInstance instance)
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
                        ValidCurrent = instance.Names.GetBoundCopy(),
                        PlaceholderText = "Name,Name2,Name3",
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
