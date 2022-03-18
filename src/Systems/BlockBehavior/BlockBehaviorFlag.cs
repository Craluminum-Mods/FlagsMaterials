using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace CFlag
{
    public class BlockBehaviorFlag : BlockBehavior
    {
        public BlockBehaviorFlag(Block block) : base(block)
        {
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            var byEntity = byPlayer.Entity;
            if (blockSel != null)
            {
                byEntity.World.RegisterCallbackUnique(tryFlipFlagUpwards, blockSel.Position, 500);
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);
        }

        private void tryFlipFlagUpwards(IWorldAccessor worldAccessor, BlockPos pos, float dt)
        {
            IBlockAccessor blockAccessor = worldAccessor.BlockAccessor;
            var upPos = pos.UpCopy();
            var flag = blockAccessor.GetBlock(pos);
            var pole = blockAccessor.GetBlock(upPos);
            if (pole.HasBehavior<BlockBehaviorPole>() && flag.HasBehavior<BlockBehaviorFlag>())
            {
                blockAccessor.ExchangeBlock(pole.Id, pos);
                blockAccessor.ExchangeBlock(flag.Id, upPos);
                worldAccessor.RegisterCallbackUnique(tryFlipFlagUpwards, upPos, 500);
            }
        }
    }
}