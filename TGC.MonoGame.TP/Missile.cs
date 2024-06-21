using System;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Transactions;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Collisions;
using TGC.MonoGame.TP;
using TGC.MonoGame.TP.Camaras;
using static System.Formats.Asn1.AsnWriter;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;
using BoundingSphere = Microsoft.Xna.Framework.BoundingSphere;
using MathHelper = Microsoft.Xna.Framework.MathHelper;
using Matrix = Microsoft.Xna.Framework.Matrix;
using NumericVector3 = System.Numerics.Vector3;

public class Missile
{
    public const string ContentFolder3D = "Models/";
    public const string ContentFolderEffects = "Effects/";
    public const string ContentFolderMusic = "Music/";
    public const string ContentFolderSounds = "Sounds/";
    public const string ContentFolderSpriteFonts = "SpriteFonts/";
    public const string ContentFolderTextures = "Textures/";
    public const string ContentFolderSoundEffects = "SoundEffects/";

    private Model Model { get; set; }
    public Matrix OBBWorld { get; set; }
    private Matrix Position { get; set; }
    public Matrix World { get; set; }
    private BoundingBox BoundingBox {  get; set; }
    public OrientedBoundingBox OBBox { get; private set; }


    private float velocityThreshold = 30f;
    public float radius;
    private Sphere BulletShape;
    private BodyHandle Handle;
    private bool firstTime;
    private Quaternion rotation;
    private float angleRot = 0;
    public bool deleteFlag = false;
    private float time = 0;
    private float Scale;

    public Missile(Simulation Simulation, CarConvexHull Car ,Model model, float scale)
    {
        this.Model = model;
        this.Scale = scale;

        if (Car.MachineMissile)
        {
            radius = 0.5f * 0.01f;
            angleRot = 90f;
        }
        else
        {
            radius = 0.2f * 1.3f;
            angleRot = 0f;
        }

        BulletShape = new Sphere(radius);
        firstTime = true;
        Vector3 forwardLocal = new Vector3(0, 0, -1);
        var forwardWorld = Vector3.Transform(forwardLocal, Car.rotationQuaternion * Car.quaternion);

        var bodyDescription = BodyDescription.CreateConvexDynamic(Car.Pose,
            new BodyVelocity(new NumericVector3(forwardWorld.X, forwardWorld.Y, forwardWorld.Z) * -50),
            BulletShape.Radius * BulletShape.Radius * BulletShape.Radius, Simulation.Shapes, BulletShape);

        Handle = Simulation.Bodies.Add(bodyDescription);

    }



    public void Draw(Matrix World, Model model, Texture2D texture, FollowCamera Camera, GameTime gameTime, Effect MissileEffect)
    {
        time += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

        
            for (int i = 0; i < model.Meshes.Count; i++)
            {
                var mesh = model.Meshes[i];

                var modelMeshBaseTransform = new Matrix[model.Bones.Count];
                model.CopyAbsoluteBoneTransformsTo(modelMeshBaseTransform);

                MissileEffect.Parameters["World"]?.SetValue(modelMeshBaseTransform[mesh.ParentBone.Index] * World);
                MissileEffect.Parameters["View"]?.SetValue(Camera.View);
                MissileEffect.Parameters["Projection"]?.SetValue(Camera.Projection);
                MissileEffect.Parameters["ModelTexture"]?.SetValue(texture);
                MissileEffect.Parameters["Time"]?.SetValue(Convert.ToSingle(time));

                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = MissileEffect;
                }

                mesh.Draw();
            }
        

    }

    public void update(Simulation Simulation, Quaternion carQuaternion)
    {

        Vector3 scale;
        Quaternion rot;
        Vector3 translation;

        var rotationQuaternionX = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(angleRot));
        var rotationQuaternionY = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(180));

        if (!deleteFlag)
        {
            var body = Simulation.Bodies.GetBodyReference(Handle);
            var pose = body.Pose;
            var position = pose.Position;
            var quaternion = pose.Orientation;



            if (body.Velocity.Linear.LengthSquared() < Math.Pow(velocityThreshold, 2))
            {
                Simulation.Bodies.Remove(Handle);
                deleteFlag = true;
            }

            if (firstTime)
            {
                World = Matrix.CreateScale(radius) *
                        Matrix.CreateFromQuaternion(rotationQuaternionX) *
                        Matrix.CreateFromQuaternion(rotationQuaternionY * carQuaternion) *
                        Matrix.CreateTranslation(new Vector3(position.X, position.Y, position.Z));
                rotation = rotationQuaternionY * carQuaternion;

            }
            else
            {
                World = Matrix.CreateScale(radius) *
                                Matrix.CreateFromQuaternion(rotationQuaternionX) *
                                Matrix.CreateFromQuaternion(rotation) *
                                Matrix.CreateTranslation(new Vector3(position.X, position.Y, position.Z));
            }

            firstTime = false;

            BoundingBox = BoundingVolumesExtensions.CreateAABBFrom(Model);
            BoundingBox = BoundingVolumesExtensions.ScaleCentered(BoundingBox, Scale);
            OBBox = OrientedBoundingBox.FromAABB(BoundingBox);

            World.Decompose(out scale, out rot, out translation);
            OBBox.Orientation = Matrix.CreateFromQuaternion(rot);
            OBBox.Center = translation;
            Position = Matrix.CreateTranslation(translation);
            OBBWorld = Matrix.CreateScale(OBBox.Extents) *
                 OBBox.Orientation *
                 Position;



        }
        else
        {
            return;
        }
    }
}
