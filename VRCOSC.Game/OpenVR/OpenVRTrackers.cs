using System.Collections.Generic;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace VRCOSC.Game.OpenVR;

public class OpenVRTrackers
{
    private readonly OpenVRInterface instance;
    private readonly Dictionary<uint, OpenVRTracker> trackersDict = new();

    public IEnumerable<OpenVRTracker> TrackerEnumerable => trackersDict.Values;

    public OpenVRTrackers(OpenVRInterface instance)
    {
        this.instance = instance;
    }

    public void Update(IEnumerable<uint> ids)
    {
        ids.ForEach(id =>
        {
            if (!trackersDict.ContainsKey(id)) trackersDict[id] = new OpenVRTracker(instance);
            trackersDict[id].Update(id);
        });
    }
}
