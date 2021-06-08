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
    /// ボタンを統括するクラス
    /// </summary>
    public class ButtonMaster : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private Game game;
        private List<Button> buttons;
        private Vector2[] vec;
        private Vector2[] vecOld;
        private const float stickWidth = 0.5f;
        private SoundEffect move;
        private enum Dir
        {
            up,
            down,
            right,
            left
        }
        public ButtonMaster(Game game, SoundEffect move)
            : base(game)
        {
            this.game = game;
            this.move = move;
            buttons = new List<Button>();
            vec = new Vector2[4];
            vecOld = new Vector2[4];
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
            if (InputManager.IsJustKeyDown(Keys.Up))
            {
                UpdateButton(Dir.up);
            }
            if (InputManager.IsJustKeyDown(Keys.Down)) 
            {
                UpdateButton(Dir.down);
            }
            if (InputManager.IsJustKeyDown(Keys.Right))
            {
                UpdateButton(Dir.right);
            }
            if (InputManager.IsJustKeyDown(Keys.Left))
            {
                UpdateButton(Dir.left);
            }

            //接続されているすべてのコントローラーでボタン移動ができるように
            foreach(PlayerIndex p in Enum.GetValues(typeof(PlayerIndex)))
            {
                if (InputManager.IsConnected(p))
                {
                    int player = 0;
                    switch (p)
                    {
                        case PlayerIndex.One:
                            player = 1;
                            break;
                        case PlayerIndex.Two:
                            player = 2;
                            break;
                        case PlayerIndex.Three:
                            player = 3;
                            break;
                        case PlayerIndex.Four:
                            player = 4;
                            break;

                    }

                    //押されたボタンから入力
                    if (InputManager.IsJustPressedDPadRight(p))
                    {
                        UpdateButton(Dir.right);
                    }
                    if (InputManager.IsJustPressedDPadLeft(p))
                    {
                        UpdateButton(Dir.left);
                    }
                    if (InputManager.IsJustPressedDPadUp(p))
                    {
                        UpdateButton(Dir.up);
                    }
                    if (InputManager.IsJustPressedDPadDown(p))
                    {
                        UpdateButton(Dir.down);
                    }

                    //押されたスティックから入力
                    vec[player] = InputManager.GetThumbSticksLeft(p);
                    if (vec[player].X > stickWidth && vecOld[player].X < stickWidth) 
                    {
                        UpdateButton(Dir.right);
                    }
                    if (vec[player].X < -stickWidth && vecOld[player].X > -stickWidth)
                    {
                        UpdateButton(Dir.left);
                    }
                    if (vec[player].Y > stickWidth && vecOld[player].Y < stickWidth) 
                    {
                        UpdateButton(Dir.up);
                    }
                    if (vec[player].Y < -stickWidth && vecOld[player].Y > -stickWidth)
                    {
                        UpdateButton(Dir.down);
                    }
                    vecOld[player] = vec[player];
                }
            }
            base.Update(gameTime);
        }


        /// <summary>
        /// ボタンを追加する
        /// </summary>
        /// <param name="button">ボタン</param>
        public void AddButton(Button button)
        {
            buttons.Add(button);
            if (!game.Components.Contains(button))
            {
                game.Components.Add(button);
            }
            buttons[0].IsSelect = true;
        }

        //ボタンの更新
        private void UpdateButton(Dir dir)
        {
            int select = -1;
            Rectangle rect = Rectangle.Empty;

            //選択されているボタンを取得
            for (int i = 0; i < buttons.Count; i++)
            {
                if (buttons[i].IsSelect)
                {
                    select = i;
                    rect = buttons[i].Rect;
                    break;
                }
            }
            if (select == -1)
            {
                //選択されているボタンが無ければreturn
                return;
            }

            List<Button> findDir = new List<Button>();
            FindDirection(dir, rect, findDir);
            if (findDir.Count == 0)
            {
                return;
            }

            //近くにあるボタンをチェックしあれば移動する
            int nextSelect = NearCheck(rect, findDir);
            buttons[select].IsSelect = false;
            findDir[nextSelect].IsSelect = true;
            move.Play();
        }

        //角度の検索
        private void FindDirection(Dir dir, Rectangle rect, List<Button> findDir)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                switch (dir)
                {
                    case Dir.up:
                        if (rect.X == buttons[i].Rect.X && rect.Y > buttons[i].Rect.Y)
                        {
                            findDir.Add(buttons[i]);
                        }
                        break;
                    case Dir.down:
                        if (rect.X == buttons[i].Rect.X && rect.Y < buttons[i].Rect.Y)
                        {
                            findDir.Add(buttons[i]);
                        }
                        break;
                    case Dir.right:
                        if (rect.Y == buttons[i].Rect.Y && rect.X < buttons[i].Rect.X)
                        {
                            findDir.Add(buttons[i]);
                        }
                        break;
                    case Dir.left:
                        if (rect.Y == buttons[i].Rect.Y && rect.X > buttons[i].Rect.X)
                        {
                            findDir.Add(buttons[i]);
                        }
                        break;
                }
            }
            if (findDir.Count != 0)
            {
                return;
            }

            for (int i = 0; i < buttons.Count; i++)
            {
                switch (dir)
                {
                    case Dir.up:
                        if (rect.Y > buttons[i].Rect.Y)
                        {
                            findDir.Add(buttons[i]);
                        }
                        break;
                    case Dir.down:
                        if (rect.Y < buttons[i].Rect.Y)
                        {
                            findDir.Add(buttons[i]);
                        }
                        break;
                    case Dir.right:
                        if (rect.X < buttons[i].Rect.X)
                        {
                            findDir.Add(buttons[i]);
                        }
                        break;
                    case Dir.left:
                        if (rect.X > buttons[i].Rect.X)
                        {
                            findDir.Add(buttons[i]);
                        }
                        break;
                }
            }
        }

        //近い位置のボタンの検索
        private int NearCheck(Rectangle rect,List<Button> findDir)
        {
            int n = 0;
            double tmp = 0;
            double direction = 0;
            Console.WriteLine(findDir.Count);
            for (int i = 0; i < findDir.Count; i++)
            {
                //角度を算出
                direction = Calculation.GetDistance(rect.Center.X, rect.Center.X, 
                    findDir[i].Rect.Center.X, findDir[i].Rect.Center.X);
                if (i == 0)
                {
                    tmp = direction;
                }
                else if (tmp > direction)
                {
                    n = i;
                    tmp = direction;
                }
            }
            return n;
        }
    }
}
