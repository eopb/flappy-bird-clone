﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace newdotnettest
{
    public class Bird
    {
        private double position = 0.0;
        private Texture2D texture;
        public Bird(Texture2D texture2)
        {
            texture = texture2;
        }
        public void update(double interval) { position += interval * 20; }
        public void draw(SpriteBatch spriteBatch) { spriteBatch.Draw(texture, new Rectangle(Convert.ToInt32(position), 0, 50, 50), Color.White); }
    }
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private SpriteFont font;
        private TimeSpan lastTime;

        private Bird bird;

        private int score = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("score");
            bird = new Bird(Content.Load<Texture2D>("flappybird"));
        }

        protected override void Update(GameTime gameTime)
        {
            if (lastTime == null) lastTime = gameTime.TotalGameTime;
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            double interval = (gameTime.TotalGameTime - lastTime).TotalSeconds;
            Console.WriteLine(interval);
            bird.update(interval);
            lastTime = gameTime.TotalGameTime;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            bird.draw(spriteBatch);
            spriteBatch.DrawString(font, "Score: " + score, new Vector2(100, 100), Color.Green);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}