using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace CFlag
{
    public class BlockBehaviorFlag : BlockBehavior
    {
        public BlockBehaviorFlag(Block block) : base(block)
        {
        }
        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref EnumHandling handling, ref string failureCode)
        {
            handling = EnumHandling.PreventDefault;
            BlockPos pos = blockSel.Position.AddCopy(blockSel.Face.Opposite);
            Block attachingBlock = world.BlockAccessor.GetBlock(pos);
            if (attachingBlock.HasBehavior<BlockBehaviorPole>())
            {
                // Might need a little extra logic for handling different flag facing
                world.BlockAccessor.ExchangeBlock(block.Id, pos);
                return true;
            }
            else
            {
                failureCode = "cflag-flag-polerequired";
                return false;
            }
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            handling = EnumHandling.PreventDefault;
            var byEntity = byPlayer.Entity;
            if (blockSel != null)
            {
                if (byPlayer.Entity.Controls.Sprint)
                {
                    byEntity.World.RegisterCallbackUnique(tryFlipFlagUpwards, blockSel.Position, 500);
                }
                else
                {
                    if (byPlayer.InventoryManager.TryGiveItemstack(new ItemStack(block)) && byPlayer.Entity.Controls.Sneak)
                    {
                        var pole = world.BlockAccessor.GetBlock(new AssetLocation("cflag", "pole"));
                        world.BlockAccessor.ExchangeBlock(pole.Id, blockSel.Position);
                    }
                }
            }
            return true;
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

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer, ref EnumHandling handling)
        {
            handling = EnumHandling.PassThrough;
            return new WorldInteraction[]
            {
                new WorldInteraction()
                {
                    ActionLangCode = "cflag-pole-upwards",
                    HotKeyCode = "sprint",
                    MouseButton = EnumMouseButton.Right
                },
                new WorldInteraction()
                {
                    ActionLangCode = "cflag-flag-pickup",
                    HotKeyCode = "sneak",
                    MouseButton = EnumMouseButton.Right
                }
            };
        }
    }
}