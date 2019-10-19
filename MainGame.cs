using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace flappyBird
{
    //TODO maybe static members would be better than this!
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
    class HelperFunc
    {
        public static bool interlaced(Rectangle r1, Rectangle r2) =>
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

    public class Bird
    {
        public bool dead = false;
        const double jump_time = 0.4;
        double acceleration = 250.0,
            velocity = 0,
            position = 0.0;
        float angle = 0f;
        Texture2D texture,
            texture_jump,
            debug_texture;
        double last_jump_time = jump_time * -1;

        public Bird(Texture2D texture, Texture2D texture_jump, Texture2D debug_texture)
        {
            this.texture = texture;
            this.debug_texture = debug_texture;
            this.texture_jump = texture_jump;
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
            velocity = -165;
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
            (int)(Constants.bird_res * Constants.bird_scale),
            (int)(Constants.bird_res * Constants.bird_scale)
        );
    }

    public class Pipes
    {
        static int score_est = 0;
        Random rnd = new Random();
        double velocity = 60;
        List<Pipe> pipe_list = new List<Pipe>();
        Texture2D texture;

        public Pipes(Texture2D texture)
        {
            this.texture = texture;
            add_new_pipe();
        }
        public void update(double interval)
        {
            foreach (Pipe pipe in pipe_list) pipe.update(interval, velocity);
            if (pipe_list[pipe_list.Count - 1].position
                < Constants.window_width - Constants.pipe_distance)
                add_new_pipe();
            if (pipe_list[0].position < 0 - Constants.pipe_width)
            {
                pipe_list.RemoveAt(0);
                score_est++;
            }
        }
        public int score() =>
            pipe_list[0].position + (Constants.pipe_width / 2)
            <= Constants.bird_x_distance + ((Constants.bird_res * (Constants.bird_scale / 2)) / 2)
                ? score_est + 1
                : score_est;
        public void draw(SpriteBatch spriteBatch)
        {
            foreach (Pipe pipe in pipe_list) pipe.draw(spriteBatch);
        }

        void add_new_pipe()
        {
            int random_gap_pos = rnd.Next(0, Constants.window_height - Constants.pipe_gap);
            pipe_list.Add(new Pipe(texture, random_gap_pos));
        }
        public void die() => velocity = 0;
        public bool pipe_collision(Rectangle bird_rect)
        {
            foreach (Pipe pipe in pipe_list)
                foreach (Rectangle pipe_rec in pipe.pipe_rectangles())
                    if (HelperFunc.interlaced(bird_rect, pipe_rec)) return true;
            return false;
        }
    }
    public class Pipe
    {
        public double position;
        int gap_position;
        Texture2D texture;
        public Pipe(Texture2D texture, int gap_position)
        {
            position = Constants.window_width;
            this.texture = texture;
            this.gap_position = gap_position;
        }
        public void update(double interval, double velocity) =>
            position -= velocity * interval;
        public void draw(SpriteBatch spriteBatch)
        {
            foreach (Rectangle rect in pipe_rectangles())
                spriteBatch.Draw(texture, rect, Color.White);
        }

        public Rectangle[] pipe_rectangles() =>
            new Rectangle[] {
                new Rectangle(
                    (int)position,
                    0,
                    Constants.pipe_width,
                    gap_position
                ),
                new Rectangle(
                    (int)position,
                    gap_position + Constants.pipe_gap,
                    Constants.pipe_width,
                    Constants.window_height - gap_position + Constants.pipe_gap
                )
            };
    }
    public class GameOverCard
    {
        double position = Constants.window_height * -1;
        bool dead = false;
        const double velocity = 300;
        Texture2D texture;
        SpriteFont font;
        public void die() => dead = true;
        public GameOverCard(Texture2D texture, SpriteFont font)
        {
            this.texture = texture;
            this.font = font;
        }

        public void update(double interval)
        {
            if (dead)
            {
                if (position >= 0)
                    position = 0;
                else
                    position += interval * velocity;
            }
        }
        public void draw(SpriteBatch spriteBatch, int score)
        {
            spriteBatch.Draw(texture, new Rectangle(
                0,
                (int)position,
                Constants.window_width,
                Constants.window_height
            ), Color.Red);
            spriteBatch.DrawString(font, $"Game Over - Score: {score}", new Vector2(230, 200 + (int)position), Color.White);
        }

    }

    public class MainGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        TimeSpan lastTime;
        Bird bird;
        Pipes pipes;
        GameOverCard gameOverCard;
        Boolean pressedLastTick = false;

        public MainGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("score");
            pipes = new Pipes(Content.Load<Texture2D>("pipe"));
            bird = new Bird(
                Content.Load<Texture2D>("sprite_0"),
                Content.Load<Texture2D>("sprite_1"),
                Content.Load<Texture2D>("debug")
            );
            gameOverCard = new GameOverCard(Content.Load<Texture2D>("pipe"), font);
        }

        protected override void Update(GameTime gameTime)
        {
            double interval = (gameTime.TotalGameTime - lastTime).TotalSeconds;
            if (lastTime == null) lastTime = gameTime.TotalGameTime;
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            lastTime = gameTime.TotalGameTime;
            bird.update(interval);
            pipes.update(interval);
            gameOverCard.update(interval);
            if ((bird.dead == false) &&
                (Keyboard.GetState().IsKeyDown(Keys.Space) ||
                Keyboard.GetState().IsKeyDown(Keys.Up)))
            {
                if (!pressedLastTick)
                    bird.jump(gameTime);
                pressedLastTick = true;
            }
            else pressedLastTick = false;

            if (Keyboard.GetState().IsKeyDown(Keys.W) || pipes.pipe_collision(bird.hit_box()))
            {
                pipes.die();
                bird.die();
                gameOverCard.die();
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.BlanchedAlmond);

            spriteBatch.Begin();

            bird.draw(spriteBatch, gameTime);
            pipes.draw(spriteBatch);
            spriteBatch.DrawString(font, $"Score: {pipes.score()}", new Vector2(630, 400), Color.Black);
            gameOverCard.draw(spriteBatch, pipes.score());

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
