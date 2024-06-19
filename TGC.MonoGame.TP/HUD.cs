using Microsoft.Xna.Framework;
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

        private SpriteFont SpriteFont;
        private SpriteBatch SpriteBatch;
        private Texture2D texture;

        Texture2D healthBarTexture;
        float currentHealth = 100f;
        float maxHealth = 100f;

        Texture2D OilBarTexture;
        float currentOil = 100f;

        public int Stars { get; private set; }

        float maxOil = 100f;

        private Vector2 position { get; set; }
        private Texture2D hudImage { get; set; }
        private float scale { get; set; }
        private GraphicsDevice GraphicsDevice;
        private ContentManager Content;

        public HUD(ContentManager content, GraphicsDevice graphicsDevice)
        {
            this.Content = content;
            this.GraphicsDevice = graphicsDevice;
            healthBarTexture = CreateRectangleTexture(GraphicsDevice, 200, 20, Color.Red);
            OilBarTexture = CreateRectangleTexture(GraphicsDevice, 200, 20, Color.Blue);

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
            //GraphicsDevice.Clear(Color.Black);
            //SpriteBatch.Begin();
            //SpriteBatch.DrawString(SpriteFont, "DERBY GAMES", new Vector2(300, 500), Color.White);
            //SpriteBatch.End();
            GraphicsDevice.Clear(Color.Black);
            PrepareHud();
            SpriteBatch.Begin();
            SpriteBatch.Draw(hudImage, position, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            SpriteBatch.End();
            DrawCenterTextY("DERBY GAMES", 100, 5, Color.Black);
            DrawCenterTextY("CONTROLES -  WASD", 250, 1, Color.Black);
            DrawCenterTextY("SALTO - SPACE", 300, 1, Color.Black);
            DrawCenterTextY("SHOOT - Z", 350, 1, Color.Black);
            DrawCenterTextY("CLAXON - B", 400, 1, Color.Black);
            DrawCenterTextY("RESTART - R", 450, 1, Color.Black);
            DrawCenterTextY("GOD MODE  -  G", 500, 1, Color.Black);
            DrawCenterTextY("Presione SPACE para comenzar...", 600, 1, Color.Black);
        }

        public void DrawInGameHUD(GameTime gameTime)
        {

            SpriteBatch.Begin();

            SpriteBatch.DrawString(SpriteFont, "HEALTH", new Vector2(10, 10), Color.YellowGreen);
            DrawHealthBar(new Vector2(150, 10), currentHealth / maxHealth);
            SpriteBatch.DrawString(SpriteFont, currentHealth.ToString("F1", CultureInfo.InvariantCulture) + "%", new Vector2(160, 10), Color.Black);

            SpriteBatch.DrawString(SpriteFont, "O I L", new Vector2(10, 30), Color.YellowGreen);
            DrawOilBar(new Vector2(150, 30), currentOil / maxOil);
            SpriteBatch.DrawString(SpriteFont, currentOil.ToString("F1", CultureInfo.InvariantCulture) + "%", new Vector2(160, 30), Color.Black);

            SpriteBatch.End();

            DrawCenterTextY("Stars: " + Stars.ToString() + "/ 10", 25f, 1, Color.YellowGreen);

            if (currentHealth == 0)
                DrawCenterText(" L O S E ", 5f);

            if (Stars == 3)
                DrawCenterText(" W I N ", 5f);



            var secs = Convert.ToInt32(Math.Floor(gameTime.TotalGameTime.TotalSeconds));

            DrawRightText("Tiempo: " + secs.ToString(), 25f, 1);
        }

        private void PrepareHud()
        {
            float screenWidth = GraphicsDevice.Viewport.Width;
            float screenHeight = GraphicsDevice.Viewport.Height;
            float imageWidth = hudImage.Width;
            float imageHeight = hudImage.Height;

            float screenAspectRatio = screenWidth / screenHeight;
            float imageAspectRatio = imageWidth / imageHeight;

            if (screenAspectRatio > imageAspectRatio)
            {
                // Screen is wider than the image
                scale = screenHeight / imageHeight;
            }
            else
            {
                // Screen is taller than the image
                scale = screenWidth / imageWidth;
            }

            // Calculate the position to center the image
            float scaledWidth = imageWidth * scale;
            float scaledHeight = imageHeight * scale;
            position = new Vector2(
                (screenWidth - scaledWidth) / 2,
                (screenHeight - scaledHeight) / 2
            );
        }

        public void Initialize()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
        }

        public void LoadContent()
        {
            SpriteFont = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CascadiaCodePL");
            SpriteFont = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CarCrash");
            hudImage = Content.Load<Texture2D>(ContentFolder3D + "HUD/HUD1");

        }

        public void UnloadContent()
        {
        }

        public void DrawCenterText(string msg, float escala)
        {
            var W = GraphicsDevice.Viewport.Width;
            var H = GraphicsDevice.Viewport.Height;
            var size = SpriteFont.MeasureString(msg) * escala;
            SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, DepthStencilState.Default, null, null,
                Matrix.CreateScale(escala) * Matrix.CreateTranslation((W - size.X) / 2, (H - size.Y) / 2, 0));
            SpriteBatch.DrawString(SpriteFont, msg, new Vector2(0, 0), Color.YellowGreen);
            SpriteBatch.End();
        }

        public void DrawCenterTextY(string msg, float Y, float escala, Color color)
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
            int barWidth = (int)(healthBarTexture.Width * healthPercentage);
            Rectangle sourceRectangle = new Rectangle(0, 0, barWidth, healthBarTexture.Height);
            Rectangle destinationRectangle = new Rectangle((int)position.X, (int)position.Y, barWidth, healthBarTexture.Height);

            SpriteBatch.Draw(healthBarTexture, destinationRectangle, sourceRectangle, Color.White);
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

        public void GameOver()
        {
            DrawCenterText("GAME OVER", 5);

        }
    }

}
