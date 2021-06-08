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
    /// ボタンの処理をするクラス
    /// </summary>
    public class Button : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private SpriteBatch spriteBatch;
        private Texture2D normal;
        private Texture2D select;
        private Texture2D texture;
        private Rectangle rect;
        private bool isSelect = false;
        private bool isFill = false;
        private SoundEffect push;

        public event EventHandler e_enter;

        public Button(Game game,string s_normal,string s_select,string pushSound)
            : base(game)
        {
            normal = Game.Content.Load<Texture2D>(s_normal);
            select = Game.Content.Load<Texture2D>(s_select);
            push = game.Content.Load<SoundEffect>(pushSound);
            rect = Rectangle.Empty;
            rect.Width = normal.Width;
            rect.Height = normal.Height;
            
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
        /// UnloadContent はゲームごとに 1 回呼び出され、ここですべてのコンテンツを
        /// アンロードします。
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: ここで ContentManager 以外のすべてのコンテンツをアンロードします。
            Game.Components.Remove(this);
        }
        /// <summary>
        /// ゲーム コンポーネントが自身を更新するためのメソッドです。
        /// </summary>
        /// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: ここにアップデートのコードを追加します。
            if (isSelect)
            {
                texture = select;

                //エンターキーを押したとき
                if (InputManager.IsJustKeyDown(Keys.Space))
                {
                    push.Play();
                    e_enter(this, EventArgs.Empty);
                }

                //Aボタンを押したとき
                foreach (PlayerIndex p in Enum.GetValues(typeof(PlayerIndex)))
                {
                    if (InputManager.IsConnected(p))
                    {
                        if (InputManager.IsJustButtonDown(p, Buttons.A))
                        {
                            push.Play();
                            e_enter(this, EventArgs.Empty);
                        }
                    }
                }
            }
            else
            {
                texture = normal;
            }
            base.Update(gameTime);
        }
        /// <summary>
        /// ゲームが自身を描画するためのメソッドです。
        /// </summary>
        /// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
        public override void Draw(GameTime gameTime)
        {
            // TODO: ここに描画コードを追加します。
            if (texture != null)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(texture, rect, Color.White);
                spriteBatch.End();
            }
            base.Draw(gameTime);
        }

        /// <summary>
        /// ボタンのRectangle
        /// </summary>
        public Rectangle Rect
        {
            get { return rect; }
            set { rect = value; }
        }

        /// <summary>
        /// 位置のセット
        /// </summary>
        public void SetPosition(int x, int y)
        {
            rect.X = x;
            rect.Y = y;
        }

        /// <summary>
        /// サイズのセット
        /// </summary>
        public void SetSize(int w, int h)
        {
            rect.Width = w;
            rect.Height = h;
        }

        /// <summary>
        /// 選択されているか
        /// </summary>
        public bool IsSelect
        {
            get { return isSelect; }
            set { isSelect = value; }
        }

        /// <summary>
        /// Fill描画するか
        /// </summary>
        public bool IsFill
        {
            get { return isFill; }
            set { isFill = value; }
        }
    }
}
