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
    /// ステージのクラス
    /// </summary>
    public class Stage : Microsoft.Xna.Framework.GameComponent
    {
        private Camera camera;
        private Field field;

        //フィールドの取得
        public Field GetField
        {
            get { return field; }
        }
        private Sphere sphere;
        private CharacterMaster chara;
        private ParticleSystem p;
        private Mesh m;

        public Stage(Game game, int defEnergy)
            : base(game)
        {
            // TODO: ここで子コンポーネントを作成します。

            //カメラの追加
            camera = new Camera(game);
            camera.Move(new Vector3(0, 500, -350));
            camera.SetRotation(new Vector3(MathHelper.ToRadians(10), MathHelper.ToRadians(180), 0));
            if (!game.Components.Contains(camera))
            {
                game.Components.Add(camera);
            }

            //フィールドの追加
            field = new Field(game, camera);
            if (!game.Components.Contains(field))
            {
                game.Components.Add(field);
            }

            //キャラの追加
            chara = new CharacterMaster(game, field, camera);
            if (!game.Components.Contains(chara))
            {
                game.Components.Add(chara);
            }

            //Sphereの追加
            sphere = new Sphere(game, defEnergy, new Vector3(0, 25, 150), Vector3.Left, field, chara, camera);
            if (!game.Components.Contains(sphere))
            {
                game.Components.Add(sphere);
            }
            chara.SetSphere(sphere);
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
        /// ゲーム コンポーネントが自身を更新するためのメソッドです。
        /// </summary>
        /// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: ここにアップデートのコードを追加します。
            if (InputManager.IsKeyDown(Keys.NumPad2))
            {
                p.Start();
            }
            base.Update(gameTime);
        }
        public Sphere GetSphere
        {
            get { return sphere; }
        }
        public Camera GetCamera
        {
            get { return camera; }
        }
        public CharacterMaster GetChara
        {
            get { return chara; }
        }
    }
}
