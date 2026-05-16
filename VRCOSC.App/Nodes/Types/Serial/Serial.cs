// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.IO.Ports;

namespace VRCOSC.App.Nodes.Types.Serial;

[Node("Serial Write", "Serial")]
public sealed class SerialWriteNode : TryActionNode
{
    public ValueInput<string?> PortName = new();
    public ValueInput<int> BaudRate = new();
    public ValueInput<Parity> Parity = new();
    public ValueInput<int> DataBits = new();
    public ValueInput<StopBits> StopBits = new();
    public ValueInput<string?> Command = new();

    protected override bool TryAction(IPulseContext c)
    {
        var portName = PortName.Read(c);
        if (string.IsNullOrWhiteSpace(portName)) return false;

        SerialPort? serial;

        try
        {
            serial = new SerialPort(portName, BaudRate.Read(c), Parity.Read(c), DataBits.Read(c), StopBits.Read(c));
        }
        catch
        {
            return false;
        }

        var command = Command.Read(c);
        if (command is null) return false;

        try
        {
            serial.Open();
            serial.WriteLine(command);
            serial.Close();
            return true;
        }
        catch
        {
            serial.Close();
            return false;
        }
    }
}