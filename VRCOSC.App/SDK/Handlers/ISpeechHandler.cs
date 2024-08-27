// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Handlers;

public interface ISpeechHandler
{
    public void OnPartialSpeechResult(string text);
    public void OnFinalSpeechResult(string text);
}
