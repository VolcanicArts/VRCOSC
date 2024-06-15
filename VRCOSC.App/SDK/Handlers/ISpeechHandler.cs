// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.Audio;

namespace VRCOSC.App.SDK.Handlers;

public interface ISpeechHandler
{
    public void OnPartialResult(string text);
    public void OnFinalResult(SpeechResult result);
}
