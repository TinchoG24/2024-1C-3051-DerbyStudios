﻿using BepuPhysics.Collidables;
using BepuPhysics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Collisions;
using Microsoft.Xna.Framework;
using NumericVector3 = System.Numerics.Vector3;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using TGC.MonoGame.TP.Camaras;
using static System.Formats.Asn1.AsnWriter;
using System.Transactions;
using Quaternion = Microsoft.Xna.Framework.Quaternion;
using System.Reflection;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using System.Reflection.Metadata;
using BepuPhysics.Trees;

namespace TGC.MonoGame.TP.Entities
{
    public class Enemy
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";
        public const string ContentFolderSoundEffects = "SoundEffects/";

        public Model EnemyModel { get; set; }
        public Effect EnemyEffect { get; set; }
        public Matrix EnemyWorld { get; set; }
        public Box EnemyBox { get; set; }
        public Vector3 Frent { get; private set; }
        public Vector3 Position { get; set; }
        public OrientedBoundingBox EnemyOBB { get; set; }
        public Matrix EnemyOBBPosition { get; private set; }
        public Matrix EnemyOBBWorld { get; private set; }
        public BodyHandle EnemyHandle { get; private set; }

        public List<Texture2D> EnemyTexture = new List<Texture2D>();

        private float time { get; set; }
        public bool Activated;
        private int ArenaWidth = 200;
        private int ArenaHeight = 200;
        float friction = 0.68f;

        private Random _random = new Random();

        public Enemy(Vector3 initialPos, Model model, Effect effect, Simulation simulation, List<Texture2D> EnemyTextures)
        {
            Position = initialPos;

            EnemyWorld = Matrix.CreateScale(0.05f) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(initialPos);

            Frent = Vector3.Normalize(EnemyWorld.Forward);

            EnemyEffect = effect;

            EnemyModel = model;

            EnemyTexture = EnemyTextures;



            var temporaryCubeAABB = BoundingVolumesExtensions.CreateAABBFrom(EnemyModel);
            temporaryCubeAABB = BoundingVolumesExtensions.Scale(temporaryCubeAABB, 0.001f);
            EnemyOBB = OrientedBoundingBox.FromAABB(temporaryCubeAABB);
            EnemyOBB.Center = Position;

            EnemyBox = new Box(EnemyOBB.Extents.X, EnemyOBB.Extents.Y, EnemyOBB.Extents.Z);

            EnemyHandle = simulation.Bodies.Add(
               BodyDescription.CreateConvexDynamic(
                  new NumericVector3(Position.X, Position.Y, Position.Z),
                  new BodyVelocity(new NumericVector3(0, 0, 0)),
                  1,
                  simulation.Shapes,
                  EnemyBox
                ));


        }

        public void Update(CarConvexHull MainCar, GameTime gameTime, Simulation simulation)
        {
            Vector3 scale;
            Quaternion rot;
            Vector3 translation;

            var bodyReference = simulation.Bodies.GetBodyReference(EnemyHandle);
            bodyReference.Awake = true;

            NumericVector3 enemyFrent = NumericVector3.Normalize(new NumericVector3(Frent.X, 0, Frent.Z));
            NumericVector3 directionToMainCar = NumericVector3.Normalize(Utils.ToNumericVector3(MainCar.Position - Position));

            var force = directionToMainCar * 0.2f;

            if (!bodyReference.Awake) bodyReference.SetLocalInertia(bodyReference.LocalInertia);
            bodyReference.ApplyLinearImpulse(new NumericVector3(force.X, 0, force.Z));

            float diffX = directionToMainCar.X - enemyFrent.X;
            float diffZ = directionToMainCar.Z - enemyFrent.Z;

            float angle = (float)Math.Atan2(diffZ, diffX);

            if (!bodyReference.Awake) bodyReference.SetLocalInertia(bodyReference.LocalInertia);
            bodyReference.ApplyAngularImpulse(new NumericVector3(0, -angle, 0));

            bodyReference.Pose.Position += Utils.ToNumericVector3(Vector3.Normalize(EnemyWorld.Left)) * 2 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position = bodyReference.Pose.Position;

            EnemyWorld = Matrix.CreateScale(0.05f) *
                Matrix.CreateRotationY(-MathHelper.Pi) *
                Matrix.CreateRotationY(-angle * 2) *
                Matrix.CreateTranslation(Position);

            // Descomponer la matriz del mundo del enemigo para obtener la escala, la rotación y la traslación
            EnemyWorld.Decompose(out scale, out rot, out translation);

            bodyReference.Pose.Orientation = new System.Numerics.Quaternion(rot.X, rot.Y, rot.Z, rot.W);

            // Actualiza la orientación y la posición del OBB (Oriented Bounding Box) del enemigo
            EnemyOBB.Orientation = Matrix.CreateFromQuaternion(rot);
            EnemyOBBPosition = Matrix.CreateTranslation(translation);
            EnemyOBB.Center = translation;

            // Actualiza la matriz del mundo del OBB del enemigo
            EnemyOBBWorld = Matrix.CreateScale(EnemyOBB.Extents) *
                            EnemyOBB.Orientation *
                            EnemyOBBPosition;

            //float distanceToTarget = Vector3.Distance(MainCar.Position, Position);
            //if (distanceToTarget < 7f)
            //{
            //    // Aplica una fuerza negativa para frenar
            //    bodyReference.ApplyLinearImpulse(new NumericVector3(-force.X, -force.Y, -force.Z) * 1.5f);
            //}

        }

        public void Draw(FollowCamera Camera, GameTime gameTime, BoundingFrustum boundingFrustum)
        {
            if (EnemyOBB.Intersects(boundingFrustum))
            {

                for (int i = 0; i < EnemyModel.Meshes.Count; i++)
                {
                    var mesh = EnemyModel.Meshes[i];
                    var texture = EnemyTexture[i];


                    time += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

                    EnemyEffect.Parameters["World"].SetValue(EnemyWorld);
                    EnemyEffect.Parameters["View"].SetValue(Camera.View);
                    EnemyEffect.Parameters["Projection"].SetValue(Camera.Projection);
                    EnemyEffect.Parameters["ModelTexture"].SetValue(EnemyTexture[i]);
                    EnemyEffect.Parameters["Time"]?.SetValue(Convert.ToSingle(time));


                    foreach (var part in mesh.MeshParts)
                    {
                        part.Effect = EnemyEffect;
                    }

                    mesh.Draw();

                }
            }
        }

        public void Destroy() { }

    }
}
