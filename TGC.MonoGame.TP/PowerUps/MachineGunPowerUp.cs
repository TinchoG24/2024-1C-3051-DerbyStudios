﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.MonoGame.Samples.Collisions;

namespace TGC.MonoGame.TP.PowerUps
{
    public class MachineGunPowerUp : PowerUp
    {

        public MachineGunPowerUp(Vector3 position) : base(position)
        {
            Position = position;

            PowerUpWorld = Matrix.CreateScale(3f, 3f, 3f) * Matrix.CreateTranslation(position);
            
        }

        public override async void Activate(CarConvexHull carConvexHull)
        {

            if (!Activated)
            {
                PowerUpSound.Play();
                carConvexHull.MachineGun = true;
                carConvexHull.CanShoot = true;
                Activated = true;
                await Task.Delay(4000);
                carConvexHull.CanShoot = false;
                carConvexHull.MachineGun = false;
                await Task.Delay(4000);
                Activated = false;
            }

        }

        public override void LoadContent(ContentManager Content)
        {
            PowerUpSound = Content.Load<SoundEffect>(ContentFolderSoundEffects + "PowerUpSoundEffectMachine");

            PowerUpEffect = Content.Load<Effect>(ContentFolderEffects + "PowerUpsShader");

            PowerUpModel = Content.Load<Model>(ContentFolder3D + "PowerUps/MachineGun3");

            PowerUpTexture = ((BasicEffect)PowerUpModel.Meshes.FirstOrDefault()?.MeshParts.FirstOrDefault()?.Effect)?.Texture;

            BoundingSphere = BoundingVolumesExtensions.CreateSphereFrom(PowerUpModel);

            BoundingSphere.Center = Position;

            BoundingSphere.Radius = 2f;
        }

    }
}