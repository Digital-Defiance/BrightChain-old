using BrightChain.Services;
using CSharpTest.Net.Collections;
using CSharpTest.Net.Interfaces;
using CSharpTest.Net.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BrightChain.Tests
{
    /// <summary>
    /// Test object for exercizing the BPlus tree
    /// </summary>
    public class TestDiskCacheObject : ITransactable
    {
        /// <summary>
        /// last committed value
        /// </summary>
        protected Memory<byte> lastCommittedMemory { get; private set; }
        /// <summary>
        /// working memory with new values
        /// </summary>
        public Memory<byte> Memory { get; private set; }

        public TestDiskCacheObject()
        {
            this.Memory = new Memory<byte>();
            this.lastCommittedMemory = new Memory<byte>();
        }

        public TestDiskCacheObject(byte[] data)
        {
            this.Memory = new Memory<byte>(data);
            this.lastCommittedMemory = new Memory<byte>();
        }

        public void Commit()
        {
            this.lastCommittedMemory = new Memory<byte>(this.Memory.ToArray());
        }

        public void Rollback()
        {
            this.Memory = new Memory<byte>(this.lastCommittedMemory.ToArray());
        }

        public void Dispose()
        {
            this.Memory = null;
            this.lastCommittedMemory = null;
        }
    }

    /// <summary>
    /// Serializer for the test object
    /// TODO: use internal serializers or make this a helper?
    /// </summary>
    public class TestDiskCacheObjectSerializer : ISerializer<TestDiskCacheObject>
    {
        public TestDiskCacheObject ReadFrom(Stream stream)
        {
            List<byte> aggregatedBytes = new List<byte>();
            int bytesRead;

            byte[] buffer = new byte[sizeof(int)];
            bytesRead = stream.Read(buffer, 0, sizeof(int));
            int bytesRemaining = Convert.ToInt32(buffer);

            const int bufferSize = 100;
            buffer = new byte[bufferSize > bytesRemaining ? bytesRemaining : bufferSize];
            do
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead <= 0)
                    break;
                bytesRemaining -= bytesRead;

                if (bytesRead == bufferSize)
                    aggregatedBytes.AddRange(buffer);
                else
                    for (int i = 0; i < bytesRead; i++)
                        aggregatedBytes.Add(buffer[i]);

            } while (bytesRemaining > 0);

            return new TestDiskCacheObject(aggregatedBytes.ToArray());
        }

        public void WriteTo(TestDiskCacheObject value, Stream stream)
        {
            byte[] buffer = BitConverter.GetBytes(value.Memory.Length);
            stream.Write(
                buffer: buffer,
                offset: 0,
                count: buffer.Length);
            stream.Write(
                buffer: value.Memory.ToArray(),
                offset: 0,
                count: value.Memory.Length);

        }
    }

    /// <summary>
    /// Tests the DiskBPlusTreeCacheManager with the BPlusTreeCacheManagerTest
    /// </summary>
    [TestClass]
    public class DiskBPlusTreeCacheManagerTest : BPlusTreeCacheManagerTest<DiskBPlusTreeCacheManager<string, TestDiskCacheObject, PrimitiveSerializer, TestDiskCacheObjectSerializer>, string, TestDiskCacheObject, PrimitiveSerializer, TestDiskCacheObjectSerializer>
    {
        private int TestKeyLength { get; } = 11;
        public string GenerateTestKey()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var random = new Random(Guid.NewGuid().GetHashCode());
            var randomString = new string(Enumerable.Repeat(chars, TestKeyLength)
                                                    .Select(s => s[random.Next(s.Length)]).ToArray());
            return randomString;
        }

        public BPlusTree<string, TestDiskCacheObject>.OptionsV2 DefaultOptions()
        {
            BPlusTree<string, TestDiskCacheObject>.OptionsV2 options = new BPlusTree<string, TestDiskCacheObject>.OptionsV2(
                keySerializer: CSharpTest.Net.Serialization.PrimitiveSerializer.String,
                valueSerializer: new TestDiskCacheObjectSerializer());
            return options;
        }

        internal override DiskBPlusTreeCacheManager<string, TestDiskCacheObject, PrimitiveSerializer, TestDiskCacheObjectSerializer> NewCacheManager(ILogger logger) =>
            new DiskBPlusTreeCacheManager<string, TestDiskCacheObject, PrimitiveSerializer, TestDiskCacheObjectSerializer>(logger: logger, optionsV2: DefaultOptions());

        internal override KeyValuePair<string, TestDiskCacheObject> NewKeyValue()
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            var data = new byte[TestKeyLength];
            for (int i = 0; i < TestKeyLength; i++)
                data[i] = (byte)random.Next(0, 255);
            return new KeyValuePair<string, TestDiskCacheObject>(GenerateTestKey(), new TestDiskCacheObject(data));
        }

        internal override TestDiskCacheObject NewNullData() => null;
    }
}
