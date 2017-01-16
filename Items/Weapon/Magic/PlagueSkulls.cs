using Terraria;
using System;
using Terraria.ID;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
namespace SpiritMod.Items.Weapon.Magic
{
	public class PlagueSkulls : ModItem
	{
		public override void SetDefaults()
		{
			item.reuseDelay = 60;
			item.name = "Plague Skulls";
			item.width = 36;
			item.height = 36;
			item.value = Item.buyPrice(0, 30, 6, 0);
			item.rare = 8;
			item.toolTip = "The longer you use it, the faster it gets, but less dangerous each one becomes";
			item.crit += 6;
			item.damage = 56;
            item.mana = 1;
            item.useStyle = 5;
			item.useTime = 60;
			item.useAnimation = 60;
            item.reuseDelay = 5;
			item.magic = true;
			item.channel = true;
			item.noMelee = true;
			item.noUseGraphic = true;
			item.shoot = mod.ProjectileType("PlagueSkullsHandle");
			item.shootSpeed = 26f;
		}
		 public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(null,"CursedFire", 10);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
	}
}