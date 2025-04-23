// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VRCOSC.App.Utils;

public sealed class ObservableVector2 : INotifyPropertyChanged
{
    private double x;
    private double y;

    public double X
    {
        get => x;
        set
        {
            x = value;
            OnPropertyChanged();
        }
    }

    public double Y
    {
        get => y;
        set
        {
            y = value;
            OnPropertyChanged();
        }
    }

    public ObservableVector2(double x = 0, double y = 0)
    {
        this.x = x;
        this.y = y;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}