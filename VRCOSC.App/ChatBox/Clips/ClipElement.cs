// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using VRCOSC.App.ChatBox.Clips.Variables;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ChatBox.Clips;

public class ClipElement : INotifyPropertyChanged
{
    public Observable<string> Format { get; set; } = new(string.Empty);
    public Observable<bool> Enabled { get; set; } = new();
    public Observable<bool> ShowTyping { get; set; } = new();
    public Observable<bool> UseMinimalBackground { get; set; } = new();
    public ObservableCollection<ClipVariable> Variables { get; init; } = new();

    public Observable<bool> IsChosenElement { get; } = new();

    public virtual string DisplayName => string.Empty;

    public virtual bool IsDefault => Format.IsDefault && Enabled.IsDefault && ShowTyping.IsDefault && UseMinimalBackground.IsDefault && Variables.All(clipVariable => clipVariable.IsDefault());
    public virtual bool ShouldBeVisible => true;

    public virtual void UpdateUI()
    {
        OnPropertyChanged(nameof(ShouldBeVisible));
    }

    public string RunFormatting()
    {
        IsChosenElement.Value = true;

        var localFormat = Format.Value;

        for (var i = 0; i < Variables.Count; i++)
        {
            var variable = Variables[i];
            localFormat = localFormat.Replace("{" + i + "}", variable.GetFormattedValue());
        }

        return localFormat;
    }

    public virtual ClipElement Clone(bool copySettings = true)
    {
        var clone = (ClipElement)Activator.CreateInstance(GetType())!;

        if (copySettings)
        {
            clone.Format.Value = Format.Value;
            clone.Enabled.Value = Enabled.Value;
            clone.ShowTyping.Value = ShowTyping.Value;
            clone.UseMinimalBackground.Value = UseMinimalBackground.Value;
            clone.Variables.AddRange(Variables.Select(variable => variable.Clone()));
        }

        return clone;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}