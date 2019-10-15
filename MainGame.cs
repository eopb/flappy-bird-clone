using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace flappyBird
{
    class HelperFunc
    {
        public static bool inter(Rectangle r1, Rectangle r2) =>
                    ((r1.X < r2.X && r2.X < r1.X + r1.Width)
                    || (r1.X < r2.X + r2.Width && r2.X + r2.Width < r1.X + r1.Width)
                    || (r2.X < r1.X && r1.X < r2.X + r2.Width)
                    || (r2.X < r1.X + r1.Width && r1.X + r1.Width < r2.X + r2.Width))
                    &&
                    ((r1.Y < r2.Y && r2.Y < r1.Y + r1.Height)
                    || (r1.Y < r2.Y + r2.Height && r2.Y + r2.Height < r1.Y + r1.Height)
                    || (r2.Y < r1.Y && r1.Y < r2.Y + r2.Height)
                    || (r2.Y < r1.Y + r1.Height && r1.Y + r1.Height < r2.Y + r2.Height));
    }
    class Constants
    {
        public const int window_width = 800,
            window_height = 480,
            pipe_width = 60,
            pipe_gap = 150,
            bird_size = 260,
            bird_x_distance = 60,
            bird_res = 80;
        public const double pipe_distance = 200;
        public const float bird_scale = 0.9f;
        public const bool debug = false;

    }

    public class Bird
    {
        private const double jump_time = 0.4;
        public bool dead = false;
        private double acceleration = 200.0,
            velocity = 0,
            position = 0.0;
        private float angle = 0f;
        private Texture2D texture,
            texture_jump,
            debug_texture;

        private double last_jump_time = jump_time * -1;
        public Bird(Texture2D texture_, Texture2D debug_texture_, Texture2D texture_jump_)
        {
            texture = texture_;
            debug_texture = debug_texture_;
            texture_jump = texture_jump_;
        }

        public void update(double interval)

        {
            velocity += acceleration * interval;
            position += velocity * interval;

            if ((angle <= 1.5 && velocity > 0) || (angle >= -1.5 && velocity < 0))
                angle = (float)velocity / 200;
        }
        public void draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(
                texture:
                    gameTime.TotalGameTime.TotalSeconds - last_jump_time < jump_time
                        ? texture_jump
                        : texture,
                position: new Vector2(Constants.bird_x_distance, Convert.ToInt32(position)),
                sourceRectangle: new Rectangle(0, 0, Constants.bird_res, Constants.bird_res),
                origin: new Vector2(Constants.bird_res / 2, Constants.bird_res / 2),
                rotation: angle,
                scale: new Vector2(Constants.bird_scale, Constants.bird_scale),
                color: Color.White,
                effects: SpriteEffects.None,
                layerDepth: 1
            );
#pragma warning disable 0162
            if (Constants.debug)
                spriteBatch.Draw(debug_texture, hit_box(), Color.White);
        }

        public void jump(GameTime gameTime)
        {
            last_jump_time = gameTime.TotalGameTime.TotalSeconds;
            velocity = -150;
        }
        public void die()
        {
            dead = true;
            acceleration = 600.0;
            if (velocity < 0)
                velocity = 0;
        }
        public Rectangle hit_box() => new Rectangle(
            Constants.bird_x_distance - (int)(Constants.bird_res * (Constants.bird_scale / 2)),
            (Convert.ToInt32(position)) - (int)(Constants.bird_res * (Constants.bird_scale / 2)),
            (Constants.bird_res * Constants.bird_scale),
            (int)(Constants.bird_res * Constants.bird_scale)
        );
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
        public bool colis(Rectangle bird_rect)
        {
            foreach (Pipe pipe in pipe_list)
                foreach (Rectangle pipe_rec in pipe.pipe_rectangles())
                    if (HelperFunc.inter(bird_rect, pipe_rec)) return true;
            return false;
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
        public Rectangle[] pipe_rectangles() =>
            new Rectangle[]{new Rectangle(
                    (int)position,
                    0,
                    Constants.pipe_width,
                    gap_position
                ),new Rectangle(
                    (int)position,
                    gap_position + Constants.pipe_gap,
                    Constants.pipe_width,
                    Constants.window_height - (gap_position + Constants.pipe_gap)
                )};

        public void draw(SpriteBatch spriteBatch)
        {
            foreach (Rectangle rect in pipe_rectangles())
                spriteBatch.Draw(
                    texture,
                    rect,
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
            bird = new Bird(Content.Load<Texture2D>("sprite_0"), Content.Load<Texture2D>("debug"), Content.Load<Texture2D>("sprite_1"));
            pipes = new Pipes(Content.Load<Texture2D>("pipe"));
        }

        protected override void Update(GameTime gameTime)
        {
            if (lastTime == null) lastTime = gameTime.TotalGameTime;
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            double interval = (gameTime.TotalGameTime - lastTime).TotalSeconds;
            lastTime = gameTime.TotalGameTime;
            // Console.WriteLine(bird.velocity);
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
            if (Keyboard.GetState().IsKeyDown(Keys.W) || pipes.colis(bird.hit_box()))
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
