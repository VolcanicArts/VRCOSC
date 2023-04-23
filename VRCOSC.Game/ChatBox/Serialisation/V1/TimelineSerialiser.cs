// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using osu.Framework.Platform;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.ChatBox.Serialisation.V1.Structures;

namespace VRCOSC.Game.ChatBox.Serialisation.V1;

public class TimelineSerialiser : ITimelineSerialiser
{
    private const string file_name = @"chatbox.json";
    private readonly Storage storage;

    public TimelineSerialiser(Storage storage)
    {
        this.storage = storage;
    }

    public void Serialise(List<Clip> clips)
    {
        using var stream = storage.CreateFileSafely(file_name);
        using var writer = new StreamWriter(stream);
        writer.Write(JsonConvert.SerializeObject(new SerialisableTimeline(clips)));
    }

    public SerialisableTimeline? Deserialise()
    {
        using (var stream = storage.GetStream(file_name))
        {
            if (stream is not null)
            {
                using var reader = new StreamReader(stream);

                return JsonConvert.DeserializeObject<SerialisableTimeline>(reader.ReadToEnd());
            }
        }

        return null;
    }
}
