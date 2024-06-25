using Microsoft.Xna.Framework;
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
    public class Star : PowerUp
    {
        bool First = true;

        public Star(Vector3 position) : base(position)
        {
            Position = position;

            PowerUpWorld = Matrix.CreateScale(0.7f, 0.7f, 0.7f) * Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateTranslation(position);

        }

        public override async void Activate(CarConvexHull carConvexHull)
        {

            if (!Activated && First)
            {
                PowerUpSound.Play();
                Activated = true;
                carConvexHull.Stars += 1;
                First = false;
            }

        }

        public override void LoadContent(ContentManager Content)
        {
            PowerUpSound = Content.Load<SoundEffect>(ContentFolderSoundEffects + "StarSoundEffect");

            PowerUpEffect = Content.Load<Effect>(ContentFolderEffects + "StarsShader");

            PowerUpModel = Content.Load<Model>(ContentFolder3D + "PowerUps/simple-star-lowpoly/source/star");

            // PowerUpTexture = ((BasicEffect)PowerUpModel.Meshes.FirstOrDefault()?.MeshParts.FirstOrDefault()?.Effect)?.Texture;

            BoundingSphere = BoundingVolumesExtensions.CreateSphereFrom(PowerUpModel);

            BoundingSphere.Center = Position;

            BoundingSphere.Radius = 2f;
        }

    }
}