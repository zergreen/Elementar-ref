using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EarthDefender
{
    class BallGrid
    {
        public readonly int width, height;

        private float yOffset = 0;
        private float xOffset = 0;

        private Ball[,] balls;
        private float ballSize = 64;

        private float gridWidth;
        private float gridHeight;

        public BallGrid(int width, int height)
        {
            this.width = width;
            this.height = height;
            balls = new Ball[width, height];
            gridWidth = ballSize;
            gridHeight = gridWidth;
        }

        public void Move(float dx, float dy)
        {
            xOffset += dx;
            yOffset += dy;
            ForEachBall(ball =>
            {
                ball.position.X += dx;
                ball.position.Y += dy;
            });
        }

        public bool AddBallToGrid(Ball ball)
        {
            var gridPos = WorldToGridPosition(ball.position.X, ball.position.Y);
            if (gridPos != null && ContainPosition(gridPos.Value.X, gridPos.Value.Y))
            {
                SetBall(gridPos.Value.X, gridPos.Value.Y, ball);
                return true;
            }
            return false;
        }

        public bool AddBallToGridIfEmpty(Ball ball)
        {
            var gridPos = WorldToGridPosition(ball.position.X, ball.position.Y);
            if (gridPos != null && ContainPosition(gridPos.Value.X, gridPos.Value.Y) && GetBall(gridPos.Value.X, gridPos.Value.Y) == null)
            {
                SetBall(gridPos.Value.X, gridPos.Value.Y, ball);
                return true;
            }
            return false;
        }

        public Ball GetBall(int x, int y)
        {
            return balls[x, y];
        }

        public void SetBall(int x, int y, Ball ball)
        {
            balls[x, y] = ball;
            if (ball != null)
            {
                ball.position = GridToWorldPosition(x, y);
            }

        }

        public Ball TryGetBall(int x, int y)
        {
            return ContainPosition(x, y) ? GetBall(x, y) : null;
        }

        public List<Ball> GetAdjacentBalls(int x, int y)
        {
            List<Ball> adjacents = new List<Ball>();

            var topLeft = (y % 2) == 0 ? TryGetBall(x - 1, y - 1) : TryGetBall(x, y - 1);
            if (topLeft != null)
            {
                adjacents.Add(topLeft);
            }

            var topRight = (y % 2) == 0 ? TryGetBall(x, y - 1) : TryGetBall(x + 1, y - 1);
            if (topRight != null)
            {
                adjacents.Add(topRight);
            }

            var middleLeft = TryGetBall(x - 1, y);
            if (middleLeft != null)
            {
                adjacents.Add(middleLeft);
            }

            var middleRight = TryGetBall(x + 1, y);
            if (middleRight != null)
            {
                adjacents.Add(middleRight);
            }

            var bottomLeft = (y % 2) == 0 ? TryGetBall(x - 1, y + 1) : TryGetBall(x, y + 1);
            if (bottomLeft != null)
            {
                adjacents.Add(bottomLeft);
            }

            var bottomRight = (y % 2) == 0 ? TryGetBall(x, y + 1) : TryGetBall(x + 1, y + 1);
            if (bottomRight != null)
            {
                adjacents.Add(bottomRight);
            }

            return adjacents;
        }

        public Vector2 GridToWorldPosition(int x, int y)
        {
            float halfGridWidth = gridWidth / 2f;
            var worldPos = new Vector2(
                x * gridWidth + xOffset,
                y * gridHeight + yOffset
                );
            if (y % 2 == 0) worldPos.X -= halfGridWidth;
            return worldPos;
        }

        public Point? WorldToGridPosition(float x, float y)
        {
            Point pos = new();
            pos.Y = (int)MathF.Round((y - yOffset) / gridWidth);
            if (pos.Y % 2 == 0)
            {
                pos.X = (int)MathF.Round((x - xOffset + gridWidth / 2f) / gridWidth);
            }
            else
            {
                pos.X = (int)MathF.Round((x - xOffset) / gridWidth);
            }

            return ContainPosition(pos.X, pos.Y) ? pos : null;
        }

        public bool ContainPosition(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        public void ForEachBall(Action<Ball> action)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var ball = TryGetBall(x, y);
                    if (ball != null)
                    {
                        action(ball);
                    }
                }
            }
        }

        public void RemoveBallIf(Func<Ball, bool> condition)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var ball = TryGetBall(x, y);
                    if (ball != null && condition(ball))
                    {
                        SetBall(x, y, null);
                    }
                }
            }
        }
    }
}