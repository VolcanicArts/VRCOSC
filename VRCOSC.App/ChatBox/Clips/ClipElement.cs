// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using VRCOSC.App.ChatBox.Clips.Variables;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ChatBox.Clips;

public class ClipElement : INotifyPropertyChanged
{
    public Observable<string> Format { get; set; } = new(string.Empty);
    public Observable<bool> Enabled { get; set; } = new();

    public ObservableCollection<ClipVariable> Variables { get; init; } = new();

    public virtual string DisplayName => string.Empty;
    public virtual bool IsDefault => Format.IsDefault && Enabled.IsDefault;
    public virtual bool ShouldBeVisible => true;
    public Visibility HasVariables => Variables.Any() ? Visibility.Visible : Visibility.Collapsed;

    public virtual void UpdateUI()
    {
        OnPropertyChanged(nameof(IsDefault));
        OnPropertyChanged(nameof(ShouldBeVisible));
        OnPropertyChanged(nameof(HasVariables));
    }

    public string RunFormatting()
    {
        var localFormat = Format.Value;
        return string.Format(localFormat, Variables.Select(clipVariable => (object)clipVariable.GetFormattedValue()).ToArray());
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
