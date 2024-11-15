// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.ChatBox.Clips.Variables.Instances.BuiltIn;

internal class TextClipVariable : ClipVariable
{
    public TextClipVariable()
    {
    }

    public TextClipVariable(ClipVariableReference reference)
        : base(reference)
    {
    }

    [ClipVariableOption("text", "Text", "The text to display")]
    public string Text { get; set; } = string.Empty;

    public override bool IsDefault() => base.IsDefault() && string.IsNullOrEmpty(Text);

    public override TextClipVariable Clone()
    {
        var clone = (TextClipVariable)base.Clone();

        clone.Text = Text;

        return clone;
    }

    protected override string Format(object value) => Text;
}