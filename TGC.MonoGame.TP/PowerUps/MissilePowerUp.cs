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
    public class MissilePowerUp : PowerUp
    {

        public MissilePowerUp(): base()
        {
            RandomPositions = true;
        }


        public MissilePowerUp(Vector3 position) : base(position)
        {
            RandomPositions=false;

            //PowerUpWorld = Matrix.CreateScale(1f, 1f, 1f) * Matrix.CreateTranslation(position);

            //var worldBounding = Matrix.CreateScale(1.5f, 1.5f, 1.5f) * Matrix.CreateTranslation(position);
            //BoundingBox.Add( BoundingVolumesExtensions.FromMatrix(worldBounding));

        }

        public override async void Activate(CarConvexHull carConvexHull )
        {

            if (!Activated)
            {
                carConvexHull.CanShoot = true;
                Activated = true;
                await Task.Delay(4000);
                carConvexHull.CanShoot = false;
                await Task.Delay(4000);
                Activated = false;
            }

        }

        public override void LoadContent(ContentManager Content)
        {
            PowerUpEffect = Content.Load<Effect>(ContentFolderEffects + "PowerUpsShader");

            if(RandomPositions)
            PowerUpModel = new GameModel(Content.Load<Model>(ContentFolder3D + "PowerUps/MLRS_Rocket"), PowerUpEffect, 0.8f, GenerateRandomPositions(20));
            else
            PowerUpModel = new GameModel(Content.Load<Model>(ContentFolder3D + "PowerUps/MLRS_Rocket"), PowerUpEffect, 0.8f, Position);

            PowerUpListWorld = PowerUpModel.World;

            PowerUpListWorld.ForEach(World => BoundingBox.Add(BoundingVolumesExtensions.FromMatrix(Matrix.CreateScale(1.5f, 1.5f, 1.5f) * World)));


            //PowerUpTexture = ((BasicEffect)PowerUpModel.Model.Meshes.FirstOrDefault()?.MeshParts.FirstOrDefault()?.Effect)?.Texture;

        }

    }
}