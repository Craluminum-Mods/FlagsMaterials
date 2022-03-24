using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace CFlag
{
    public class BlockBehaviorPole : BlockBehavior
    {
        public BlockBehaviorPole(Block block) : base(block)
        {
        }

        WorldInteraction[] interactions;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            if (api.Side != EnumAppSide.Client) return;
            ICoreClientAPI capi = api as ICoreClientAPI;

            interactions = ObjectCacheUtil.GetOrCreate(api, "flagpoleInteractions", () =>
            {
                List<ItemStack> canAddFlagStacks = new List<ItemStack>();

                foreach (CollectibleObject obj in api.World.Collectibles)
                {
                    if (obj is Block && (obj as Block).HasBehavior<BlockBehaviorFlag>())
                    {
                        List<ItemStack> stacks = obj.GetHandBookStacks(capi);
                        if (stacks != null) canAddFlagStacks.AddRange(stacks);
                    }
                }

                return new WorldInteraction[] {
                    new WorldInteraction()
                    {
                        ActionLangCode = "cflag:pole-addflag",
                        HotKeyCode = "sneak",
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = canAddFlagStacks.ToArray()
                    },
                    new WorldInteraction()
                    {
                        ActionLangCode = "cflag:pole-downwards",
                        HotKeyCode = "sprint",
                        MouseButton = EnumMouseButton.Right
                    },
                    new WorldInteraction()
                    {
                        ActionLangCode = "cflag:pole-pickup",
                        MouseButton = EnumMouseButton.Right,
                        RequireFreeHand = true
                    }
                };
            });
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer, ref EnumHandling handling)
        {
            return interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer, ref handling));
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
                if ((byEntity.RightHandItemSlot.Empty
                        || byEntity.RightHandItemSlot.Itemstack.Block?.HasBehavior<BlockBehaviorPole>() == true)
                    && byPlayer.InventoryManager.TryGiveItemstack(new ItemStack(block)))
                {
                    world.BlockAccessor.ExchangeBlock(0, blockSel.Position);
                    var polePos = blockSel.Position.UpCopy();
                    var poleBlock = world.BlockAccessor.GetBlock(polePos);
                    while (poleBlock.HasBehavior<BlockBehaviorPole>() || poleBlock.HasBehavior<BlockBehaviorFlag>())
                    {
                        world.BlockAccessor.ExchangeBlock(poleBlock.Id, polePos.DownCopy());
                        world.BlockAccessor.ExchangeBlock(0, polePos);
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
    }
}