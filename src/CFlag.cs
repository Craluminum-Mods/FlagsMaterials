using Vintagestory.API.Common;

namespace CFlag
{
    public class CFlag : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterBlockBehaviorClass("BlockFlag", typeof(BlockBehaviorFlag));
            api.RegisterBlockBehaviorClass("BlockPole", typeof(BlockBehaviorPole));
        }
    }
}