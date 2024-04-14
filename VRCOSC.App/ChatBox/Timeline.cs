// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using VRCOSC.App.ChatBox.Clips;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ChatBox;

public class Timeline : INotifyPropertyChanged
{
    private const int default_layer_count = 32;
    private const int default_length_seconds = 60;

    public int LayerCount { get; } = default_layer_count;
    public Observable<TimeSpan> Length { get; } = new(TimeSpan.FromSeconds(default_length_seconds));

    public int LengthSeconds
    {
        set
        {
            var secondsValue = value;
            secondsValue = Math.Clamp(secondsValue, 1, 60 * 4);
            Length.Value = TimeSpan.FromSeconds(secondsValue);
        }
        get => (int)Length.Value.TotalSeconds;
    }

    public float Resolution => 1f / (float)Length.Value.TotalSeconds;

    public ObservableCollection<Layer> Layers { get; } = new();

    public Timeline()
    {
        SetupLayers();
    }

    public void SetupLayers()
    {
        Layers.Clear();

        Length.Subscribe(_ => Layers.ForEach(layer => layer.UpdateUIBinds()));

        for (var i = 0; i < LayerCount; i++)
        {
            Layers.Add(new Layer(i));
        }
    }

    public void Init()
    {
        Layers.ForEach(layer => layer.Init());
        OnPropertyChanged(nameof(LengthSeconds));
    }

    public Layer FindLayerOfClip(Clip clip)
    {
        return Layers.First(layer => layer.Clips.Contains(clip));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
