// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.UI.Core;
using VRCOSC.App.Utils;

// ReSharper disable MemberCanBePrivate.Global

namespace VRCOSC.App.UI.Windows.Modules;

public sealed partial class ModuleParametersWindow : IManagedWindow
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
        Module.Parameters.Values.ForEach(parameter => parameter.Name.SetDefault());
    }

    private void ModuleParametersWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        Module.Serialise();
    }

    public object GetComparer() => Module;
}

public class ParameterModeToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ParameterMode parameterMode)
        {
            return parameterMode switch
            {
                ParameterMode.Read => "Receive",
                ParameterMode.Write => "Send",
                ParameterMode.ReadWrite => "Send/Receive",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}