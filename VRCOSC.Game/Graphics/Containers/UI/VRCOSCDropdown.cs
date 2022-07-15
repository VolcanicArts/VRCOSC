// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;

namespace VRCOSC.Game.Graphics.Containers.UI;

public class VRCOSCDropdown<T> : Dropdown<T>
{
    protected override DropdownMenu CreateMenu() => new VRCOSCDropdownMenu();

    protected override DropdownHeader CreateHeader() => new VRCOSCDropdownHeader();

    private class VRCOSCDropdownHeader : DropdownHeader
    {
        private readonly SpriteText label;

        protected override LocalisableString Label
        {
            get => label.Text;
            set => label.Text = value;
        }

        public VRCOSCDropdownHeader()
        {
            var font = FrameworkFont.Regular.With(size: 30);

            Foreground.Padding = new MarginPadding(5);
            BackgroundColour = VRCOSCColour.Gray4;
            BackgroundColourHover = VRCOSCColour.Gray5;

            Children = new[]
            {
                label = new SpriteText
                {
                    AlwaysPresent = true,
                    Font = font,
                    Height = font.Size,
                },
            };
        }
    }

    private class VRCOSCDropdownMenu : DropdownMenu
    {
        protected override Menu CreateSubMenu() => new BasicMenu(Direction.Vertical);

        protected override DrawableDropdownMenuItem CreateDrawableDropdownMenuItem(MenuItem item) => new DrawableVRCOSCDropdownMenuItem(item);

        protected override ScrollContainer<Drawable> CreateScrollContainer(Direction direction)
        {
            return new BasicScrollContainer(direction)
            {
                ClampExtension = 0
            };
        }

        private class DrawableVRCOSCDropdownMenuItem : DrawableDropdownMenuItem
        {
            public DrawableVRCOSCDropdownMenuItem(MenuItem item)
                : base(item)
            {
                Foreground.Padding = new MarginPadding(5);
                BackgroundColour = VRCOSCColour.Gray3;
                BackgroundColourSelected = BackgroundColour;
                BackgroundColourHover = VRCOSCColour.Gray6;
            }

            protected override Drawable CreateContent() => new SpriteText
            {
                Font = FrameworkFont.Regular.With(size: 20)
            };

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);
                // this only works when a schedule is used despite being on the update thread
                // meaning this is called straight away anyway
                Schedule(() => Background.Colour = BackgroundColour);
            }
        }
    }
}
