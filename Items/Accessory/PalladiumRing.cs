using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpiritMod.Items.Accessory
{
	public class PalladiumRing : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Palladium Ring";
			item.width = 18;
			item.height = 18;
            item.toolTip = "Applies life regeneration when an enemy is hit";
            item.value = Item.buyPrice(0, 10, 0, 0);
			item.rare = 4;

			item.accessory = true;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<MyPlayer>(mod).hpRegenRing = true;
		}
		public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.PalladiumBar, 10);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
	}
}
