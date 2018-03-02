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

namespace Nevermind.Discovery
{
    public interface IDiscoveryConfigurationProvider
    {
        /// <summary>
        /// Kademlia - k
        /// </summary>
        int BucketSize { get; }

        /// <summary>
        /// Buckets count
        /// </summary>
        int BucketsCount { get; }

        /// <summary>
        /// Kademlia - alpha
        /// </summary>
        int Concurrency { get; }

        /// <summary>
        /// Kademlia - b
        /// </summary>
        int BitsPerHop { get; }

        /// <summary>
        /// Current Node host
        /// </summary>
        string MasterHost { get; }

        /// <summary>
        /// Current Node port
        /// </summary>
        int MasterPort { get; }

        /// <summary>
        /// Max Discovery Rounds
        /// </summary>
        int MaxDiscoveryRounds { get; }

        /// <summary>
        /// Eviction check interval in ms
        /// </summary>
        int EvictionCheckInterval { get; }

        /// <summary>
        /// Send Node Timeout in ms
        /// </summary>
        int SendNodeTimeout { get; }

        /// <summary>
        /// Pong Timeout in ms
        /// </summary>
        int PongTimeout { get; }

        /// <summary>
        /// Boot Node Pong Timeout in ms
        /// </summary>
        int BootNodePongTimeout { get; }

        /// <summary>
        /// Pong Timeout in ms
        /// </summary>
        int PingRetryCount { get; }

        /// <summary>
        /// Time between running dicovery processes in miliseconds
        /// </summary>
        int DiscoveryInterval { get; }

        /// <summary>
        /// Time between running refresh processes in miliseconds
        /// </summary>
        int RefreshInterval { get; }

        /// <summary>
        /// Boot nodes connection details
        /// </summary>
        (string Host, int Port)[] BootNodes { get; }

        /// <summary>
        /// Key Pass
        /// </summary>
        string KeyPass { get; }

        /// <summary>
        /// Timeout for closing UDP channel in miliseconds
        /// </summary>
        int UdpChannelCloseTimeout { get; }

        /// <summary>
        /// Version of the Ping message
        /// </summary>
        int PingMessageVersion { get; }

        /// <summary>
        /// Ping expiry time in seconds
        /// </summary>
        int DiscoveryMsgExpiryTime { get; }

        /// <summary>
        /// Maximum count of NodeLifecycleManagers stored in memory
        /// </summary>
        int MaxNodeLifecycleManagersCount { get; }

        /// <summary>
        /// Count of NodeLifecycleManagers to remove in one cleanup cycle
        /// </summary>
        int NodeLifecycleManagersCleaupCount { get; }
    }
}