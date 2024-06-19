using BepuPhysics.Collidables;
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

        public OrientedBoundingBox EnemyOBB { get; set; }

        public Box EnemyBox { get; set; }

        private Quaternion rotationQuaternion;

        public Matrix EnemyWorld { get; set; }
        public Vector3 Frent { get; private set; }
        public Model EnemyModel { get; set; }

        public Effect EnemyEffect { get; set; }

        public Texture EnemyTexture { get; set; }

        public BodyHandle EnemyHandle { get; private set; }

        private float time { get; set; }

        public Vector3 Position { get; set; }

        public Vector3 PosDirection { get; private set; }
        public Matrix EnemyOBBPosition { get; private set; }
        public Matrix EnemyOBBWorld { get; private set; }
        public Matrix rotation { get; private set; }

        public bool Activated;

        private int ArenaWidth = 200;
        private int ArenaHeight = 200;

        private Random _random = new Random();

        public Enemy(Vector3 initialPos)
        {
            Position = initialPos;
            EnemyWorld = Matrix.CreateScale(0.05f) * Matrix.CreateRotationY(MathHelper.PiOver2) *  Matrix.CreateTranslation(initialPos);
            Frent = Vector3.Normalize(EnemyWorld.Forward);

        }

        public void LoadContent(ContentManager Content, Simulation simulation)
        {
            Vector3 scale;
            Quaternion rot;
            Vector3 translation;

            EnemyEffect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");

            EnemyModel = Content.Load<Model>(ContentFolder3D + "weapons/Vehicle");

            EnemyTexture = ((BasicEffect)EnemyModel.Meshes.FirstOrDefault()?.MeshParts.FirstOrDefault()?.Effect)?.Texture;

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

            Vector3 enemyFrent = Vector3.Normalize(new Vector3(Frent.X, 0, Frent.Z));
            Vector3 directionToMainCar = Vector3.Normalize(new Vector3(MainCar.Position.X - Position.X, 0, MainCar.Position.Z - Position.Z));

            float acceleration = 0.2f;
            Vector3 force = directionToMainCar * acceleration;

            if (!bodyReference.Awake) bodyReference.SetLocalInertia(bodyReference.LocalInertia);
            bodyReference.ApplyLinearImpulse(new NumericVector3(force.X, 0, force.Z));

            float diffX = directionToMainCar.X - enemyFrent.X; //diferencias en X entre vectores
            float diffZ = directionToMainCar.Z - enemyFrent.Z; //diferencias en z entre vectores

            float angle = (float)Math.Atan2(diffZ, diffX); //arco tangente de las diferencias en Radianes
            bodyReference.ApplyAngularImpulse(new NumericVector3(0, -angle * 2 , 0));

            EnemyWorld = Matrix.CreateScale(0.05f) * Matrix.CreateRotationY(-MathHelper.PiOver2) * Matrix.CreateRotationY(-angle * 5 ) * Matrix.CreateTranslation(Position);

            Position += Vector3.Normalize(EnemyWorld.Left) * 18 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Descomponer la matriz del mundo del enemigo para obtener la escala, la rotación y la traslación
            EnemyWorld.Decompose(out scale, out rot, out translation);

            bodyReference.Pose.Position = new NumericVector3(Position.X, Position.Y, Position.Z);
            bodyReference.Pose.Orientation = new System.Numerics.Quaternion(rot.X, rot.Y, rot.Z, rot.W);

            // Actualiza la orientación y la posición del OBB (Oriented Bounding Box) del enemigo
            EnemyOBB.Orientation = Matrix.CreateFromQuaternion(rot);
            EnemyOBBPosition = Matrix.CreateTranslation(Position);
            EnemyOBB.Center = Position;

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

        public List<Vector3> GenerateRandomPositions(int count, float y)
        {
            var positions = new List<Vector3>();

            for (int i = 0; i < count; i++)
            {
                int x = _random.Next(-ArenaWidth, ArenaWidth);
                int z = _random.Next(-ArenaHeight, ArenaHeight);
                positions.Add(new Vector3(x, y, z));
            }

            return positions;
        }

        public List<Vector3> GenerateRandomPositions(int count)
        {
            var positions = new List<Vector3>();

            for (int i = 0; i < count; i++)
            {
                int x = _random.Next(-ArenaWidth, ArenaWidth);
                int z = _random.Next(-ArenaHeight, ArenaHeight);
                positions.Add(new Vector3(x, 0, z));
            }

            return positions;
        }

        public void Draw(FollowCamera Camera, GameTime gameTime)
        {

            time += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

            EnemyEffect.Parameters["World"].SetValue(EnemyWorld);
            EnemyEffect.Parameters["View"].SetValue(Camera.View);
            EnemyEffect.Parameters["Projection"].SetValue(Camera.Projection);
            EnemyEffect.Parameters["ModelTexture"].SetValue(EnemyTexture);
            EnemyEffect.Parameters["Time"]?.SetValue(Convert.ToSingle(time));

            var mesh = EnemyModel.Meshes.FirstOrDefault();
            if (mesh != null)
            {
                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = EnemyEffect;
                }

                mesh.Draw();
            }
        }

        public void Destroy() { }

    }
}
