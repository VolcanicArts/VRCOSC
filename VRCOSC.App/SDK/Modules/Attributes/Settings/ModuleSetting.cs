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
    /// <summary>
    /// The metadata for this <see cref="ModuleSetting"/>
    /// </summary>
    public new ModuleSettingMetadata Metadata => (ModuleSettingMetadata)base.Metadata;

    public Action? OnSettingChange;

    /// <summary>
    /// Creates a new <see cref="UserControl"/> instance as set in the <see cref="Metadata"/>
    /// </summary>
    public UserControl? ViewInstance
    {
        get
        {
            try
            {
                return (UserControl)Activator.CreateInstance(Metadata.ViewType, this)!;
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e, $"View instance creation failed for module setting {Metadata.Title}\nThis is usually caused by version mismatch");
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

    public override object GetSerialisableValue() => GetRawValue();

    protected ModuleSetting(ModuleSettingMetadata metadata)
        : base(metadata)
    {
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
