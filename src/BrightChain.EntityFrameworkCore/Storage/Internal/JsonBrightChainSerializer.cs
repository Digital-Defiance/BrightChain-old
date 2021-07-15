// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Interfaces;
using System.IO;
using System.Text;
using System.Text.Json;

namespace BrightChain.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class JsonBrightChainSerializer<T> : IBrightChainSerializer<T> where T : class
    {
        private static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);

        /// <inheritdoc />
        public T ReadFrom(Stream stream)
        {
            using (stream)
            {
                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    return (T)(object)stream;
                }

                using var streamReader = new StreamReader(stream);
                return JsonSerializer.Deserialize<T>(streamReader.ReadToEnd());
            }
        }

        /// <inheritdoc />
        public void WriteTo(T input, Stream stream)
        {
            var streamPayload = new MemoryStream();
            using (var streamWriter = new StreamWriter(streamPayload, encoding: DefaultEncoding, bufferSize: 1024, leaveOpen: true))
            {
                streamWriter.Write(JsonSerializer.Serialize(input));
                streamWriter.Flush();
            }

            streamPayload.Position = 0;
        }
    }
}
