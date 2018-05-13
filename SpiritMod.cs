using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Initializers;
using Terraria.IO;
using Terraria.GameContent;
using Terraria.ModLoader;
using System.Linq;
using Terraria.UI;
using Terraria.GameContent.UI;
using SpiritMod.NPCs.Boss.Overseer;
using SpiritMod.NPCs.Boss.Atlas;
using SpiritMod.Tide;
using SpiritMod.Projectiles;
using Terraria.Graphics;

namespace SpiritMod
{
	class SpiritMod : Mod
	{
		public const string EMPTY_TEXTURE = "SpiritMod/Empty";
		public const string customEventName = "The Tide";
		public static Texture2D EmptyTexture
		{
			get;
			private set;
		}

		public static int customEvent;
		public static ModHotKey SpecialKey;
		public static ModHotKey ReachKey;
		public static ModHotKey HolyKey;
		public static int GlyphCurrencyID;

		internal static SpiritMod instance;


		public ModPacket GetPacket(MessageType type, int capacity)
		{
			ModPacket packet = base.GetPacket(capacity + 1);
			packet.Write((byte)type);
			return packet;
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			MessageType id = (MessageType)reader.ReadByte();
			byte player;
			switch (id)
			{
				case MessageType.ProjectileData:
					gProj.ReceiveProjectileData(reader, whoAmI);
					break;
				case MessageType.Dodge:
					player = reader.ReadByte();
					byte type = reader.ReadByte();
					if (Main.netMode == 2)
					{
						ModPacket packet = GetPacket(MessageType.Dodge, 2);
						packet.Write(player);
						packet.Write(type);
						packet.Send(-1, whoAmI);
					}
					if (type == 1)
						Items.Glyphs.VeilGlyph.Block(Main.player[player]);
					else
						ErrorLogger.Log("SpiritMod: Unknown message (2:"+ type +")");
					break;
				case MessageType.Dash:
					player = reader.ReadByte();
					DashType dash = (DashType)reader.ReadByte();
					sbyte dir = reader.ReadSByte();
					if (Main.netMode == 2)
					{
						ModPacket packet = GetPacket(MessageType.Dash, 3);
						packet.Write(player);
						packet.Write((byte)dash);
						packet.Write(dir);
						packet.Send(-1, whoAmI);
					}
					Main.player[player].GetModPlayer<MyPlayer>().PerformDash(dash, dir, false);
					break;
				case MessageType.PlayerGlyph:
					player = reader.ReadByte();
					GlyphType glyph = (GlyphType)reader.ReadByte();
					if (Main.netMode == 2)
					{
						ModPacket packet = GetPacket(MessageType.PlayerGlyph, 2);
						packet.Write(player);
						packet.Write((byte)glyph);
						packet.Send(-1, whoAmI);
					}
					if (player == Main.myPlayer)
						break;
					Main.player[player].GetModPlayer<MyPlayer>().glyph = glyph;
					break;
				default:
					ErrorLogger.Log("SpiritMod: Unknown message ("+ id +")");
					break;
			}
		}


		public override void UpdateMusic(ref int music, ref MusicPriority priority)
		{
			if (Main.gameMenu)
				return;
			if (priority > MusicPriority.Event)
				return;
			Player player = Main.LocalPlayer;
			if (!player.active)
				return;
			MyPlayer spirit = player.GetModPlayer<MyPlayer>();
			if (TideWorld.TheTide && TideWorld.InBeach)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/DepthInvasion");
				priority = MusicPriority.Event;
			}

			if (priority > MusicPriority.Environment)
				return;
			if (spirit.ZoneBlueMoon && !Main.dayTime)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/BlueMoon");
				priority = MusicPriority.Environment;
			}

			if (priority > MusicPriority.BiomeHigh)
				return;
			if (spirit.ZoneReach)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/Reach");
				priority = MusicPriority.BiomeHigh;
			}

			if (priority > MusicPriority.BiomeMedium)
				return;
			if (spirit.ZoneSpirit)
			{
				priority = MusicPriority.BiomeMedium;
				if (player.ZoneRockLayerHeight)
				{
					music = GetSoundSlot(SoundType.Music, "Sounds/Music/SpiritUnderground");
				}
				else
				{
					music = GetSoundSlot(SoundType.Music, "Sounds/Music/spirit_overworld");
				}
			}
			else if (spirit.ZoneAsteroid)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/Asteroids");
				priority = MusicPriority.BiomeMedium;
			}
		}


		public override object Call(params object[] args)
		{
			if (args.Length < 1)
			{
				var stack = new System.Diagnostics.StackTrace(true);
				ErrorLogger.Log("Spirit Mod Call Error: No arguments given:\n" + stack.ToString());
				return null;
			}
			CallContext context;
			int? contextNum = args[0] as int?;
			if (contextNum.HasValue)
				context = (CallContext)contextNum.Value;
			else
				context = ParseCallName(args[0] as string);
			if (context == CallContext.Invalid && !contextNum.HasValue)
			{
				var stack = new System.Diagnostics.StackTrace(true);
				ErrorLogger.Log("Spirit Mod Call Error: Context invalid or null:\n" + stack.ToString());
				return null;
			}
			if (context <= CallContext.Invalid || context >= CallContext.Limit)
			{
				var stack = new System.Diagnostics.StackTrace(true);
				ErrorLogger.Log("Spirit Mod Call Error: Context invalid:\n" + stack.ToString());
				return null;
			}
			try
			{
				if (context == CallContext.Downed)
					return BossDowned(args);
				if (context == CallContext.GlyphGet)
					return GetGlyph(args);
				if (context == CallContext.GlyphSet)
				{
					SetGlyph(args);
					return null;
				}
			}
			catch (Exception e)
			{
				ErrorLogger.Log("Spirit Mod Call Error: "+ e.Message + "\n" + e.StackTrace);
			}
			return null;
		}

		private static CallContext ParseCallName(string context)
		{
			if (context == null)
				return CallContext.Invalid;
			switch (context)
			{
				case "downed":
					return CallContext.Downed;
				case "getGlyph":
					return CallContext.GlyphGet;
				case "setGlyph":
					return CallContext.GlyphSet;
			}
			return CallContext.Invalid;
		}

		private static bool BossDowned(object[] args)
		{
			if (args.Length < 2)
				throw new ArgumentException("No boss name specified");
			string name = args[1] as string;
			switch (name)
			{
				case "Scarabeus": return MyWorld.downedScarabeus;
				case "Vinewrath Bane": return MyWorld.downedReachBoss;
				case "Ancient Flier": return MyWorld.downedAncientFlier;
				case "Starplate Raider": return MyWorld.downedRaider;
				case "Infernon": return MyWorld.downedInfernon;
				case "Dusking": return MyWorld.downedDusking;
				case "Ethereal Umbra": return MyWorld.downedSpiritCore;
				case "Illuminant Master": return MyWorld.downedIlluminantMaster;
				case "Atlas": return MyWorld.downedAtlas;
				case "Overseer": return MyWorld.downedOverseer;
			}
			throw new ArgumentException("Invalid boss name:" + name);
		}

		private static void SetGlyph(object[] args)
		{
			if (args.Length < 2)
				throw new ArgumentException("Missing argument: Item");
			else if (args.Length < 3)
				throw new ArgumentException("Missing argument: Glyph");
			Item item = args[1] as Item;
			if (item == null)
				throw new ArgumentException("First argument must be of type Item");
			int? glyphID = args[2] as int?;
			if (!glyphID.HasValue)
				throw new ArgumentException("Second argument must be of type int");
			GlyphType glyph = (GlyphType)glyphID;
			if (glyph < GlyphType.None || glyph >= GlyphType.Count)
				throw new ArgumentException("Glyph must be in range ["+
					(int)GlyphType.None +","+ (int)GlyphType.Count +")");
			item.GetGlobalItem<Items.GItem>().SetGlyph(item, glyph);
		}

		private static int GetGlyph(object[] args)
		{
			if (args.Length < 2)
				throw new ArgumentException("Missing argument: Item");
			Item item = args[1] as Item;
			if (item == null)
				throw new ArgumentException("First argument must be of type Item");
			return (int)item.GetGlobalItem<Items.GItem>().Glyph;
		}


		public override void Load()
		{
			instance = this;
			if (Main.rand == null)
				Main.rand = new Terraria.Utilities.UnifiedRandom();
			if (!Main.dedServ)
				EmptyTexture = GetTexture("Empty");
			//Always keep this call in the first line of Load!
			LoadReferences();
			//Don't add any code before this point,
			// unless you know what you're doing.
			Items.Halloween.CandyBag.Initialize();
			Filters.Scene["SpiritMod:AsteroidSky"] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(0.1f, 0.1f, 0.1f).UseOpacity(0.8f), EffectPriority.High);
			Filters.Scene["SpiritMod:SpiritSky"] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(0f, 0.5f, 1f).UseOpacity(0.3f), EffectPriority.High);
			Filters.Scene["SpiritMod:BlueMoonSky"] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(0f, 0.3f, 1f).UseOpacity(0.75f), EffectPriority.High);

			SpecialKey = RegisterHotKey("Armor Bonus", "Q");
			ReachKey = RegisterHotKey("Frenzy Plant", "E");
			HolyKey = RegisterHotKey("Holy Ward", "Z");

			GlyphCurrencyID = CustomCurrencyManager.RegisterCurrency(new Currency(ItemType<Items.Glyphs.Glyph>(), 999L));

			if (!Main.dedServ)
			{

				Filters.Scene["SpiritMod:Overseer"] = new Filter(new SeerScreenShaderData("FilterMiniTower").UseColor(0f, 0.3f, 1f).UseOpacity(0.75f), EffectPriority.VeryHigh);
				SkyManager.Instance["SpiritMod:Overseer"] = new SeerSky();
				Filters.Scene["SpiritMod:IlluminantMaster"] = new Filter(new SeerScreenShaderData("FilterMiniTower").UseColor(1.2f, 0.1f, 1f).UseOpacity(0.75f), EffectPriority.VeryHigh);
				SkyManager.Instance["SpiritMod:IlluminantMasterr"] = new SeerSky();
				Filters.Scene["SpiritMod:Atlas"] = new Filter(new AtlasScreenShaderData("FilterMiniTower").UseColor(0.5f, 0.5f, 0.5f).UseOpacity(0.6f), EffectPriority.VeryHigh);
				SkyManager.Instance["SpiritMod:Atlas"] = new AtlasSky();
			}
		}

		/// <summary>
		/// Scans all classes deriving from any Mod type
		/// for a field called _ref
		/// and populates it, if it exists.
		/// </summary>
		private void LoadReferences()
		{
			foreach (Type type in Code.GetTypes())
			{
				if (type.IsAbstract)
				{
					continue;
				}

				System.Reflection.FieldInfo _refField = type.GetField("_ref");
				System.Reflection.FieldInfo _typeField = type.GetField("_type");
				bool isDefR = _refField != null && _refField.IsStatic;
				bool isDefT = _typeField != null && _typeField.IsStatic && _typeField.FieldType == typeof(int);
				bool modType = true;

				if (type.IsSubclassOf(typeof(ModItem)))
				{
					if (isDefR && _refField.FieldType == typeof(ModItem))
						_refField.SetValue(null, GetItem(type.Name));
					if (isDefT)
						_typeField.SetValue(null, ItemType(type.Name));
				}
				else if (type.IsSubclassOf(typeof(ModNPC)))
				{
					if (isDefR && _refField.FieldType == typeof(ModNPC))
						_refField.SetValue(null, GetNPC(type.Name));
					if (isDefT)
						_typeField.SetValue(null, NPCType(type.Name));
				}
				else if (type.IsSubclassOf(typeof(ModProjectile)))
				{
					if (isDefR && _refField.FieldType == typeof(ModProjectile))
						_refField.SetValue(null, GetProjectile(type.Name));
					if (isDefT)
						_typeField.SetValue(null, ProjectileType(type.Name));
				}
				else if (type.IsSubclassOf(typeof(ModDust)))
				{
					if (isDefR && _refField.FieldType == typeof(ModDust))
						_refField.SetValue(null, GetDust(type.Name));
					if (isDefT)
						_typeField.SetValue(null, DustType(type.Name));
				}
				else if (type.IsSubclassOf(typeof(ModTile)))
				{
					if (isDefR && _refField.FieldType == typeof(ModTile))
						_refField.SetValue(null, GetTile(type.Name));
					if (isDefT)
						_typeField.SetValue(null, TileType(type.Name));
				}
				else if (type.IsSubclassOf(typeof(ModWall)))
				{
					if (isDefR && _refField.FieldType == typeof(ModWall))
						_refField.SetValue(null, GetWall(type.Name));
					if (isDefT)
						_typeField.SetValue(null, WallType(type.Name));
				}
				else if (type.IsSubclassOf(typeof(ModBuff)))
				{
					if (isDefR && _refField.FieldType == typeof(ModBuff))
						_refField.SetValue(null, GetBuff(type.Name));
					if (isDefT)
						_typeField.SetValue(null, BuffType(type.Name));
				}
				else if (type.IsSubclassOf(typeof(ModMountData)))
				{
					if (isDefR && _refField.FieldType == typeof(ModMountData))
						_refField.SetValue(null, GetMount(type.Name));
					if (isDefT)
						_typeField.SetValue(null, MountType(type.Name));
				}
				else
					modType = false;

				if (Main.dedServ || !modType)
					continue;

				System.Reflection.FieldInfo _texField = type.GetField("_textures");
				if (_texField == null || !_texField.IsStatic || _texField.FieldType != typeof(Texture2D[]))
					continue;

				string path = type.FullName.Substring(10).Replace('.', '/'); //Substring(10) removes "SpiritMod."
				int texCount = 0;
				while (TextureExists(path + "_" + (texCount + 1)))
				{
					texCount++;
				}
				Texture2D[] textures = new Texture2D[texCount + 1];
				if (TextureExists(path))
					textures[0] = GetTexture(path);
				for (int i = 1; i <= texCount; i++)
				{
					textures[i] = GetTexture(path + "_" + i);
				}
				_texField.SetValue(null, textures);
			}
		}

		public override void AddRecipeGroups()
		{
			RecipeGroup group = new RecipeGroup(() => Lang.misc[37] + " Gold Bar" + Lang.GetItemNameValue(ItemType("Gold Bar")), new int[]
			{
				19,
				706
			});
			RecipeGroup.RegisterGroup("GoldBars", group);
			group = new RecipeGroup(() => Lang.misc[37] + " Lunar Fragment" + Lang.GetItemNameValue(ItemType("Lunar Fragment")), new int[]
			{
				3456,
				3457,
				3458,
				3459
			});
			RecipeGroup.RegisterGroup("CelestialFragment", group);
			group = new RecipeGroup(() => Lang.misc[37] + " Cursed Flame" + Lang.GetItemNameValue(ItemType("Cursed Flame")), new int[]
			{
				ItemID.Ichor,
				ItemID.CursedFlame
			});
			RecipeGroup.RegisterGroup("EvilMaterial", group);
			group = new RecipeGroup(() => Lang.misc[37] + " Ichor Pendant" + Lang.GetItemNameValue(ItemType("Ichor Pendant")), new int[]
			{
				ItemType("CursedPendant"),
				ItemType("IchorPendant")
			});

			RecipeGroup.RegisterGroup("EvilNecklace", group);
			group = new RecipeGroup(() => Lang.misc[37] + " Shadow Scale" + Lang.GetItemNameValue(ItemType("Shadow Scale")), new int[]
			{
				ItemID.ShadowScale,
				ItemID.TissueSample
			});

			RecipeGroup.RegisterGroup("EvilMaterial1", group);
			group = new RecipeGroup(() => Lang.misc[37] + " Nightmare Fuel" + Lang.GetItemNameValue(ItemType("Nightmare Fuel")), new int[]
			{
				ItemType("CursedFire"),
				ItemType("NightmareFuel")
			});

			RecipeGroup.RegisterGroup("ModEvil", group);
		}

		public override void PostSetupContent()
		{
			Items.Glyphs.GlyphBase.InitializeGlyphLookup();

			Mod bossChecklist = ModLoader.GetMod("BossChecklist");
			if (bossChecklist != null)
			{
				// 14 is moolord, 12 is duke fishron
				bossChecklist.Call("AddBossWithInfo", "Scarabeus", 0.8f, (Func<bool>)(() => MyWorld.downedScarabeus), "Use a [i:" + ItemType("ScarabIdol") + "] in the Desert biome at any time");
				bossChecklist.Call("AddBossWithInfo", "Vinewrath Bane", 3.5f, (Func<bool>)(() => MyWorld.downedReachBoss), "Use a [i:" + ItemType("ReachBossSummon") + "] in the Reach at daytime");
				bossChecklist.Call("AddBossWithInfo", "Ancient Flier", 4.2f, (Func<bool>)(() => MyWorld.downedAncientFlier), "Use a [i:" + ItemType("JewelCrown") + "] in the sky at any time");
				bossChecklist.Call("AddBossWithInfo", "Starplate Raider", 5.2f, (Func<bool>)(() => MyWorld.downedRaider), "Use a [i:" + ItemType("StarWormSummon") + "] at nighttime");
				bossChecklist.Call("AddBossWithInfo", "Infernon", 6.5f, (Func<bool>)(() => MyWorld.downedInfernon), "Use [i:" + ItemType("CursedCloth") + "] in the underworld at any time");

				bossChecklist.Call("AddBossWithInfo", "Dusking", 7.3f, (Func<bool>)(() => MyWorld.downedDusking), "Use a [i:" + ItemType("DuskCrown") + "] at nighttime");
				bossChecklist.Call("AddBossWithInfo", "Ethereal Umbra", 7.8f, (Func<bool>)(() => MyWorld.downedSpiritCore), "Use a [i:" + ItemType("UmbraSummon") + "] in the Spirit Biome at nighttime");
				bossChecklist.Call("AddBossWithInfo", "Illuminant Master", 9.9f, (Func<bool>)(() => MyWorld.downedIlluminantMaster), "Use [i:" + ItemType("ChaosFire") + "] in the Hallowed Biome at Nighttime");
				bossChecklist.Call("AddBossWithInfo", "Atlas", 12.4f, (Func<bool>)(() => MyWorld.downedAtlas), "Use a [i:" + ItemType("StoneSkin") + "] at any time");
				bossChecklist.Call("AddBossWithInfo", "Overseer", 14.2f, (Func<bool>)(() => MyWorld.downedOverseer), "Use a [i:" + ItemType("SpiritIdol") + "] at the Spirit Biome during nighttime");
			}
			
		}


		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			TidePlayer modPlayer1 = Main.player[Main.myPlayer].GetModPlayer<TidePlayer>();
			if (TideWorld.TheTide)
			{
				int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
				if (index >= 0)
				{
					LegacyGameInterfaceLayer TideThing = new LegacyGameInterfaceLayer("SpiritMod: TideBenis",
						delegate
						{
							DrawTide(Main.spriteBatch);
							return true;
						},
						InterfaceScaleType.UI);
					layers.Insert(index, TideThing);
				}
			}
		}

		public override void HotKeyPressed(string name)
		{
			if (name == "Concentration_Hotkey")
			{
				MyPlayer mp = Main.player[Main.myPlayer].GetModPlayer<MyPlayer>(this);
				if (mp.leatherSet && !mp.concentrated && mp.concentratedCooldown <= 0)
				{
					mp.concentrated = true;
				}
			}
		}


		const int ShakeLength = 5;
		int ShakeCount = 0;
		float previousRotation = 0;
		float targetRotation = 0;
		float previousOffsetX = 0;
		float previousOffsetY = 0;
		float targetOffsetX = 0;
		float targetOffsetY = 0;

		public static float shittyModTime;

		//public override Matrix ModifyTransformMatrix(Matrix Transform)
		//{
		//	shittyModTime--;
		//	if (shittyModTime > 0)
		//	{
		//		if (shittyModTime % ShakeLength == 0)
		//		{
		//			ShakeCount = 0;
		//			previousRotation = targetRotation;
		//			previousOffsetX = targetOffsetX;
		//			previousOffsetY = targetOffsetY;
		//			targetRotation = (Main.rand.NextFloat() - .5f) * MathHelper.ToRadians(15);
		//			targetOffsetX = Main.rand.Next(60) - 30;
		//			targetOffsetY = Main.rand.Next(40) - 20;
		//			if (shittyModTime == ShakeLength)
		//			{
		//				targetRotation = 0;
		//				targetOffsetX = 0;
		//				targetOffsetY = 0;
		//			}
		//		}
		//		float transX = Main.screenWidth / 2;
		//		float transY = Main.screenHeight / 2;

		//		float lerp = (float)(ShakeCount) / ShakeLength;
		//		float rotation = MathHelper.Lerp(previousRotation, targetRotation, lerp);
		//		float offsetX = MathHelper.Lerp(previousOffsetX, targetOffsetX, lerp);
		//		float offsetY = MathHelper.Lerp(previousOffsetY, targetOffsetY, lerp);

		//		shittyModTime--;
		//		ShakeCount++;


		//		return Transform
		//			* Matrix.CreateTranslation(-transX, -transY, 0f)
		//			* Matrix.CreateRotationZ(rotation)
		//			* Matrix.CreateTranslation(transX, transY, 0f)
		//			* Matrix.CreateTranslation(offsetX, offsetY, 0f);
		//		//Matrix.CreateFromAxisAngle(new Vector3(Main.screenWidth / 2, Main.screenHeight / 2, 0f), .2f);
		//		//Matrix.CreateRotationZ(MathHelper.ToRadians(30));
		//	}
		//	return Transform;
		//}

		public void DrawTide(SpriteBatch spriteBatch)
		{
			TidePlayer modPlayer1 = Main.player[Main.myPlayer].GetModPlayer<TidePlayer>();
			if (TideWorld.TheTide && TideWorld.InBeach)
			{

				float alpha = 0.5f;
				Texture2D backGround1 = Main.colorBarTexture;
				Texture2D progressColor = Main.colorBarTexture;
				Texture2D TideIcon = SpiritMod.instance.GetTexture("Effects/InvasionIcons/Depths_Icon");
				float scmp = 0.5f + 0.75f * 0.5f;
				Color descColor = new Color(77, 39, 135);
				Color waveColor = new Color(255, 241, 51);
				Color barrierColor = new Color(255, 241, 51);
				const int offsetX = 20;
				const int offsetY = 20;
				int width = (int)(200f * scmp);
				int height = (int)(46f * scmp);
				Rectangle waveBackground = Utils.CenteredRectangle(new Vector2(Main.screenWidth - offsetX - 100f, Main.screenHeight - offsetY - 23f), new Vector2(width, height));
				Utils.DrawInvBG(spriteBatch, waveBackground, new Color(63, 65, 151, 255) * 0.785f);
				float cleared = TideWorld.TidePoints2/80f;
				string waveText = "Cleared " + Math.Round(100*cleared) + "%";
				Utils.DrawBorderString(spriteBatch, waveText, new Vector2(waveBackground.X + waveBackground.Width / 2, waveBackground.Y + 5), Color.White, scmp * 0.8f, 0.5f, -0.1f);
				Rectangle waveProgressBar = Utils.CenteredRectangle(new Vector2(waveBackground.X + waveBackground.Width * 0.5f, waveBackground.Y + waveBackground.Height * 0.75f), new Vector2(progressColor.Width, progressColor.Height));
				Rectangle waveProgressAmount = new Rectangle(0, 0, (int)(progressColor.Width * MathHelper.Clamp(cleared, 0f, 1f)), progressColor.Height);
				Vector2 offset = new Vector2((waveProgressBar.Width - (int)(waveProgressBar.Width * scmp)) * 0.5f, (waveProgressBar.Height - (int)(waveProgressBar.Height * scmp)) * 0.5f);
				spriteBatch.Draw(backGround1, waveProgressBar.Location.ToVector2() + offset, null, Color.White * alpha, 0f, new Vector2(0f), scmp, SpriteEffects.None, 0f);
				spriteBatch.Draw(backGround1, waveProgressBar.Location.ToVector2() + offset, waveProgressAmount, waveColor, 0f, new Vector2(0f), scmp, SpriteEffects.None, 0f);
				const int internalOffset = 6;
				Vector2 descSize = new Vector2(154, 40) * scmp;
				Rectangle barrierBackground = Utils.CenteredRectangle(new Vector2(Main.screenWidth - offsetX - 100f, Main.screenHeight - offsetY - 19f), new Vector2(width, height));
				Rectangle descBackground = Utils.CenteredRectangle(new Vector2(barrierBackground.X + barrierBackground.Width * 0.5f, barrierBackground.Y - internalOffset - descSize.Y * 0.5f), descSize * .8f);
				Utils.DrawInvBG(spriteBatch, descBackground, descColor * alpha);
				int descOffset = (descBackground.Height - (int)(32f * scmp)) / 2;
				Rectangle icon = new Rectangle(descBackground.X + descOffset + 7, descBackground.Y + descOffset, (int)(32 * scmp), (int)(32 * scmp));
				spriteBatch.Draw(TideIcon, icon, Color.White);
				Utils.DrawBorderString(spriteBatch, customEventName, new Vector2(barrierBackground.X + barrierBackground.Width * 0.5f, barrierBackground.Y - internalOffset - descSize.Y * 0.5f), Color.White, 0.8f, 0.3f, 0.4f);
			}
		}
	}

	internal enum CallContext
	{
		Invalid = -1,
		Downed,
		GlyphGet,
		GlyphSet,
		Limit
	}
}