// Vintagestory.GameContent.ItemShield
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace CFlag
{
  public class CFItemFlag : Item, ITexPositionSource, IContainedMeshSource
  {
    private ICoreClientAPI capi;
    private ITextureAtlasAPI targetAtlas;
    private readonly Dictionary<string, AssetLocation> tmpTextures = new();
    public Size2i AtlasSize => targetAtlas.Size;
    private Dictionary<int, MeshRef> Meshrefs => ObjectCacheUtil.GetOrCreate(api, "shieldmeshrefs", () => new Dictionary<int, MeshRef>());
    public string FlagSize => Variant["size"];
    public string FlagType => Variant["type"];
    public string mainTexPrefix;
    public string colorTexPrefix;

    public TextureAtlasPosition this[string textureCode] => GetOrCreateTexPos(tmpTextures[textureCode]);

    protected TextureAtlasPosition GetOrCreateTexPos(AssetLocation texturePath)
    {
      TextureAtlasPosition texpos = targetAtlas[texturePath];
      if (texpos == null)
      {
        IAsset texAsset = capi.Assets.TryGet(texturePath.Clone().WithPathPrefixOnce("textures/").WithPathAppendixOnce(".png"));
        if (texAsset != null)
        {
          targetAtlas.GetOrInsertTexture(texturePath, out var _, out texpos, () => texAsset.ToBitmap(capi));
        }
        else
        {
          capi.World.Logger.Warning("For render in flag {0}, require texture {1}, but no such texture found.", Code, texturePath);
        }
      }
      return texpos;
    }

    public override void OnLoaded(ICoreAPI api)
    {
      base.OnLoaded(api);
      capi = api as ICoreClientAPI;

      mainTexPrefix = GetTextureLocationPrefix("main");
      colorTexPrefix = GetTextureLocationPrefix("color");
      AddAllTypesToCreativeInventory();
    }

    public string GetTextureLocationPrefix(string key) => Attributes["texturePrefixes"][key].AsString();

    public void AddAllTypesToCreativeInventory()
    {
      var stacks = new List<JsonItemStack>();
      var variantGroups = Attributes["variantGroups"].AsObject<Dictionary<string, string[]>>();

      if (FlagType == "country")
      {
        foreach (var country in variantGroups["country"])
          stacks.Add(GenJstack(string.Format("{{ country: \"{0}\"}}", country)));
      }

      if (FlagType == "onecolor")
      {
        foreach (var color1 in variantGroups["onecolor"])
          stacks.Add(GenJstack(string.Format("{{ onecolor: \"{0}\"}}", color1)));
      }

      CreativeInventoryStacks = new CreativeTabAndStackList[]
      {
        new CreativeTabAndStackList() { Stacks = stacks.ToArray(), Tabs = new string[] { "cflag" } }
      };
    }

    private JsonItemStack GenJstack(string json)
    {
      var jsonItemStack = new JsonItemStack
      {
        Code = Code,
        Type = EnumItemClass.Item,
        Attributes = new JsonObject(JToken.Parse(json))
      };
      jsonItemStack.Resolve(api.World, "flag type");
      return jsonItemStack;
    }

    public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
    {
      int meshRefId = itemstack.TempAttributes.GetInt("meshRefId");
      if (meshRefId == 0 || !Meshrefs.TryGetValue(meshRefId, out renderinfo.ModelRef))
      {
        var num = Meshrefs.Count + 1;
        var meshRef = capi.Render.UploadMesh(GenMesh(itemstack, capi.ItemTextureAtlas));
        renderinfo.ModelRef = Meshrefs[num] = meshRef;
        itemstack.TempAttributes.SetInt("meshRefId", num);
      }
      base.OnBeforeRender(capi, itemstack, target, ref renderinfo);
    }

    public MeshData GenMesh(ItemStack itemStack, ITextureAtlasAPI targetAtlas)
    {
      this.targetAtlas = targetAtlas;
      tmpTextures.Clear();

      tmpTextures["pole"] = new AssetLocation("block/liquid/dye/gray.png");

      var country = itemStack.Attributes.GetString("country");
      var onecolor = itemStack.Attributes.GetString("onecolor");
      var color1 = itemStack.Attributes.GetString("color1");
      var color2 = itemStack.Attributes.GetString("color2");
      var color3 = itemStack.Attributes.GetString("color3");
      var color4 = itemStack.Attributes.GetString("color4");
      var color5 = itemStack.Attributes.GetString("color5");
      var color6 = itemStack.Attributes.GetString("color6");

      if (FlagType == "country" && string.IsNullOrEmpty(country)) return new MeshData();
      if (FlagType == "onecolor" && string.IsNullOrEmpty(onecolor)) return new MeshData();
      if (FlagType == "h2color"
        && string.IsNullOrEmpty(color1)
        && string.IsNullOrEmpty(color2))
      {
        return new MeshData();
      }

      if (FlagType == "h3color"
        && string.IsNullOrEmpty(color1)
        && string.IsNullOrEmpty(color2)
        && string.IsNullOrEmpty(color3))
      {
        return new MeshData();
      }

      if (FlagType == "h4color"
        && string.IsNullOrEmpty(color1)
        && string.IsNullOrEmpty(color2)
        && string.IsNullOrEmpty(color3)
        && string.IsNullOrEmpty(color4))
      {
        return new MeshData();
      }

      if (FlagType == "h5color"
        && string.IsNullOrEmpty(color1)
        && string.IsNullOrEmpty(color2)
        && string.IsNullOrEmpty(color3)
        && string.IsNullOrEmpty(color4)
        && string.IsNullOrEmpty(color5))
      {
        return new MeshData();
      }

      if (FlagType == "h6color"
        && string.IsNullOrEmpty(color1)
        && string.IsNullOrEmpty(color2)
        && string.IsNullOrEmpty(color3)
        && string.IsNullOrEmpty(color4)
        && string.IsNullOrEmpty(color5)
        && string.IsNullOrEmpty(color6))
      {
        return new MeshData();
      }

      if (FlagType == "country")
      {
        tmpTextures["main"] = new AssetLocation(mainTexPrefix + country + ".png");
      }
      if (FlagType == "onecolor")
      {
        tmpTextures["main"] = new AssetLocation(mainTexPrefix + onecolor + ".png");
      }
      if (FlagType == "h2color")
      {
        tmpTextures["color1"] = new AssetLocation(colorTexPrefix + color1 + ".png");
        tmpTextures["color2"] = new AssetLocation(colorTexPrefix + color2 + ".png");
      }
      if (FlagType == "h3color")
      {
        tmpTextures["color1"] = new AssetLocation(colorTexPrefix + color1 + ".png");
        tmpTextures["color2"] = new AssetLocation(colorTexPrefix + color2 + ".png");
        tmpTextures["color3"] = new AssetLocation(colorTexPrefix + color3 + ".png");
      }
      if (FlagType == "h4color")
      {
        tmpTextures["color1"] = new AssetLocation(colorTexPrefix + color1 + ".png");
        tmpTextures["color2"] = new AssetLocation(colorTexPrefix + color2 + ".png");
        tmpTextures["color3"] = new AssetLocation(colorTexPrefix + color3 + ".png");
        tmpTextures["color4"] = new AssetLocation(colorTexPrefix + color4 + ".png");
      }
      if (FlagType == "h5color")
      {
        tmpTextures["color1"] = new AssetLocation(colorTexPrefix + color1 + ".png");
        tmpTextures["color2"] = new AssetLocation(colorTexPrefix + color2 + ".png");
        tmpTextures["color3"] = new AssetLocation(colorTexPrefix + color3 + ".png");
        tmpTextures["color4"] = new AssetLocation(colorTexPrefix + color4 + ".png");
        tmpTextures["color5"] = new AssetLocation(colorTexPrefix + color5 + ".png");
      }
      if (FlagType == "h6color")
      {
        tmpTextures["color1"] = new AssetLocation(colorTexPrefix + color1 + ".png");
        tmpTextures["color2"] = new AssetLocation(colorTexPrefix + color2 + ".png");
        tmpTextures["color3"] = new AssetLocation(colorTexPrefix + color3 + ".png");
        tmpTextures["color4"] = new AssetLocation(colorTexPrefix + color4 + ".png");
        tmpTextures["color5"] = new AssetLocation(colorTexPrefix + color5 + ".png");
        tmpTextures["color6"] = new AssetLocation(colorTexPrefix + color6 + ".png");
      }

      capi.Tesselator.TesselateItem(this, out var modeldata, this);
      return modeldata;
    }

    public override string GetHeldItemName(ItemStack itemStack) => Lang.Get("cflag:flag");

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
      base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

      if (FlagType == "onecolor")
      {
        var colorDsc = Lang.Get("color-" + inSlot.Itemstack.Attributes.GetString("onecolor"));
        dsc.Append(Lang.Get("Color: ")).AppendLine(colorDsc);
      }

      if (FlagType == "h2color")
      {
        var color1 = inSlot.Itemstack.Attributes.GetString("color1");
        var color2 = inSlot.Itemstack.Attributes.GetString("color2");

        AppendIfExists(dsc, "cflag:h2color-" + color1 + "-" + color2);

        dsc.Append(Lang.Get("1st color: ")).AppendLine(Lang.Get("color-" + color1));
        dsc.Append(Lang.Get("2nd color: ")).AppendLine(Lang.Get("color-" + color2));

        var simpleCode = "simplecode: " + FlagType + "-" + color1 + "-" + color2;
        dsc.Append("\n").Append(simpleCode);
      }

      if (FlagType == "h3color")
      {
        var color1 = inSlot.Itemstack.Attributes.GetString("color1");
        var color2 = inSlot.Itemstack.Attributes.GetString("color2");
        var color3 = inSlot.Itemstack.Attributes.GetString("color3");

        AppendIfExists(dsc, "cflag:h3color-" + color1 + "-" + color2 + "-" + color3);

        dsc.Append(Lang.Get("1st color: ")).AppendLine(Lang.Get("color-" + color1));
        dsc.Append(Lang.Get("2nd color: ")).AppendLine(Lang.Get("color-" + color2));
        dsc.Append(Lang.Get("3rd color: ")).AppendLine(Lang.Get("color-" + color3));

        var simpleCode = "simplecode: " + FlagType + "-" + color1 + "-" + color2 + "-" + color3;
        dsc.Append("\n").Append(simpleCode);
      }

      if (FlagType == "h4color")
      {
        var color1 = inSlot.Itemstack.Attributes.GetString("color1");
        var color2 = inSlot.Itemstack.Attributes.GetString("color2");
        var color3 = inSlot.Itemstack.Attributes.GetString("color3");
        var color4 = inSlot.Itemstack.Attributes.GetString("color4");

        AppendIfExists(dsc, "cflag:h3color-" + color1 + "-" + color2 + "-" + color3 + "-" + color4);

        dsc.Append(Lang.Get("1st color: ")).AppendLine(Lang.Get("color-" + color1));
        dsc.Append(Lang.Get("2nd color: ")).AppendLine(Lang.Get("color-" + color2));
        dsc.Append(Lang.Get("3rd color: ")).AppendLine(Lang.Get("color-" + color3));
        dsc.Append(Lang.Get("4th color: ")).AppendLine(Lang.Get("color-" + color4));

        var simpleCode = "simplecode: " + FlagType + "-" + color1 + "-" + color2 + "-" + color3 + "-" + color4;
        dsc.Append("\n").Append(simpleCode);
      }

      if (FlagType == "h5color")
      {
        var color1 = inSlot.Itemstack.Attributes.GetString("color1");
        var color2 = inSlot.Itemstack.Attributes.GetString("color2");
        var color3 = inSlot.Itemstack.Attributes.GetString("color3");
        var color4 = inSlot.Itemstack.Attributes.GetString("color4");
        var color5 = inSlot.Itemstack.Attributes.GetString("color5");

        AppendIfExists(dsc, "cflag:h3color-" + color1 + "-" + color2 + "-" + color3 + "-" + color4 + "-" + color5);

        dsc.Append(Lang.Get("1st color: ")).AppendLine(Lang.Get("color-" + color1));
        dsc.Append(Lang.Get("2nd color: ")).AppendLine(Lang.Get("color-" + color2));
        dsc.Append(Lang.Get("3rd color: ")).AppendLine(Lang.Get("color-" + color3));
        dsc.Append(Lang.Get("4th color: ")).AppendLine(Lang.Get("color-" + color4));
        dsc.Append(Lang.Get("5th color: ")).AppendLine(Lang.Get("color-" + color5));

        var simpleCode = "simplecode: " + FlagType + "-" + color1 + "-" + color2 + "-" + color3 + "-" + color4 + "-" + color5;
        dsc.Append("\n").Append(simpleCode);
      }

      if (FlagType == "h6color")
      {
        var color1 = inSlot.Itemstack.Attributes.GetString("color1");
        var color2 = inSlot.Itemstack.Attributes.GetString("color2");
        var color3 = inSlot.Itemstack.Attributes.GetString("color3");
        var color4 = inSlot.Itemstack.Attributes.GetString("color4");
        var color5 = inSlot.Itemstack.Attributes.GetString("color5");
        var color6 = inSlot.Itemstack.Attributes.GetString("color6");

        AppendIfExists(dsc, "cflag:h3color-" + color1 + "-" + color2 + "-" + color3 + "-" + color4 + "-" + color5 + "-" + color6);

        dsc.Append(Lang.Get("1st color: ")).AppendLine(Lang.Get("color-" + color1));
        dsc.Append(Lang.Get("2nd color: ")).AppendLine(Lang.Get("color-" + color2));
        dsc.Append(Lang.Get("3rd color: ")).AppendLine(Lang.Get("color-" + color3));
        dsc.Append(Lang.Get("4th color: ")).AppendLine(Lang.Get("color-" + color4));
        dsc.Append(Lang.Get("5th color: ")).AppendLine(Lang.Get("color-" + color5));
        dsc.Append(Lang.Get("6th color: ")).AppendLine(Lang.Get("color-" + color6));

        var simpleCode = "simplecode: " + FlagType + "-" + color1 + "-" + color2 + "-" + color3 + "-" + color4 + "-" + color5 + "-" + color6;
        dsc.Append("\n").Append(simpleCode);
      }

      if (FlagType == "country")
      {
        var alpha2code = inSlot.Itemstack.Attributes.GetString("country");

        AppendIfExists(dsc, "cflag:country-" + alpha2code);

        dsc.Append(Lang.Get("Country: ")).AppendLine(Lang.Get("cflag:country-" + alpha2code));
      }
    }

    private static void AppendIfExists(StringBuilder dsc, string firstPart)
    {
      var flagDsc = firstPart + "-dsc";

      if (Lang.HasTranslation(flagDsc))
      {
        dsc.Append("\n");
        dsc.AppendLine(Lang.Get(flagDsc));
        dsc.Append("\n");
      }
    }

    public MeshData GenMesh(ItemStack itemstack, ITextureAtlasAPI targetAtlas, BlockPos atBlockPos)
    {
      return GenMesh(itemstack, targetAtlas);
    }

    public string GetMeshCacheKey(ItemStack itemstack)
    {
      if (FlagType == "country")
      {
        var country = itemstack.Attributes.GetString("country");
        return $"{Code.ToShortString()}-{country}";
      }
      if (FlagType == "onecolor")
      {
        var onecolor = itemstack.Attributes.GetString("onecolor");
        return $"{Code.ToShortString()}-{onecolor}";
      }
      if (FlagType == "h2color")
      {
        var color1 = itemstack.Attributes.GetString("color1");
        var color2 = itemstack.Attributes.GetString("color2");
        return $"{Code.ToShortString()}-{color1}-{color2}";
      }
      if (FlagType == "h3color")
      {
        var color1 = itemstack.Attributes.GetString("color1");
        var color2 = itemstack.Attributes.GetString("color2");
        var color3 = itemstack.Attributes.GetString("color3");
        return $"{Code.ToShortString()}-{color1}-{color2}-{color3}";
      }
      if (FlagType == "h4color")
      {
        var color1 = itemstack.Attributes.GetString("color1");
        var color2 = itemstack.Attributes.GetString("color2");
        var color3 = itemstack.Attributes.GetString("color3");
        var color4 = itemstack.Attributes.GetString("color4");
        return $"{Code.ToShortString()}-{color1}-{color2}-{color3}-{color4}";
      }
      if (FlagType == "h5color")
      {
        var color1 = itemstack.Attributes.GetString("color1");
        var color2 = itemstack.Attributes.GetString("color2");
        var color3 = itemstack.Attributes.GetString("color3");
        var color4 = itemstack.Attributes.GetString("color4");
        var color5 = itemstack.Attributes.GetString("color5");
        return $"{Code.ToShortString()}-{color1}-{color2}-{color3}-{color4}-{color5}";
      }
      if (FlagType == "h6color")
      {
        var color1 = itemstack.Attributes.GetString("color1");
        var color2 = itemstack.Attributes.GetString("color2");
        var color3 = itemstack.Attributes.GetString("color3");
        var color4 = itemstack.Attributes.GetString("color4");
        var color5 = itemstack.Attributes.GetString("color5");
        var color6 = itemstack.Attributes.GetString("color6");
        return $"{Code.ToShortString()}-{color1}-{color2}-{color3}-{color4}-{color5}-{color6}";
      }
      else
      {
        return $"{Code.ToShortString()}-{FlagSize}-{FlagType}";
      }
    }
  }
}