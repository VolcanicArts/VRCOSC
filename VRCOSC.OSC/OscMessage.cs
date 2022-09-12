using System.Text;

namespace VRCOSC.OSC
{
    public sealed class OscMessage : OscPacket
    {
        public readonly string Address;
        public readonly List<object> Arguments;

        public OscMessage(string address, IEnumerable<object> args)
        {
            Address = address;
            Arguments = new List<object>();
            Arguments.AddRange(args);
        }

        public byte[] GetBytes()
        {
            var parts = new List<byte[]>();

            string typeString = ",";
            int i = 0;

            while (i < Arguments.Count)
            {
                var arg = Arguments[i];

                switch (arg)
                {
                    case int intArg:
                        typeString += "i";
                        parts.Add(SetInt(intArg));
                        break;

                    case float floatArg:
                        typeString += "f";
                        parts.Add(SetFloat(floatArg));
                        break;

                    case string stringArg:
                        typeString += "s";
                        parts.Add(SetString(stringArg));
                        break;

                    case bool boolArg:
                        typeString += (boolArg) ? "T" : "F";
                        break;

                    default:
                        throw new InvalidOperationException();
                }

                i++;
            }

            int addressLen = (Address.Length == 0) ? 0 : OscUtils.AlignedStringLength(Address);
            int typeLen = OscUtils.AlignedStringLength(typeString);

            int total = addressLen + typeLen + parts.Sum(x => x.Length);

            byte[] output = new byte[total];
            i = 0;

            Encoding.ASCII.GetBytes(Address).CopyTo(output, i);
            i += addressLen;

            Encoding.ASCII.GetBytes(typeString).CopyTo(output, i);
            i += typeLen;

            foreach (byte[] part in parts)
            {
                part.CopyTo(output, i);
                i += part.Length;
            }

            return output;
        }
    }
}
