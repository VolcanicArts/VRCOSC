// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading;
using System.Windows.Forms;

namespace VRCOSC.Game.Processes;

public static class WinForms
{
    [STAThread]
    public static void OpenFileDialog(string filter, Action<string> fileNameCallback)
    {
        var t = new Thread(() =>
        {
            var dlg = new OpenFileDialog
            {
                Multiselect = false,
                Filter = filter
            };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            fileNameCallback.Invoke(dlg.FileName);
        });

        t.SetApartmentState(ApartmentState.STA);
        t.Start();
    }
}
