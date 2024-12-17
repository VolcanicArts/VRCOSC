// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows.Controls;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Modules.Attributes.Settings;

public abstract class ModuleSetting
{
    internal Module ParentModule { get; set; } = null!;

    /// <summary>
    /// The title of this <see cref="ModuleSetting"/>
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// The description of this <see cref="ModuleSetting"/>
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// The type of <see cref="UserControl"/> to instance when this <see cref="ModuleSetting"/> needs to be rendered
    /// </summary>
    public Type ViewType { get; }

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

    public Action? OnSettingChange;

    public Observable<bool> IsEnabled { get; } = new(true);

    internal abstract bool Deserialise(object? ingestValue);
    internal abstract object? Serialise();
    internal abstract bool IsDefault();
    public abstract bool GetValue<T>(out T returnValue);

    protected ModuleSetting(string title, string description, Type viewType)
    {
        Title = title;
        Description = description;
        ViewType = viewType;
    }
}