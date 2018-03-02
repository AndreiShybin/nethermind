﻿using System.Net;
using Nevermind.Core;
using Nevermind.Core.Crypto;
using Nevermind.Discovery.Messages;
using Nevermind.Discovery.RoutingTable;
using Nevermind.Discovery.Serializers;
using Nevermind.Network;
using NUnit.Framework;

namespace Nevermind.Discovery.Test
{
    [TestFixture]
    public class DiscoveryMessageSerializerTests
    {
        private readonly PrivateKey _privateKey = new PrivateKey("49a7b37aa6f6645917e7b807e9d1c00d4fa71f18343b0d4122a4d2df64dd6fee");
        //private readonly PrivateKey _farPrivateKey = new PrivateKey("3a1076bf45ab87712ad64ccb3b10217737f7faacbf2872e88fdd9a537d8fe266");
        private IPEndPoint _farAddress;
        private IPEndPoint _nearAddress;
        private ISigner _signer;
        private IDiscoveryConfigurationProvider _config;
        private IMessageSerializationService _messageSerializationService;

        [SetUp]
        public void Initialize()
        {
            _config = new DiscoveryConfigurationProvider();
            _farAddress = new IPEndPoint(IPAddress.Parse("192.168.1.2"), 1);
            _nearAddress = new IPEndPoint(IPAddress.Parse(_config.MasterHost), _config.MasterPort);
            _signer = new Signer();

            var pingSerializer = new PingMessageSerializer(_signer, _privateKey, new DiscoveryMessageFactory(_config), new NodeIdResolver(_signer), new NodeFactory());
            var pongSerializer = new PongMessageSerializer(_signer, _privateKey, new DiscoveryMessageFactory(_config), new NodeIdResolver(_signer), new NodeFactory());
            var findNodeSerializer = new FindNodeMessageSerializer(_signer, _privateKey, new DiscoveryMessageFactory(_config), new NodeIdResolver(_signer), new NodeFactory());
            var neighborsSerializer = new NeighborsMessageSerializer(_signer, _privateKey, new DiscoveryMessageFactory(_config), new NodeIdResolver(_signer), new NodeFactory());

            _messageSerializationService = new MessageSerializationService();
            _messageSerializationService.Register(pingSerializer);
            _messageSerializationService.Register(pongSerializer);
            _messageSerializationService.Register(findNodeSerializer);
            _messageSerializationService.Register(neighborsSerializer);
        }

        [Test]
        public void PingMessageTest()
        {
            var message = new PingMessage
            {
                FarAddress = _farAddress,
                DestinationAddress = _nearAddress,
                SourceAddress = _farAddress,
                Version = _config.PingMessageVersion,
                FarPublicKey = _privateKey.PublicKey,
                ExpirationTime = _config.DiscoveryMsgExpiryTime + Timestamp.UnixUtcUntilNowMilisecs
            };

            var data = _messageSerializationService.Serialize(message);
            var deserializedMessage = _messageSerializationService.Deserialize<PingMessage>(data);

            Assert.AreEqual(message.MessageType, deserializedMessage.MessageType);
            Assert.AreEqual(message.FarPublicKey, deserializedMessage.FarPublicKey);
            Assert.AreEqual(message.ExpirationTime, deserializedMessage.ExpirationTime);

            Assert.AreEqual(message.FarAddress, deserializedMessage.SourceAddress);
            Assert.AreEqual(message.DestinationAddress, deserializedMessage.DestinationAddress);
            Assert.AreEqual(message.SourceAddress, deserializedMessage.SourceAddress);
            Assert.AreEqual(message.Version, deserializedMessage.Version);
            Assert.IsNotNull(deserializedMessage.Mdc);
        }

        [Test]
        public void PongMessageTest()
        {
            var message = new PongMessage
            {
                FarAddress = _farAddress,
                PingMdc = new byte[] {1, 2, 3},
                FarPublicKey = _privateKey.PublicKey,
                ExpirationTime = _config.DiscoveryMsgExpiryTime + Timestamp.UnixUtcUntilNowMilisecs
            };

            var data = _messageSerializationService.Serialize(message);
            var deserializedMessage = _messageSerializationService.Deserialize<PongMessage>(data);

            Assert.AreEqual(message.MessageType, deserializedMessage.MessageType);
            Assert.AreEqual(message.FarPublicKey, deserializedMessage.FarPublicKey);
            Assert.AreEqual(message.ExpirationTime, deserializedMessage.ExpirationTime);

            Assert.AreEqual(message.PingMdc, deserializedMessage.PingMdc);
        }

        [Test]
        public void FindNodeMessageTest()
        {
            var message = new FindNodeMessage
            {
                FarAddress = _farAddress,
                SearchedNodeId = new byte[] { 1, 2, 3 },
                FarPublicKey = _privateKey.PublicKey,
                ExpirationTime = _config.DiscoveryMsgExpiryTime + Timestamp.UnixUtcUntilNowMilisecs
            };

            var data = _messageSerializationService.Serialize(message);
            var deserializedMessage = _messageSerializationService.Deserialize<FindNodeMessage>(data);

            Assert.AreEqual(message.MessageType, deserializedMessage.MessageType);
            Assert.AreEqual(message.FarPublicKey, deserializedMessage.FarPublicKey);
            Assert.AreEqual(message.ExpirationTime, deserializedMessage.ExpirationTime);

            Assert.AreEqual(message.SearchedNodeId, deserializedMessage.SearchedNodeId);
        }

        [Test]
        public void NeighborsMessageTest()
        {
            var nodeFactory = new NodeFactory();

            var message = new NeighborsMessage
            {
                FarAddress = _farAddress,
                Nodes = new[] { nodeFactory.CreateNode("192.168.1.2", 1), nodeFactory.CreateNode("192.168.1.3", 2), nodeFactory.CreateNode("192.168.1.4", 3) },
                FarPublicKey = _privateKey.PublicKey,
                ExpirationTime = _config.DiscoveryMsgExpiryTime + Timestamp.UnixUtcUntilNowMilisecs
            };

            var data = _messageSerializationService.Serialize(message);
            var deserializedMessage = _messageSerializationService.Deserialize<NeighborsMessage>(data);

            Assert.AreEqual(message.MessageType, deserializedMessage.MessageType);
            Assert.AreEqual(message.FarPublicKey, deserializedMessage.FarPublicKey);
            Assert.AreEqual(message.ExpirationTime, deserializedMessage.ExpirationTime);

            for (var i = 0; i < message.Nodes.Length; i++)
            {
                Assert.AreEqual(message.Nodes[i].Host, deserializedMessage.Nodes[i].Host);
                Assert.AreEqual(message.Nodes[i].Port, deserializedMessage.Nodes[i].Port);
                Assert.AreEqual(message.Nodes[i].IdHashText, deserializedMessage.Nodes[i].IdHashText);
                Assert.AreEqual(message.Nodes[i], deserializedMessage.Nodes[i]);
            }
        }
    }
}