using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace EarthDefender
{
    public class Player
    {
        public bool isShooting;

        private Texture2D texture;

        public Vector2 position = Vector2.Zero;
        public Vector2 velocity = Vector2.Zero;
        public Vector2 scale = Vector2.One;

        private float speed = 5f;
        private float animationTime = 0f;
        private float animationDuration = 2.5f;

        private float ballShootSpeed = 1000f;
        private float ballRefreshTime = 1f;

        private int currentFrame = 0;
        private int maxFrame = 2;
        private Ball nextBall;
        private float nextBallDelay = 0f;

        private bool dying = false;
        private bool dead = false;
        private float dyingTime = 0f;
        private float dieAnimationTime = 1f;

        public Player(Texture2D texture, Vector2 position)
        {
            this.texture = texture;
            this.position = position;
        }


        public void Update(GameTime gameTime)
        {
            if (dying)
            {
                if (!dead)
                {
                    dyingTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    float dyingProgress = dyingTime / dieAnimationTime;
                    if (dyingTime >= dieAnimationTime)
                    {
                        dead = true;
                    }
                    else
                    {
                        scale.X = dyingProgress * 4;
                        scale.X *= scale.X;
                        scale.X += 1;
                        scale.Y = (1 - dyingProgress);
                        scale.Y *= scale.Y;
                    }
                }
                return;
            }

            animationTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            var durationPerFrame = animationDuration / maxFrame;
            if (animationTime >= durationPerFrame)
            {
                currentFrame = (currentFrame + 1) % maxFrame;
                animationTime -= durationPerFrame;
            }

            if (isShooting)
            {
                nextBallDelay -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (nextBallDelay <= 0)
                {
                    isShooting = false;
                }
            }

            if (!isShooting && nextBall == null)
            {
                RefreshNextBall();
            }

            velocity.X = 0f;
            velocity.Y = 0f;

            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Up))
            {
                velocity.Y -= speed;
            }
            if (keyboardState.IsKeyDown(Keys.Down))
            {
                velocity.Y += speed;
            }

            position += velocity;

            position.X = MathHelper.Clamp(position.X, 0f, Singleton.GAMEWIDTH);
            position.Y = MathHelper.Clamp(position.Y, 0f, Singleton.GAMEHEIGHT);

            if (Singleton.I.MouseState.LeftButton == ButtonState.Pressed && Singleton.I.PreviousMouseState.LeftButton == ButtonState.Released && !isShooting)
            {
                ShootNextBall();
            }

            if (nextBall != null)
            {
                nextBall.Update(gameTime);
            }

        }


        public void ShootNextBall()
        {
            if (nextBall == null) return;

            Vector2 mousePosition = new Vector2(Singleton.I.MouseState.X, Singleton.I.MouseState.Y);

            var shootDirection = Vector2.Normalize(mousePosition - position);
            var ball = nextBall;
            ball.position = position;
            ball.velocity = shootDirection * ballShootSpeed;
            Singleton.I.balls.Add(ball);

            isShooting = true;
            animationDuration = 1f;
            currentFrame = 0;
            nextBallDelay = ballRefreshTime;
            nextBall = null;
        }

        public void RefreshNextBall()
        {
            // ball gen 
            nextBall = new Ball(Singleton.I.ballTexture, Vector2.Zero);

            Random random = Singleton.I.random;
            float randValue = random.NextSingle();
            if (randValue < 0.1f)
            {
                nextBall.type = 5; // blank ball
            }
            else if (randValue < 0.2f)
            {
                nextBall.type = 6; // black ball
            }
            else if (randValue < 0.3f)
            {
                nextBall.type = 7; // rasengan ball
            }
            else
            {
                List<int> remainingBallTypes = new List<int>();
                foreach (Ball ball in Singleton.I.balls)
                {
                    if (ball.IsNormalBall() && !ball.IsDying() && !remainingBallTypes.Contains(ball.type))
                    {
                        remainingBallTypes.Add(ball.type);
                    }
                }
                if (remainingBallTypes.Count > 0)
                {
                    nextBall.type = remainingBallTypes[random.Next(remainingBallTypes.Count)];
                }
                else
                {
                    nextBall = null;
                }
            }
        }

        public void Kill()
        {
            dying = true;
            dyingTime = 0f;
        }

        public bool IsDying()
        {
            return dying;
        }

        public bool IsDead()
        {
            return dead;
        }

        public void Draw(SpriteBatch spriteBatch)
        {

            spriteBatch.DrawString(Singleton.I.font, "Score " + Singleton.I.score, Vector2.Zero, Color.White);
            spriteBatch.DrawString(Singleton.I.font, "High Score " + Singleton.I.highscore, new Vector2(Singleton.GAMEWIDTH - Singleton.I.font.MeasureString("High Score " + Singleton.I.highscore).X, 0f), Color.White);

            if (dead) return;

            // player body
            Rectangle sourceRectangle = new Rectangle(currentFrame * 128, 64, 128, 128);
            Vector2 origin = new Vector2(sourceRectangle.Width, sourceRectangle.Height) / 2f;
            spriteBatch.Draw(texture, position, sourceRectangle, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);

            // next ball hint
            if (isShooting)
            {
                sourceRectangle.Y = 192;
            }
            else if (nextBall != null)
            {
                spriteBatch.DrawString(Singleton.I.font, "Next", new Vector2(0f, 656f), Color.White);

                nextBall.position = new Vector2(64f, 656f);
                nextBall.Draw(spriteBatch);
            }

        }
    }
}