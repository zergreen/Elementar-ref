using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EarthDefender
{
    public class Ball
    {
        private Texture2D texture, _ball;
        public Vector2 position = Vector2.Zero;
        public Vector2 velocity = Vector2.Zero;
        public Vector2 scale = Vector2.One;

        public int type;

        private float gravity = 1200f;

        public bool NoGravity = false;

        private bool dying = false;
        private bool dead = false;
        private float dyingTime = 0f;
        private float dieAnimationTime = 1f;

        private float animationTime = 0f;
        private float animationDuration = 0.5f;
        private int currentFrame = 0;
        private int maxFrame = 4;

        public Ball(Texture2D texture, Vector2 position)
        {
            this.texture = texture;
            this.position = position;
        }

        public void Update(GameTime gameTime)
        {
            float deltatime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (dying)
            {
                if (!dead)
                {
                    dyingTime += deltatime;
                    float dyingProgress = dyingTime / dieAnimationTime;
                    if (dyingTime >= dieAnimationTime)
                    {
                        dead = true;
                    }
                    else
                    {
                        scale.X = (1 - dyingProgress);
                        scale.Y = (1 - dyingProgress);
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

            position += velocity * deltatime; // Sx = Vx * t

            if (!NoGravity)
            {
                velocity.Y += gravity * deltatime;
            }

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Color color = Color.White;
            if (dying)
            {
                float dyingProgress = dyingTime / dieAnimationTime;
                float alpha = (1 - dyingProgress) * (1 - dyingProgress);
                color *= alpha;
            }
            Rectangle sourceRectangle = new Rectangle(type * 64, 0, 64, 64);

            if (IsBlackBall())
            {
                sourceRectangle.Y = 386;
                sourceRectangle.X = currentFrame * 64;
            }
            else if (IsRasenganBall())
            {
                sourceRectangle.Y = 386;
                sourceRectangle.X = (64 * 4) + currentFrame * 64;
            }

            Vector2 origin = new Vector2(sourceRectangle.Width, sourceRectangle.Height) / 2f;
            spriteBatch.Draw(texture, position, sourceRectangle, color, 0f, origin, scale, SpriteEffects.None, 0f);
        }


        public bool CollideWith(Ball other, out Vector2 direction, out float distance)
        {
            float radius = 64 / 2;

            float ditsance = Vector2.Distance(other.position, this.position);
            if (ditsance <= radius * 2)
            {
                direction = ditsance == 0f ? Vector2.Zero : Vector2.Normalize(other.position - this.position);
                distance = radius * 2 - ditsance;
                return true;
            }
            else
            {
                direction = Vector2.Zero;
                distance = 0.0f;
                return false;
            }
        }

        public void Destroy(bool instant = false)
        {
            dying = true;
            dyingTime = 0f;
            if (instant)
            {
                dead = true;
            }
        }

        public bool IsDying()
        {
            return dying;
        }

        public bool IsDead()
        {
            return dead;
        }

        public bool IsNormalBall()
        {
            return type < 5;
        }

        public bool IsBlankBall()
        {
            return type == 5;
        }

        public bool IsBlackBall()
        {
            return type == 6;
        }

        public bool IsRasenganBall()
        {
            return type == 7;
        }

        public bool CanPopTogether(Ball other)
        {
            if (IsNormalBall())
            {
                return other.IsNormalBall() ? type == other.type : false;
            }
            return false;
        }
    }
}