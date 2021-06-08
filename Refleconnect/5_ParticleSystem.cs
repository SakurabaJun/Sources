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
    /// パーティクルを描画するクラス
    /// </summary>
    public class ParticleSystem : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private Game game;
        private Camera camera;
        public Camera GetCamera { get { return camera; } }
        private string textureName;
        private Model mesh;

        /// <summary>
        /// Model
        /// </summary>
        public Model GetMesh { get { return mesh; } }

        private Vector3 position = Vector3.Zero;
        /// <summary>
        /// 位置
        /// </summary>
        public Vector3 Position { get { return position; } set { position = value; } }

        private Vector3 rotation = Vector3.Zero;
        /// <summary>
        /// 回転
        /// </summary>
        public Vector3 Rotation { get { return rotation; } set { rotation = value; } }

        private Vector3 scale = Vector3.One;
        /// <summary>
        /// 拡大縮小
        /// </summary>
        public Vector3 Scale { get { return scale; } set { scale = value; } }

        private int maxParticles = 10;
        /// <summary>
        /// パーティクルが生成される間隔
        /// </summary>
        public int MaxParticles { get { return MaxParticles; } set { maxParticles = value; } }

        private int minDuration = 1000;
        /// <summary>
        /// 持続時間の最小値
        /// </summary>
        public int MinDuration { get { return minDuration; } set { minDuration = value; } }

        private int maxDuration = 1000;
        /// <summary>
        /// 持続時間の最大値
        /// </summary>
        public int MaxDuration { get { return maxDuration; } set { maxDuration = value; } }

        private int minOSLVelocity = 100;
        /// <summary>
        /// 一秒後の速度の最小値
        /// </summary>
        public int MinOSLVelocity { get { return minOSLVelocity; } set { minOSLVelocity = value; } }

        private int maxOSLVelocity = 100;
        /// <summary>
        /// 一秒後の速度の最大値
        /// </summary>
        public int MaxOSLVelocity { get { return maxOSLVelocity; } set { maxOSLVelocity = value; } }

        private Vector3 gravity = Vector3.Zero;
        /// <summary>
        /// 重力
        /// </summary>
        public Vector3 Gravity { get { return gravity; } set { gravity = value; } }

        private int minStartSize = 100;
        /// <summary>
        /// 生成時の最小サイズ
        /// </summary>
        public int MinStartSize { get { return minStartSize; } set { minStartSize = value; } }

        private int maxStartSize = 100;
        /// <summary>
        /// 生成時の最大サイズ
        /// </summary>
        public int MaxStartSize { get { return maxStartSize; } set { maxStartSize = value; } }

        private int minEndSize = 100;
        /// <summary>
        /// 終了時の最小サイズ
        /// </summary>
        public int MinEndSize { get { return minEndSize; } set { minEndSize = value; } }

        private int maxEndSize = 100;
        /// <summary>
        /// 終了時の最大サイズ
        /// </summary>
        public int MaxEndSize { get { return maxEndSize; } set { maxEndSize = value; } }

        private bool endDispose = false;
        /// <summary>
        /// 最後にDispaseするか
        /// </summary>
        public bool EndDispose { get { return endDispose; } set { endDispose = value; } }

        public ParticleSystem(Game game,string name,Camera camera)
            : base(game)
        {
            this.game = game;
            this.textureName = name;
            this.camera = camera;
            mesh = game.Content.Load<Model>(name);
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
            game.Components.Remove(this);
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
            // TODO: ここに描画コードを追加します。
            base.Draw(gameTime);
        }
        private void AddParticle()
        {
            Particle np = new Particle(game, this);
            game.Components.Add(np);
        }
        public void Start()
        {
            for (int i = 0; i < maxParticles; i++)
            {
                AddParticle();
            }
            if (endDispose)
            {
                Dispose();
            }
        }
    }
}
