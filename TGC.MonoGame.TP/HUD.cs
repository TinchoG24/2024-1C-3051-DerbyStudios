using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.MonoGame.TP;

namespace TGC.MonoTP
{
    public class HUD
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";

        //Sprites
        private SpriteFont SpriteFont {  get; set; }
        public SpriteFont SpriteFontPlus { get; private set; }
        private SpriteBatch SpriteBatch {  get; set; }

        //Textures
        private Texture2D HudTexture { get; set; }
        private Texture2D GameOverTexture { get; set; }
        private Texture2D HealthBarTexture {  get; set; }
        private Texture2D OilBarTexture { get; set; }


        public int Stars { get; private set; }
        private Vector2 Position { get; set; }
        private float Scale { get; set; }
        public int Seconds { get; private set; }


        float currentHealth = 100f;
        float maxHealth = 100f;
        float currentOil = 100f;
        float maxOil = 100f;


        private GraphicsDevice GraphicsDevice;
        private ContentManager Content;
        private bool activeSound = false;

        public HUD(ContentManager content, GraphicsDevice graphicsDevice)
        {
            this.Content = content;
            this.GraphicsDevice = graphicsDevice;
            HealthBarTexture = CreateRectangleTexture(GraphicsDevice, 200, 20, Color.Red);
            OilBarTexture = CreateRectangleTexture(GraphicsDevice, 200, 20, Color.Blue);

        }

        public void Initialize()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
        }

        public void LoadContent()
        {
            SpriteFont = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "TechnoRaceItalic");
            SpriteFontPlus = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "TechnoRaceItalicPlus");
            HudTexture = Content.Load<Texture2D>(ContentFolder3D + "HUD/Pista");
            GameOverTexture = Content.Load<Texture2D>(ContentFolder3D + "HUD/Pista2");
        }
        
        private void PrepareHud(Texture2D texture2D)
        {
            float screenWidth = GraphicsDevice.Viewport.Width;
            float screenHeight = GraphicsDevice.Viewport.Height;
            float imageWidth = texture2D.Width;
            float imageHeight = texture2D.Height;

            float screenAspectRatio = screenWidth / screenHeight;
            float imageAspectRatio = imageWidth / imageHeight;

            if (screenAspectRatio > imageAspectRatio)
            {
                // Screen is wider than the image
                Scale = screenHeight / imageHeight;
            }
            else
            {
                // Screen is taller than the image
                Scale = screenWidth / imageWidth;
            }

            // Calculate the position to center the image
            float scaledWidth = imageWidth * Scale;
            float scaledHeight = imageHeight * Scale;
            Position = new Vector2(
                (screenWidth - scaledWidth) / 2,
                (screenHeight - scaledHeight) / 2
            );
        }
        
        public void Update(GameTime gameTime, float health, float oil, int stars)
        {
            this.currentHealth = health;
            this.currentOil = oil;
            this.Stars = stars;
            currentHealth = MathHelper.Clamp(currentHealth, 0, maxHealth);
            currentOil = MathHelper.Clamp(currentOil, 0, maxOil);
        }

        public void DrawMenu(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            PrepareHud(HudTexture);
            SpriteBatch.Begin();
            SpriteBatch.Draw(HudTexture, Position, null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
            SpriteBatch.End();

            DrawCenterTextY("DERBY GAMES", 100, 1, Color.Black, SpriteFontPlus);
            DrawCenterTextY("CONTROLES -  WASD", 250, 1, Color.Black, SpriteFont);
            DrawCenterTextY("SALTO - SPACE", 300, 1, Color.Black, SpriteFont);
            DrawCenterTextY("SHOOT - Z", 350, 1, Color.Black, SpriteFont);
            DrawCenterTextY("CLAXON - B", 400, 1, Color.Black, SpriteFont);
            DrawCenterTextY("RESTART - R", 450, 1, Color.Black, SpriteFont);
            DrawCenterTextY("GOD MODE  -  G", 500, 1, Color.Black, SpriteFont);
            DrawCenterTextY("Presione SPACE para comenzar...", 600, 1, Color.Black , SpriteFont);
        }

        public void DrawInGameHUD(GameTime gameTime , float FPS )
        {

            SpriteBatch.Begin();

            SpriteBatch.DrawString(SpriteFont, "HEALTH", new Vector2(10, 10), Color.YellowGreen);
            DrawHealthBar(new Vector2(120, 15), currentHealth / maxHealth);
            SpriteBatch.DrawString(SpriteFont, currentHealth.ToString("F1", CultureInfo.InvariantCulture) + "%", new Vector2(120, 10), Color.Black);

            SpriteBatch.DrawString(SpriteFont, "O I L", new Vector2(10, 40), Color.YellowGreen);
            DrawOilBar(new Vector2(120, 50), currentOil / maxOil);
            SpriteBatch.DrawString(SpriteFont, currentOil.ToString("F1", CultureInfo.InvariantCulture) + "%", new Vector2(120, 40), Color.Black);

            SpriteBatch.End();

            DrawCenterTextY("Stars: " + Stars.ToString() + "/ 10", 25f, 1, Color.YellowGreen , SpriteFont);

            Seconds = Convert.ToInt32(Math.Floor(gameTime.TotalGameTime.TotalSeconds));

            DrawRightText("Tiempo: " + Seconds.ToString(), 25f, 1);
            DrawRightText("FPS: " + FPS.ToString("F1", CultureInfo.InvariantCulture), 55f, 1);
        }

        public void GameOver(SoundEffect soundEffect)
        {
            //DrawCenterText("GAME OVER", 1 , SpriteFontPlus);
            PrepareHud(GameOverTexture);
            SpriteBatch.Begin();
            SpriteBatch.Draw(GameOverTexture, Position, null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
            SpriteBatch.End();

            if (!activeSound)
            {
                soundEffect.Play();
                activeSound = true;
            }

            if (currentHealth == 0 || currentOil == 0)
                DrawCenterTextY("GAME OVER", 520f, 1 , Color.YellowGreen , SpriteFontPlus);

            if (Stars == 10)
                DrawCenterTextY("V I C TO R Y", 520f, 1, Color.YellowGreen, SpriteFontPlus);

            if (Seconds == 150)
                DrawCenterTextY(" T I M E ", 520f, 1, Color.YellowGreen, SpriteFontPlus);

        }

        public void DrawCenterText(string msg, float escala , SpriteFont SpriteFont)
        {
            var W = GraphicsDevice.Viewport.Width;
            var H = GraphicsDevice.Viewport.Height;
            var size = SpriteFont.MeasureString(msg) * escala;
            SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, DepthStencilState.Default, null, null,
                Matrix.CreateScale(escala) * Matrix.CreateTranslation((W - size.X) / 2, (H - size.Y) / 2, 0));
            SpriteBatch.DrawString(SpriteFont, msg, new Vector2(0, 0), Color.YellowGreen);
            SpriteBatch.End();
        }
        
        public void DrawCenterTextY(string msg, float Y, float escala, Color color, SpriteFont SpriteFont)
        {
            var W = GraphicsDevice.Viewport.Width;
            var H = GraphicsDevice.Viewport.Height;
            var size = SpriteFont.MeasureString(msg) * escala;
            SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, DepthStencilState.Default, null, null,
                Matrix.CreateScale(escala) * Matrix.CreateTranslation((W - size.X) / 2, Y, 0));
            SpriteBatch.DrawString(SpriteFont, msg, new Vector2(0, 0), color);
            SpriteBatch.End();
        }

        public void DrawRightText(string msg, float Y, float escala)
        {
            var W = GraphicsDevice.Viewport.Width;
            var H = GraphicsDevice.Viewport.Height;
            var size = SpriteFont.MeasureString(msg) * escala;
            SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, DepthStencilState.Default, null, null,
                Matrix.CreateScale(escala) * Matrix.CreateTranslation(W - size.X - 20, Y, 0));
            SpriteBatch.DrawString(SpriteFont, msg, new Vector2(0, 0), Color.YellowGreen);
            SpriteBatch.End();
        }
        
        private void DrawHealthBar(Vector2 position, float healthPercentage)
        {
            int barWidth = (int)(HealthBarTexture.Width * healthPercentage);
            Rectangle sourceRectangle = new Rectangle(0, 0, barWidth, HealthBarTexture.Height);
            Rectangle destinationRectangle = new Rectangle((int)position.X, (int)position.Y, barWidth, HealthBarTexture.Height);

            SpriteBatch.Draw(HealthBarTexture, destinationRectangle, sourceRectangle, Color.White);
        }
        
        private void DrawOilBar(Vector2 position, float OilPorcentage)
        {
            int barWidth = (int)(OilBarTexture.Width * OilPorcentage);
            Rectangle sourceRectangle = new Rectangle(0, 0, barWidth, OilBarTexture.Height);
            Rectangle destinationRectangle = new Rectangle((int)position.X, (int)position.Y, barWidth, OilBarTexture.Height);

            SpriteBatch.Draw(OilBarTexture, destinationRectangle, sourceRectangle, Color.White);
        }

        private Texture2D CreateRectangleTexture(GraphicsDevice graphicsDevice, int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            Color[] colorData = new Color[width * height];

            for (int i = 0; i < colorData.Length; i++)
                colorData[i] = color;

            texture.SetData(colorData);
            return texture;
        }

        public void UnloadContent()
        {
        }
    }

}
