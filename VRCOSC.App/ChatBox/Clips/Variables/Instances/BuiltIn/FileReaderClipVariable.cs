// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.IO;

namespace VRCOSC.App.ChatBox.Clips.Variables.Instances.BuiltIn;

public class FileReaderClipVariable : ClipVariable
{
    public FileReaderClipVariable()
    {
    }

    public FileReaderClipVariable(ClipVariableReference reference)
        : base(reference)
    {
    }

    [ClipVariableOption("file_location", "File Location", "The location of the file to read")]
    public string FileLocation { get; set; } = string.Empty;

    public override bool IsDefault() => base.IsDefault() && string.IsNullOrEmpty(FileLocation);

    protected override string Format(object value) => File.Exists(FileLocation) ? File.ReadAllText(FileLocation) : "INVALID FILE LOCATION";
}