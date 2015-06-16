namespace hcGate
{
    static class CheckSum
    {

        public static byte Sum8(string str)
        {
            byte result = 0;
            for (var i = 0; i < str.Length; i++)
                result += (byte)str[i];
            return result;
        }

        public static byte Sum8(byte[] buffer, int size)
        {
            byte result = 0;
            for (var i = 0; i < size; i++)
                result += buffer[i];
            return result;
        }

    }
}
