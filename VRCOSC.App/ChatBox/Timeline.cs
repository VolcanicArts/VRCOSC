// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.ObjectModel;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ChatBox;

public class Timeline
{
    private const int default_layer_count = 8;
    private const int default_length_seconds = 60;
    public Observable<int> LayerCount { get; } = new(default_layer_count);
    public Observable<TimeSpan> Length { get; } = new(TimeSpan.FromSeconds(default_length_seconds));

    public ObservableCollection<Layer> Layers = new();
}
