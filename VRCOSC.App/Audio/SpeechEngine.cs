// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.Audio;

public abstract class SpeechEngine
{
    public Action<SpeechResult>? OnPartialResult;
    public Action<SpeechResult>? OnFinalResult;

    public abstract void Initialise();
    public abstract void Teardown();
}

public record SpeechResult(string Text, float Confidence);
