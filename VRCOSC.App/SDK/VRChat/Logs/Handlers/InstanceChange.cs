// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Text.RegularExpressions;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.VRChat.Logs.Handlers;

public class InstanceChangeLogLineHandler : VRChatLogLineHandler
{
    public override Regex Regex => new(@"^.+Joining (?<id>(?<world>wrld[^:]+):(?<number>\d+)(?:~(?<type>\w+)\((?<ownerId>[^)]+)\))?(?<extra>(?:~(?!region\()[^~]+)*)~region\((?<region>[^)]+)\)(?:~nonce\((?<nonce>[^)]+)\))?).*$");

    public override IVRChatClientEvent? HandleMatch(LogReaderState state, VRChatLogLineMatch logLine)
    {
        try
        {
            var segmentRegex = new Regex(@"^(?<key>\w+)(?:\((?<value>[^)]+)\))?$");

            var world = logLine.Match.Groups["world"].Value;
            var id = logLine.Match.Groups["id"].Value;
            var number = logLine.Match.Groups["number"].Value;
            var region = InstanceHelper.CodeToRegion(logLine.Match.Groups["region"].Value);
            var nonce = logLine.Match.Groups["nonce"].Success ? logLine.Match.Groups["nonce"].Value : null;
            var ownerId = logLine.Match.Groups["ownerId"].Success ? logLine.Match.Groups["ownerId"].Value : null;

            string? groupAccessType = null;
            var ageGate = false;
            var canRequestInvite = false;
            var queue = false;

            var extraSegments = logLine.Match.Groups["extra"].Value.Split('~', StringSplitOptions.RemoveEmptyEntries);

            foreach (var segment in extraSegments)
            {
                var s = segmentRegex.Match(segment);
                if (!s.Success) continue;

                switch (s.Groups["key"].Value)
                {
                    case "groupAccessType": groupAccessType = s.Groups["value"].Value; break;

                    case "ageGate": ageGate = true; break;

                    case "canRequestInvite": canRequestInvite = true; break;

                    case "queue": queue = true; break;
                }
            }

            var type = logLine.Match.Groups["type"].Success ? InstanceHelper.CodeToType(logLine.Match.Groups["type"].Value, canRequestInvite, groupAccessType) : InstanceType.Public;

            state.WorldId = world;
            state.InstanceId = id;
            state.InstanceOwnerId = ownerId;
            state.InstanceNumber = number;
            state.InstanceType = type;
            state.InstanceRegion = region;
            state.InstanceAgeGated = ageGate;
            state.InstanceHasQueue = queue;
            return null;
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Failed to parse instance change for log line: '{logLine.Match.Value}'");
            return null;
        }
    }
}