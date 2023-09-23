﻿using InsurgencyWeapons.Helpers;
using InsurgencyWeapons.Items.Ammo;
using InsurgencyWeapons.Items.Weapons.SniperRifles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace InsurgencyWeapons.Projectiles.SniperRifles
{
    internal class MosinHeld : WeaponBase
    {
        public int CurrentAmmo
        {
            get
            {
                return MagazineTracking.MosinBox;
            }
            set
            {
                MagazineTracking.MosinBox = value;
            }
        }

        private bool AllowedToFire => Player.channel && CurrentAmmo > 0 && ReloadTimer == 0 && CanFire;

        private SoundStyle Fire => new("InsurgencyWeapons/Sounds/Weapons/Ins2/mosin/shoot")
        {
            Pitch = Main.rand.NextFloat(-0.1f, 0.1f),
            MaxInstances = 0,
            Volume = 0.4f
        };

        private bool SemiAuto;

        private SoundStyle Empty => new("InsurgencyWeapons/Sounds/Weapons/Ins2/mosin/empty");
        private SoundStyle BoltRelease => new("InsurgencyWeapons/Sounds/Weapons/Ins2/mosin/bltrel");

        private SoundStyle BoltForward => new("InsurgencyWeapons/Sounds/Weapons/Ins2/mosin/bltfd")
        {
            Pitch = Main.rand.NextFloat(-0.1f, 0.1f),
            MaxInstances = 0,
        };

        private SoundStyle BoltBack => new("InsurgencyWeapons/Sounds/Weapons/Ins2/mosin/bltbk")
        {
            Pitch = Main.rand.NextFloat(-0.1f, 0.1f),
            MaxInstances = 0,
        };

        private SoundStyle Insert => new("InsurgencyWeapons/Sounds/Weapons/Ins2/mosin/ins")
        {
            Pitch = Main.rand.NextFloat(-0.1f, 0.1f),
        };

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 80;
            MaxAmmo = 5;
            AmmoType = ModContent.ItemType<Bullet76254R>();
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
            CurrentAmmo = MagazineTracking.MosinBox;
            ShotDelay = HeldItem.useTime;
        }

        public override void AI()
        {
            Ammo = Player.FindItemInInventory(AmmoType);
            Ammo ??= ContentSamples.ItemsByType[AmmoType];
            ShowAmmoCounter(CurrentAmmo, AmmoType);
            OffsetFromPlayerCenter = 4f;
            SpecificWeaponFix = new Vector2(0, -2.5f);
            if (!Player.channel)
                SemiAuto = false;

            if (AllowedToFire && !UnderAlternateFireCoolDown && !SemiAuto && BoltActionTimer == 0)
            {
                SemiAuto = true;
                ShotDelay = 0;
                CurrentAmmo--;
                if (CurrentAmmo > 0)
                    BoltActionTimer = HeldItem.useTime * 2;

                SoundEngine.PlaySound(Fire, Projectile.Center);
                Vector2 aim = Player.MountedCenter.DirectionTo(MouseAim).RotatedByRandom(MathHelper.ToRadians(Main.rand.Next(1))) * HeldItem.shootSpeed;
                int damage = (int)((Projectile.originalDamage + Player.GetTotalDamage(DamageClass.Ranged).ApplyTo(Ammo.damage)) * Player.GetStealth());
                Shoot(aim, BulletType, damage, dropCasing: false, ai0: (float)Insurgency.APCaliber.c762x54Rmm);
            }

            if (CurrentAmmo == 0 && Player.CountItem(AmmoType) > 0 && !ReloadStarted)
            {
                ReloadTimer = HeldItem.useTime * (int)Insurgency.ReloadModifiers.SniperRifles;
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
                case 5:
                    SoundEngine.PlaySound(BoltForward, Projectile.Center);
                    Projectile.frame = (int)Insurgency.MagazineState.Reloaded;
                    ReloadStarted = false;
                    break;

                case 10:
                    SoundEngine.PlaySound(BoltRelease, Projectile.Center);
                    break;

                case 20:
                    Projectile.frame = (int)Insurgency.MagazineState.EmptyMagOut;
                    if (CurrentAmmo < MaxAmmo)
                    {
                        if (Ammo.stack > 0)
                        {
                            SoundEngine.PlaySound(Insert, Projectile.Center);
                            AmmoStackCount = Math.Clamp(Player.CountItem(Ammo.type), 1, 1);
                            Ammo.stack -= AmmoStackCount;
                            CurrentAmmo += AmmoStackCount;
                            ReloadTimer = 50;
                        }
                    }
                    break;

                case 90:
                    SoundEngine.PlaySound(BoltBack, Projectile.Center);
                    DropCasingManually();
                    Projectile.frame = (int)Insurgency.MagazineState.EmptyMagOut;
                    break;
            }

            switch (BoltActionTimer)
            {
                case 10:
                    SoundEngine.PlaySound(BoltForward, Projectile.Center);
                    Projectile.frame = (int)Insurgency.MagazineState.Reloaded;
                    break;

                case 20:
                    Projectile.frame = (int)Insurgency.MagazineState.EmptyMagOut;
                    break;

                case 39:
                    Projectile.frame = (int)Insurgency.MagazineState.EmptyMagIn;
                    break;

                case 42:
                    Projectile.frame = (int)Insurgency.MagazineState.Fired;
                    DropCasingManually();
                    break;

                case 45:
                    SoundEngine.PlaySound(BoltBack, Projectile.Center);
                    Projectile.frame = (int)Insurgency.MagazineState.Reloaded;
                    break;
            }

            if (HeldItem.type != ModContent.ItemType<Mosin>())
                Projectile.Kill();

            base.AI();
        }
    }
}