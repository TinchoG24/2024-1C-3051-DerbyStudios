﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Constraints;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Viewer.Gizmos;
using TGC.MonoGame.Samples.Physics.Bepu;
using TGC.MonoGame.TP.Geometries;
using NumericVector3 = System.Numerics.Vector3;
using TGC.MonoGame.Samples.Geometries.Textures;
using TGC.MonoGame.TP.Physics;
using TGC.MonoGame.TP.PowerUps;
using System.Linq;

namespace TGC.MonoGame.TP
{
    /// <summary>
    ///     Esta es la clase principal del juego.
    ///     Inicialmente puede ser renombrado o copiado para hacer mas ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
    /// </summary>
    public class TGCGame : Game
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";
        public const float ViewDistance = 20f;
        public const float Offset = 10f;
        private const int SEED = 0;
        public const float CameraSpeed = 50f;
        private const float Gravity = 10f;
        public Vector3 LookAtVector = new Vector3(0, 0, Offset);

        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public TGCGame()
        {
            // Maneja la configuracion y la administracion del dispositivo grafico.
            Graphics = new GraphicsDeviceManager(this);
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;
            Gizmos = new Gizmos();

            // Para que el juego sea pantalla completa se puede usar Graphics IsFullScreen.
            // Carpeta raiz donde va a estar toda la Media.
            Content.RootDirectory = "Content";
            // Hace que el mouse sea visible.
            IsMouseVisible = true;
        }
        public Gizmos Gizmos { get; set; }

        private GraphicsDeviceManager Graphics { get; set; }
        private Random _random;

        private FollowCamera FollowCamera { get; set; }
        private SpriteBatch SpriteBatch { get; set; }
        private Texture2D FloorTexture { get; set; }

        //Auto Principal 
        private CarConvexHull MainCar { get; set; }
        private CarSimulation CarSimulation { get; set; }
        private Simulation Simulation { get; set; }

        //Piso y paredes 
        private QuadPrimitive FloorQuad { get; set; }
        private Matrix FloorWorld { get; set; }

        private List<Matrix> WallWorlds = new List<Matrix>();

        //Efectos 
        private Effect Effect { get; set; }
        private Effect EffectNoTextures { get; set; }
        private Effect TilingEffect { get; set; }


        //Modelos y PowerUps
        private PowerUp[] PowerUps { get; set; }
        private GameModel[] GameModels { get; set; }

        private int ArenaWidth = 200;
        private int ArenaHeight = 200;
        private float time;

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

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void Initialize()
        {
            // La logica de inicializacion que no depende del contenido se recomienda poner en este metodo.

            _random = new Random(SEED);

            // Apago el backface culling.
            // Esto se hace por un problema en el diseno del modelo del logo de la materia.
            // Una vez que empiecen su juego, esto no es mas necesario y lo pueden sacar.
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;

            // Creo una camara para seguir a nuestro auto.
            FollowCamera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio);

            //  Simulacion del auto principal 
            CarSimulation = new CarSimulation();
            Simulation = CarSimulation.Init();
            MainCar = new CarConvexHull(Vector3.Zero, Gravity, Simulation);


            FloorQuad = new QuadPrimitive(GraphicsDevice);
            FloorWorld = Matrix.CreateScale(2500f, 1f, 2500f);

            // PowerUps
            PowerUps = new PowerUp[]
            {
                new VelocityPowerUp(new Vector3(20f, 2f, 20f))
            };


            base.Initialize();
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
        ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
        ///     que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void LoadContent()
        {

            // Aca es donde deberiamos cargar todos los contenido necesarios antes de iniciar el juego.
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            Gizmos.LoadContent(GraphicsDevice, Content);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            Array.ForEach(PowerUps, powerUp => powerUp.LoadContent(Content));

            // Cargo un efecto basico propio declarado en el Content pipeline.
            // En el juego no pueden usar BasicEffect de MG, deben usar siempre efectos propios.
            Effect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
            EffectNoTextures = Content.Load<Effect>(ContentFolderEffects + "BasicShaderNoTextures");
            TilingEffect = Content.Load<Effect>(ContentFolderEffects + "TextureTiling");
            FloorTexture = Content.Load<Texture2D>(ContentFolderTextures + "FloorTexture");

            var vertices = new NumericVector3[] {

                    // Bottom vertices
                    new NumericVector3(0f, 0.0f, 0f),
                    new NumericVector3(4.5f, 0.0f, 0f),
                    new NumericVector3(4.5f, 0.0f, 9f),
                    new NumericVector3(0f, 0.0f, 9f),

                    // Top vertices
                    new NumericVector3(0f, 0.5f, 1f),
                    new NumericVector3(4.5f, 0.5f, 1f),
                    new NumericVector3(0f, 1.2f, 2.5f),
                    new NumericVector3(4.5f, 1.2f, 2.5f),
                    new NumericVector3(0f, 2.0f, 4.5f),
                    new NumericVector3(4.5f, 2.0f, 4.5f),
                    new NumericVector3(4.5f, 3f, 9f),
                    new NumericVector3(0f, 3f, 9f)
                };

            NumericVector3 center;

            var CarModel = Content.Load<Model>(ContentFolder3D + "car/RacingCar");
            MainCar.Load(CarModel, Effect);

            GameModels = new GameModel[]
             {
                new GameModel(Content.Load<Model>(ContentFolder3D + "trees/Tree2"), Effect, 60f, GenerateRandomPositions(100), Simulation, new Sphere(1.5f)),
                new GameModel(Content.Load < Model >(ContentFolder3D + "Street/model/Electronic box"), Effect, 1f, GenerateRandomPositions(100), Simulation, new Box(3f, 3f, 2f)),
                new GameModel(Content.Load < Model >(ContentFolder3D + "Street/model/towers"), Effect, 1f, GenerateRandomPositions(15), Simulation, new Box(5f, 10f, 4f)),
                new GameModel(Content.Load < Model >(ContentFolder3D + "weapons/Weapons"), Effect, 0.1f, GenerateRandomPositions(20), Simulation, new Box(2.5f, 2f, 2.5f)),
                new GameModel(Content.Load < Model >(ContentFolder3D + "gasoline/gasoline"), Effect, 1.5f, GenerateRandomPositions(100), Simulation, new Box(2f, 3f, 2f)),
                new GameModel(Content.Load<Model>(ContentFolder3D + "Street/model/House"), Effect , 1f , new Vector3(30f, 0 , 30f ) , Simulation ,  new Box(17.5f, 10f, 17.5f)),
                new GameModel(Content.Load < Model >(ContentFolder3D + "ramp/ramp"), Effect, 4f, GenerateRandomPositions(50), Simulation, new ConvexHull(vertices, Simulation.BufferPool, out center)),
                new GameModel(Content.Load < Model >(ContentFolder3D + "Street/model/WatercolorScene"), Effect, 1f, GenerateRandomPositions(10), Simulation, new Box(10f, 3f, 10f)),
                new GameModel(Content.Load<Model>(ContentFolder3D + "carDBZ/carDBZ"), Effect ,1f , new Vector3(50f, 0, 50f) ),
                new GameModel(Content.Load < Model >(ContentFolder3D + "car2/car2"), Effect, 1f, GenerateRandomPositions(4)),
                new GameModel(Content.Load<Model>(ContentFolder3D + "Bushes/source/bush1"), Effect,1f , GenerateRandomPositions(4)),
                new GameModel(Content.Load < Model >(ContentFolder3D + "Truck/source/KAMAZ"), Effect, 1f, GenerateRandomPositions(4)),
                new GameModel(Content.Load < Model >(ContentFolder3D + "Street/model/fence"), Effect, 1f, GenerateRandomPositions(4)),
                new GameModel(Content.Load < Model >(ContentFolder3D + "Street/model/fence2"), Effect, 1f, GenerateRandomPositions(4)),

            };


            // Add walls
            WallWorlds.Add(Matrix.CreateRotationY(0f) * Matrix.CreateTranslation(200f, 0f, 0f));
            WallWorlds.Add(Matrix.CreateRotationY(0f) * Matrix.CreateTranslation(-200f, 0f, 0f));
            WallWorlds.Add(Matrix.CreateRotationY(Convert.ToSingle(Math.PI / 2)) * Matrix.CreateTranslation(0f, 0f, 200f));
            WallWorlds.Add(Matrix.CreateRotationY(Convert.ToSingle(Math.PI / 2)) * Matrix.CreateTranslation(0f, 0f, -200f));
            var wallShape = new Box(1f, 10f, 1000f);
            for (int i = 0; i < 2; i++)
            {
                var world = WallWorlds[i];
                var wallDescription = new StaticDescription(
                    new NumericVector3(world.Translation.X, world.Translation.Y, world.Translation.Z),
                    Simulation.Shapes.Add(wallShape)
                );
                Simulation.Statics.Add(wallDescription);
            }
            for (int i = 2; i < 4; i++)
            {
                var world = WallWorlds[i];
                var wallDescription = new StaticDescription(
                    new NumericVector3(world.Translation.X, world.Translation.Y, world.Translation.Z),
                    System.Numerics.Quaternion.CreateFromAxisAngle(NumericVector3.UnitY, Convert.ToSingle(Math.PI / 2)),
                    Simulation.Shapes.Add(wallShape),
                    ContinuousDetection.Discrete
                );
                Simulation.Statics.Add(wallDescription);
            }

            var planeShape = new Box(2500f, 1f, 2500f);
            var planeDescription = new StaticDescription(
                new NumericVector3(0, -0.5f, 0),
                Simulation.Shapes.Add(planeShape)
            );
            Simulation.Statics.Add(planeDescription);

            base.LoadContent();
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            CarSimulation.Update();

            Array.ForEach(PowerUps, PowerUp => PowerUp.Update());

            // Capturar Input teclado
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                //Salgo del juego.
                Exit();
            }

            // Capto el estado del teclado.
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                // Salgo del juego.
                Exit();
            }

            // Actualizar estado del auto
            MainCar.Update(Keyboard.GetState(), gameTime, Simulation);

            Array.ForEach(PowerUps, PowerUp => PowerUp.ActivateIfBounding(Simulation, MainCar));

            // Actualizo la camara, enviandole la matriz de mundo del auto.
            FollowCamera.Update(gameTime, MainCar.World);

            Gizmos.UpdateViewProjection(FollowCamera.View, FollowCamera.Projection);

            base.Update(gameTime);
        }


        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {

            time += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            // Aca deberiamos poner toda la logia de renderizado del juego.
            GraphicsDevice.Clear(Color.Beige);

            // Para dibujar le modelo necesitamos pasarle informacion que el efecto esta esperando.
            Effect.Parameters["View"].SetValue(FollowCamera.View);
            Effect.Parameters["Projection"].SetValue(FollowCamera.Projection);

            EffectNoTextures.Parameters["View"].SetValue(FollowCamera.View);
            EffectNoTextures.Parameters["Projection"].SetValue(FollowCamera.Projection);

            Array.ForEach(PowerUps, PowerUp => PowerUp.Draw(FollowCamera, gameTime));

            Array.ForEach(GameModels, GameModel => GameModel.Draw(GameModel.World));

            foreach (GameModel model in GameModels)
                foreach (Matrix world in model.World)
                    Gizmos.DrawCube(world/* * Matrix.CreateScale(model.Scale)*/, Color.Red);


            DrawFloor(FloorQuad);

            DrawWalls();

            MainCar.Draw();

            Gizmos.Draw();

        }

        private void DrawFloor(QuadPrimitive geometry)
        {
            // EffectNoTextures.Parameters["DiffuseColor"].SetValue(Color.DarkSeaGreen.ToVector3());
            TilingEffect.CurrentTechnique = TilingEffect.Techniques["BaseTiling"];
            var world = FloorWorld * Matrix.CreateTranslation(0f, -0.1f, 0f);
            TilingEffect.Parameters["WorldViewProjection"].SetValue(world * FollowCamera.View * FollowCamera.Projection);
            TilingEffect.Parameters["Tiling"].SetValue(new Vector2(350f, 350f));
            TilingEffect.Parameters["Texture"].SetValue(FloorTexture);
            geometry.Draw(TilingEffect);

        }
        private void DrawWalls()
        {
            var prim = new BoxPrimitive(GraphicsDevice, new Vector3(1f, 10f, 200f), Color.HotPink);
            foreach (var wall in WallWorlds)
            {
                EffectNoTextures.Parameters["DiffuseColor"].SetValue(Color.HotPink.ToVector3());
                EffectNoTextures.Parameters["World"]?.SetValue(wall);
                prim.Draw(EffectNoTextures);
            }
        }

        /// <summary>
        ///     Libero los recursos que se cargaron en el juego.
        /// </summary>
        protected override void UnloadContent()
        {
            // Libero los recursos.
            Content.Unload();

            Gizmos.Dispose();

            base.UnloadContent();
        }
    }
}