/*
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

namespace Nethermind.Core.Encoding
{
    public class ChainLevelDecoder : IRlpDecoder<ChainLevelInfo>
    {
        public ChainLevelInfo Decode(Rlp.DecoderContext context, RlpBehaviors rlpBehaviors = RlpBehaviors.None)
        {
            if (context.IsNextItemNull())
            {
                return null;
            }
            
            int lastCheck = context.ReadSequenceLength() + context.Position;
            bool hasMainChainBlock = context.DecodeBool();

            List<BlockInfo> blockInfos = new List<BlockInfo>();

            context.ReadSequenceLength();
            while (context.Position < lastCheck)
            {
                blockInfos.Add(Rlp.Decode<BlockInfo>(context, RlpBehaviors.AllowExtraData));
            }

            if (!rlpBehaviors.HasFlag(RlpBehaviors.AllowExtraData))
            {
                context.Check(lastCheck);
            }

            ChainLevelInfo info = new ChainLevelInfo(hasMainChainBlock, blockInfos.ToArray());
            return info;
        }

        public Rlp Encode(ChainLevelInfo item, RlpBehaviors rlpBehaviors = RlpBehaviors.None)
        {
            if (item == null)
            {
                return Rlp.OfEmptySequence;
            }
            
            Rlp[] elements = new Rlp[2];
            elements[0] = Rlp.Encode(item.HasBlockOnMainChain);
            elements[1] = Rlp.Encode(item.BlockInfos);

            if (item.BlockInfos.Any(bi => bi == null))
            {
                throw new Exception();
            }
            
            Rlp rlp = Rlp.Encode(elements);
            
            
            return rlp;
        }

        public void Encode(MemoryStream stream, ChainLevelInfo item, RlpBehaviors rlpBehaviors = RlpBehaviors.None)
        {
            throw new System.NotImplementedException();
        }

        public int GetLength(ChainLevelInfo item, RlpBehaviors rlpBehaviors)
        {
            throw new System.NotImplementedException();
        }
    }
}