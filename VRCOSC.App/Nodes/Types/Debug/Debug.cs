// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

#if DEBUG
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Debug;

[Node("Log", "Debug")]
public sealed class LogNode<T>() : SimpleActionValueConsumeNode<T>(value => Logger.Log(value?.ToString() ?? "null", LoggingTarget.Information));
#endif