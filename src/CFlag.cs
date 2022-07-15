using Vintagestory.API.Common;

[assembly: ModInfo("CR Flags",
    Authors = new[] { "Craluminum2413" })]

namespace CFlag
{
    public class CFlag : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterItemClass("CFItemFlag", typeof(CFItemFlag));
            api.RegisterBlockBehaviorClass("BlockFlag", typeof(BlockBehaviorFlag));
            api.RegisterBlockBehaviorClass("BlockPole", typeof(BlockBehaviorPole));
        }
    }
}