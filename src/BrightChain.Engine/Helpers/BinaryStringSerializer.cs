using System;
using System.IO;
using System.Linq;
using FASTER.core;

namespace BrightChain.Engine.Helpers;

public class BinaryStringSerializer : BinaryObjectSerializer<string>
{
    public override void Deserialize(out string obj)
    {
        var mem = new MemoryStream();
        var streamReader = new StreamReader(stream: this.reader.BaseStream);
        var streamWriter = new StreamWriter(stream: mem);
        int b;
        while ((b = streamReader.Read()) > 0)
        {
            streamWriter.Write(value: (byte)b);
        }

        var memBuf = mem.ToArray();
        var origBytes = Convert.FromBase64CharArray(inArray: memBuf.Select(selector: b => (char)b).ToArray(),
            offset: 0,
            length: (int)mem.Length);
        obj = new string(value: origBytes.Select(selector: b => (char)b).ToArray());
    }

    public override void Serialize(ref string obj)
    {
        var stringArr = obj.ToCharArray().Select(selector: c => (byte)c).ToArray();
        this.writer.Write(value: Convert.ToBase64String(inArray: stringArr,
            offset: 0,
            length: stringArr.Length,
            options: Base64FormattingOptions.None));
        this.writer.Write(value: 0);
    }
}
