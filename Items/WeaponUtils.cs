﻿using InsurgencyWeapons.Helpers;
using InsurgencyWeapons.Items.Ammo;
using InsurgencyWeapons.Projectiles;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace InsurgencyWeapons.Items
{
    public abstract class AmmoItem : ModItem
    {
        private static readonly Dictionary<int, List<int>> AmmoRelationship = [];

        public static void AddRelationShip(int ammoType, int weaponType)
        {
            if (!AmmoRelationship.TryGetValue(ammoType, out var weapons))
            {
                weapons = [];
                AmmoRelationship[ammoType] = weapons;
            }

            if (!weapons.Contains(weaponType))
                weapons.Add(weaponType);
        }

        public static List<int> GetWeaponsThatUseThisAmmo(int ammoType)
        {
            return AmmoRelationship.TryGetValue(ammoType, out var weapons) ? weapons : [];
        }

        /// <summary>
        /// How much money it costs
        /// </summary>
        public int MoneyCost { get; set; }

        /// <summary>
        /// How much to get when crafting
        /// </summary>
        public int CraftStack { get; set; }

        public List<int> CompatibleWeapons = [];

        public override void SetDefaults()
        {
            if (MoneyCost == 0 || CraftStack == 0)
                throw new ArgumentException("MoneyCost or CraftStack property can't be 0");

            Item.value = MoneyCost * 20;
            base.SetDefaults();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            CompatibleWeapons = GetWeaponsThatUseThisAmmo(Type);
            if (CompatibleWeapons.Count > 0)
            {
                TooltipLine usedBy = new(Mod, "UsedBy", Language.GetTextValue("Mods.InsurgencyWeapons.AmmoItem.UsedBy"));
                tooltips.Add(usedBy);
                int index = 0;
                foreach (int weaponType in CompatibleWeapons)
                {
                    Item weaponItem = ContentSamples.ItemsByType[weaponType];
                    TooltipLine ammo = new(Mod, "Ammo%" + index, string.Join(" ", $"[i:InsurgencyWeapons/{weaponItem.ModItem?.Name}]", $"[c/c29463:{weaponItem.ModItem?.DisplayName}]"));
                    tooltips.Add(ammo);
                }
            }
            base.ModifyTooltips(tooltips);
        }

        public override void SetStaticDefaults() => Item.ResearchUnlockCount = 60;

        public override bool CanStackInWorld(Item source) => true;

        public override void AddRecipes() => this.RegisterINS2RecipeAmmo(MoneyCost, CraftStack);
    }

    /// <summary>
    /// Main class
    /// </summary>
    public abstract class WeaponUtils : ModItem
    {
        public int WeaponHeldProjectile { get; set; }
        public int MoneyCost { get; set; }
        public int WeaponPerk { get; set; } = -1;
        private Projectile Gun;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            ItemID.Sets.gunProj[Type] = true;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            if (MoneyCost == 0 && this is not Grenade)
                throw new ArgumentException("MoneyCost property can't be 0");

            if (WeaponPerk == -1 && this is not Grenade)
                throw new ArgumentException("Must assign a perk to a weapon");

            Item.value = MoneyCost * 40;
            Item.rare = SetRarity();
            base.SetDefaults();
        }

        private int SetRarity()
        {
            if (HelperStats.TestRange(MoneyCost, 0, 100))
                return ItemRarityID.Green;
            if (HelperStats.TestRange(MoneyCost, 100, 200))
                return ItemRarityID.LightRed;
            if (HelperStats.TestRange(MoneyCost, 200, 300))
                return ItemRarityID.LightPurple;
            if (HelperStats.TestRange(MoneyCost, 300, 500))
                return ItemRarityID.Yellow;
            return ItemRarityID.Red;
        }

        public override void HoldItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer && WeaponHeldProjectile != 0 && player.ownedProjectileCounts[WeaponHeldProjectile] < 1)
            {
                Gun = Projectile.NewProjectileDirect(player.GetSource_ItemUse_WithPotentialAmmo(Item, Item.useAmmo), player.Center, Vector2.Zero, WeaponHeldProjectile, Item.damage, Item.knockBack, player.whoAmI);
                Gun.GetGlobalProjectile<ProjPerkTracking>().Perk = WeaponPerk;
            }
            base.HoldItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => false;

        public override void AddRecipes() => this.RegisterINS2RecipeWeapon(MoneyCost);
    }

    public abstract class AssaultRifle : WeaponUtils
    {
        public override void SetDefaults()
        {
            WeaponPerk = (int)PerkSystem.Perks.Commando;
            base.SetDefaults();
        }
    }

    public abstract class BattleRifle : WeaponUtils
    {
        public override void SetDefaults()
        {
            WeaponPerk = (int)PerkSystem.Perks.Commando;
            base.SetDefaults();
        }
    }

    public abstract class Carbine : WeaponUtils
    {
        public override void SetDefaults()
        {
            WeaponPerk = (int)PerkSystem.Perks.Commando;
            base.SetDefaults();
        }
    }

    public abstract class Rifle : WeaponUtils
    {
        public override void SetDefaults()
        {
            WeaponPerk = (int)PerkSystem.Perks.Sharpshooter;
            base.SetDefaults();
        }
    }

    public abstract class Shotgun : WeaponUtils
    {
        private Projectile Gun;

        private int
            shotGunSwitchTimer;

        private bool useSlug = false;

        public override void SetDefaults()
        {
            WeaponPerk = (int)PerkSystem.Perks.SupportSpecialist;
            base.SetDefaults();
        }

        public override void HoldItem(Player player)
        {
            if (shotGunSwitchTimer > 0)
                shotGunSwitchTimer--;
            if (player.whoAmI == Main.myPlayer && WeaponHeldProjectile != 0 && player.ownedProjectileCounts[WeaponHeldProjectile] < 1)
            {
                Gun = Projectile.NewProjectileDirect(player.GetSource_ItemUse_WithPotentialAmmo(Item, Item.useAmmo), player.Center, Vector2.Zero, WeaponHeldProjectile, Item.damage, Item.knockBack, player.whoAmI);
                Gun.GetGlobalProjectile<ProjPerkTracking>().Perk = WeaponPerk;
                if (Gun.active && Gun.ModProjectile is WeaponBase changeSlug)
                {
                    if (useSlug)
                    {
                        changeSlug.AmmoType = ModContent.ItemType<TwelveGaugeSlug>();
                    }
                    else
                    {
                        changeSlug.AmmoType = ModContent.ItemType<TwelveGauge>();
                    }
                }
            }

            if (player.HasItem(ModContent.ItemType<TwelveGaugeSlug>()) && Gun != null && Gun.active && Gun.ModProjectile is WeaponBase shotgun && !shotgun.ReloadStarted && shotgun.MouseRightPressed && shotGunSwitchTimer == 0)
            {
                shotGunSwitchTimer = 90;
                shotgun.ReturnAmmo();
                shotgun.CurrentAmmo = 0;
                useSlug = useSlug != true;
                Gun.Kill();
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag[$"InsurgenyWeaponsSlugChoice{Item.ModItem?.Name}"] = useSlug;
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey($"InsurgenyWeaponsSlugChoice{Item.ModItem?.Name}"))
                useSlug = tag.GetBool($"InsurgenyWeaponsSlugChoice{Item.ModItem?.Name}");
        }
    }

    public abstract class SniperRifle : WeaponUtils
    {
        public override void SetDefaults()
        {
            WeaponPerk = (int)PerkSystem.Perks.Sharpshooter;
            base.SetDefaults();
        }
    }

    public abstract class SubMachineGun : WeaponUtils
    {
        public override void SetDefaults()
        {
            WeaponPerk = (int)PerkSystem.Perks.Commando;
            base.SetDefaults();
        }
    }

    public abstract class LightMachineGun : WeaponUtils
    {
        public override void SetDefaults()
        {
            WeaponPerk = (int)PerkSystem.Perks.Commando;
            base.SetDefaults();
        }
    }

    public abstract class Launcher : WeaponUtils
    {
        public override void SetDefaults()
        {
            Item.shootSpeed = 16f;
            Item.channel = true;
            WeaponPerk = (int)PerkSystem.Perks.Demolitons;
            base.SetDefaults();
        }
    }

    public abstract class Pistol : WeaponUtils
    {
        public override void SetDefaults()
        {
            WeaponPerk = (int)PerkSystem.Perks.Sharpshooter;
            base.SetDefaults();
        }
    }

    public abstract class Revolver : WeaponUtils
    {
        public override void SetDefaults()
        {
            WeaponPerk = (int)PerkSystem.Perks.Sharpshooter;
            base.SetDefaults();
        }
    }

    public abstract class Grenade : WeaponUtils
    {
        public int GrenadeType { get; set; }

        public SoundStyle
            Cook,
            Spoon,
            Throw;

        public bool Fired;

        public int Timer;

        public override void SetDefaults()
        {
            if (GrenadeType == 0)
                throw new ArgumentException("GrenadeType can't be 0");

            WeaponHeldProjectile = 0;
            Item.knockBack = 8f;
            Item.channel = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = Item.useTime = 120;
            Item.width = 10;
            Item.height = 32;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.shootSpeed = 6f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.value = Item.sellPrice(0, 0, 1, 5);

            Item.maxStack = Item.CommonMaxStack;
            Item.DamageType = DamageClass.Ranged;
            base.SetDefaults();
        }

        public override bool? PrefixChance(int pre, UnifiedRandom rand)
        {
            if (pre == -3 || pre == -1)
                return false;
            return base.PrefixChance(pre, rand);
        }

        public override void HoldItem(Player player)
        {
            PerkSystem PerkTracking = player.GetModPlayer<PerkSystem>();
            if (Item.stack <= 0)
                Item.TurnToAir();

            if (player.channel && !Fired)
                Fired = true;

            if (Fired)
            {
                if (player.whoAmI == Main.myPlayer)
                {
                    int mouseDirection = player.DirectionTo(Main.MouseWorld).X > 0f ? 1 : -1;
                    player.ChangeDir(mouseDirection);
                }
                Timer++;
                switch (Timer)
                {
                    case 10:
                        SoundEngine.PlaySound(Cook, player.Center);
                        break;

                    case 50:
                        SoundEngine.PlaySound(Spoon, player.Center);
                        Item.useStyle = ItemUseStyleID.Swing;
                        break;

                    case 90:
                        SoundEngine.PlaySound(Throw, player.Center);
                        Item.stack--;
                        Vector2 aim = player.Center.DirectionTo(Main.MouseWorld) * Item.shootSpeed * 1.75f;
                        int damage = (int)player.GetTotalDamage(Item.DamageType).ApplyTo(Item.damage);
                        if (PerkTracking.DemolitionsWeapons(Item) && PerkTracking.Level[(int)PerkSystem.Perks.Demolitons] > 0)
                            damage = (int)(damage * (1f + PerkTracking.GetDamageMultPerLevel((int)PerkSystem.Perks.Demolitons)));
                        float knockback = (int)player.GetTotalKnockback(Item.DamageType).ApplyTo(Item.knockBack);
                        if (player.whoAmI == Main.myPlayer)
                            Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, aim, GrenadeType, damage, knockback, player.whoAmI).GetGlobalProjectile<ProjPerkTracking>().Grenade = true;
                        break;
                }
            }
            if (!player.channel && Fired && Timer > Item.useTime)
            {
                Timer = 0;
                Fired = false;
                Item.useStyle = ItemUseStyleID.Shoot;
            }
            base.HoldItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => false;

        public override void AddRecipes() => this.RegisterINS2RecipeAmmo(MoneyCost);
    }
}