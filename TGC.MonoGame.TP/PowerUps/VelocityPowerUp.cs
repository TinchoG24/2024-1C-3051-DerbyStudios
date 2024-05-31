﻿using Microsoft.Xna.Framework;
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
    public class VelocityPowerUp : PowerUp
    {
        public VelocityPowerUp(Vector3 position) : base(position)
        {
            PowerUpWorld = Matrix.CreateScale(1f, 1f, 1f) * Matrix.CreateTranslation(position);
           
            var worldBounding = Matrix.CreateScale(1.5f, 1.5f, 1.5f) * Matrix.CreateTranslation(position);
            BoundingBox = BoundingVolumesExtensions.FromMatrix(worldBounding);
        }
        public override void LoadContent(ContentManager Content)
        {
            PowerUpEffect = Content.Load<Effect>(ContentFolderEffects + "PowerUpsShader");
            
            PowerUpModel = new GameModel(Content.Load<Model>(ContentFolder3D + "PowerUps/ModeloTurbo"), PowerUpEffect, 1.5f , Position);

        }

        public override async void Activate(CarConvexHull carConvexHull)
        {
            if (!Activated)
            {
                carConvexHull.maxSpeed *= 2;
                carConvexHull.maxTurn *= 2;
                Activated = true;
                await Task.Delay(4000);
                carConvexHull.maxSpeed /= 2;
                carConvexHull.maxTurn /= 2;
                await Task.Delay(4000);
                Activated = false;
            }
        }
    }
}
