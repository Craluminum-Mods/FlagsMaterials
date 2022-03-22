using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace CFlag
{
    public class BlockBehaviorPole : BlockBehavior
    {
        public BlockBehaviorPole(Block block) : base(block)
        {
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref EnumHandling handling, ref string failureCode)
        {
            if (byPlayer.Entity.Controls.Sneak && !byPlayer.Entity.Controls.Sprint)
            {
                var pos = blockSel.Face == BlockFacing.UP ? blockSel.Position.Copy() : blockSel.Position.AddCopy(blockSel.Face.Opposite);
                var attachingBlock = world.BlockAccessor.GetBlock(pos);
                while (attachingBlock.HasBehavior<BlockBehaviorPole>() || attachingBlock.HasBehavior<BlockBehaviorFlag>())
                {
                    pos = pos.Up();
                    attachingBlock = world.BlockAccessor.GetBlock(pos);
                    if (attachingBlock.Id == 0)
                    {
                        blockSel.Position = pos;
                        return base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref handling, ref failureCode);
                    }
                }
            }
            return base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref handling, ref failureCode);
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            handling = EnumHandling.PreventDefault;
            var byEntity = byPlayer.Entity;
            if (byPlayer.Entity.Controls.Sprint && !byPlayer.Entity.Controls.Sneak)
            {
                var blockAccessor = world.BlockAccessor;
                var upPos = blockSel.Position.Copy();
                while (blockAccessor.GetBlock(upPos).HasBehavior<BlockBehaviorPole>())
                {
                    upPos = upPos.Up();
                    if (blockAccessor.GetBlock(upPos).HasBehavior<BlockBehaviorFlag>())
                    {
                        world.RegisterCallbackUnique(tryFlipFlagDownwards, upPos, 500);
                    }
                }
                return true;
            }
            else if (!byPlayer.Entity.Controls.Sprint && !byPlayer.Entity.Controls.Sneak)
            {
                // should not need a null check here, player should always have hands
                if ((byEntity.RightHandItemSlot.Empty
                        || byEntity.RightHandItemSlot.Itemstack.Block?.HasBehavior<BlockBehaviorPole>() == true)
                    // if we successfully give the player the lowest pole, we can start the lowering part
                    && byPlayer.InventoryManager.TryGiveItemstack(new ItemStack(block)))
                {
                    // exchanging the current plockpos with blockid 0 transforms the block into air
                    world.BlockAccessor.ExchangeBlock(0, blockSel.Position);
                    var polePos = blockSel.Position.UpCopy();
                    var poleBlock = world.BlockAccessor.GetBlock(polePos);
                    // while the upper block is either a flag or a pole we want to continue moving down the pole
                    while (poleBlock.HasBehavior<BlockBehaviorPole>() || poleBlock.HasBehavior<BlockBehaviorFlag>())
                    {
                        // duplicate the current block to the lower position
                        world.BlockAccessor.ExchangeBlock(poleBlock.Id, polePos.DownCopy());
                        // replace the current block by thin air
                        world.BlockAccessor.ExchangeBlock(0, polePos);

                        // go to the upper block
                        polePos = polePos.Up();
                        poleBlock = world.BlockAccessor.GetBlock(polePos);
                    }
                    return true;
                }
            }
            return false;
        }

        private void tryFlipFlagDownwards(IWorldAccessor worldAccessor, BlockPos pos, float dt)
        {
            IBlockAccessor blockAccessor = worldAccessor.BlockAccessor;
            var downPos = pos.DownCopy();
            var flag = blockAccessor.GetBlock(pos);
            var pole = blockAccessor.GetBlock(downPos);
            if (pole.HasBehavior<BlockBehaviorPole>() && flag.HasBehavior<BlockBehaviorFlag>())
            {
                blockAccessor.ExchangeBlock(pole.Id, pos);
                blockAccessor.ExchangeBlock(flag.Id, downPos);
                worldAccessor.RegisterCallbackUnique(tryFlipFlagDownwards, downPos, 500);
            }
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer, ref EnumHandling handling)
        {
            handling = EnumHandling.PassThrough;
            return new WorldInteraction[]
            {
                new WorldInteraction()
                {
                    ActionLangCode = "cflag-pole-downwards",
                    HotKeyCode = "sprint",
                    MouseButton = EnumMouseButton.Right
                },
                new WorldInteraction()
                {
                    ActionLangCode = "cflag-pole-addflag",
                    HotKeyCode = "sneak",
                    MouseButton = EnumMouseButton.Right
                }
            };
        }
    }
}