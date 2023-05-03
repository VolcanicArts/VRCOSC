// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.IO;
using System.Text;
using Newtonsoft.Json;
using osu.Framework.Platform;
using VRCOSC.Game.ChatBox.Serialisation.V1.Structures;

namespace VRCOSC.Game.ChatBox.Serialisation.V1;

public class TimelineSerialiser : ITimelineSerialiser
{
    private const string file_name = @"chatbox.json";
    private readonly Storage storage;
    private readonly object saveLock = new();

    public TimelineSerialiser(Storage storage)
    {
        this.storage = storage;
    }

    public void Serialise(ChatBoxManager chatBoxManager)
    {
        lock (saveLock)
        {
            File.WriteAllBytes(storage.GetFullPath(file_name, true), Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(new SerialisableTimeline(chatBoxManager))));
        }
    }

    public SerialisableTimeline? Deserialise()
    {
        lock (saveLock)
        {
            if (!storage.Exists(file_name)) return null;

            try
            {
                return JsonConvert.DeserializeObject<SerialisableTimeline>(Encoding.Unicode.GetString(File.ReadAllBytes(storage.GetFullPath(file_name))));
            }
            catch // migration from UTF-8
            {
                return JsonConvert.DeserializeObject<SerialisableTimeline>(File.ReadAllText(storage.GetFullPath(file_name)));
            }
        }
    }
}
