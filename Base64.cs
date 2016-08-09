using System;
using System.IO;
using System.Text;

namespace opcode4.utilities
{
    public static class Base64
    {
        private static readonly char[] TableBase64 = new char[65]
                                                         {	'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
                                                          	'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
                                                          	'0','1','2','3','4','5','6','7','8','9','+','/','='};
        private static readonly char[] TableBase64URL = new char[65]
                                                            {	'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
                                                             	'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
                                                             	'0','1','2','3','4','5','6','7','8','9','!','-','_'};

        

        #region Decode
        public static byte[] GetDecoded(char[] Table, char[] input)
        {
            int temp = 0;
            char[] source = input;
            int length = input.Length;

            //find how many padding are there
            for (int x = 0; x < 2; x++)
            {
                if (input[length - x - 1] == '=')
                    temp++;
            }
            int paddingCount = temp;
            //calculate the blockCount;
            //assuming all whitespace and carriage returns/newline were removed.
            int blockCount = length / 4;
            int length2 = blockCount * 3;

            var buffer=new byte[length];//first conversion result
            var buffer2=new byte[length2];//decoded array with padding

            for(int x=0;x<length;x++)
            {
                buffer[x]=char2sixbit(Table, source[x]);
            }

            for(int x=0;x<blockCount;x++)
            {
                byte temp1 = buffer[x*4];
                byte temp2 = buffer[x*4+1];
                byte temp3 = buffer[x*4+2];
                byte temp4 = buffer[x*4+3];				

                var b = (byte)(temp1<<2);
                var b1 = (byte)((temp2 & 48)>>4);
                b1+=b;

                b=(byte)((temp2 & 15)<<4);
                var b2 = (byte)((temp3 & 60)>>2);
                b2+=b;

                b=(byte)((temp3 & 3)<<6);
                byte b3 = temp4;
                b3+=b;

                buffer2[x*3]=b1;
                buffer2[x*3+1]=b2;
                buffer2[x*3+2]=b3;
            }
            //remove paddings
            int length3 = length2-paddingCount;
            var result=new byte[length3];

            for(int x=0;x<length3;x++)
            {
                result[x]=buffer2[x];
            }

            return result;
        }
        private static byte char2sixbit(char[] Table, char c)
        {
            if(c == Table[64])
                return 0;
			

            for (int x=0;x<64;x++)
            {
                if (Table[x]==c)
                    return (byte)x;
            }
            //should not reach here
            return 0;
			

        }

        public static byte[] DecodeBase64(string input)
        {
            return GetDecoded(TableBase64, input.ToCharArray());
        }
        public static byte[] DecodeBase64URL(string input)
        {
            return GetDecoded(TableBase64URL, input.ToCharArray());
        }

        public static string DecodeBase64_ToString(string input, Encoding encoding = null)
        {
            return (encoding ?? Encoding.UTF8).GetString(GetDecoded(TableBase64, input.ToCharArray()));
        }
        public static string DecodeBase64URL_ToString(string input, Encoding encoding = null)
        {
            return (encoding ?? Encoding.UTF8).GetString(GetDecoded(TableBase64URL, input.ToCharArray()));
        }
        #endregion Decode
        #region Encode
        public static char[] GetEncoded(char[] Table, byte[] input)
        {
            int blockCount;
            int paddingCount;

            byte[] source = input;
            int length = input.Length;
            if ((length % 3) == 0)
            {
                paddingCount = 0;
                blockCount = length / 3;
            }
            else
            {
                paddingCount = 3 - (length % 3);//need to add padding
                blockCount = (length + paddingCount) / 3;
            }
            int length2 = length + paddingCount;

            var source2 = new byte[length2];
            //copy data over insert padding
            for (int x = 0; x < length2; x++)
            {
                if (x < length)
                {
                    source2[x] = source[x];
                }
                else
                {
                    source2[x] = 0;
                }
            }

            var buffer = new byte[blockCount * 4];
            var result = new char[blockCount * 4];
            for (int x = 0; x < blockCount; x++)
            {
                byte b1 = source2[x * 3];
                byte b2 = source2[x * 3 + 1];
                byte b3 = source2[x * 3 + 2];

                var temp1 = (byte)((b1 & 252) >> 2);

                var temp = (byte)((b1 & 3) << 4);
                var temp2 = (byte)((b2 & 240) >> 4);
                temp2 += temp; //second

                temp = (byte)((b2 & 15) << 2);
                var temp3 = (byte)((b3 & 192) >> 6);
                temp3 += temp; //third

                var temp4 = (byte)(b3 & 63); //fourth

                buffer[x * 4] = temp1;
                buffer[x * 4 + 1] = temp2;
                buffer[x * 4 + 2] = temp3;
                buffer[x * 4 + 3] = temp4;

            }

            for (int x = 0; x < blockCount * 4; x++)
            {
                result[x] = sixbit2char(Table, buffer[x]);
            }

            //covert last "A"s to "=", based on paddingCount
            switch (paddingCount)
            {
                case 0: break;
                case 1: result[blockCount * 4 - 1] = Table[64]; break;
                case 2: result[blockCount * 4 - 1] = Table[64];
                    result[blockCount * 4 - 2] = Table[64];
                    break;
                default: break;
            }
            return result;
        }

        private static char sixbit2char(char[] Table, byte b)
        {

            if ((b >= 0) && (b <= 63))
                return Table[b];

            //should not happen;
            return ' ';
        }

        public static string EncodeBase64(byte[] input)
        {
            return new string(GetEncoded(TableBase64, input));
        }
        public static string EncodeBase64(Stream input)
        {
            if (input == null)
                throw new ArgumentException("EncodeBase64: Stream is not assigned");

            if (input.Length == 0)
                return "";

            var buf = new Byte[input.Length];
            input.Read(buf, 0, buf.Length);
            return new string(GetEncoded(TableBase64, buf));
        }
        public static string EncodeBase64URL(byte[] input)
        {
            return new string(GetEncoded(TableBase64URL, input));
        }
        public static string EncodeBase64URL(string input, Encoding encoding = null)
        {
            return new string(GetEncoded(TableBase64URL, (encoding ?? Encoding.UTF8).GetBytes(input)));
        }

        #endregion Encode


    }
}