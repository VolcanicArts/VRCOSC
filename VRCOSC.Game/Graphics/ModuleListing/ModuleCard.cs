// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using VRCOSC.Game.Graphics.Containers.UI.Button;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleListing;

public sealed class ModuleCard : Container, IFilterable
{
    private readonly Module module;

    public ModuleCard(Module module)
    {
        this.module = module;

        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Masking = true;
        CornerRadius = 5;

        FilterTerms = new List<LocalisableString>
        {
            module.Title
        };
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        TextFlowContainer metadataTextFlow;
        Container editButton;
        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray2
            },
            new Box
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.Y,
                Width = 5,
                Colour = module.Colour
            },
            new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Horizontal,
                Padding = new MarginPadding
                {
                    Left = 5
                },
                Children = new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fit,
                        Padding = new MarginPadding(7),
                        Child = new IconButton
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            IconPadding = 5,
                            Stateful = true,
                            Masking = true,
                            CornerRadius = 5,
                            State = (BindableBool)module.Enabled.GetBoundCopy()
                        }
                    },
                    metadataTextFlow = new TextFlowContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding
                        {
                            Vertical = 5
                        }
                    }
                }
            },
            editButton = new Container
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                Padding = new MarginPadding(7),
                Child = new IconButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Icon = FontAwesome.Solid.Get(0xF013),
                    IconPadding = 5,
                    CornerRadius = 5,
                    //Action = () => screenManager.EditModule(sourceModule),
                    BackgroundColour = VRCOSCColour.Gray5
                }
            },
        };

        editButton.Alpha = module.HasAttributes ? 1 : 0;

        metadataTextFlow.AddText(module.Title, t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 25);
        });
        metadataTextFlow.AddParagraph(module.Description, t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 20);
            t.Colour = VRCOSCColour.GrayC;
        });
    }

    public IEnumerable<LocalisableString> FilterTerms { get; }

    public bool MatchingFilter
    {
        set => Alpha = value ? 1 : 0;
    }

    public bool FilteringActive { get; set; }
}
