// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.Screen;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleAttributes;

public partial class ModuleAttributesScreen : BaseScreen
{
    private ModuleAttributeFlowContainer settingFlow = null!;
    private ModuleAttributeFlowContainer parameterFlow = null!;

    [Resolved(name: "EditingModule")]
    private Bindable<Module?> editingModule { get; set; } = null!;

    protected override BaseHeader CreateHeader() => new ModuleAttributesHeader();

    protected override Drawable CreateBody() => new Container
    {
        RelativeSizeAxes = Axes.Both,
        Padding = new MarginPadding
        {
            Horizontal = 10,
            Bottom = 10,
            Top = 5
        },
        Child = new GridContainer
        {
            RelativeSizeAxes = Axes.Both,
            ColumnDimensions = new[]
            {
                new Dimension(),
                new Dimension(GridSizeMode.Absolute, 5),
                new Dimension()
            },
            Content = new[]
            {
                new Drawable?[]
                {
                    settingFlow = new ModuleAttributeFlowContainer("Setting"),
                    null,
                    parameterFlow = new ModuleAttributeFlowContainer("Parameter")
                }
            }
        }
    };

    protected override void LoadComplete()
    {
        editingModule.BindValueChanged(e =>
        {
            if (e.NewValue is null) return;

            settingFlow.AttributeFlow.AttributeList.Clear();
            settingFlow.AttributeFlow.AttributeList.AddRange(e.NewValue.Settings.Values);

            parameterFlow.AttributeFlow.AttributeList.Clear();
            parameterFlow.AttributeFlow.AttributeList.AddRange(e.NewValue.Parameters.Values);
        }, true);
    }
}
