// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.ChatBox.Serialisation.V1.Structures;

namespace VRCOSC.Game.ChatBox.Serialisation;

public interface ITimelineSerialiser
{
    public void Serialise(ChatBoxManager chatBoxManager);
    public SerialisableTimeline? Deserialise();
}
