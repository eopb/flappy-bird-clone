using Microsoft.Xna.Framework;
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
        public const bool debug = false;
    }

    public class Bird
    {
        public bool dead = false;
        private double acceleration = 200.0;
        public double velocity = 0;
        private double position = 0.0;
        private float angle = 0f;
        private Texture2D texture;
        private Texture2D texture_jump;
        private Texture2D test_texture;
        private const double jump_time = 0.4;
        private double last_jump_time = jump_time * -1;
        public void die()
        {
            dead = true;
            acceleration = 600.0;
            if (velocity < 0)
            {
                velocity = 0;
            }
        }
        public Bird(Texture2D texture2, Texture2D texture3, Texture2D texture4)
        {
            texture = texture2;
            test_texture = texture3;
            texture_jump = texture4;
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
        public void jump(GameTime gameTime) { velocity = -150; last_jump_time = gameTime.TotalGameTime.TotalSeconds; }
        public void draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            int bird_res = 80;

            Vector2 location = new Vector2(Constants.bird_x_distance, Convert.ToInt32(position));
            Rectangle sourceRectangle = new Rectangle(0, 0, bird_res, bird_res);
            Vector2 origin = new Vector2(bird_res / 2, bird_res / 2);
            float scale = 0.9f;

            spriteBatch.Draw(
                texture: gameTime.TotalGameTime.TotalSeconds - last_jump_time < jump_time ? texture_jump : texture,
                position: location,
                sourceRectangle: sourceRectangle,
                origin: origin,
                rotation: angle,
                scale: new Vector2(scale, scale),
                color: Color.White,
                effects: SpriteEffects.None,
                layerDepth: 1
            ); if (Constants.debug)
            {
                spriteBatch.Draw(
                    test_texture,
                    new Rectangle(
                        Constants.bird_x_distance - (int)(bird_res * (scale / 2)),
                        (Convert.ToInt32(position)) - (int)(bird_res * (scale / 2)),
                        (int)(bird_res * scale), (int)(bird_res * scale)
                    ),
                    Color.White
                );
            }
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
            if (pipe_list[0].position < 0 - Constants.pipe_width)
            {
                pipe_list.RemoveAt(0);
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
            bird = new Bird(Content.Load<Texture2D>("sprite_0"), Content.Load<Texture2D>("pipe"), Content.Load<Texture2D>("sprite_1"));
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
                    Console.WriteLine("Jump key pressed");
                    bird.jump(gameTime);
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
            GraphicsDevice.Clear(Color.BlanchedAlmond);

            spriteBatch.Begin();

            bird.draw(spriteBatch, gameTime);
            pipes.draw(spriteBatch);
            // spriteBatch.DrawString(font, "Score: " + score, new Vector2(100, 100), Color.Green);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
