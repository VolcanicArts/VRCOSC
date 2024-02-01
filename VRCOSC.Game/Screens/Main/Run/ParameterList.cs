// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
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
    private readonly object listingCacheLock = new();

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
        var drawableParameter = this.FirstOrDefault(drawableParameter => drawableParameter.ParameterAddress == message.ParameterName);

        if (drawableParameter is not null)
        {
            drawableParameter.UpdateValue(message.ParameterValue);
            return;
        }

        if (listingCache.TryGetValue(message.Address, out var cachedDrawableParameter))
        {
            cachedDrawableParameter.UpdateValue(message.ParameterValue);
            return;
        }

        lock (listingCacheLock)
        {
            var address = message.IsAvatarParameter ? message.ParameterName : message.Address;
            var newDrawableParameter = new DrawableParameter(address, message.ParameterValue);
            listingCache.Add(message.Address, newDrawableParameter);
        }
    }

    protected override void Update()
    {
        Scheduler.Add(() =>
        {
            if (listingCache.Any())
            {
                lock (listingCacheLock)
                {
                    listingCache.Values.ForEach(Add);
                    listingCache.Clear();
                }

                Scheduler.Add(() =>
                {
                    var depth = 0f;

                    foreach (var sortedDrawableParameter in this.OrderBy(drawableParameter => drawableParameter.ParameterAddress))
                    {
                        ChangeListChildPosition(sortedDrawableParameter, depth);
                        depth++;
                    }
                });
            }
        });
    }
}
