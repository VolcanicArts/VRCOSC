using System.Text;

namespace VRCOSC.OSC
{
    public sealed class OscMessage : OscPacket
    {
        public readonly string Address;
        public readonly List<object> Arguments;

        public OscMessage(string address, params object[] args)
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

                var type = Type.GetTypeCode(arg.GetType());

                switch (type)
                {
                    case TypeCode.Int32:
                        typeString += "i";
                        parts.Add(SetInt((int)arg));
                        break;

                    case TypeCode.Single:
                        typeString += "f";
                        parts.Add(SetFloat((float)arg));
                        break;

                    case TypeCode.String:
                        typeString += "s";
                        parts.Add(SetString((string)arg));
                        break;

                    case TypeCode.Boolean:
                        typeString += ((bool)arg) ? "T" : "F";
                        break;

                    default:
                        throw new Exception("Unable to transmit values of type " + type);
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
