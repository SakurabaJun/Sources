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
    /// IUpdateable インターフェイスを実装したゲーム コンポーネントです。
    /// </summary>
    public class Stage2 : Microsoft.Xna.Framework.GameComponent
    {
        private Stage stage;
        private Obstacle[] pillar = null;
        private Slimes slimes;
        private const int pillarAmount = 0;
        public Stage2(Game game)
            : base(game)
        {
            //ステージの追加
            stage = new Stage(game, 10);
            if(!game.Components.Contains(stage))
            {
                game.Components.Add(stage);
            }

            //スライムの追加
            slimes = new Slimes(game, 3, 0, stage.GetField.GetWall, stage.GetChara, 2, stage.GetCamera);
            if (!game.Components.Contains(slimes))
            {
                game.Components.Add(slimes);
            }
            stage.GetSphere.SetObstacle = pillar;
            slimes.SetSphere = stage.GetSphere;
            stage.GetSphere.SetSlimes = slimes;
        }

        //柱の追加
        private void PillarInit(Game game,ref Obstacle p,Vector3 pos)
        {
            p = new Obstacle(game, "Model\\Hashira\\hashira", stage.GetCamera);
            p.GetMesh.Position = pos;
            p.GetMesh.SSize = 25;
            p.GetMesh.Scale = new Vector3(3, 3, 3);
            if (!game.Components.Contains(p))
            {
                game.Components.Add(p);
            }
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
            base.Update(gameTime);
        }
    }
}
