using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osuTK;
using VRCOSC.Game.Graphics.Drawables;

namespace VRCOSC.Game.Graphics.Containers;

public class SenderFlowContainer : Container
{
    private readonly FillFlowContainer senderFlow;

    public SenderFlowContainer()
    {
        Child = new BasicScrollContainer
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            ClampExtension = 20,
            ScrollbarVisible = false,
            Child = senderFlow = new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Full,
                Spacing = new Vector2(10),
                Padding = new MarginPadding(10)
            }
        };
    }

    public void AddSender()
    {
        Scheduler.Add(addSender);
    }

    private void addSender()
    {
        senderFlow.Add(new SenderContainer()
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            Size = new Vector2(300, 200),
            Masking = true,
            CornerRadius = 10,
            EdgeEffect = new EdgeEffectParameters
            {
                Colour = Colour4.Black.Opacity(0.6f),
                Radius = 2.5f,
                Type = EdgeEffectType.Shadow,
                Offset = new Vector2(0.0f, 1.5f)
            }
        });
    }
}
