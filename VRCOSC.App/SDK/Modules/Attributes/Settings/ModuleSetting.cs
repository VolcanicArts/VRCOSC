// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Modules.Attributes.Settings;

public abstract class ModuleSetting : ModuleAttribute, INotifyPropertyChanged
{
    internal Module ParentModule { get; set; }

    /// <summary>
    /// The type of <see cref="UserControl"/> to instance when this <see cref="ModuleSetting"/> needs to be rendered
    /// </summary>
    public Type ViewType { get; }

    /// <summary>
    /// Called whenever this <see cref="ModuleSetting"/>'s value changed
    /// </summary>
    public Action? OnSettingChange;

    /// <summary>
    /// Creates a new <see cref="UserControl"/> instance of <see cref="ViewType"/>
    /// </summary>
    public UserControl? ViewInstance
    {
        get
        {
            try
            {
                return (UserControl)Activator.CreateInstance(ViewType, ParentModule, this)!;
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e, $"View instance creation failed for module setting {Title}\nThis is usually caused by version mismatch");
                return null;
            }
        }
    }

    private bool isEnabled = true;

    /// <summary>
    /// When true, the user can edit this <see cref="ModuleSetting"/>, otherwise, the UI prevents the user from editing this <see cref="ModuleSetting"/>
    /// </summary>
    public bool IsEnabled
    {
        get => isEnabled;
        set
        {
            isEnabled = value;
            OnPropertyChanged();
        }
    }

    protected ModuleSetting(string title, string description, Type viewType)
        : base(title, description)
    {
        ViewType = viewType;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
