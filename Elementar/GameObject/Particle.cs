using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarthDefender.GameObject
{

    class Particle
    {
        private Texture2D texture;
        private Rectangle sourceRectangle;

        public Vector2 position;
        public Vector2 velocity;
        public Vector2 scale = Vector2.One;

        private float animationDuration;
        private int maxFrame;
        private float duration;

        private float animationTime = 0f;
        private int currentFrame = 0;
        private float lifetime = 0;

        private bool expired = false;

        public Particle(Texture2D texture, Rectangle sourceRectangle, Vector2 position, float duration, float animationDuration, int maxFrame)
        {
            this.texture = texture;
            this.sourceRectangle = sourceRectangle;
            this.position = position;
            this.animationDuration = animationDuration;
            this.maxFrame = maxFrame;
            this.duration = duration;
        }

        public void Update(GameTime gameTime)
        {
            float deltatime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (expired) return;

            lifetime += deltatime;

            if (lifetime >= duration)
            {
                expired = true;
            }

            animationTime += deltatime;
            var durationPerFrame = animationDuration / maxFrame;
            if (animationTime >= durationPerFrame)
            {
                currentFrame = (currentFrame + 1) % maxFrame;
                animationTime -= durationPerFrame;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (expired) return;
            Rectangle rectangle = new Rectangle(sourceRectangle.X, sourceRectangle.Y, sourceRectangle.Width, sourceRectangle.Height);
            rectangle.X += currentFrame * sourceRectangle.Width;
            Vector2 origin = new Vector2(sourceRectangle.Width, sourceRectangle.Height) / 2f;
            spriteBatch.Draw(texture, position, rectangle, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
        }

        public bool IsExpired() => expired;
    }
}
