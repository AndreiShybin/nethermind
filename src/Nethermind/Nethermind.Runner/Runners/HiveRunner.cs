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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nethermind.Blockchain;
using Nethermind.Config;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Core.Encoding;
using Nethermind.Core.Extensions;
using Nethermind.Core.Logging;
using Nethermind.Core.Model;
using Nethermind.Core.Specs;
using Nethermind.Dirichlet.Numerics;
using Nethermind.KeyStore;
using Nethermind.KeyStore.Config;
using Nethermind.Runner.Config;
using Nethermind.Runner.Data;
using Nethermind.Store;
using Nethermind.Wallet;

namespace Nethermind.Runner.Runners
{
    public class HiveRunner : IRunner
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IBlockTree _blockTree;
        private readonly IBlockchainProcessor _blockchainProcessor;
        private readonly IStateProvider _stateProvider;
        private readonly ISnapshotableDb _stateDb;
        private readonly ISpecProvider _specProvider;
        private readonly HiveWallet _wallet;
        private readonly ILogger _logger;
        private readonly IConfigProvider _configurationProvider;

        public HiveRunner(IJsonSerializer jsonSerializer, IBlockchainProcessor blockchainProcessor,
            IBlockTree blockTree, IStateProvider stateProvider, ISnapshotableDb stateDb, ILogger logger,
            IConfigProvider configurationProvider, ISpecProvider specProvider, HiveWallet wallet)
        {
            _jsonSerializer = jsonSerializer;
            _blockchainProcessor = blockchainProcessor;
            _blockTree = blockTree;
            _stateProvider = stateProvider;
            _stateDb = stateDb;
            _logger = logger;
            _configurationProvider = configurationProvider;
            _specProvider = specProvider;
            _wallet = wallet;
        }

        public Task Start()
        {
            _logger.Info("Ethereum");
            var initConfig = _configurationProvider.GetConfig<IHiveConfig>();
            _blockchainProcessor.Start();
            InitializeKeys(initConfig.KeysDir);
            InitializeGenesis(initConfig.GenesisFilePath);
            InitializeChain(initConfig.ChainFile);
            InitializeBlocks(initConfig.BlocksDir);
            _logger.Info("Ethereum initialization completed");
            return Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            await Task.CompletedTask;
        }

        private void InitializeChain(string chainFile)
        {
            if (!File.Exists(chainFile))
            {
                _logger.Info($"Chain file does not exist: {chainFile}, skipping");
                return;
            }

            var chainFileContent = File.ReadAllBytes(chainFile);
            var context = new Rlp.DecoderContext(chainFileContent);
            var blocks = new List<Block>();
            while (context.ReadNumberOfItemsRemaining() > 0)
            {
                context.PeekNextItem();
                blocks.Add(Rlp.Decode<Block>(context));
            }
            
            for (int i = 0; i < blocks.Count; i++)
            {
                ProcessBlock(blocks[i]);
            }
        }

        private void InitializeBlocks(string blocksDir)
        {
            if (!Directory.Exists(blocksDir))
            {
                _logger.Info($"Blocks dir does not exist: {blocksDir}, skipping");
                return;
            }

            var files = Directory.GetFiles(blocksDir).OrderBy(x => x).ToArray();
            var blocks = files.Select(x => new { File = x, Block = DecodeBlock(x) }).OrderBy(x => x.Block.Header.Number).ToArray();
            foreach (var block in blocks)
            {
                _logger.Info($"Processing block file: {block.File}, blockNumber: {block.Block.Header.Number}");
                ProcessBlock(block.Block);
            }
        }

        private Block DecodeBlock(string file)
        {
            var fileContent = File.ReadAllBytes(file);
            var blockRlp = new Rlp(fileContent);
            
            return Rlp.Decode<Block>(blockRlp);
        }

        private void ProcessBlock(Block block)
        {
            try
            {
                _blockTree.SuggestBlock(block);
            }
            catch (InvalidBlockException e)
            {
                _logger.Error($"Invalid block: {block.Hash}, ignoring", e);
            }
        }

        private void InitializeKeys(string keysDir)
        {
            if (!Directory.Exists(keysDir))
            {
                _logger.Info($"Keys dir does not exist: {keysDir}, skipping");
                return;
            }

            var keyStoreDir = GetStoreDirectory();
            var files = Directory.GetFiles(keysDir);
            foreach (var file in files)
            {
                _logger.Info($"Processing key file: {file}");
                var fileContent = File.ReadAllText(file);
                var keyStoreItem = _jsonSerializer.Deserialize<KeyStoreItem>(fileContent);
                _wallet.Add(new Address(keyStoreItem.Address));
            }
        }

        private void InitializeGenesis(string genesisFile)
        {
            var genesisBlockRaw = File.ReadAllText(genesisFile);
            var blockJson = _jsonSerializer.Deserialize<TestGenesisJson>(genesisBlockRaw);
            var stateRoot = InitializeAccounts(blockJson.Alloc);
            var block = Convert(blockJson, stateRoot);
            _blockTree.SuggestBlock(block);
        }

        private static Block Convert(TestGenesisJson headerJson, Keccak stateRoot)
        {
            if (headerJson == null)
            {
                return null;
            }

            var header = new BlockHeader(
                new Keccak(headerJson.ParentHash),
                Keccak.OfAnEmptySequenceRlp,
                new Address(headerJson.Coinbase),
                Bytes.FromHexString(headerJson.Difficulty).ToUInt256(),
                0,
                (long) Bytes.FromHexString(headerJson.GasLimit).ToUnsignedBigInteger(),
                Bytes.FromHexString(headerJson.Timestamp).ToUInt256(),
                Bytes.FromHexString(headerJson.ExtraData)
            )
            {
                Bloom = Bloom.Empty,
                MixHash = new Keccak(headerJson.MixHash),
                Nonce = (ulong) Bytes.FromHexString(headerJson.Nonce).ToUnsignedBigInteger(),               
                ReceiptsRoot = Keccak.EmptyTreeHash,
                StateRoot = Keccak.EmptyTreeHash,
                TransactionsRoot = Keccak.EmptyTreeHash
            };

            header.StateRoot = stateRoot;
            header.Hash = BlockHeader.CalculateHash(header);

            return new Block(header);
        }

        private Keccak InitializeAccounts(IDictionary<string, TestAccount> alloc)
        {   
            foreach (var account in alloc)
            {
                UInt256.CreateFromBigEndian(out UInt256 allocation, Bytes.FromHexString(account.Value.Balance));
                _stateProvider.CreateAccount(new Address(account.Key), account.Value.Balance.StartsWith("0x") 
                    ? allocation : UInt256.Parse(account.Value.Balance));
            }
            
            _stateProvider.Commit(_specProvider.GenesisSpec);
            _stateDb.Commit();
            
            return _stateProvider.StateRoot;
        }

        private string GetStoreDirectory()
        {
            var directory = _configurationProvider.GetConfig<IKeyStoreConfig>().KeyStoreDirectory;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return directory;
        }
    }
}