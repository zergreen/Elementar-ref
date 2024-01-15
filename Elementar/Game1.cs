using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using EarthDefender.GameObject;

namespace EarthDefender
{

    public class Game1 : Game
    {

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = Singleton.GAMEWIDTH;
            _graphics.PreferredBackBufferHeight = Singleton.GAMEHEIGHT;
            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            Singleton.I.ballTexture = Content.Load<Texture2D>("Sprite");
            Singleton.I.background = Content.Load<Texture2D>("BG");
            Singleton.I.whiteTexture = new Texture2D(GraphicsDevice, 1, 1);
            Singleton.I.whiteTexture.SetData(new Color[] { Color.White });

            Singleton.I.font = Content.Load<SpriteFont>("GameFont");


            Reset();
        }

        private void Reset()
        {

            if (Singleton.I.score > Singleton.I.highscore)
            {
                //keep highscore
                Singleton.I.highscore = Singleton.I.score;
                Singleton.I.score = 0;
            }
            else
            {
                Singleton.I.score = 0;
            }

            Singleton.I.player = new Player(Singleton.I.ballTexture, new(Singleton.GAMEWIDTH / 2f, Singleton.GAMEHEIGHT / 2f));
            Singleton.I.player.position = new(Singleton.GAMEWIDTH / 2, Singleton.GAMEHEIGHT - 128);
            //Singleton.I.player.position = new(100,500);
            Singleton.I.balls = new();
            Singleton.I.particles = new();
            Singleton.I.wallSpeed = 1f;
            Singleton.I.groundY = 615f;
            Singleton.I.CurrentGameState = Singleton.GameState.GamePlaying;
            Singleton.I.ballGrid = new BallGrid(40, 10);

            for (int y = 1; y < Singleton.I.ballGrid.height - 4 ; y++)
            {
                for (int x = 8; x < 15; x++)
                {
                    Ball ball = new Ball(Singleton.I.ballTexture, Vector2.Zero);

                    Vector2 gridToWorldPosition = Singleton.I.ballGrid.GridToWorldPosition(x, y);

                    Console.WriteLine($"Grid Position ({x}, {y}) to World Position: {gridToWorldPosition}");

                    ball.position = gridToWorldPosition;
                    ball.type = Singleton.I.random.Next(5);
                    ball.NoGravity = true;
                    Singleton.I.ballGrid.SetBall(x, y, ball);
                    Singleton.I.balls.Add(ball);
                }
            }

        }

        protected override void Update(GameTime gameTime)
        {
            Singleton.I.CurrentKey = Keyboard.GetState();
            Singleton.I.MouseState = Mouse.GetState();

            switch (Singleton.I.CurrentGameState)
            {
                // Pause
                case Singleton.GameState.GamePlaying:

                    if (Singleton.I.CurrentKey.IsKeyDown(Keys.Escape) && Singleton.I.PreviousKey.IsKeyUp(Keys.Escape))
                        Singleton.I.CurrentGameState = Singleton.GameState.GamePaused;

                    Singleton.I.particles.ForEach(particle => particle.Update(gameTime));
                    Singleton.I.particles.RemoveAll(particle => particle.IsExpired());

                    Singleton.I.player.Update(gameTime);

                    List<List<Ball>> chains = new();
                    List<Ball> activatedBall = new();

                    foreach (var ball in Singleton.I.balls)
                    {
                        ball.Update(gameTime);

                        if (ball.IsDying()) continue;

                        if (ball.position.Y + 32 >= Singleton.I.groundY)
                        {
                            ball.Destroy();
                            Singleton.I.score += -50;
                        }

                        if (ball.NoGravity) continue;

                        Singleton.I.ballGrid.ForEachBall(otherBall =>
                        {
                            if (otherBall.IsDying()) return;
                            if (ball.CollideWith(otherBall, out Vector2 direction, out float distance)) // distance <= ball.radius * 2
                            {
                                ball.position -= direction * distance;

                                if (ball.IsBlankBall())
                                {
                                    ball.type = otherBall.type;
                                }
                                else if (ball.IsBlackBall())
                                {
                                    activatedBall.Add(ball);
                                    return;
                                }
                                else if (ball.IsRasenganBall())
                                {
                                    activatedBall.Add(ball);
                                    return;
                                }
                                if (Singleton.I.ballGrid.AddBallToGridIfEmpty(ball))
                                {
                                    ball.velocity = Vector2.Zero;
                                    ball.NoGravity = true;
                                    var chain = GetBallChain(ball);
                                    chains.Add(chain);
                                }
                            }
                        });
                    }

                    // activate ball effect
                    foreach (var ball in activatedBall)
                    {
                        if (ball.IsBlackBall())
                        {
                            float explodeRadius = 150f;
                            Singleton.I.ballGrid.ForEachBall(other =>
                            {
                                if (Vector2.Distance(ball.position, other.position) <= explodeRadius)
                                {
                                    other.Destroy(true);
                                }
                            });
                            var explodeParticle = new Particle(Singleton.I.ballTexture, new Rectangle(192, 512 - 64, 64, 64), ball.position, 0.3f, 0.3f, 3);
                            explodeParticle.scale = Vector2.One * (explodeRadius / 32f);
                            Singleton.I.particles.Add(explodeParticle);
                            ball.Destroy();
                        }
                        else if (ball.IsRasenganBall())
                        {
                            float explodeHorizontalRadius = 300f;
                            float explodeVerticalRadius = 50f;
                            Singleton.I.ballGrid.ForEachBall(other =>
                            {
                                var dx = other.position.X - ball.position.X;
                                if (
                                    dx >= 0f && dx <= explodeHorizontalRadius &&
                                    MathF.Abs(other.position.Y - ball.position.Y) <= explodeVerticalRadius
                                    )
                                {
                                    other.Destroy(true);
                                }
                            });

                            var beamPos = ball.position;
                            float beamDistance = 0f;
                            while (beamDistance < explodeHorizontalRadius)
                            {
                                var beamParticle = new Particle(Singleton.I.ballTexture, new Rectangle(384, 512 - 64, 64, 64), beamPos, 0.3f, 0.3f, 3);
                                beamParticle.scale = Vector2.One * (explodeVerticalRadius / 32f);
                                Singleton.I.particles.Add(beamParticle);
                                beamPos.X += 32;
                                beamDistance += 32;
                            }


                            ball.Destroy();
                        }
                    }

                    // activate chain
                    foreach (var chain in chains)
                    {
                        if (chain.Count >= 3)
                        {
                            foreach (var ball in chain)
                            {
                                if (ball.IsDying()) continue;
                                var gridPos = Singleton.I.ballGrid.WorldToGridPosition(ball.position.X, ball.position.Y);
                                if (gridPos != null)
                                {
                                    ball.Destroy();
                                    Singleton.I.score += 100;
                                    Singleton.I.ballGrid.SetBall(gridPos.Value.X, gridPos.Value.Y, null);
                                }
                            }
                        }
                    }

                    // dying is here
                    if (!Singleton.I.player.IsDying())
                    {
                        Singleton.I.ballGrid.ForEachBall(ball =>
                        {
                            if (ball.IsDying() && (ball.position.X - 32) <= (Singleton.I.player.position.X + 60))
                            {
                                Console.WriteLine("XXXXXXXXXX");
                                Console.WriteLine($"Status: {!ball.IsDying()}");
                                Console.WriteLine($"ball.position.X - 32: {ball.position.X - 32}");
                                Console.WriteLine($"Singleton.I.player.position.X + 60: {Singleton.I.player.position.X + 60}");
                                Console.WriteLine("XXXXXXXXXX");
                                Singleton.I.player.Kill();
                            }
                        });
                    }

                    if (Singleton.I.player.IsDead())
                    {
                        Singleton.I.CurrentGameState = Singleton.GameState.GameLose;
                    }

                    Singleton.I.balls.RemoveAll(ball => ball.IsDead());
                    Singleton.I.ballGrid.RemoveBallIf(ball => ball.IsDead());

                    //waterfall ball direction
                    //Singleton.I.ballGrid.Move(-Singleton.I.wallSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds, 0f);
                    Singleton.I.ballGrid.Move(0f, Singleton.I.wallSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds);

                    Singleton.I.wallSpeed += 0.025f * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (Singleton.I.balls.Count == 0)
                    {
                        Singleton.I.CurrentGameState = Singleton.GameState.GameWin;
                    }

                    break;

                // Resume
                case Singleton.GameState.GamePaused:
                    if (Singleton.I.CurrentKey.IsKeyDown(Keys.Escape) && Singleton.I.PreviousKey.IsKeyUp(Keys.Escape))
                        Singleton.I.CurrentGameState = Singleton.GameState.GamePlaying;
                    break;

                // Lose
                case Singleton.GameState.GameLose:
                    if (Singleton.I.CurrentKey.IsKeyDown(Keys.Enter) || Singleton.I.CurrentKey.IsKeyDown(Keys.Space) && Singleton.I.PreviousKey.IsKeyUp(Keys.Enter))
                        Reset();
                    break;

                // Win
                case Singleton.GameState.GameWin:
                    if (Singleton.I.CurrentKey.IsKeyDown(Keys.Enter) || Singleton.I.CurrentKey.IsKeyDown(Keys.Space) && Singleton.I.PreviousKey.IsKeyUp(Keys.Enter))
                        Reset();
                    break;
            }

            Singleton.I.PreviousMouseState = Singleton.I.MouseState;
            Singleton.I.PreviousKey = Singleton.I.CurrentKey;

            base.Update(gameTime);
        }

        private List<Ball> GetBallChain(Ball startBall)
        {

            List<Ball> chain = new List<Ball>();
            Stack<Ball> ballStack = new Stack<Ball>();
            ballStack.Push(startBall);

            while (ballStack.Count > 0)
            {
                var currentBall = ballStack.Pop();
                chain.Add(currentBall);

                var gridPos = Singleton.I.ballGrid.WorldToGridPosition(currentBall.position.X, currentBall.position.Y);

                if (gridPos == null) continue;
                var adjacentBalls = Singleton.I.ballGrid.GetAdjacentBalls(gridPos.Value.X, gridPos.Value.Y);

                foreach (Ball adjacentBall in adjacentBalls)
                {
                    if (!chain.Contains(adjacentBall) && adjacentBall.type == startBall.type)
                    {
                        ballStack.Push(adjacentBall);
                    }
                }
            }

            return chain;
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            _spriteBatch.Draw(Singleton.I.background, new Vector2(0, 0), Color.White);

            // death line
            Rectangle deathLineRectangle = new Rectangle(285, 63, 4, 258); //ตัดไทล์ ที่ตำแหน่ง 285, 63 ใน sprite.png ด้วยขนาด 4 * 258
            _spriteBatch.Draw(Singleton.I.ballTexture, new Rectangle(1000, 450, 5, (int)Singleton.I.groundY), deathLineRectangle, Color.White, MathHelper.PiOver2, Vector2.Zero, SpriteEffects.None, 0f);
            //_spriteBatch.Draw(Singleton.I.ballTexture, new Rectangle((int)Singleton.I.player.position.X + 32, 0, 5, (int)Singleton.I.groundY), deathLineRectangle, Color.White);
            //_spriteBatch.Draw(_line, new Vector2(200, 0), null, Color.White, MathHelper.Pi / 2, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // block two line
            _spriteBatch.Draw(Singleton.I.ballTexture, new Rectangle(390, 0, 5, (int)Singleton.I.groundY), deathLineRectangle, Color.White);
            _spriteBatch.Draw(Singleton.I.ballTexture, new Rectangle(990, 0, 5, (int)Singleton.I.groundY), deathLineRectangle, Color.White);


            
            foreach (var ball in Singleton.I.balls)
            {
                ball.Draw(_spriteBatch);
            }

            Singleton.I.ballGrid.ForEachBall(ball => ball.Draw(_spriteBatch));

            Singleton.I.player.Draw(_spriteBatch);

            Singleton.I.particles.ForEach(particle => particle.Draw(_spriteBatch));

            // Pause
            if (Singleton.I.CurrentGameState == Singleton.GameState.GamePaused)
            {
                _spriteBatch.Draw(Singleton.I.whiteTexture, new Rectangle(0, 0, Singleton.GAMEWIDTH, Singleton.GAMEHEIGHT), null, Color.Black * 0.6f);
                Vector2 fontSize = Singleton.I.font.MeasureString("Pause");
                _spriteBatch.DrawString(Singleton.I.font, "Pause", new Vector2((Singleton.GAMEWIDTH - fontSize.X) / 2, (Singleton.GAMEHEIGHT - fontSize.Y) / 2), Color.White);
            }
            // Lose
            else if (Singleton.I.CurrentGameState == Singleton.GameState.GameLose)
            {
                _spriteBatch.Draw(Singleton.I.whiteTexture, new Rectangle(0, 0, Singleton.GAMEWIDTH, Singleton.GAMEHEIGHT), null, Color.Black * 0.6f);
                Vector2 fontSize1 = Singleton.I.font.MeasureString("Game Over");
                Vector2 fontSize2 = Singleton.I.font.MeasureString(Singleton.I.score.ToString());
                Vector2 fontSize3 = Singleton.I.font.MeasureString("Press Space Bar to continue");
                _spriteBatch.DrawString(Singleton.I.font, "Game Over", new Vector2((Singleton.GAMEWIDTH - fontSize1.X) / 2, ((Singleton.GAMEHEIGHT - fontSize1.Y) / 2) - 50), Color.White);
                _spriteBatch.DrawString(Singleton.I.font, Singleton.I.score.ToString(), new Vector2((Singleton.GAMEWIDTH - fontSize2.X) / 2, (Singleton.GAMEHEIGHT - fontSize2.Y) / 2), Color.White);
                _spriteBatch.DrawString(Singleton.I.font, "Press Space Bar to continue", new Vector2((Singleton.GAMEWIDTH - fontSize3.X) / 2, ((Singleton.GAMEHEIGHT - fontSize3.Y) / 2) + 50), Color.White);

            }
            // Win
            else if (Singleton.I.CurrentGameState == Singleton.GameState.GameWin)
            {
                _spriteBatch.Draw(Singleton.I.whiteTexture, new Rectangle(0, 0, Singleton.GAMEWIDTH, Singleton.GAMEHEIGHT), null, Color.Black * 0.6f);
                Vector2 fontSize = Singleton.I.font.MeasureString("You Win");
                Vector2 fontSize2 = Singleton.I.font.MeasureString(Singleton.I.score.ToString());
                Vector2 fontSize3 = Singleton.I.font.MeasureString("Press Space Bar to continue");
                _spriteBatch.DrawString(Singleton.I.font, "You Win", new Vector2((Singleton.GAMEWIDTH - fontSize.X) / 2, ((Singleton.GAMEHEIGHT - fontSize.Y) / 2) - 50), Color.White);
                _spriteBatch.DrawString(Singleton.I.font, Singleton.I.score.ToString(), new Vector2((Singleton.GAMEWIDTH - fontSize2.X) / 2, (Singleton.GAMEHEIGHT - fontSize2.Y) / 2), Color.White);
                _spriteBatch.DrawString(Singleton.I.font, "Press Space Bar to continue", new Vector2((Singleton.GAMEWIDTH - fontSize3.X) / 2, ((Singleton.GAMEHEIGHT - fontSize3.Y) / 2) + 50), Color.White);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}