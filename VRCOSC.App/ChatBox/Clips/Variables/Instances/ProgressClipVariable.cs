// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.ChatBox.Clips.Variables.Instances;

public class ProgressClipVariable : ClipVariable
{
    public ProgressClipVariable()
    {
    }

    public ProgressClipVariable(ClipVariableReference reference)
        : base(reference)
    {
    }

    [ClipVariableOption("use_visual", "Use Visual", "Should we show as a visual instead of percentage?")]
    public bool UseVisual { get; set; } = true;

    [ClipVariableOption("visual_resolution", "Visual Resolution", "What resolution should the visual be at?")]
    public int VisualResolution { get; set; } = 10;

    [ClipVariableOption("visual_line", "Visual Line", "The character to be shown on the visual where the position isn't")]
    public string VisualLine { get; set; } = "\u2501";

    [ClipVariableOption("visual_position", "Visual Position", "The character to be shown on the visual where the position is")]
    public string VisualPosition { get; set; } = "\u25CF";

    [ClipVariableOption("visual_start", "Visual Start", "The character to be shown at the start of the visual")]
    public string VisualStart { get; set; } = "\u2523";

    [ClipVariableOption("visual_end", "Visual End", "The character to be shown at the end of the visual")]
    public string VisualEnd { get; set; } = "\u252B";

    public override bool IsDefault() => base.IsDefault() && UseVisual && VisualResolution == 10 && VisualLine == "\u2501" && VisualPosition == "\u25CF" && VisualStart == "\u2523" && VisualEnd == "\u252B";

    public override ProgressClipVariable Clone()
    {
        var clone = (ProgressClipVariable)base.Clone();

        clone.UseVisual = UseVisual;
        clone.VisualResolution = VisualResolution;
        clone.VisualLine = VisualLine;
        clone.VisualPosition = VisualPosition;
        clone.VisualStart = VisualStart;
        clone.VisualEnd = VisualEnd;

        return clone;
    }

    protected override string Format(object value)
    {
        var floatValue = (float)value;
        floatValue = MathF.Max(0, floatValue);
        floatValue = MathF.Min(1, floatValue);

        if (UseVisual)
        {
            var dotPosition = VisualResolution * floatValue;

            var visual = string.Empty;
            visual += VisualStart;

            for (var i = 0; i < VisualResolution; i++)
            {
                visual += (i <= dotPosition && i + 1 > dotPosition) ? VisualPosition : VisualLine;
            }

            visual += VisualEnd;

            return visual;
        }

        return $"{(int)MathF.Round(floatValue * 100f)}%";
    }
}
