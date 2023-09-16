﻿using InsurgencyWeapons.Helpers;
using InsurgencyWeapons.Items.Ammo;
using InsurgencyWeapons.Items.Weapons.Rifles;
using InsurgencyWeapons.Projectiles.WeaponExtras;
using InsurgencyWeapons.Projectiles.WeaponMagazines;
using InsurgencyWeapons.Projectiles.WeaponMagazines.Casings;
using InsurgencyWeapons.Projectiles.WeaponMagazines.Rifles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace InsurgencyWeapons.Projectiles.Rifles
{
    internal class M1GarandHeld : WeaponBase
    {
        public int CurrentAmmo
        {
            get
            {
                return MagazineTracking.GarandMagazine;
            }
            set
            {
                MagazineTracking.GarandMagazine = value;
            }
        }

        private bool AllowedToFire => Player.channel && CurrentAmmo > 0 && ReloadTimer == 0 && CanFire;

        private SoundStyle Fire => new("InsurgencyWeapons/Sounds/Weapons/Ins2/garand/shoot")
        {
            Pitch = Main.rand.NextFloat(-0.1f, 0.1f),
            MaxInstances = 0,
            Volume = 0.4f
        };

        private SoundStyle GarandPing => new("InsurgencyWeapons/Sounds/Weapons/Ins2/garand/ping", 5)
        {
            MaxInstances = 0,
            Volume = 0.25f
        };

        private bool SemiAuto;

        private SoundStyle Empty => new("InsurgencyWeapons/Sounds/Weapons/Ins2/garand/empty");
        private SoundStyle MagIn => new("InsurgencyWeapons/Sounds/Weapons/Ins2/garand/magin");
        private SoundStyle BoltLock => new("InsurgencyWeapons/Sounds/Weapons/Ins2/garand/bltrel");

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 72;
            MaxAmmo = 8;
            AmmoType = ModContent.ItemType<Bullet3006>();
            base.SetDefaults();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D myTexture = Projectile.MyTexture();
            Rectangle rect = myTexture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);
            ExtensionMethods.BetterEntityDraw(myTexture, Projectile.Center, rect, lightColor, Projectile.rotation, rect.Size() / 2, 0.9f, (SpriteEffects)(Player.direction > 0 ? 0 : 1), 0);
            DrawMuzzleFlash(Color.Yellow, 44f, 1f, new Vector2(0, -4f));
            return false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            CurrentAmmo = MagazineTracking.GarandMagazine;
            ShotDelay = HeldItem.useTime;
        }

        public override void AI()
        {
            Ammo = Player.FindItemInInventory(AmmoType);
            Ammo ??= ContentSamples.ItemsByType[AmmoType];
            ShowAmmoCounter(CurrentAmmo, AmmoType);
            OffsetFromPlayerCenter = 12f;
            SpecificWeaponFix = new Vector2(0, -1.4f);
            if (!Player.channel)
                SemiAuto = false;

            if (AllowedToFire && !UnderAlternateFireCoolDown && !SemiAuto)
            {
                SemiAuto = true;
                ShotDelay = 0;
                CurrentAmmo--;
                SoundEngine.PlaySound(Fire, Projectile.Center);
                Vector2 aim = Player.MountedCenter.DirectionTo(MouseAim).RotatedByRandom(MathHelper.ToRadians(Main.rand.Next(-2, 2))) * HeldItem.shootSpeed;
                int damage = (int)((Projectile.damage + Player.GetTotalDamage(DamageClass.Ranged).ApplyTo(Ammo.damage)) * Player.GetStealth());
                int type = Ammo.shoot;
                if (type == ProjectileID.Bullet)
                    type = Insurgency.Bullet;
                if (Player.whoAmI == Main.myPlayer)
                {
                    //Bullet
                    Projectile.NewProjectileDirect(
                        spawnSource: Player.GetSource_ItemUse_WithPotentialAmmo(HeldItem, HeldItem.useAmmo),
                        position: Player.MountedCenter,
                        velocity: aim,
                        type: type,
                        damage: damage,
                        knockback: Player.GetTotalKnockback(DamageClass.Ranged).ApplyTo(HeldItem.knockBack),
                        owner: Player.whoAmI,
                        ai0: (int)Insurgency.RiflesEnum.M1Garand);
                    //Casing
                    Projectile.NewProjectileDirect(
                        spawnSource: Player.GetSource_ItemUse_WithPotentialAmmo(HeldItem, HeldItem.useAmmo),
                        position: Player.MountedCenter,
                        velocity: new Vector2(0, -Main.rand.NextFloat(2f, 3f)).RotatedByRandom(MathHelper.PiOver4),
                        type: ModContent.ProjectileType<Casing>(),
                        damage: 0,
                        knockback: 0,
                        owner: Player.whoAmI);
                }
            }
            if (CurrentAmmo == 0 && Player.CountItem(AmmoType) > 0 && !ReloadStarted)
            {
                ReloadTimer = HeldItem.useTime * (int)Insurgency.ReloadModifiers.Rifles;
                SoundEngine.PlaySound(GarandPing, Projectile.Center);
                if (Player.whoAmI == Main.myPlayer)
                {
                    Projectile.NewProjectileDirect(
                            Player.GetSource_ItemUse_WithPotentialAmmo(HeldItem, HeldItem.useAmmo),
                            Projectile.Center,
                            new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -Main.rand.NextFloat(2f, 3f)).RotatedByRandom(MathHelper.PiOver4),
                            ModContent.ProjectileType<M1GarandEnbloc>(),
                            0,
                            0f,
                            Player.whoAmI);
                }
                Projectile.frame = (int)Insurgency.MagazineState.EmptyMagOut;
                ReloadStarted = true;
            }

            if (ReloadTimer > 0)
            {
                Player.SetDummyItemTime(2);
            }
            if (Player.channel && CurrentAmmo == 0 && CanFire && Projectile.soundDelay == 0)
            {
                SoundEngine.PlaySound(Empty, Projectile.Center);
                Projectile.soundDelay = HeldItem.useTime * 2;
            }
            switch (ReloadTimer)
            {
                case 15:
                    SoundEngine.PlaySound(BoltLock, Projectile.Center);
                    CurrentAmmo = MaxAmmo;
                    Projectile.frame = (int)Insurgency.MagazineState.Reloaded;
                    ReloadStarted = false;
                    break;

                case 40:
                    SoundEngine.PlaySound(MagIn, Projectile.Center);
                    Projectile.frame = (int)Insurgency.MagazineState.EmptyMagOut;
                    if (Ammo.stack > 0)
                    {
                        AmmoStackCount = Math.Clamp(Player.CountItem(Ammo.type), 1, MaxAmmo);
                        Ammo.stack -= AmmoStackCount;
                        CurrentAmmo = AmmoStackCount;
                    }
                    break;
            }
            if (CurrentAmmo != 0 && ShotDelay % 4 == 0 && Player.channel)
            {
                Projectile.frame++;
                if (Projectile.frame == 3)
                {
                    Projectile.frame = 0;
                }
            }
            else if (ShotDelay == HeldItem.useTime && !(ReloadTimer > 0))
                Projectile.frame = 0;

            if (HeldItem.type != ModContent.ItemType<M1Garand>()) Projectile.Kill();
            base.AI();
        }
    }
}