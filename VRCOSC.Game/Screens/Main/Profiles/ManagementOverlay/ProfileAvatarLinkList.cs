// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Graphics;
using VRCOSC.Profiles;

namespace VRCOSC.Screens.Main.Profiles.ManagementOverlay;

public partial class ProfileAvatarLinkList : Container
{
    private readonly Profile profile;

    protected override FillFlowContainer Content { get; }

    private readonly FillFlowContainer flowWrapper;
    private readonly ProfileAvatarLinkListHeader header;
    private readonly Box footer;

    public ProfileAvatarLinkList(Profile profile)
    {
        this.profile = profile;
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        InternalChild = flowWrapper = new FillFlowContainer
        {
            Name = "Flow Wrapper",
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Masking = true,
            CornerRadius = 5,
            Children = new Drawable[]
            {
                header = new ProfileAvatarLinkListHeader
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    Height = 50
                },
                Content = new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical
                },
                footer = new Box
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    Height = 5,
                    Colour = Colours.GRAY0
                }
            }
        };
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        profile.LinkedAvatars.BindCollectionChanged(onLinkedAvatarCollectionChanged, true);
    }

    private void onLinkedAvatarCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Clear();

        var even = false;
        profile.LinkedAvatars.ForEach(linkedAvatar =>
        {
            Add(new ProfileAvatarLinkListInstance(profile, linkedAvatar, even));
            even = !even;
        });
    }

    protected override void UpdateAfterChildren()
    {
        footer.Alpha = this.Any() ? 1 : 0;
    }
}
