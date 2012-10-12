using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

/*To Do List;
 * Use microeconomic simulation to make regional statistics
 * Progressively simulate from small people to communities to towns to nations to worlds
 */
namespace MicroeconomicSimulator
{
    public class MyGame : Microsoft.Xna.Framework.Game
    {
        public static GraphicsDeviceManager graphics;
        public static SpriteBatch spriteBatch;
        public static SpriteFont Kootenay16;
        public static Texture2D BlankBox;
        public static Effect OrdinaryEffect;
        public static Rectangle GameWindow;
        public static ContentManager content;
        public static Random random;
        public static MouseState PreviousMouse;

        Economy.Manager economicManager;
        Economy.DropDownMenu DataChooser;
        Economy.QueueButton UpdateButton;

        public MyGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            content = this.Content;
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            GameWindow = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            graphics.IsFullScreen = false;  //For debug purposes, full screen is off
            graphics.ApplyChanges();
            Window.AllowUserResizing = false;
            IsMouseVisible = true;

            random = new Random();
            PreviousMouse = Mouse.GetState();

            economicManager = new Economy.Manager();
            ThreadPool.QueueUserWorkItem(new WaitCallback(economicManager.Update));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            Kootenay16 = content.Load<SpriteFont>("Kootenay16");
            BlankBox = content.Load<Texture2D>("BlankBox");
            OrdinaryEffect = content.Load<Effect>("Ordinary");

            DataChooser = new Economy.DropDownMenu();
            UpdateButton = new Economy.QueueButton(100, new Vector2(5, GameWindow.Height - 30));
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            MouseState Input = Mouse.GetState();
            DataChooser.Update(gameTime, Input);
            UpdateButton.Update(Input);

            PreviousMouse = Input;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            DataChooser.Draw();
            UpdateButton.Draw();

            base.Draw(gameTime);
        }
    }
}
