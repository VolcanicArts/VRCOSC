// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osuTK;
using osuTK.Graphics;

namespace VRCOSC.Game.Graphics.UI;

// Taken and modified from https://github.com/ppy/osu/blob/4bc26dbb487241e2bbae73751dbe9e93a4e427da/osu.Game/Graphics/UserInterface/OsuDropdown.cs
public sealed class VRCOSCDropdown<T> : Dropdown<T>
{
    private const float corner_radius = 5;

    protected override DropdownHeader CreateHeader() => new VRCOSCDropdownHeader();

    protected override DropdownMenu CreateMenu() => new VRCOSCDropdownMenu();

    #region OsuDropdownMenu

    private class VRCOSCDropdownMenu : DropdownMenu
    {
        public override bool HandleNonPositionalInput => State == MenuState.Open;

        public VRCOSCDropdownMenu()
        {
            CornerRadius = corner_radius;

            MaskingContainer.CornerRadius = corner_radius;
            Alpha = 0;

            ItemsContainer.Padding = new MarginPadding(5);
        }

        [BackgroundDependencyLoader(true)]
        private void load()
        {
            BackgroundColour = VRCOSCColour.Gray2;
            HoverColour = VRCOSCColour.Gray5;
            SelectionColour = VRCOSCColour.Gray5;
        }

        private bool wasOpened;

        protected override void AnimateOpen()
        {
            wasOpened = true;
            this.FadeIn(300, Easing.OutQuint);
        }

        protected override void AnimateClose()
        {
            if (wasOpened)
            {
                this.FadeOut(300, Easing.OutQuint);
            }
        }

        protected override void UpdateSize(Vector2 newSize)
        {
            if (Direction == Direction.Vertical)
            {
                Width = newSize.X;
                this.ResizeHeightTo(newSize.Y, 300, Easing.OutQuint);
            }
            else
            {
                Height = newSize.Y;
                this.ResizeWidthTo(newSize.X, 300, Easing.OutQuint);
            }
        }

        private Color4 hoverColour;

        public Color4 HoverColour
        {
            get => hoverColour;
            set
            {
                hoverColour = value;
                foreach (var c in Children.OfType<DrawableVRCOSCDropdownMenuItem>())
                    c.BackgroundColourHover = value;
            }
        }

        private Color4 selectionColour;

        public Color4 SelectionColour
        {
            get => selectionColour;
            set
            {
                selectionColour = value;
                foreach (var c in Children.OfType<DrawableVRCOSCDropdownMenuItem>())
                    c.BackgroundColourSelected = value;
            }
        }

        protected override Menu CreateSubMenu() => new BasicMenu(Direction.Vertical);

        protected override DrawableDropdownMenuItem CreateDrawableDropdownMenuItem(MenuItem item) => new DrawableVRCOSCDropdownMenuItem(item)
        {
            BackgroundColourHover = HoverColour,
            BackgroundColourSelected = SelectionColour
        };

        protected override ScrollContainer<Drawable> CreateScrollContainer(Direction direction) => new BasicScrollContainer(direction)
        {
            ClampExtension = 0,
            ScrollbarVisible = false
        };

        #region DrawableOsuDropdownMenuItem

        public class DrawableVRCOSCDropdownMenuItem : DrawableDropdownMenuItem
        {
            public new Color4 BackgroundColourHover
            {
                get => base.BackgroundColourHover;
                set
                {
                    base.BackgroundColourHover = value;
                    updateColours();
                }
            }

            public new Color4 BackgroundColourSelected
            {
                get => base.BackgroundColourSelected;
                set
                {
                    base.BackgroundColourSelected = value;
                    updateColours();
                }
            }

            private void updateColours()
            {
                BackgroundColour = BackgroundColourHover.Opacity(0);

                UpdateBackgroundColour();
                UpdateForegroundColour();
            }

            public DrawableVRCOSCDropdownMenuItem(MenuItem item)
                : base(item)
            {
                Foreground.Padding = new MarginPadding(2);

                Masking = true;
                CornerRadius = corner_radius;
            }

            protected override void UpdateBackgroundColour()
            {
                Background.FadeColour(IsPreSelected ? BackgroundColourHover : BackgroundColourSelected, 100, Easing.OutQuint);

                if (IsPreSelected || IsSelected)
                    Background.FadeIn(100, Easing.OutQuint);
                else
                    Background.FadeOut(600, Easing.OutQuint);
            }

            protected override void UpdateForegroundColour()
            {
                base.UpdateForegroundColour();

                if (Foreground.Children.FirstOrDefault() is Content content)
                    content.Hovering = IsHovered;
            }

            protected override Drawable CreateContent() => new Content();

            protected new class Content : CompositeDrawable, IHasText
            {
                public LocalisableString Text
                {
                    get => Label.Text;
                    set => Label.Text = value;
                }

                public readonly SpriteText Label;
                public readonly SpriteIcon Chevron;

                private const float chevron_offset = -3;

                public Content()
                {
                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;

                    InternalChildren = new Drawable[]
                    {
                        Chevron = new SpriteIcon
                        {
                            Icon = FontAwesome.Solid.ChevronRight,
                            Size = new Vector2(8),
                            Alpha = 0,
                            X = chevron_offset,
                            Margin = new MarginPadding { Left = 3, Right = 3 },
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                        },
                        Label = new SpriteText
                        {
                            X = 15,
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                        },
                    };
                }

                [BackgroundDependencyLoader]
                private void load()
                {
                    Chevron.Colour = Color4.Black;
                }

                private bool hovering;

                public bool Hovering
                {
                    get => hovering;
                    set
                    {
                        if (value == hovering)
                            return;

                        hovering = value;

                        if (hovering)
                        {
                            Chevron.FadeIn(400, Easing.OutQuint);
                            Chevron.MoveToX(0, 400, Easing.OutQuint);
                        }
                        else
                        {
                            Chevron.FadeOut(200);
                            Chevron.MoveToX(chevron_offset, 200, Easing.In);
                        }
                    }
                }
            }
        }

        #endregion
    }

    #endregion

    public class VRCOSCDropdownHeader : DropdownHeader
    {
        protected readonly SpriteText Text;

        protected override LocalisableString Label
        {
            get => Text.Text;
            set => Text.Text = value;
        }

        protected readonly SpriteIcon Icon;

        public VRCOSCDropdownHeader()
        {
            Foreground.Padding = new MarginPadding(10);

            AutoSizeAxes = Axes.None;
            Margin = new MarginPadding { Bottom = 4 };
            CornerRadius = corner_radius;
            Height = 40;

            Foreground.Child = new GridContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                },
                ColumnDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(GridSizeMode.AutoSize),
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
                            Font = FrameworkFont.Regular.With(size: 25)
                        },
                        Icon = new SpriteIcon
                        {
                            Icon = FontAwesome.Solid.ChevronDown,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Size = new Vector2(16),
                        },
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            BackgroundColour = VRCOSCColour.Gray2;
            BackgroundColourHover = VRCOSCColour.Gray4;
        }
    }
}
