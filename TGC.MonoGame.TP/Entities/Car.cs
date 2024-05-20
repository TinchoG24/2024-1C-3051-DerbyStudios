using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Collisions;
using NumericVector3 = System.Numerics.Vector3;

namespace TGC.MonoGame.TP;

public class Car
{
    private Vector3 Position;
    private Matrix World { get; set; }
    private Model Model;
    private ModelMesh MainBody;
    private ModelMesh FrontLeftWheel;
    private ModelMesh FrontRightWheel;
    private ModelMesh BackLeftWheel;
    private ModelMesh BackRightWheel;
    private BodyHandle Handle;
    private float Scale = 1f;
    private Effect Effect;
    private float CarVelocity { get; set; }
    private float CarRotation { get; set; }
    private float acceleration = 3f;
    private float frictionCoefficient = 0.5f;
    private float maxVelocity = 0.7f;
    private float minVelocity = -0.7f;
    private float stopCar = 0f;
    private float carRotatingVelocity = 2.3f;
    private float jumpSpeed = 100f;
    private float gravity = 10f;
    private float carMass = 1f;
    private float carInFloor = 0f;
    private float wheelRotation = 0f;
    private static NumericVector3[] carColliderVertices = new NumericVector3[]
    {
        // Bottom vertices
        new NumericVector3(-1.0f, 0.0f, -2.0f),
        new NumericVector3(1.0f, 0.0f, -2.0f),
        new NumericVector3(1.0f, 0.0f, 2.0f),
        new NumericVector3(-1.0f, 0.0f, 2.0f),

        // Top vertices
        new NumericVector3(-0.8f, 0.6f, -1.5f),
        new NumericVector3(0.8f, 0.6f, -1.5f),
        new NumericVector3(0.8f, 0.6f, 1.5f),
        new NumericVector3(-0.8f, 0.6f, 1.5f)
    };
    private List<List<Texture2D>> MeshPartTextures = new List<List<Texture2D>>();



    public Car(Vector3 pos)
    {
        Position = pos;
    }

    public Matrix getWorld() { return World; }

    public void LoadPhysics(Simulation Simulation)
    {
        NumericVector3 center;
        var convexHullShape  = new ConvexHull(carColliderVertices, Simulation.BufferPool, out center);
        var carBodyDescription = BodyDescription.CreateConvexDynamic(
            new NumericVector3(0, 0, 0),
            new BodyVelocity(new NumericVector3(0,0,0)),
            1,
            Simulation.Shapes, convexHullShape
        );
        var bh = Simulation.Bodies.Add(carBodyDescription);
        Handle = bh;
    }

    public void Load(Model model, Effect effect)
    {

        Effect = effect;
        Model = model;
        World = Matrix.Identity;

        MainBody = Model.Meshes[0];
        FrontRightWheel = Model.Meshes[1];
        FrontLeftWheel = Model.Meshes[2];
        BackLeftWheel = Model.Meshes[3];
        BackRightWheel = Model.Meshes[4];

        for (int mi = 0; mi < Model.Meshes.Count; mi++)
        {
            var mesh = model.Meshes[mi];
            MeshPartTextures.Add(new List<Texture2D>());
            // Un mesh puede tener mas de 1 mesh part (cada 1 puede tener su propio efecto).
            for (int mpi = 0; mpi < mesh.MeshParts.Count; mpi++)
            {
                var meshPart = mesh.MeshParts[mpi];
                var texture = ((BasicEffect)meshPart.Effect).Texture;
                MeshPartTextures[mi].Add(texture);
                meshPart.Effect = Effect;
            }
        }

    }

    private void MoveCar(KeyboardState keyboardState, GameTime gameTime, BodyReference bodyReference)
    {
         float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Define constants for car physics
            float acceleration = 50f;
            float braking = 30f;
            float maxSpeed = 20f;
            float maxTurn = 3f;
            float turnSpeed = 50f;
            float friction = 0.98f;

            // Calculate forward and backward impulses
            var forwardImpulse = new System.Numerics.Vector3(0, 0, -acceleration) * elapsedTime;
            var backwardImpulse = new System.Numerics.Vector3(0, 0, braking) * elapsedTime;
            var linearVelocity = bodyReference.Velocity.Linear;

            // Apply forward/backward impulses relative to the car's orientation
            if (keyboardState.IsKeyDown(Keys.W))
            {
                var transformedForwardImpulse = System.Numerics.Vector3.Transform(forwardImpulse, bodyReference.Pose.Orientation);
                bodyReference.ApplyLinearImpulse(transformedForwardImpulse);
            }
            if (keyboardState.IsKeyDown(Keys.S))
            {
                var transformedBackwardImpulse = System.Numerics.Vector3.Transform(backwardImpulse, bodyReference.Pose.Orientation);
                bodyReference.ApplyLinearImpulse(transformedBackwardImpulse);
            }

            // Apply friction to simulate resistance
            bodyReference.Velocity.Linear *= friction;
            bodyReference.Velocity.Angular *= friction;

            // Apply angular impulses for turning
            if (keyboardState.IsKeyDown(Keys.A))
            {
                bodyReference.ApplyAngularImpulse(new System.Numerics.Vector3(0, turnSpeed, 0) * elapsedTime);
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                bodyReference.ApplyAngularImpulse(new System.Numerics.Vector3(0, -turnSpeed, 0) * elapsedTime);
            }

            // Limit the maximum speed
            if (linearVelocity.Length() > maxSpeed)
            {
                linearVelocity = System.Numerics.Vector3.Normalize(linearVelocity) * maxSpeed;
                bodyReference.Velocity.Linear = linearVelocity;
            }
            var angularVelocity = bodyReference.Velocity.Angular;
            if (angularVelocity.Length() > maxTurn)
            {
                angularVelocity = System.Numerics.Vector3.Normalize(angularVelocity) * maxTurn;
                bodyReference.Velocity.Angular = angularVelocity;
            }
    }

    public void Update(KeyboardState keyboardState, GameTime gameTime, Simulation simulation)
    {

        var bodyHandle = Handle;
        var bodyReference = simulation.Bodies.GetBodyReference(bodyHandle);
        MoveCar(keyboardState, gameTime, bodyReference);
        var position = bodyReference.Pose.Position;
        Quaternion quaternion = bodyReference.Pose.Orientation;
        Quaternion rotationQuaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(180));
        World = Matrix.CreateFromQuaternion(rotationQuaternion * quaternion) * Matrix.CreateTranslation(new Vector3(position.X, position.Y, position.Z));

    }



    public void Draw()
    {
        DrawCarBody();
        DrawFrontWheels();
        DrawBackWheels();
    }

    private void DrawCarBody()
    {
        for (int mpi = 0; mpi < MainBody.MeshParts.Count; mpi++)
        {
            var meshPart = MainBody.MeshParts[mpi];
            var texture = MeshPartTextures[0][mpi];
            meshPart.Effect.Parameters["World"].SetValue(World);
            Effect.Parameters["ModelTexture"].SetValue(texture);
        }
        MainBody.Draw();
    }

    private void DrawFrontWheels()
    {
        var frontLeftWorld = FrontLeftWheel.ParentBone.ModelTransform * World;
        for (int mpi = 0; mpi < FrontLeftWheel.MeshParts.Count; mpi++)
        {
            var meshPart = MainBody.MeshParts[mpi];
            var texture = MeshPartTextures[2][mpi];
            meshPart.Effect.Parameters["World"].SetValue(Matrix.CreateRotationY(wheelRotation) * frontLeftWorld);
            Effect.Parameters["ModelTexture"].SetValue(texture);
        }
        FrontLeftWheel.Draw();

        var frontRightWorld = FrontRightWheel.ParentBone.ModelTransform * World;
        for (int mpi = 0; mpi < FrontRightWheel.MeshParts.Count; mpi++)
        {
            var meshPart = MainBody.MeshParts[mpi];
            var texture = MeshPartTextures[1][mpi];
            meshPart.Effect.Parameters["World"].SetValue(Matrix.CreateRotationY(wheelRotation) * frontRightWorld);
            Effect.Parameters["ModelTexture"].SetValue(texture);
        }
        FrontRightWheel.Draw();
    }

    private void DrawBackWheels()
    {
        var backLeftWorld = BackLeftWheel.ParentBone.ModelTransform * World;
        for (int mpi = 0; mpi < BackLeftWheel.MeshParts.Count; mpi++)
        {
            var meshPart = MainBody.MeshParts[mpi];
            var texture = MeshPartTextures[3][mpi];
            meshPart.Effect.Parameters["World"].SetValue(backLeftWorld);
            Effect.Parameters["ModelTexture"].SetValue(texture);
        }
        BackLeftWheel.Draw();

        var backRightWorld = BackRightWheel.ParentBone.ModelTransform * World;
        for (int mpi = 0; mpi < BackRightWheel.MeshParts.Count; mpi++)
        {
            var meshPart = MainBody.MeshParts[mpi];
            var texture = MeshPartTextures[4][mpi];
            meshPart.Effect.Parameters["World"].SetValue(backRightWorld);
            Effect.Parameters["ModelTexture"].SetValue(texture);
        }
        BackRightWheel.Draw();
    }
}
