// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using VRCOSC.App.ChatBox.Clips;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ChatBox;

public class Timeline
{
    private const int default_layer_count = 8;
    private const int default_length_seconds = 60;
    public Observable<int> LayerCount { get; } = new(default_layer_count);
    public Observable<TimeSpan> Length { get; } = new(TimeSpan.FromSeconds(default_length_seconds));

    public int LengthSeconds => (int)Length.Value.TotalSeconds;
    public float Resolution => 1f / (float)Length.Value.TotalSeconds;

    public ObservableCollection<Layer> Layers { get; } = new();

    public Layer FindLayerOfClip(Clip clip)
    {
        return Layers.First(layer => layer.Clips.Contains(clip));
    }
}
