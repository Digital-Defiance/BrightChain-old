namespace BrightChain.Engine.Helpers
{
    using System;
    using System.Linq;
    using FASTER.core;

    public class BinaryStringSerializer : BinaryObjectSerializer<string>
    {
        public override void Deserialize(out string obj)
        {
            var mem = new MemoryStream();
            var streamReader = new StreamReader(this.reader.BaseStream);
            var streamWriter = new StreamWriter(mem);
            int b;
            while ((b = streamReader.Read()) > 0)
            {
                streamWriter.Write((byte)b);
            }

            mem.Position = 0;
            var memBuf = mem.GetBuffer();
            var origBytes = Convert.FromBase64CharArray(memBuf.Select(b => (char)b).ToArray(), 0, (int)mem.Length);
            obj = new string(origBytes.Select(b => (char)b).ToArray());
        }

        public override void Serialize(ref string obj)
        {
            var stringArr = obj.ToCharArray().Select(c => (byte)c).ToArray();
            this.writer.Write(Convert.ToBase64String(stringArr, 0, stringArr.Length, Base64FormattingOptions.None));
            this.writer.Write((byte)0);
        }
    }
}
