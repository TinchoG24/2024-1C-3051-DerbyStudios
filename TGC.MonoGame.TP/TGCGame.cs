using System;
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
using System.Collections;
using TGC.MonoTP;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using TGC.MonoGame.Samples.Collisions;
using static System.Formats.Asn1.AsnWriter;
using System.Runtime.ConstrainedExecution;
using TGC.MonoGame.TP.Entities;
using TGC.MonoGame.TP.Camaras;
using TGC.MonoGame.Samples.Cameras;
using System.Transactions;
using System.Security.Claims;
using System.Reflection.Metadata.Ecma335;
using BepuPhysics.Trees;
using System.Reflection.Metadata;
using System.Threading;
using TGC.MonoGame.Samples.Geometries;

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
        public const string ContentFolderSoundEffects = "SoundEffects/";
        public const string ContentFolderSoundSongs = "SoundSongs/";

        public const float ViewDistance = 20f;
        public const float Offset = 10f;
        private const int SEED = 0;
        public const float CameraSpeed = 50f;
        private const float Gravity = 10f;
        public Vector3 LookAtVector = new Vector3(0, 0, Offset);
        public const int ST_STAGE_1 = 1;
        public const int ST_STAGE_2 = 2;
        public const int ST_GAME_OVER = 99;

        private int gameState = ST_STAGE_1;

        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public TGCGame()
        {
            // Maneja la configuracion y la administracion del dispositivo grafico.
            Graphics = new GraphicsDeviceManager(this);
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;

            Graphics.IsFullScreen = true;

            Gizmos = new Gizmos();

            // Para que el juego sea pantalla completa se puede usar Graphics IsFullScreen.
            // Carpeta raiz donde va a estar toda la Media.

            Content.RootDirectory = "Content";
            // Hace que el mouse sea visible.
            IsMouseVisible = true;
        }

        public Gizmos Gizmos { get; set; }
        private GraphicsDeviceManager Graphics { get; set; }

        //Random 
        private Random _random { get; set; }

        //Frustum Optimizacion
        private BoundingFrustum BoundingFrustum { get; set; }

        //Camara
        private FollowCamera FollowCamera { get; set; }

        //Sprites
        public SpriteFont SpriteFont { get; set; }
        private SpriteBatch SpriteBatch { get; set; }


        //Piso y paredes  Limites
        private Texture2D FloorTexture { get; set; }
        private Texture2D FloorNormalMap { get; set; }
        private Texture2D WallTexture { get; set; }
        private Texture2D WallNormalMap { get; set; }
        private QuadPrimitive FloorQuad { get; set; }
        private Matrix FloorWorld { get; set; }

        private List<Matrix> WallWorlds = new List<Matrix>();


        //Auto Principal 
        private CarConvexHull MainCar { get; set; }
        private CarSimulation CarSimulation { get; set; }
        private Simulation Simulation { get; set; }
        public Model CarModel { get; private set; }



        //Efectos 
        private Effect Effect { get; set; }
        private Effect EffectTexture { get; set; }
        private Effect EffectNoTextures { get; set; }
        private Effect TilingEffect { get; set; }
        private Effect EnvironmentMapEffect { get; set; }
        private Effect GaussianBlurEffect { get; set; }
        private Effect IntegrateEffect { get; set; }


        //Modelos y PowerUps
        private PowerUp[] PowerUps { get; set; }
        private GameModel[] GameModels { get; set; }
        private List<GameModel> GameModelList { get; set; }
        private PowerUp[] HealthPacks { get; set; }
        private PowerUp[] Stars { get; set; }

        //SoundEffects 
        private SoundEffect MachineGunSound { get; set; }
        private SoundEffect MissileSound { get; set; }
        private SoundEffect Claxon { get; set; }
        private SoundEffect Explosion { get; set; }

        //Misiles
        private List<Missile> Missiles { get; set; }
        private bool CanShoot { get; set; }
        // private Effect MissileEffect { get; set; }
        public Model MissileModel { get; set; }
        public Model BulletModel { get; private set; }
        public Texture2D MissileTexture { get; set; }
        public Texture2D BulletTexture { get; private set; }

        //EnviromentMAP
        private RenderTargetCube EnvironmentMapRenderTarget { get; set; }
        private StaticCamera EnvironmentMapCamera { get; set; }

        // Bloom
        private RenderTarget2D MainRenderTarget { get; set; }
        private RenderTarget2D BloomRenderTarget { get; set; }
        private RenderTarget2D FinalBloomRenderTarget { get; set; }
        private FullScreenQuad FullScreenQuad { get; set; }

        //Enemy
        private List<Enemy> Enemies { get; set; }
        public Model EnemyModel { get; private set; }

        public List<Texture2D> EnemyTexture = new List<Texture2D>();


        //HUD 
        HUD HUD { get; set; }
        public Song backgroundMusic { get; private set; }
        public SoundEffect soundEffect { get; private set; }
        public SoundEffect GameOverSoundEffect { get; private set; }
        public SoundEffect SongWin { get; private set; }
        public SoundEffect SongOil { get; private set; }
        public GameModel Gasoline { get; private set; }
        public List<GameModel> Gasolines { get; private set; }
        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }
        public Model CarModelMenu { get; private set; }
        public Matrix[] relativeMatrices { get; private set; }
        public SoundEffect ChooseSoundEffect { get; private set; }

        //Dibujar Autos de menu
        private ModelBone leftBackWheelBone;
        private ModelBone rightBackWheelBone;
        private ModelBone leftFrontWheelBone;
        private ModelBone rightFrontWheelBone;
        private Matrix leftBackWheelTransform = Matrix.Identity;
        private Matrix rightBackWheelTransform = Matrix.Identity;
        private Matrix leftFrontWheelTransform = Matrix.Identity;
        private Matrix rightFrontWheelTransform = Matrix.Identity;
        public List<Texture2D> ModelTextures = new List<Texture2D>();
        private Vector3 posicionCamara = new Vector3(-200, 100, 0);
        private float posAutoMenu = -350;
        private float currentHealth;


        private float time = 0;
        private bool addEnemy;
        private int count = 0;
        private Matrix rotationCar;
        private bool ChargeOil = false;
        private bool activeSound = false;
        private bool activeGodMode = false;
        private bool previousGKeyState = false;


        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void Initialize()
        {

            //HUD
            HUD = new HUD(Content, GraphicsDevice);
            HUD.Initialize();

            //Random SEED = 0
            _random = new Random(SEED);

            // Apago el backface culling.
            // Esto se hace por un problema en el diseno del modelo del logo de la materia.
            // Una vez que empiecen su juego, esto no es mas necesario y lo pueden sacar.
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;

            // Camara para seguir al auto principal
            FollowCamera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio);
            BoundingFrustum = new BoundingFrustum(FollowCamera.View * FollowCamera.Projection);

            //  Simulacion del auto principal 
            CarModel = Content.Load<Model>(ContentFolder3D + "car/RacingCar");
            CarSimulation = new CarSimulation();
            Simulation = CarSimulation.Init();
            MainCar = new CarConvexHull(Vector3.Zero, Gravity, Simulation, CarModel);


            //Piso
            FloorQuad = new QuadPrimitive(GraphicsDevice);
            FloorWorld = Matrix.CreateScale(2500f, 1f, 2500f);

            // PowerUps
            PowerUps = new PowerUp[]
            {
                new VelocityPowerUp(new Vector3(-10,2,-10)),
                new MissilePowerUp(new Vector3(10,2,-10)),
                new MachineGunPowerUp(new Vector3(-10,2,10)),

                new VelocityPowerUp(new Vector3(-60,2,-60)),
                new MissilePowerUp(new Vector3(60,2,-60)),
                new MachineGunPowerUp(new Vector3(-60,2,60)),

                new VelocityPowerUp(new Vector3(-110,2,-110)),
                new MissilePowerUp(new Vector3(110,2,-110)),
                new MachineGunPowerUp(new Vector3(-110,2,110)),

                new VelocityPowerUp(new Vector3(-170,2,-170)),
                new MissilePowerUp(new Vector3(170,2,-170)),
                new MachineGunPowerUp(new Vector3(-170,2,170))
            };

            HealthPacks = new PowerUp[]
            {
                new HealthPack(new Vector3(35 ,3 ,-35)),
                new HealthPack(new Vector3(-35 ,3 ,35)),
                new HealthPack(new Vector3(-35 ,3 ,-35)),

                new HealthPack(new Vector3(95 ,3 ,-95)),
                new HealthPack(new Vector3(-95 ,3 ,95)),
                new HealthPack(new Vector3(-95 ,3 ,-95)),

                new HealthPack(new Vector3(135 ,3 ,-135)),
                new HealthPack(new Vector3(-135 ,3 ,135)),
                new HealthPack(new Vector3(-135 ,3 ,-135))
            };

            Stars = new PowerUp[]
           {
               //10 Estrellas para ganar 
                new Star(new Vector3(0 ,5 ,10)),
                new Star(new Vector3(-37 ,5 ,28)),
                new Star(new Vector3(-57 ,5 ,-98)),
                new Star(new Vector3(-52 ,5 ,48)),
                new Star(new Vector3(152 ,5 ,28)),
                new Star(new Vector3(-12 ,5 ,28)),
                new Star(new Vector3(-189 ,5 ,-28)),
                new Star(new Vector3(189 ,5 ,-128)),
                new Star(new Vector3(109 ,5 ,-66)),
                new Star(new Vector3(-129 ,5 ,-196)),
           };

            //Bullets y Misiles 
            Missiles = new List<Missile>();

            EnvironmentMapRenderTarget = new RenderTargetCube(GraphicsDevice, 2048, false,
                SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            GraphicsDevice.BlendState = BlendState.Opaque;
            EnvironmentMapCamera = new StaticCamera(1f, MainCar.Position, Vector3.UnitX, Vector3.Up);
            EnvironmentMapCamera.BuildProjection(1f, 1f, 3000f, MathHelper.PiOver2);

            MainRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);
            BloomRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);
            FinalBloomRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None, 0,
                RenderTargetUsage.DiscardContents);

            FullScreenQuad = new FullScreenQuad(GraphicsDevice);

            base.Initialize();
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
        ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
        /// </summary>
        protected override void LoadContent()
        {
            HUD.LoadContent();

            backgroundMusic = Content.Load<Song>(ContentFolder3D + "HUD/SoundTrack");
            soundEffect = Content.Load<SoundEffect>(ContentFolder3D + "HUD/SoundEffect");
            GameOverSoundEffect = Content.Load<SoundEffect>(ContentFolderSoundEffects + "GameOverSoundEffect");
            SongWin = Content.Load<SoundEffect>(ContentFolderSoundEffects + "SongWin");
            SongOil = Content.Load<SoundEffect>(ContentFolderSoundEffects + "SongOil");

            MediaPlayer.Play(backgroundMusic);
            MediaPlayer.Volume = 0.2f;
            MediaPlayer.IsRepeating = true;

            Gizmos.LoadContent(GraphicsDevice, Content);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            //Load PowersUps
            Array.ForEach(PowerUps, powerUp => powerUp.LoadContent(Content));
            Array.ForEach(Stars, star => star.LoadContent(Content));
            Array.ForEach(HealthPacks, pack => pack.LoadContent(Content));

            // Cargo un efecto basico propio declarado en el Content pipeline.
            // En el juego no pueden usar BasicEffect de MG, deben usar siempre efectos propios.
            EffectTexture = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
            EffectNoTextures = Content.Load<Effect>(ContentFolderEffects + "BasicShaderNoTextures");
            Effect = EffectTexture;
            TilingEffect = Content.Load<Effect>(ContentFolderEffects + "TextureTiling");
            EnvironmentMapEffect = Content.Load<Effect>(ContentFolderEffects + "EnvironmentMap");
            GaussianBlurEffect = Content.Load<Effect>(ContentFolderEffects + "GaussianBlur");
            GaussianBlurEffect.Parameters["screenSize"]
                .SetValue(new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));
            IntegrateEffect = Content.Load<Effect>(ContentFolderEffects + "IntegrateEffects");

            Effect.Parameters["ambientColor"].SetValue(new Vector3(0.7f, 0.7f, 0.5f));
            Effect.Parameters["diffuseColor"].SetValue(new Vector3(0.4f, 0.5f, 0.6f));
            Effect.Parameters["specularColor"].SetValue(new Vector3(1f, 1f, 1f));
            Effect.Parameters["KAmbient"].SetValue(1.5f);
            Effect.Parameters["KDiffuse"].SetValue(1.3f);
            Effect.Parameters["KSpecular"].SetValue(0.25f);
            Effect.Parameters["shininess"].SetValue(10.0f);

            TilingEffect.Parameters["ambientColor"].SetValue(new Vector3(0.9f, 0.9f, 0.8f));
            TilingEffect.Parameters["diffuseColor"].SetValue(new Vector3(0.9f, 0.9f, 0.8f));
            TilingEffect.Parameters["specularColor"].SetValue(new Vector3(1f, 1f, 1f));
            TilingEffect.Parameters["KAmbient"].SetValue(0.5f);
            TilingEffect.Parameters["KDiffuse"].SetValue(0.5f);
            TilingEffect.Parameters["KSpecular"].SetValue(0f);
            TilingEffect.Parameters["shininess"].SetValue(0.01f);

            FloorTexture = Content.Load<Texture2D>(ContentFolderTextures + "FloorTexture");
            FloorNormalMap = Content.Load<Texture2D>(ContentFolderTextures + "FloorNormalMap");
            WallTexture = Content.Load<Texture2D>(ContentFolderTextures + "stoneTexture");
            WallNormalMap = Content.Load<Texture2D>(ContentFolderTextures + "WallNormalMap");

            MainCar.Load(EnvironmentMapEffect);

            GameModelList = new List<GameModel>();

            //Load Models posicion fija
            GameModelList.Add(new GameModel(Content.Load<Model>(ContentFolder3D + "tgcito-classic/tgcito-classic"), Effect, 0.1f, new Vector3(40f, 6.5f, 10f), Simulation));
            GameModelList.Add(new GameModel(Content.Load<Model>(ContentFolder3D + "Truck/Caterpillar_Truck"), Effect, 0.01f, new Vector3(10f, 0, 10f), Simulation));
            GameModelList.Add(new GameModel(Content.Load<Model>(ContentFolder3D + "trees/Tree4"), Effect, 0.02f, new Vector3(35f, 0f, 55f), Simulation));
            GameModelList.Add(new GameModel(Content.Load<Model>(ContentFolder3D + "Street/model/ElectronicBoxNew"), Effect, 0.01f, new Vector3(30, 0, 0), Simulation));
            GameModelList.Add(new GameModel(Content.Load<Model>(ContentFolder3D + "Street/model/old_water_tower"), Effect, 0.01f, new Vector3(50, 10, 50), Simulation));
            Gasoline = new GameModel(Content.Load<Model>(ContentFolder3D + "gasoline/gasoline"), Effect, 0.03f, new Vector3(3, 0, 0), Simulation);
            GameModelList.Add(Gasoline);
            GameModelList.Add(new GameModel(Content.Load<Model>(ContentFolder3D + "ramp/RampNew"), Effect, 1f, new Vector3(90, 0, 50), Simulation));
            GameModelList.Add(new GameModel(Content.Load<Model>(ContentFolder3D + "carDBZ/carDBZNew"), Effect, 0.05f, new Vector3(150f, 0, 50f), Simulation));
            //GameModelList.Add(new GameModel(Content.Load<Model>(ContentFolder3D + "car2/car2New"), Effect, 0.01f, new Vector3(100, 0, 20), Simulation));
            //GameModelList.Add(new GameModel(Content.Load<Model>(ContentFolder3D + "Street/model/WatercolorScene"), Effect, 0.01f, new Vector3(130, 0, 40), Simulation));
            //GameModelList.Add(new GameModel(Content.Load<Model>(ContentFolder3D + "Bushes/source/bush1"), Effect, 0.02f, new Vector3(25, 0, 25), Simulation));
            //GameModelList.Add(new GameModel(Content.Load<Model>(ContentFolder3D + "Street/model/House"), Effect, 0.01f, new Vector3(180f, 0, 80f), Simulation));
            //GameModelList.Add(new GameModel(Content.Load<Model>(ContentFolder3D + "Street/model/FencesNew"), Effect, 1f, new Vector3(-50, 0, 50), Simulation));

            //Load Models posicion variable
            Utils.AddModelRandomPositionWithY(Content.Load<Model>(ContentFolder3D + "Street/model/old_water_tower"), Effect, 0.01f, Simulation, 6, GameModelList, 10f);
            Utils.AddModelRandomPosition(Content.Load<Model>(ContentFolder3D + "Street/model/ElectronicBoxNew"), Effect, 0.01f, Simulation, 15, GameModelList);
            Gasolines = Utils.AddModelRandomPosition(Content.Load<Model>(ContentFolder3D + "gasoline/gasoline"), Effect, 0.03f, Simulation, 15, GameModelList);
            Utils.AddModelRandomPosition(Content.Load<Model>(ContentFolder3D + "Bushes/source/bush1"), Effect, 0.02f, Simulation, 30, GameModelList);

            Gasolines.Add(Gasoline);

            GameModels = GameModelList.ToArray();

            //Load Misiles y Bullets 
            MissileModel = Content.Load<Model>(ContentFolder3D + "PowerUps/Missile2");
            BulletModel = Content.Load<Model>(ContentFolder3D + "PowerUps/Bullet");

            BulletTexture = ((BasicEffect)BulletModel.Meshes.FirstOrDefault()?.MeshParts.FirstOrDefault()?.Effect)?.Texture;
            MissileTexture = ((BasicEffect)MissileModel.Meshes.FirstOrDefault()?.MeshParts.FirstOrDefault()?.Effect)?.Texture;

            LoadCarModelMenu();

            //Load enemy model
            EnemyModel = Content.Load<Model>(ContentFolder3D + "weapons/Vehicle");

            foreach (var mesh in EnemyModel.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    EnemyTexture.Add(((BasicEffect)meshPart.Effect)?.Texture);
                }
            }

            //Enemies
            Enemies = new List<Enemy>() {

                new Enemy(new Vector3(-50, 0, 50) , EnemyModel , Effect , Simulation , EnemyTexture)

            };

            //Load SoundEffects 
            MissileSound = Content.Load<SoundEffect>(ContentFolderSoundEffects + "MissileSoundeffect");
            MachineGunSound = Content.Load<SoundEffect>(ContentFolderSoundEffects + "MachineGunSoundEffect1Short");
            Claxon = Content.Load<SoundEffect>(ContentFolderSoundEffects + "Bocina");
            Explosion = Content.Load<SoundEffect>(ContentFolderSoundEffects + "ExplosionSoundEffect");

            // Add walls
            LoadWalls();

            base.LoadContent();
        }

        private void LoadWalls()
        {
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
        }

        private void LoadCarModelMenu()
        {
            CarModelMenu = Content.Load<Model>(ContentFolder3D + "CarsMenu/RacingCarMenu");

            foreach (var mesh in CarModelMenu.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    ModelTextures.Add(((BasicEffect)meshPart.Effect)?.Texture);
                }
            }
            leftBackWheelBone = CarModelMenu.Bones["WheelD"];
            rightBackWheelBone = CarModelMenu.Bones["WheelC"];
            leftFrontWheelBone = CarModelMenu.Bones["WheelA"];
            rightFrontWheelBone = CarModelMenu.Bones["WheelB"];

            leftBackWheelTransform = leftBackWheelBone.Transform;
            rightBackWheelTransform = rightBackWheelBone.Transform;
            leftFrontWheelTransform = leftFrontWheelBone.Transform;
            rightFrontWheelTransform = rightFrontWheelBone.Transform;
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            switch (gameState)
            {
                case ST_STAGE_1:
                    if (Keyboard.GetState().IsKeyDown(Keys.Space))
                    {
                        gameState = ST_STAGE_2;
                    }
                    break;
                case ST_STAGE_2:
                    mainGameUpdate(gameTime);
                    break;
            }

            // Capturar Input teclado
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                //Salgo del juego.
                Exit();
            }

            base.Update(gameTime);
        }

        public void mainGameUpdate(GameTime gameTime)
        {

            Vector3 forwardLocal = new Vector3(0, 0, -1);

            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Z) && CanShoot && MainCar.CanShoot && (MainCar.MachineGun || MainCar.MachineMissile))
            {
                CanShoot = false;

                if (MainCar.MachineMissile)
                {
                    MissileSound.Play();
                    Missiles.Add(new Missile(Simulation, MainCar, MissileModel, 0.008f));
                }
                else
                {
                    MachineGunSound.Play();
                    Missiles.Add(new Missile(Simulation, MainCar, BulletModel, 1f));
                }

            }

            if (keyboardState.IsKeyUp(Keys.Z) && MainCar.CanShoot && (MainCar.MachineMissile || MainCar.MachineGun))
                CanShoot = true;

            if (keyboardState.IsKeyDown(Keys.B))
                Claxon.Play();

            if (keyboardState.IsKeyDown(Keys.G) && !previousGKeyState)
            {
                activeGodMode = !activeGodMode;
            }
            previousGKeyState = keyboardState.IsKeyDown(Keys.G);

            if (activeGodMode)
            {
                MainCar.Health = 100;
                MainCar.Oil = 100;
            }

            CarSimulation.Update();

            Array.ForEach(PowerUps, PowerUp => PowerUp.Update());
            Array.ForEach(Stars, star => star.Update());
            Array.ForEach(HealthPacks, pack => pack.Update());

            // Actualizar estado del auto
            MainCar.Update(Keyboard.GetState(), gameTime, Simulation);

            //Restart del auto vuelve al inicio
            if (keyboardState.IsKeyDown(Keys.R))
                MainCar.Restart(new NumericVector3(0f, 10f, 0f), Simulation);

            //Verifico cuando el auto agarre un PowerUp
            Array.ForEach(PowerUps, PowerUp => PowerUp.ActivateIfBounding(MainCar.CarBox, MainCar));
            Array.ForEach(HealthPacks, pack => pack.ActivateIfBounding(MainCar.CarBox, MainCar));
            Array.ForEach(Stars, star => star.ActivateIfBounding(MainCar.CarBox, MainCar));

            // Actualizo la camara, enviandole la matriz de mundo del auto.
            FollowCamera.Update(gameTime, MainCar.World);
            EnvironmentMapCamera.Position = MainCar.Position + new Vector3(0, 1f, 0);

            //Actualizo BoundingFrustum
            BoundingFrustum.Matrix = FollowCamera.View * FollowCamera.Projection;

            var forwardDirection = NumericVector3.Transform(new NumericVector3(0, 0, -1), MainCar.Pose.Orientation);
            Effect.Parameters["eyePosition"]?.SetValue(FollowCamera.Position);
            Effect.Parameters["forwardDir"].SetValue(forwardDirection);
            Effect.Parameters["lightPosition"].SetValue(MainCar.Position + 3 * forwardDirection);
            TilingEffect.Parameters["forwardDir"].SetValue(forwardDirection);
            TilingEffect.Parameters["lightPosition"].SetValue(MainCar.Position + 3 * forwardDirection + new Vector3(0, 10f, 0));
            TilingEffect.Parameters["eyePosition"]?.SetValue(FollowCamera.Position);
            EnvironmentMapEffect.Parameters["eyePosition"]?.SetValue(FollowCamera.Position);

            var quaternionCar = MainCar.quaternion;

            var missilesToDelete = new List<Missile>();
            foreach (Missile missile in Missiles)
            {
                missile.update(Simulation, quaternionCar);
                if (missile.deleteFlag)
                {
                    missilesToDelete.Add(missile);
                }
            }
            foreach (Missile missileToDelete in missilesToDelete)
            {
                Missiles.Remove(missileToDelete);

                Explosion.Play();
            }



            Array.ForEach(GameModels, GameModel =>
            {
                if (MainCar.CarBox.Intersects(GameModel.BoundingBox))
                    GameModel.Touch = true;
                else
                    GameModel.Touch = false;

            });
            Array.ForEach(PowerUps, PowerUp =>
            {
                if (MainCar.CarBox.Intersects(PowerUp.BoundingSphere))
                    PowerUp.Touch = true;
                else
                    PowerUp.Touch = false;

            });

            foreach (var Enemy in Enemies)
                Enemy.Update(MainCar, gameTime, Simulation);

            foreach (var Enemy in Enemies)
                if (MainCar.CarBox.Intersects(Enemy.EnemyOBB) && !activeGodMode)
                    MainCar.Health -= 0.2f;
            
            //Siempre resto nafta
            MainCar.Oil -= 1 * (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (var oilBox in Gasolines)
                if (MainCar.CarBox.Intersects(oilBox.BoundingBox))
                    MainCar.Oil += 0.2f;

            foreach (var Enemy in Enemies)
            {
                foreach (Missile missile in Missiles)
                {
                    if (Enemy.EnemyOBB.Intersects(missile.OBBox))
                    {
                        addEnemy = true;
                        count++;
                    }
                }
            }

            //Pegandole a los enemigos aparecen mas 
            if (addEnemy && count > 110)
            {
                Enemies.Add(new Enemy(new Vector3(100, 0, 100), EnemyModel, Effect, Simulation, EnemyTexture));
                addEnemy = false;
                count = 0;
            }

            MainCar.Oil = MathHelper.Clamp(MainCar.Oil, 0, 100);
            MainCar.Health = MathHelper.Clamp(MainCar.Health, 0, 100);

            //Condiciones de GAME OVER 
            if (MainCar.Oil == 0 || MainCar.Health == 0 || MainCar.Stars == 10 || HUD.Seconds >= 150)
                gameState = ST_GAME_OVER;

            //Elijo sound dependiendo la condicion 
            if (MainCar.Oil == 0 || MainCar.Health == 0)
                ChooseSoundEffect = GameOverSoundEffect;
            else if (MainCar.Stars == 10)
                ChooseSoundEffect = SongWin;
            else if (HUD.Seconds >= 150)
                ChooseSoundEffect = GameOverSoundEffect;

            HUD.Update(gameTime, MainCar.Health, MainCar.Oil, MainCar.Stars);

            //Gizmos.UpdateViewProjection(FollowCamera.View, FollowCamera.Projection);
        }

        public void DrawCarsInMenu(Matrix view, Matrix projection, Effect effect, Model modelo, Matrix matrizMundo, float time)
        {
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            relativeMatrices = new Matrix[CarModelMenu.Bones.Count];

            int index = 0;
            foreach (var mesh in CarModelMenu.Meshes)
            {
                rightFrontWheelBone.Transform = Matrix.CreateRotationX(time) * Matrix.CreateRotationY(0) * rightFrontWheelTransform;
                leftFrontWheelBone.Transform = Matrix.CreateRotationX(time) * Matrix.CreateRotationY(0) * leftFrontWheelTransform;
                leftBackWheelBone.Transform = Matrix.CreateRotationX(time) * leftBackWheelTransform;
                rightBackWheelBone.Transform = Matrix.CreateRotationX(time) * rightBackWheelTransform;
                CarModelMenu.CopyAbsoluteBoneTransformsTo(relativeMatrices);

                effect.Parameters["World"].SetValue(relativeMatrices[mesh.ParentBone.Index] * matrizMundo);

                foreach (var meshPart in mesh.MeshParts)
                {
                    effect.GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                    effect.GraphicsDevice.Indices = meshPart.IndexBuffer;
                    effect.Parameters["ModelTexture"]?.SetValue(ModelTextures[index]);
                    meshPart.Effect = effect;

                }
                mesh.Draw();
                index++;
            }
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            time += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            View = Matrix.CreateLookAt(posicionCamara, Vector3.Zero, Vector3.Up);
            Projection = Matrix.CreateOrthographic(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, -80, 1000);

            switch (gameState)
            {
                case ST_STAGE_1:
                    var Time = (float)gameTime.TotalGameTime.TotalSeconds;
                    GraphicsDevice.Clear(Color.LightYellow);
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    HUD.DrawMenu(gameTime);
                    posAutoMenu += time;
                    DrawCarsInMenu(View, Projection, Effect, CarModel, Matrix.CreateScale(0.32f) * Matrix.CreateTranslation(new Vector3(-250, -100, posAutoMenu)), time);
                    DrawCarsInMenu(View, Projection, Effect, CarModel, Matrix.CreateScale(0.32f) * Matrix.CreateTranslation(new Vector3(-50, -100, posAutoMenu)), time);

                    break;

                case ST_STAGE_2:
                    GraphicsDevice.Clear(Color.Black);

                    // Environment map passes
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    
                    for (var face = CubeMapFace.PositiveX; face <= CubeMapFace.NegativeZ; face++)
                    {
                        GraphicsDevice.SetRenderTarget(EnvironmentMapRenderTarget, face);
                        GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);

                        SetCubemapCameraForOrientation(face);
                        EnvironmentMapCamera.BuildView();

                        Effect.Parameters["View"].SetValue(EnvironmentMapCamera.View);
                        Effect.Parameters["Projection"].SetValue(EnvironmentMapCamera.Projection);
                        
                        DrawMainScene(gameTime);
                    }

                    GraphicsDevice.SetRenderTarget(MainRenderTarget);
                    GraphicsDevice.Clear(Color.Black);

                    EffectTexture.Parameters["View"].SetValue(FollowCamera.View);
                    EffectTexture.Parameters["Projection"].SetValue(FollowCamera.Projection);

                    DrawMainScene(gameTime);

                    // Draw the car
                    EnvironmentMapEffect.CurrentTechnique = EnvironmentMapEffect.Techniques["EnvironmentMap"];
                    EnvironmentMapEffect.Parameters["environmentMap"].SetValue(EnvironmentMapRenderTarget);
                    MainCar.Draw(FollowCamera.View, FollowCamera.Projection);

                    // Start bloom pass (for stars)
                    GraphicsDevice.SetRenderTarget(BloomRenderTarget);
                    GraphicsDevice.Clear(Color.Black);

                    EffectNoTextures.Parameters["View"].SetValue(FollowCamera.View);
                    EffectNoTextures.Parameters["Projection"].SetValue(FollowCamera.Projection);
                    Effect = EffectNoTextures;

                    Array.ForEach(GameModels, GameModel => {
                        GameModel.Effect = EffectNoTextures;
                        GameModel.Draw(GameModel.Model, GameModel.World, FollowCamera, BoundingFrustum, GameModel.BoundingBox);
                        GameModel.Effect = EffectTexture;
                    });
                    EnvironmentMapEffect.CurrentTechnique = EnvironmentMapEffect.Techniques["NoColor"];
                    MainCar.Draw(FollowCamera.View, FollowCamera.Projection);

                    Array.ForEach(Stars, star => star.Draw(FollowCamera, gameTime, BoundingFrustum, star.BoundingSphere));

                    Effect = EffectTexture; // Devolver al effect que corresponde

                    // Start blur passes
                    var passCount = 2;
                    var bloomTexture = BloomRenderTarget;
                    var finalTarget = FinalBloomRenderTarget;
                    for (int i = 0; i < passCount; i++) {
                        GraphicsDevice.SetRenderTarget(finalTarget);
                        GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

                        GaussianBlurEffect.Parameters["baseTexture"].SetValue(bloomTexture);
                        FullScreenQuad.Draw(GaussianBlurEffect);

                        if (i != passCount - 1)
                        {
                            var aux = bloomTexture;
                            bloomTexture = finalTarget;
                            finalTarget = aux;
                        }
                    }

                    // Integrate textures
                    GraphicsDevice.DepthStencilState = DepthStencilState.None;

                    GraphicsDevice.SetRenderTarget(null);
                    GraphicsDevice.Clear(Color.Black);

                    IntegrateEffect.Parameters["baseTexture"].SetValue(MainRenderTarget);
                    IntegrateEffect.Parameters["bloomTexture"].SetValue(finalTarget);
                    FullScreenQuad.Draw(IntegrateEffect);

                    HUD.DrawInGameHUD(gameTime);

                    break;

                case ST_GAME_OVER:
                    MediaPlayer.Stop();

                    GraphicsDevice.Clear(Color.Black);

                    // Posición fija de la cámara para ver el auto entrando
                    Vector3 posicionCamara = new Vector3(-100, 0, -800);  // Cámara más cerca y más baja
                    Vector3 posicionCamara2 = new Vector3(100, 0, -800);  // Cámara más cerca y más baja
                    Matrix view = Matrix.CreateLookAt(posicionCamara, new Vector3(0, 0, 0), Vector3.Up);
                    Matrix view2 = Matrix.CreateLookAt(posicionCamara2, new Vector3(0, 0, 0), Vector3.Up);

                    // Proyección de la cámara
                    Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 400f, 2100f);

                    // Actualiza la posición del auto en el eje Z
                    posAutoMenu += 3;  // Factor de velocidad ajustado para mejor visibilidad

                    // Matriz de rotación y transformación del auto
                    Matrix rotationCar = Matrix.Identity;  // Puedes añadir rotaciones si lo deseas
                    Matrix world = Matrix.CreateScale(0.32f) * rotationCar * Matrix.CreateTranslation(new Vector3(250, -200, posAutoMenu));
                    Matrix world2 = Matrix.CreateScale(0.32f) * rotationCar * Matrix.CreateTranslation(new Vector3(-150, -200, posAutoMenu));

                    // Mostrar el HUD de Game Over
                    HUD.GameOver(ChooseSoundEffect);

                    // Dibuja el auto
                    DrawCarsInMenu(view, projection, Effect, CarModel, world, time);
                    DrawCarsInMenu(view2, projection, Effect, CarModel, world2, time);



                    break;
            }

            base.Draw(gameTime);

        }

        private void DrawMainScene(GameTime gameTime)
        {
            Array.ForEach(GameModels, GameModel => GameModel.Draw(GameModel.Model, GameModel.World, FollowCamera, BoundingFrustum, GameModel.BoundingBox));
            Array.ForEach(PowerUps, PowerUp => PowerUp.Draw(FollowCamera, gameTime, BoundingFrustum, PowerUp.BoundingSphere));
            Array.ForEach(Stars, star => star.Draw(FollowCamera, gameTime, BoundingFrustum, star.BoundingSphere));
            Array.ForEach(HealthPacks, pack => pack.Draw(FollowCamera, gameTime, BoundingFrustum, pack.BoundingSphere));

            if (MainCar.MachineMissile)
            {
                var missileWorlds = new List<Matrix>();
                foreach (Missile missile in Missiles)
                {
                    missileWorlds.Add(missile.World);
                    missile.Draw(missile.World, MissileModel, MissileTexture, FollowCamera, gameTime, Effect);
                    //Gizmos.DrawCube(missile.OBBWorld, Color.DarkBlue);
                }

            }
            else
            {
                var missileWorlds = new List<Matrix>();
                foreach (Missile missile in Missiles)
                {
                    missileWorlds.Add(missile.World);
                    missile.Draw(Matrix.CreateRotationY(MathHelper.PiOver2) * missile.World, BulletModel, BulletTexture, FollowCamera, gameTime, Effect);
                    //Gizmos.DrawCube(Matrix.CreateRotationY(MathHelper.PiOver2) * missile.OBBWorld, Color.DarkBlue);
                }

            }
            foreach (var Enemy in Enemies)
                Enemy.Draw(FollowCamera, gameTime);

            DrawFloor(FloorQuad);
            DrawWalls();


            #region DrawGizmos
            //Array.ForEach(PowerUps, PowerUp =>
            //{
            //    var r = PowerUp.BoundingSphere.Radius;
            //    if (PowerUp.Touch)
            //        Gizmos.DrawSphere(PowerUp.BoundingSphere.Center, new Vector3(r, r, r), Color.CornflowerBlue);
            //    else
            //        Gizmos.DrawSphere(PowerUp.BoundingSphere.Center, new Vector3(r, r, r), Color.Red);

            //});
            //Array.ForEach(Stars, PowerUp =>
            //{
            //    var r = PowerUp.BoundingSphere.Radius;
            //    if (PowerUp.Touch)
            //        Gizmos.DrawSphere(PowerUp.BoundingSphere.Center, new Vector3(r, r, r), Color.CornflowerBlue);
            //    else
            //        Gizmos.DrawSphere(PowerUp.BoundingSphere.Center, new Vector3(r, r, r), Color.Red);

            //});
            //Array.ForEach(HealthPacks, PowerUp =>
            //{
            //    var r = PowerUp.BoundingSphere.Radius;
            //    if (PowerUp.Touch)
            //        Gizmos.DrawSphere(PowerUp.BoundingSphere.Center, new Vector3(r, r, r), Color.CornflowerBlue);
            //    else
            //        Gizmos.DrawSphere(PowerUp.BoundingSphere.Center, new Vector3(r, r, r), Color.Red);

            //});

            //Array.ForEach(GameModels, GameModel =>
            //{
            //    if (GameModel.Touch)
            //        Gizmos.DrawCube((GameModel.BoundingBox.Max + GameModel.BoundingBox.Min) / 2f, GameModel.BoundingBox.Max - GameModel.BoundingBox.Min, Color.CornflowerBlue);
            //    else
            //        Gizmos.DrawCube((GameModel.BoundingBox.Max + GameModel.BoundingBox.Min) / 2f, GameModel.BoundingBox.Max - GameModel.BoundingBox.Min, Color.Red);
            //});


            //Gizmos.DrawCube(CarOBBWorld, Color.Red);


            //foreach (var Enemy in Enemies)
            //    Gizmos.DrawCube(Enemy.EnemyOBBWorld, Color.LightGoldenrodYellow);

            //Gizmos.Draw();
            #endregion


        }

        private void DrawFloor(QuadPrimitive geometry)
        {
            // EffectNoTextures.Parameters["DiffuseColor"].SetValue(Color.DarkSeaGreen.ToVector3());
            TilingEffect.CurrentTechnique = TilingEffect.Techniques["BaseTilingWithLights"];
            var world = FloorWorld * Matrix.CreateTranslation(0f, -0.1f, 0f);
            var inverseTransposeWorld = Matrix.Transpose(Matrix.Invert(world));
            TilingEffect.Parameters["World"].SetValue(world);
            TilingEffect.Parameters["WorldViewProjection"].SetValue(world * FollowCamera.View * FollowCamera.Projection);
            TilingEffect.Parameters["Tiling"].SetValue(new Vector2(350f, 350f));
            TilingEffect.Parameters["Texture"].SetValue(FloorTexture);
            TilingEffect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Identity);
            TilingEffect.Parameters["NormalMap"].SetValue(FloorNormalMap);
            geometry.Draw(TilingEffect);

        }

        private void DrawWalls()
        {
            TilingEffect.CurrentTechnique = TilingEffect.Techniques["BaseTilingWithLights"];
            var prim = new QuadPrimitive(GraphicsDevice);
            foreach (var wall in WallWorlds)
            {
                var quadCorrection1 = Matrix.CreateRotationZ(MathHelper.ToRadians(-90)) * Matrix.CreateScale(1f, 10f, 200f);
                var quadCorrection2 = Matrix.CreateRotationZ(MathHelper.ToRadians(90)) * Matrix.CreateScale(1f, 10f, 200f);
                var world = quadCorrection1 * wall;
                var inverseTransposeWorld = Matrix.Transpose(Matrix.Invert(world));
                TilingEffect.Parameters["World"].SetValue(world);
                TilingEffect.Parameters["Tiling"].SetValue(new Vector2(3f, 30f));
                TilingEffect.Parameters["Texture"].SetValue(WallTexture);
                TilingEffect.Parameters["NormalMap"].SetValue(WallNormalMap);
                TilingEffect.Parameters["InverseTransposeWorld"].SetValue(inverseTransposeWorld);
                TilingEffect.Parameters["WorldViewProjection"].SetValue(
                    world * FollowCamera.View * FollowCamera.Projection
                    );
                prim.Draw(TilingEffect);
                world = quadCorrection2 * wall;
                inverseTransposeWorld = Matrix.Transpose(Matrix.Invert(world));
                TilingEffect.Parameters["World"].SetValue(world);
                TilingEffect.Parameters["InverseTransposeWorld"].SetValue(inverseTransposeWorld);
                TilingEffect.Parameters["WorldViewProjection"].SetValue(
                    world * FollowCamera.View * FollowCamera.Projection
                    );
                prim.Draw(TilingEffect);
            }
        }

        private void SetCubemapCameraForOrientation(CubeMapFace face)
        {
            switch (face)
            {
                default:
                case CubeMapFace.PositiveX:
                    EnvironmentMapCamera.FrontDirection = -Vector3.UnitX;
                    EnvironmentMapCamera.UpDirection = Vector3.Down;
                    break;

                case CubeMapFace.NegativeX:
                    EnvironmentMapCamera.FrontDirection = Vector3.UnitX;
                    EnvironmentMapCamera.UpDirection = Vector3.Down;
                    break;

                case CubeMapFace.PositiveY:
                    EnvironmentMapCamera.FrontDirection = Vector3.Down;
                    EnvironmentMapCamera.UpDirection = Vector3.UnitZ;
                    break;

                case CubeMapFace.NegativeY:
                    EnvironmentMapCamera.FrontDirection = Vector3.Up;
                    EnvironmentMapCamera.UpDirection = -Vector3.UnitZ;
                    break;

                case CubeMapFace.PositiveZ:
                    EnvironmentMapCamera.FrontDirection = -Vector3.UnitZ;
                    EnvironmentMapCamera.UpDirection = Vector3.Down;
                    break;

                case CubeMapFace.NegativeZ:
                    EnvironmentMapCamera.FrontDirection = Vector3.UnitZ;
                    EnvironmentMapCamera.UpDirection = Vector3.Down;
                    break;
            }
        }

        /// <summary>
        ///     Libero los recursos que se cargaron en el juego.
        /// </summary>
        protected override void UnloadContent()
        {
            // Libero los recursos.
            Content.Unload();

            Simulation.Dispose();

            Gizmos.Dispose();

            base.UnloadContent();
        }
    }
}