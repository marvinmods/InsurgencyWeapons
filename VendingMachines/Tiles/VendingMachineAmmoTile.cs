﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace InsurgencyWeapons.VendingMachines.Tiles
{
    internal class VendingMachineAmmoTile : VendingMachineTile
    {
        private float strength;
        private bool increase;

        public override void SetStaticDefaults()
        {
            AddMapEntry(Color.Green);
            base.SetStaticDefaults();
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (increase)
            {
                strength += 0.001f;
                if (strength >= 0.75f)
                    increase = false;
            }
            else
            {
                strength -= 0.001f;
                if (strength <= 0.1f)
                    increase = true;
            }
            Vector2 pos = new Vector2(i, j) * 16;
            Lighting.AddLight(pos, Color.Green.ToVector3() * strength);
            return base.PreDraw(i, j, spriteBatch);
        }
    }
}