// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.SDK.Modules.Attributes.Parameters;
using VRCOSC.App.Utils;

// ReSharper disable MemberCanBePrivate.Global

namespace VRCOSC.App.Pages.Modules.Parameters;

public sealed partial class ModuleParametersWindow
{
    public Module Module { get; }

    public List<ModuleParameter> UIParameters => Module.Parameters.Select(pair => pair.Value).ToList();

    public ModuleParametersWindow(Module module)
    {
        InitializeComponent();

        Title = $"{module.Title.Pluralise()} Parameters";

        Module = module;
        DataContext = this;
    }

    private void ResetParameters_OnClick(object sender, RoutedEventArgs e)
    {
        Module.Parameters.Values.ForEach(parameter => parameter.SetDefault());
    }
}
