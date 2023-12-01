// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.SDK;

namespace VRCOSC.Game.Screens.Main.Modules.Parameters;

public partial class ModuleParametersContainer : VisibilityContainer
{
    protected override bool OnMouseDown(MouseDownEvent e) => true;
    protected override bool OnClick(ClickEvent e) => true;
    protected override bool OnHover(HoverEvent e) => true;
    protected override bool OnScroll(ScrollEvent e) => true;

    private Module? module;
    private readonly ModuleParametersList parameterList;

    public ModuleParametersContainer()
    {
        InternalChild = new Container
        {
            RelativeSizeAxes = Axes.Both,
            Masking = true,
            CornerRadius = 10,
            BorderThickness = 3,
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colours.GRAY1
                },
                new Container
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    Height = 56,
                    Padding = new MarginPadding(10),
                    Children = new Drawable[]
                    {
                        new TextButton
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            RelativeSizeAxes = Axes.Y,
                            Width = 200,
                            BackgroundColour = Colours.BLUE0,
                            TextContent = "Reset To Default",
                            TextFont = Fonts.REGULAR.With(size: 25),
                            TextColour = Colours.WHITE0,
                            Action = () => module?.Parameters.Values.ForEach(moduleParameter => moduleParameter.SetDefault())
                        },
                        new IconButton
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Size = new Vector2(36),
                            CornerRadius = 5,
                            Icon = Icons.Exit,
                            IconSize = 24,
                            IconColour = Colours.WHITE0,
                            BackgroundColour = Colours.RED0,
                            Action = Hide
                        }
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding
                    {
                        Horizontal = 10,
                        Top = 56,
                        Bottom = 10
                    },
                    Child = parameterList = new ModuleParametersList
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both
                    }
                }
            }
        };
    }

    public void SetModule(Module? module)
    {
        this.module = module;
        parameterList.ClearList();
        module?.Parameters.ForEach(parameterPair => parameterList.AddList(new ModuleParameterInstance(parameterPair.Value)));
    }

    protected override void PopIn()
    {
        this.FadeInFromZero(250, Easing.OutCubic);
    }

    protected override void PopOut()
    {
        this.FadeOutFromOne(250, Easing.OutCubic).Finally(_ => SetModule(null));
    }
}
