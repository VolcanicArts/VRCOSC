// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using VRCOSC.Graphics.UI.List;
using VRCOSC.OSC.VRChat;

namespace VRCOSC.Screens.Main.Run;

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

    protected override bool AnimatePositionChange => true;

    protected override Drawable CreateHeader() => new ParameterListHeader(title);

    [BackgroundDependencyLoader]
    private void load()
    {
        appManager.State.BindValueChanged(onAppManagerStateChange);
    }

    private void onAppManagerStateChange(ValueChangedEvent<AppManagerState> e) => Scheduler.Add(() =>
    {
        if (e.NewValue == AppManagerState.Starting)
        {
            listingCache.Clear();
            Clear();
        }
    }, false);

    public void UpdateParameterValue(VRChatOscMessage message)
    {
        if (listingCache.TryGetValue(message.Address, out var drawableParameter))
        {
            drawableParameter.UpdateValue(message.ParameterValue);
        }
        else
        {
            var address = message.IsAvatarParameter ? message.ParameterName : message.Address;
            var newDrawableParameter = new DrawableParameter(address, message.ParameterValue);
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
