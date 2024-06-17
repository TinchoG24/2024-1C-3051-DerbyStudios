
using System.Collections.Generic;
using System.Linq;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Constraints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Collisions;
using TGC.MonoGame.TP.Camaras;
using static System.Formats.Asn1.AsnWriter;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace TGC.MonoGame.TP;

public class GameModel : BaseModel
{
    public GameModel(Model model, Effect effect, float scale, Vector3 position, Simulation simulation)
    {
        Model = model;
        Effect = effect;
        Scale = scale;
        Position = position;
        World = Matrix.CreateScale(Scale) * Matrix.CreateTranslation(Position);

        foreach (var mesh in Model.Meshes)
        {
            foreach (var meshPart in mesh.MeshParts)
            {
                ModelTextures.Add(((BasicEffect)meshPart.Effect)?.Texture);
            }
        }

        BoundingBox = BoundingVolumesExtensions.CreateAABBFrom(Model);
        BoundingBox = BoundingVolumesExtensions.ScaleCentered(BoundingBox, Scale);
        BoundingBox = new BoundingBox(BoundingBox.Min + Position, BoundingBox.Max + Position);

        var _traslation = (BoundingBox.Max + BoundingBox.Min) / 2f;
        var _scale = BoundingBox.Max - BoundingBox.Min;

        simulation.Statics.Add(new StaticDescription(Utils.ToNumericVector3(_traslation),
        simulation.Shapes.Add(new Box(_scale.X, _scale.Y, _scale.Z))));

    }

    public void Update() { }

    public void Draw(Model model, Matrix World, FollowCamera FollowCamera, BoundingFrustum boundingFrustum, BoundingBox boundingBox)
    {

        if (boundingFrustum.Intersects(boundingBox))
        {

            for (int i = 0; i < model.Meshes.Count; i++)
            {
                var mesh = model.Meshes[i];
                var texture = ModelTextures[i];

                var inverseTransposeWorld = Matrix.Transpose(Matrix.Invert(World));
                var modelMeshBaseTransform = new Matrix[model.Bones.Count];
                model.CopyAbsoluteBoneTransformsTo(modelMeshBaseTransform);

                Effect.Parameters["World"]?.SetValue(modelMeshBaseTransform[mesh.ParentBone.Index] * World);
                Effect.Parameters["View"]?.SetValue(FollowCamera.View);
                Effect.Parameters["Projection"]?.SetValue(FollowCamera.Projection);
                Effect.Parameters["InverseTransposeWorld"]?.SetValue(inverseTransposeWorld);
                Effect.Parameters["ModelTexture"]?.SetValue(texture);

                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = Effect;
                }

                mesh.Draw();
            }

        }

    }
}






