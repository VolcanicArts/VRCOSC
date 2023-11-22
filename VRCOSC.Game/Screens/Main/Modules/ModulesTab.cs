// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Screens.Main.Modules.Parameters;
using VRCOSC.Game.Screens.Main.Modules.Settings;
using Module = VRCOSC.Game.SDK.Module;

namespace VRCOSC.Game.Screens.Main.Modules;

[Cached]
public partial class ModulesTab : Container
{
    [Resolved]
    private VRCOSCGame game { get; set; } = null!;

    [Resolved]
    private AppManager appManager { get; set; } = null!;

    private BufferedContainer bufferedContainer = null!;
    private FillFlowContainer assemblyFlowContainer = null!;
    private TextFlowContainer noModulesText = null!;
    private Box backgroundDarkener = null!;
    private ModuleSettingsContainer moduleSettingsContainer = null!;
    private ModuleParametersContainer moduleParametersContainer = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;

        Children = new Drawable[]
        {
            bufferedContainer = new BufferedContainer
            {
                RelativeSizeAxes = Axes.Both,
                BackgroundColour = Colours.BLACK,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colours.GRAY1
                    },
                    new BasicScrollContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        ClampExtension = 0,
                        ScrollContent =
                        {
                            Child = assemblyFlowContainer = new FillFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Padding = new MarginPadding(10),
                                Spacing = new Vector2(0, 10)
                            }
                        }
                    },
                    noModulesText = new TextFlowContainer(t => { t.Font = Fonts.REGULAR.With(size: 40); })
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        TextAnchor = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Text = "You have no modules!\nInstall some using the package manager"
                    }
                }
            },
            backgroundDarkener = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.Transparent
            },
            moduleSettingsContainer = new ModuleSettingsContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(20)
            },
            moduleParametersContainer = new ModuleParametersContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(20)
            }
        };

        game.OnListingRefresh += refresh;
        refresh();

        setupBlur();
    }

    public void ShowSettings(Module module)
    {
        moduleSettingsContainer.SetModule(module);
        moduleSettingsContainer.Show();
    }

    public void ShowParameters(Module module)
    {
        moduleParametersContainer.SetModule(module);
        moduleParametersContainer.Show();
    }

    private void setupBlur()
    {
        moduleSettingsContainer.State.BindValueChanged(e =>
        {
            bufferedContainer.TransformTo(nameof(BufferedContainer.BlurSigma), e.NewValue == Visibility.Visible ? new Vector2(5) : new Vector2(0), 250, Easing.OutCubic);
            backgroundDarkener.FadeColour(e.NewValue == Visibility.Visible ? Colours.BLACK.Opacity(0.25f) : Colours.Transparent, 250, Easing.OutCubic);
        });

        moduleParametersContainer.State.BindValueChanged(e =>
        {
            bufferedContainer.TransformTo(nameof(BufferedContainer.BlurSigma), e.NewValue == Visibility.Visible ? new Vector2(5) : new Vector2(0), 250, Easing.OutCubic);
            backgroundDarkener.FadeColour(e.NewValue == Visibility.Visible ? Colours.BLACK.Opacity(0.25f) : Colours.Transparent, 250, Easing.OutCubic);
        });
    }

    private void refresh()
    {
        assemblyFlowContainer.Clear();

        appManager.ModuleManager.LocalModules.ForEach(pair =>
        {
            var title = pair.Key.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "Unknown";
            assemblyFlowContainer.Add(new ModuleAssemblyContainer(title, pair.Value, true));
        });

        appManager.ModuleManager.RemoteModules.ForEach(pair =>
        {
            var title = pair.Key.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "Unknown";
            assemblyFlowContainer.Add(new ModuleAssemblyContainer(title, pair.Value));
        });

        noModulesText.Alpha = assemblyFlowContainer.Any() ? 0 : 1;
    }
}
