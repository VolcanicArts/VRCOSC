namespace VRCOSC.OSC
{
    public static class OscUtils
    {
        public static int AlignedStringLength(string val)
        {
            int len = val.Length + (4 - val.Length % 4);
            if (len <= val.Length) len += 4;

            return len;
        }
    }
}
