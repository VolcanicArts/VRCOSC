// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.UI.Core;

/// <inheritdoc />
/// <summary>
/// Used to wrap a <see cref="T:System.Windows.DependencyObject" /> in a regular object for static references
/// </summary>
public sealed class BindingProxy : Freezable
{
    protected override Freezable CreateInstanceCore() => new BindingProxy();

    public static readonly DependencyProperty DataProperty = DependencyProperty.Register(nameof(Data), typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));

    public object Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }
}