using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;

namespace VRCOSC.Game.Graphics.Containers.Module;

public class ModuleSettingBoolContainer : ModuleSettingContainer
{
    [BackgroundDependencyLoader]
    private void load()
    {
        BasicCheckbox checkBox;

        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = Colour4.Black
            },
            new Container
            {
                Name = "Content",
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding
                {
                    Vertical = 10,
                    Horizontal = 15,
                },
                Children = new Drawable[]
                {
                    new TextFlowContainer(t => t.Font = FrameworkFont.Regular.With(size: 30))
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        AutoSizeAxes = Axes.Both,
                        Text = ModuleSetting.DisplayName
                    },
                    new TextFlowContainer(t =>
                    {
                        t.Font = FrameworkFont.Regular.With(size: 20);
                        t.Colour = VRCOSCColour.Gray9;
                    })
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
                        Text = ModuleSetting.Description
                    },
                    checkBox = new BasicCheckbox
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Current = { Value = (bool)ModuleSetting.Value }
                    }
                }
            }
        };
        checkBox.Current.BindValueChanged((e) => ModuleSetting.Value = e.NewValue);
    }
}
