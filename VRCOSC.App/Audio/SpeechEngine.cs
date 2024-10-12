// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.Audio;

public abstract class SpeechEngine
{
    public Action<string>? OnPartialResult;
    public Action<string>? OnFinalResult;

    public abstract void Initialise();
    public abstract Task Teardown();
}

public record SpeechResult(bool IsFinal, string Text, float Confidence)
{
    public SpeechResult(SpeechResult speechResult)
    {
        IsFinal = speechResult.IsFinal;
        Text = speechResult.Text;
        Confidence = speechResult.Confidence;
    }
}
