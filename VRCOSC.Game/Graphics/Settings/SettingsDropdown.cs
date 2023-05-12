// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osuTK;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI;

namespace VRCOSC.Game.Graphics.Settings;

public partial class SettingsDropdown<T> : VRCOSCDropdown<T>
{
    protected override DropdownHeader CreateHeader() => new VRCOSCSettingsDropdownHeader();

    public partial class VRCOSCSettingsDropdownHeader : DropdownHeader
    {
        protected override LocalisableString Label
        {
            get => Text.Text;
            set => Text.Text = value;
        }

        protected readonly SpriteText Text;
        public readonly SpriteIcon Icon;

        public VRCOSCSettingsDropdownHeader()
        {
            Foreground.Padding = new MarginPadding(10);

            AutoSizeAxes = Axes.None;
            CornerRadius = 5;
            Masking = true;
            BorderColour = ThemeManager.Current[ThemeAttribute.Border];
            BorderThickness = 2;
            Height = 25;

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
