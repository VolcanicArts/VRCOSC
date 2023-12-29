// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Graphics.UI.Text;
using VRCOSC.SDK.Attributes.Settings.Types;

namespace VRCOSC.SDK.Graphics.Settings.Lists;

public partial class DrawableMutableKeyValuePairListModuleSetting : DrawableListModuleSetting<MutableKeyValuePairListModuleSetting, MutableKeyValuePair>
{
    public DrawableMutableKeyValuePairListModuleSetting(MutableKeyValuePairListModuleSetting moduleSetting)
        : base(moduleSetting)
    {
    }

    protected override Drawable Header => new GridContainer
    {
        Anchor = Anchor.BottomCentre,
        Origin = Anchor.BottomCentre,
        RelativeSizeAxes = Axes.X,
        AutoSizeAxes = Axes.Y,
        ColumnDimensions = new[]
        {
            new Dimension(maxSize: 150),
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
                        Text = ModuleSetting.Metadata.KeyTitle,
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
                        Text = ModuleSetting.Metadata.ValueTitle,
                        Font = FrameworkFont.Regular.With(size: 20)
                    }
                }
            }
        }
    };
}

public partial class DrawableMutableKeyValuePairListModuleSettingItem : DrawableListModuleSettingItem<MutableKeyValuePair>
{
    public DrawableMutableKeyValuePairListModuleSettingItem(MutableKeyValuePair item)
        : base(item)
    {
        Add(new GridContainer
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            ColumnDimensions = new[]
            {
                new Dimension(maxSize: 150),
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
                        ValidCurrent = item.Key.GetBoundCopy()
                    },
                    null,
                    new StringTextBox
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = 30,
                        ValidCurrent = item.Value.GetBoundCopy()
                    }
                }
            }
        });
    }
}
