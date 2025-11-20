// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Strings;

public enum ScrollDirection
{
    None,
    Left,
    Right,
    Bounce
}

[Node("String Scroll", "Strings")]
public sealed class StringScrollNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<string> Input = new();
    public ValueInput<int?> TruncateLen = new("Truncate Length");
    public ValueInput<ScrollDirection> ScrollDir = new("Scroll Direction");
    public ValueInput<string> JoinString = new("Join String");
    public ValueInput<bool> OnlyScrollWhenTruncated = new("Only Scroll When Truncated");

    public ValueOutput<string> Result = new();

    public GlobalStore<int> CurrentIndex = new();
    public GlobalStore<bool> BounceDirection = new();
    public GlobalStore<ScrollDirection> PreviousScrollDirection = new();
    public GlobalStore<bool> PreviousOnlyScrollWhenTruncated = new();

    protected override async Task Process(PulseContext c)
    {
        var input = Input.Read(c);

        if (string.IsNullOrWhiteSpace(input))
        {
            CurrentIndex.Write(0, c);
            Result.Write(string.Empty, c);
            return;
        }

        var truncateLength = TruncateLen.Read(c) ?? -1;
        var scrollDirection = ScrollDir.Read(c);
        var joinString = JoinString.Read(c);
        var onlyScrollWhenTruncated = OnlyScrollWhenTruncated.Read(c);

        var currentIndex = CurrentIndex.Read(c);
        var bounceDirection = BounceDirection.Read(c);
        var previousScrollDirection = PreviousScrollDirection.Read(c);
        var previousOnlyScrollWhenTruncated = PreviousOnlyScrollWhenTruncated.Read(c);

        if (previousScrollDirection != ScrollDirection.Bounce && scrollDirection == ScrollDirection.Bounce)
            currentIndex = 0;

        previousScrollDirection = scrollDirection;

        if (onlyScrollWhenTruncated != previousOnlyScrollWhenTruncated)
        {
            currentIndex = 0;
            previousOnlyScrollWhenTruncated = onlyScrollWhenTruncated;
        }

        var formattedValueInfo = new StringInfo(input);

        var willTruncate = truncateLength >= 0 && formattedValueInfo.LengthInTextElements > truncateLength;
        var willScroll = (!onlyScrollWhenTruncated || willTruncate) && scrollDirection != ScrollDirection.None;

        if (!willScroll) currentIndex = 0;

        var stringInfo = new StringInfo(input);

        if (willScroll && scrollDirection == ScrollDirection.Bounce && onlyScrollWhenTruncated)
        {
            var charsLeft = bounceDirection ? stringInfo.LengthInTextElements - currentIndex - truncateLength : currentIndex;

            if (charsLeft == 0)
                bounceDirection = !bounceDirection;

            var localScrollSpeed = 1;
            localScrollSpeed = charsLeft != 0 ? (int)MathF.Min(localScrollSpeed, charsLeft) : localScrollSpeed;

            input = stringInfo.SubstringByTextElements(currentIndex, truncateLength);
            currentIndex += bounceDirection ? localScrollSpeed : -localScrollSpeed;
        }
        else
        {
            if (willScroll && scrollDirection != ScrollDirection.Bounce)
            {
                if (!string.IsNullOrEmpty(joinString)) input += joinString;

                switch (scrollDirection)
                {
                    case ScrollDirection.Right:
                        currentIndex += 1;
                        break;

                    case ScrollDirection.Left:
                        currentIndex -= 1;
                        break;

                    case ScrollDirection.None:
                        break;
                }
            }

            var localStringInfo = new StringInfo(input);
            var position = currentIndex.Modulo(localStringInfo.LengthInTextElements);
            input = cropAndWrapText(localStringInfo, position, truncateLength < 0 ? localStringInfo.LengthInTextElements : truncateLength);
        }

        Result.Write(input, c);

        CurrentIndex.Write(currentIndex, c);
        BounceDirection.Write(bounceDirection, c);
        PreviousScrollDirection.Write(previousScrollDirection, c);
        PreviousOnlyScrollWhenTruncated.Write(previousOnlyScrollWhenTruncated, c);

        await Next.Execute(c);
    }

    private static string cropAndWrapText(StringInfo text, int position, int maxLength)
    {
        var endPos = (int)MathF.Min(position + maxLength, text.LengthInTextElements);
        var subText = text.SubstringByTextElements(position, endPos - position);
        var subTextInfo = new StringInfo(subText);
        return endPos != text.LengthInTextElements ? subText : subText + text.SubstringByTextElements(0, (int)MathF.Min(maxLength - subTextInfo.LengthInTextElements, position));
    }
}