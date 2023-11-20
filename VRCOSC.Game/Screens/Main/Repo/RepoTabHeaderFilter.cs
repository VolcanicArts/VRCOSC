// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osuTK;

namespace VRCOSC.Game.Screens.Main.Repo;

public partial class RepoTabHeaderFilter : ClickableContainer
{
    [Resolved]
    private RepoTab repoTab { get; set; } = null!;

    private readonly BindableBool open = new();

    private FilterDropdown dropdown = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new FilterHeader(),
            dropdown = new FilterDropdown(),
        };

        Action += () => open.Toggle();

        open.BindValueChanged(onOpenStateChanged, true);

        addTypeFilter();
    }

    private Bindable<bool> typeOfficialBindable = null!;
    private Bindable<bool> typeCuratedBindable = null!;
    private Bindable<bool> typeCommunityBindable = null!;
    private Bindable<bool> releaseUnavailableBindable = null!;
    private Bindable<bool> releaseIncompatibleBindable = null!;

    private void addTypeFilter()
    {
        typeOfficialBindable = new Bindable<bool>(repoTab.Filter.Value.HasFlagFast(PackageListingFilter.Type_Official));
        typeCuratedBindable = new Bindable<bool>(repoTab.Filter.Value.HasFlagFast(PackageListingFilter.Type_Curated));
        typeCommunityBindable = new Bindable<bool>(repoTab.Filter.Value.HasFlagFast(PackageListingFilter.Type_Community));
        releaseUnavailableBindable = new Bindable<bool>(repoTab.Filter.Value.HasFlagFast(PackageListingFilter.Release_Unavailable));
        releaseIncompatibleBindable = new Bindable<bool>(repoTab.Filter.Value.HasFlagFast(PackageListingFilter.Release_Incompatible));

        typeOfficialBindable.BindValueChanged(e => handleFilterChange(e.NewValue, PackageListingFilter.Type_Official));
        typeCuratedBindable.BindValueChanged(e => handleFilterChange(e.NewValue, PackageListingFilter.Type_Curated));
        typeCommunityBindable.BindValueChanged(e => handleFilterChange(e.NewValue, PackageListingFilter.Type_Community));
        releaseUnavailableBindable.BindValueChanged(e => handleFilterChange(e.NewValue, PackageListingFilter.Release_Unavailable));
        releaseIncompatibleBindable.BindValueChanged(e => handleFilterChange(e.NewValue, PackageListingFilter.Release_Incompatible));

        dropdown.Add(new FilterEntry("Type").AddFilter("Official", typeOfficialBindable).AddFilter("Curated", typeCuratedBindable).AddFilter("Community", typeCommunityBindable));
        dropdown.Add(new FilterEntry("Release").AddFilter("Unavailable", releaseUnavailableBindable).AddFilter("Incompatible", releaseIncompatibleBindable));
    }

    public override bool AcceptsFocus => true;
    protected override void OnFocusLost(FocusLostEvent e) => open.Value = false;

    private void handleFilterChange(bool value, PackageListingFilter filter)
    {
        if (value)
            repoTab.Filter.Value |= filter;
        else
            repoTab.Filter.Value &= ~filter;
    }

    private void onOpenStateChanged(ValueChangedEvent<bool> e)
    {
        if (e.NewValue)
        {
            dropdown.MoveToY(5, 100, Easing.OutQuint);
            dropdown.ScaleTo(Vector2.One, 100, Easing.OutQuint);
        }
        else
        {
            dropdown.MoveToY(0, 100, Easing.OutQuint);
            dropdown.ScaleTo(new Vector2(1, 0), 100, Easing.OutQuint);
        }
    }
}
