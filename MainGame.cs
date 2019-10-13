﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace flappyBird
{
    static class Constants
    {
        public const int window_width = 800,
            window_height = 480,
            pipe_width = 60,
            pipe_gap = 150,
            bird_size = 260,
            bird_x_distance = 60;
        public const double pipe_distance = 200;
    }

    public class Bird
    {
        public bool dead = false;
        private double acceleration = 200.0;
        public double velocity = 0;
        private double position = 0.0;
        private float angle = 0f;
        private Texture2D texture;
        private Texture2D test_texture;
        public void die()
        {
            dead = true;
            acceleration = 600.0;
            if (velocity < 0)
            {
                velocity = 0;
            }
        }
        public Bird(Texture2D texture2, Texture2D texture3)
        {
            texture = texture2;
            test_texture = texture3;
        }
        public void update(double interval)

        {
            velocity += acceleration * interval;
            position += velocity * interval;
            if ((angle <= 1.5 && velocity > 0) || (angle >= -1.5 && velocity < 0))
            {
                angle = (float)velocity / 200;
            }
        }
        public void jump() { velocity = -150; }
        public void draw(SpriteBatch spriteBatch)
        {


            Vector2 location = new Vector2(Constants.bird_x_distance, Convert.ToInt32(position));
            Rectangle sourceRectangle = new Rectangle(0, 0, Constants.bird_size, Constants.bird_size);
            Vector2 origin = new Vector2(Constants.bird_size / 2, Constants.bird_size / 2);
            float scale = 0.3f;

            spriteBatch.Draw(
                texture: texture,
                position: location,
                sourceRectangle: sourceRectangle,
                origin: origin,
                rotation: angle,
                scale: new Vector2(scale, scale),
                color: Color.Green,
                effects: SpriteEffects.None,
                layerDepth: 1
            );
            spriteBatch.Draw(
                test_texture,
                new Rectangle(
                    Constants.bird_x_distance - (int)(Constants.bird_size * (scale / 2)),
                    (Convert.ToInt32(position)) - (int)(Constants.bird_size * (scale / 2)),
                    (int)(Constants.bird_size * scale), (int)(Constants.bird_size * scale)
                ),
                Color.White
            );
        }
    }

    public class Pipes
    {
        private Random rnd = new Random();
        public double velocity = 50;

        private Texture2D texture;
        private List<Pipe> pipe_list = new List<Pipe>();
        public Pipes(Texture2D texture2)
        {
            texture = texture2;
            add_new_pipe();
        }
        public void update(double interval)
        {
            foreach (Pipe pipe in pipe_list) pipe.update(interval, velocity);

            if (pipe_list[pipe_list.Count - 1].position < (Constants.window_width - Constants.pipe_distance))
            {
                add_new_pipe();
            }
        }


        public void draw(SpriteBatch spriteBatch)
        {
            foreach (Pipe pipe in pipe_list) pipe.draw(spriteBatch);
        }
        private void add_new_pipe()
        {
            int random_gap_pos = rnd.Next(0, Constants.window_height - Constants.pipe_gap);
            pipe_list.Add(new Pipe(texture, random_gap_pos));
        }

        public void die()
        {
            velocity = 0;
        }
    }
    public class Pipe
    {
        private int gap_position;
        public double position;
        private Texture2D texture;
        public Pipe(Texture2D texture2, int gap_position_arg)
        {
            texture = texture2;
            position = Constants.window_width;
            gap_position = gap_position_arg;
        }
        public void update(double interval, double velocity)

        {
            position -= velocity * interval;
        }
        public void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                texture,
                new Rectangle(
                    (int)position,
                    0,
                    Constants.pipe_width,
                    gap_position
                ),
                Color.White
            );
            spriteBatch.Draw(
                texture,
                new Rectangle(
                    (int)position,
                    gap_position + Constants.pipe_gap,
                    Constants.pipe_width,
                    Constants.window_height - (gap_position + Constants.pipe_gap)
                ),
                Color.White
            );
        }
    }
    public class MainGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private SpriteFont font;
        private TimeSpan lastTime;
        private Bird bird;
        private Pipes pipes;
        private Boolean pressedLastTick = false;
        private int score = 0;

        public MainGame()
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
            bird = new Bird(Content.Load<Texture2D>("flappybird"), Content.Load<Texture2D>("pipe"));
            pipes = new Pipes(Content.Load<Texture2D>("pipe"));
        }

        protected override void Update(GameTime gameTime)
        {
            if (lastTime == null) lastTime = gameTime.TotalGameTime;
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            double interval = (gameTime.TotalGameTime - lastTime).TotalSeconds;
            lastTime = gameTime.TotalGameTime;
            Console.WriteLine(bird.velocity);
            bird.update(interval);
            pipes.update(interval);
            if ((bird.dead == false) && (Keyboard.GetState().IsKeyDown(Keys.Space) || Keyboard.GetState().IsKeyDown(Keys.Up)))
            {
                if (!pressedLastTick)
                {
                    Console.WriteLine("Space key pressed");
                    bird.jump();
                }
                pressedLastTick = true;
            }
            else
            {
                pressedLastTick = false;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                pipes.die(); bird.die();
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Red);

            spriteBatch.Begin();

            bird.draw(spriteBatch);
            pipes.draw(spriteBatch);
            // spriteBatch.DrawString(font, "Score: " + score, new Vector2(100, 100), Color.Green);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
