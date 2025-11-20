// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using CommandLine;

namespace VRCOSC.App;

public class LaunchOptions
{
    [Option("profile", Required = false, Default = "", HelpText = "A profile to switch to when opening")]
    public string Profile { get; set; } = "";
}