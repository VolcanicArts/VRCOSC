// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using VRCOSC.App.Modules;

namespace VRCOSC.App.Pages.Modules.Parameters;

public partial class ModuleParametersWindow
{
    private readonly Module module;

    public ModuleParametersWindow(Module module)
    {
        InitializeComponent();

        this.module = module;
        DataContext = this.module;
        ParameterGrid.DataContext = this.module;

        SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => evaluateContentHeight();

    private void evaluateContentHeight()
    {
        var contentHeight = module.Parameters.Count * 50;
        var targetHeight = GridContainer.ActualHeight - 55;
        module.ParameterScrollViewerHeight = contentHeight >= targetHeight ? targetHeight : double.NaN;
    }
}
