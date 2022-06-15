// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing.Settings;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing;

public class SettingsFlow : FillFlowContainer
{
    [Resolved]
    private Bindable<Module> SourceModule { get; set; }

    [BackgroundDependencyLoader]
    private void load()
    {
        FillFlowContainer<AttributeCard> settingsFlow;

        InternalChildren = new Drawable[]
        {
            new SpriteText
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Font = FrameworkFont.Regular.With(size: 50),
                Text = "Settings"
            },
            settingsFlow = new FillFlowContainer<AttributeCard>
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10)
            },
        };

        SourceModule.BindValueChanged(_ =>
        {
            if (SourceModule.Value == null) return;

            settingsFlow.Clear();

            SourceModule.Value.Settings.ForEach(pair =>
            {
                var (_, attributeData) = pair;

                switch (Type.GetTypeCode(attributeData.Attribute.Value.GetType()))
                {
                    case TypeCode.String:
                        settingsFlow.Add(new StringAttributeCard(attributeData));
                        break;

                    case TypeCode.Int32:
                        settingsFlow.Add(new IntAttributeCard(attributeData));
                        break;

                    case TypeCode.Boolean:
                        settingsFlow.Add(new BoolAttributeCard(attributeData));
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
        }, true);
    }
}
