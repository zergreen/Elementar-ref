using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using EarthDefender.GameObject;

namespace EarthDefender
{
    class Singleton
    {
        public Random random = new Random();

        public const int GAMEWIDTH = 1280;
        public const int GAMEHEIGHT = 720;

        public Texture2D ballTexture, background, whiteTexture;

        public SpriteFont font;

        public KeyboardState PreviousKey, CurrentKey;
        public MouseState MouseState, PreviousMouseState;

        public Player player;

        public List<Ball> balls;
        public List<Particle> particles;

        public BallGrid ballGrid;

        public float wallSpeed;
        public float groundY;

        public int score, highscore;

        public enum GameState
        {
            GamePlaying,
            GamePaused,
            GameWin,
            GameLose
        }

        public GameState CurrentGameState;

        private static Singleton instance;

        private Singleton()
        {
        }

        public static Singleton Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Singleton();
                }
                return instance;
            }
        }

        public static Singleton I => Instance;

    }
}
