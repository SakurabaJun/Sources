using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Refleconnect
{
    /// <summary>
    /// メインキャラクターを統括するクラス
    /// </summary>
    public class CharacterMaster : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private Game game;
        private SpriteBatch spriteBatch;
        private MainCharacter chara1;
        private MainCharacter chara2;
        private Vector2 position;
        private Texture2D t_hat;
        private Texture2D t_bg;
        private Texture2D t_hp;
        private int hp;
        private const int maxHp = 3;
        private GameOver gameOver;
        private Field field;
        public Field GetField
        {
            get { return field; }
        }

        /// <summary>
        /// メインキャラクターを統括するクラス
        /// </summary>
        /// <param name="game">コンポーネントの追加元</param>
        /// <param name="field">フィールド</param>
        /// <param name="camera">カメラ</param>
        public CharacterMaster(Game game,Field field,Camera camera)
            : base(game)
        {
            this.field = field;
            this.game = game;

            //キャラクター１のインスタンス化
            chara1 = new MainCharacter(game, "Model\\Character\\Black\\blackTpose", "Model\\Character\\Black\\Bmofazhen", PlayerIndex.One, field.GetWall, this, camera);
            chara1.GetMesh.Position = new Vector3(0, 0, 200);
            chara1.GetMesh.Rotation = new Vector3(0, 180, 0);
            if (!game.Components.Contains(chara1))
            {
                game.Components.Add(chara1);
            }

            //キャラクター２のインスタンス化
            chara2 = new MainCharacter(game, "Model\\Character\\White\\whiteTpose", "Model\\Character\\White\\Wmofazhen", PlayerIndex.Two, field.GetWall, this, camera);
            chara2.GetMesh.Position = new Vector3(100, 0, 150);
            chara2.GetMesh.Rotation = new Vector3(0, -90, 0);
            if (!game.Components.Contains(chara2))
            {
                game.Components.Add(chara2);
            }

            //相手の設定
            chara1.SetOpponent(chara2);
            chara2.SetOpponent(chara1);
            hp = maxHp;
            position.X = 350;
            position.Y = 80;
            t_hat = game.Content.Load<Texture2D>("Textures\\hat");
            t_bg = game.Content.Load<Texture2D>("Textures\\Health\\bg");
            t_hp = game.Content.Load<Texture2D>("Textures\\Health\\hp");
            DrawOrder = 1;
            // TODO: ここで子コンポーネントを作成します。
        }

        /// <summary>
        /// ゲーム コンポーネントの初期化を行います。
        /// ここで、必要なサービスを照会して、使用するコンテンツを読み込むことができます。
        /// </summary>
        public override void Initialize()
        {
            // TODO: ここに初期化のコードを追加します。

            base.Initialize();
        }
        /// <summary>
        /// LoadContent はゲームごとに 1 回呼び出され、ここですべてのコンテンツを
        /// 読み込みます。
        /// </summary>
        protected override void LoadContent()
        {
            // 新規の SpriteBatch を作成します。これはテクスチャーの描画に使用できます。
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: this.Content クラスを使用して、ゲームのコンテンツを読み込みます。
        }
        /// <summary>
        /// ゲーム コンポーネントが自身を更新するためのメソッドです。
        /// </summary>
        /// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: ここにアップデートのコードを追加します。

            base.Update(gameTime);
        }
                /// <summary>
        /// ゲームが自身を描画するためのメソッドです。
        /// </summary>
        /// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(t_hat,Vector2.Zero, Color.White);
            for (int i = 0; i < maxHp; i++)
            {
                spriteBatch.Draw(t_bg, new Vector2(position.X + (i * t_bg.Width), position.Y), Color.White);
            }
            for (int i = 0; i < hp; i++)
            {
                spriteBatch.Draw(t_hp, new Vector2(position.X + (i * t_bg.Width), position.Y), Color.White);
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
        public void SetSphere(Sphere sphere)
        {
            chara1.SetSphere(sphere);
            chara2.SetSphere(sphere);
        }
        public MainCharacter GetChara1
        {
            get { return chara1; }
        }
        public MainCharacter GetChara2
        {
            get { return chara2; }
        }
        public Boss SetBoss
        {
            set 
            { 
                chara1.SetBoss = value;
                chara2.SetBoss = value;
            }
        }

        //ダメージを与える関数
        public void ApplyDamage(int d)
        {
            hp -= d;
            if (hp <= 0)
            {
                if (!game.Components.Contains(gameOver))
                {
                    gameOver = new GameOver(game, this);
                    game.Components.Add(gameOver);
                    chara1.GetSphere.Dispose();
                }
            }
        }
        public bool SetMove
        {
            set
            {
                chara1.SetMove = value;
                chara2.SetMove = value;
            }
        }
    }
}
