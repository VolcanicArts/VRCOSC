// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.UI.List;
using VRCOSC.Game.OSC.VRChat;

namespace VRCOSC.Game.Screens.Main.Run;

public partial class ParameterList : HeightLimitedScrollableList<DrawableParameter>
{
    [Resolved]
    private AppManager appManager { get; set; } = null!;

    private readonly string title;

    private readonly SortedDictionary<string, DrawableParameter> listingCache = new();

    public ParameterList(string title)
    {
        this.title = title;
    }

    protected override Drawable CreateHeader() => new Container
    {
        Anchor = Anchor.TopCentre,
        Origin = Anchor.TopCentre,
        RelativeSizeAxes = Axes.X,
        Height = 40,
        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY0
            },
            new TextFlowContainer(t =>
            {
                t.Font = Fonts.BOLD.With(size: 30);
                t.Colour = Colours.WHITE2;
            })
            {
                RelativeSizeAxes = Axes.Both,
                TextAnchor = Anchor.Centre,
                Text = title
            }
        }
    };

    [BackgroundDependencyLoader]
    private void load()
    {
        appManager.State.BindValueChanged(onAppManagerStateChange);
    }

    private void onAppManagerStateChange(ValueChangedEvent<AppManagerState> e)
    {
        if (e.NewValue == AppManagerState.Starting)
        {
            listingCache.Clear();
            Clear();
        }
    }

    public void UpdateParameterValue(VRChatOscMessage message)
    {
        if (listingCache.TryGetValue(message.Address, out var drawableParameter))
        {
            drawableParameter.UpdateValue(message.ParameterValue);
        }
        else
        {
            var newDrawableParameter = new DrawableParameter(message.Address, message.ParameterValue);
            listingCache.Add(message.Address, newDrawableParameter);
            Add(newDrawableParameter);

            var depth = 0f;

            foreach (var sortedDrawableParameter in listingCache.Values)
            {
                ChangeListChildPosition(sortedDrawableParameter, depth);
                depth++;
            }
        }
    }
}
