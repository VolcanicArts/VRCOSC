using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI.TextBox;

namespace VRCOSC.Game.Graphics.Containers.Module;

public class ModuleSettingStringContainer : ModuleSettingContainer
{
    [BackgroundDependencyLoader]
    private void load()
    {
        VRCOSCTextBox textBox;

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
                        Text = SourceModule.Metadata.Settings[Key].DisplayName
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
                        Text = SourceModule.Metadata.Settings[Key].Description
                    },
                    textBox = new VRCOSCTextBox
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.5f, 0.8f),
                        Text = (string)SourceModule.Data.Settings[Key].Value
                    }
                }
            }
        };

        textBox.OnCommit += (_, _) =>
        {
            SourceModule.UpdateSetting(Key, textBox.Text);
        };
    }
}
