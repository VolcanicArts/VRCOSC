// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Serial;

[Node("Serial Write", "Serial")]
public sealed class SerialWriteNode : Node, IFlowInput
{
    public FlowContinuation OnSuccess = new("On Success");
    public FlowContinuation OnFail = new("On Fail");

    public ValueInput<string> PortName = new("Port Name");
    public ValueInput<int> BaudRate = new("Baud Rate");
    public ValueInput<Parity> Parity = new();
    public ValueInput<int> DataBits = new("Data Bits");
    public ValueInput<StopBits> StopBits = new("Stop Bits");
    public ValueInput<string> Command = new();

    protected override async Task Process(PulseContext c)
    {
        var portName = PortName.Read(c);
        if (string.IsNullOrWhiteSpace(portName)) return;

        using SerialPort serial = new SerialPort(portName, BaudRate.Read(c), Parity.Read(c), DataBits.Read(c), StopBits.Read(c));
        var command = Command.Read(c);

        if (string.IsNullOrWhiteSpace(command))
        {
            await OnFail.Execute(c);
            return;
        }

        try
        {
            serial.Open();
            serial.WriteLine(command);
        }
        catch (Exception)
        {
            await OnFail.Execute(c);
            return;
        }
        finally
        {
            serial.Close();
        }

        await OnSuccess.Execute(c);
    }
}