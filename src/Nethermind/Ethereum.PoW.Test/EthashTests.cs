﻿/*
 * Copyright (c) 2018 Demerzel Solutions Limited
 * This file is part of the Nethermind library.
 *
 * The Nethermind library is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * The Nethermind library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Ethereum.Test.Base;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Core.Encoding;
using Nethermind.Core.Extensions;
using Nethermind.Core.Potocol;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Ethereum.PoW.Test
{
    public class EthashTests
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        }

        private static IEnumerable<EthashTest> LoadTests()
        {
            return TestLoader.LoadFromFile<Dictionary<string, EthashTestJson>, EthashTest>(
                "keyaddrtest.json",
                c => c.Select(p => Convert(p.Key, p.Value)));
        }

        private static EthashTest Convert(string name, EthashTestJson testJson)
        {
            byte[] nonceBytes = new Hex(testJson.Nonce);
            ulong nonceValue = nonceBytes.ToUInt64();
            
            return new EthashTest(
                name,
                nonceValue,
                new Keccak(new Hex(testJson.MixHash)),
                new Hex(testJson.Header),
                new Keccak(new Hex(testJson.Seed)),
                testJson.CacheSize,
                testJson.FullSize,
                new Keccak(new Hex(testJson.HeaderHash)),
                new Keccak(new Hex(testJson.CacheHash)),
                new Keccak(new Hex(testJson.Result)));
        }
        
        [TestCaseSource(nameof(LoadTests))]
        public void Test(EthashTest test)
        {
            Assert.Fail("not implemented");
        }

        private class EthashTestJson
        {
            public string Nonce { get; set; }
            public string MixHash { get; set; }
            public string Header { get; set; }
            public string Seed { get; set; }
            [JsonProperty("cache_size")]
            public int CacheSize { get; set; }
            [JsonProperty("full_size")]
            public int FullSize { get; set; }
            [JsonProperty("header_hash")]
            public string HeaderHash { get; set; }
            [JsonProperty("cache_hash")]
            public string CacheHash { get; set; }
            public string Result { get; set; }
        }

        public class EthashTest
        {
            public EthashTest(
                string name,
                ulong nonce,
                Keccak mixHash,
                byte[] header,
                Keccak seed,
                BigInteger cacheSize,
                BigInteger fullSize,
                Keccak headerHash,
                Keccak cacheHash,
                Keccak result)
            {
                Name = name;
                Nonce = nonce;
                MixHash = mixHash;
                Header = header;
                Seed = seed;
                CacheSize = cacheSize;
                FullSize = fullSize;
                CacheHash = cacheHash;
                HeaderHash = headerHash;
                Result = result;
            }

            public string Name { get; }
            public ulong Nonce { get; }
            public Keccak MixHash { get; }
            public byte[] Header { get; }
            public Keccak Seed { get; }
            public BigInteger CacheSize { get; }
            public BigInteger FullSize { get; }
            public Keccak HeaderHash { get; }
            public Keccak CacheHash { get; }
            public Keccak Result { get; }

            public override string ToString() => Name;
        }
    }
}