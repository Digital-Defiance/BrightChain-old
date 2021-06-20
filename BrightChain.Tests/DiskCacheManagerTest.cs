using CSharpTest.Net.Collections;
using CSharpTest.Net.Serialization;
using BrightChain.Interfaces;
using BrightChain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BrightChain.Tests
{
    public class TestDiskCacheObject
    {
        public Memory<byte> Memory { get; set; }

        public TestDiskCacheObject()
        {
            this.Memory = new Memory<byte>();
        }

        public TestDiskCacheObject(byte[] data)
        {
            this.Memory = new Memory<byte>(data);
        }
    }

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

    [TestClass]
    public class DiskCacheManagerTest : CacheManagerTest<DiskCacheManager<string, TestDiskCacheObject>, string, TestDiskCacheObject>
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

        internal override ICacheManager<string, TestDiskCacheObject> NewCacheManager(ILogger logger) => new DiskCacheManager<string, TestDiskCacheObject>(logger: logger, optionsV2: DefaultOptions());

        internal override KeyValuePair<string, TestDiskCacheObject> NewKeyValue()
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            var data = new byte[TestKeyLength];
            for (int i = 0; i < TestKeyLength; i++)
                data[i] = (byte)random.Next(0, 255);
            return new KeyValuePair<string, TestDiskCacheObject>(GenerateTestKey(), new TestDiskCacheObject(data));
        }

        internal override TestDiskCacheObject NewNullData() => null;

        [TestMethod]
        public void TestSetGetIntegrity()
        {
            // Arrange
            var expectation = testPair.Value;
            cacheManager.Set(testPair.Key, expectation);

            // Act
            TestDiskCacheObject result = cacheManager.Get(testPair.Key);

            // Assert
            Assert.IsNotNull(expectation);
            Assert.AreEqual(expectation, result);
            Assert.AreSame(expectation, result);
            Assert.AreEqual(expectation.Memory, result.Memory);
        }
    }
}
