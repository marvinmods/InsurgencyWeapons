﻿using InsurgencyWeapons.Helpers;
using Terraria.Utilities;

namespace InsurgencyWeapons.Items
{
    public abstract class AmmoItem : ModItem
    {
        /// <summary>
        /// How much money it costs
        /// </summary>
        public int MoneyCost { get; set; }

        /// <summary>
        /// How much to get when crafting
        /// </summary>
        public int CraftStack { get; set; }

        public override void SetDefaults()
        {
            if (MoneyCost == 0 || CraftStack == 0)
                throw new ArgumentException("MoneyCost or CraftStack property can't be 0");

            Item.value = MoneyCost * 20;
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

            Item.value = MoneyCost * 40;
            base.SetDefaults();
        }

        public override void HoldItem(Player player)
        {
            if (WeaponHeldProjectile != 0 && player.ownedProjectileCounts[WeaponHeldProjectile] < 1)
            {
                int damage = (int)player.GetTotalDamage(DamageClass.Ranged).ApplyTo(Item.damage);
                PerkSystem PerkTracking = player.GetModPlayer<PerkSystem>();
                InsurgencyCustomSetBonusModPlayer SetTracking = player.GetModPlayer<InsurgencyCustomSetBonusModPlayer>();

                if (PerkTracking.CommandoWeapons(Item) && PerkTracking.Level[(int)PerkSystem.Perks.Commando] > 0)
                    damage = (int)(damage * 1f + PerkTracking.GetDamageMultPerLevel((int)PerkSystem.Perks.Commando));

                if (PerkTracking.SupportSpecialistWeapons(Item) && PerkTracking.Level[(int)PerkSystem.Perks.SupportSpecialist] > 0)
                    damage = (int)(damage * 1f + PerkTracking.GetDamageMultPerLevel((int)PerkSystem.Perks.SupportSpecialist));

                if (PerkTracking.DemolitionsWeapons(Item) && PerkTracking.Level[(int)PerkSystem.Perks.Demolitons] > 0)
                    damage = (int)(damage * 1f + PerkTracking.GetDamageMultPerLevel((int)PerkSystem.Perks.Demolitons));

                if (PerkTracking.SharpshooterWeapons(Item) && PerkTracking.Level[(int)PerkSystem.Perks.Sharpshooter] > 0)
                    damage = (int)(damage * 1f + PerkTracking.GetDamageMultPerLevel((int)PerkSystem.Perks.Sharpshooter));

                if ((Insurgency.Pistols.Contains(Item.type) || Insurgency.Revolvers.Contains(Item.type)) && SetTracking.revolverSet)
                    damage *= 2;

                Projectile gun = BetterNewProjectile(player, player.GetSource_ItemUse_WithPotentialAmmo(Item, Item.useAmmo), player.Center, Vector2.Zero, WeaponHeldProjectile, Item.damage, Item.knockBack, player.whoAmI);
                gun.originalDamage = damage;
            }
            base.HoldItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => false;

        public override void AddRecipes() => this.RegisterINS2RecipeWeapon(MoneyCost);
    }

    public abstract class AssaultRifle : WeaponUtils
    { }

    public abstract class BattleRifle : WeaponUtils
    { }

    public abstract class Carbine : WeaponUtils
    { }

    public abstract class Rifle : WeaponUtils
    { }

    public abstract class Shotgun : WeaponUtils
    { }

    public abstract class SniperRifle : WeaponUtils
    { }

    public abstract class SubMachineGun : WeaponUtils
    { }

    public abstract class LightMachineGun : WeaponUtils
    { }

    public abstract class Launcher : WeaponUtils
    { }

    public abstract class Pistol : WeaponUtils
    { }

    public abstract class Revolver : WeaponUtils
    { }

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
            Item.rare = ItemRarityID.LightRed;
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
                        Vector2 aim = player.Center.DirectionTo(Main.MouseWorld) * Item.shootSpeed * 3.5f;
                        int damage = (int)player.GetTotalDamage(Item.DamageType).ApplyTo(Item.damage);
                        float knockback = (int)player.GetTotalKnockback(Item.DamageType).ApplyTo(Item.knockBack);
                        BetterNewProjectile(player, player.GetSource_ItemUse(Item), player.Center, aim, GrenadeType, damage, knockback, player.whoAmI).GetGlobalProjectile<ProjPerkTracking>().Grenade = true;
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