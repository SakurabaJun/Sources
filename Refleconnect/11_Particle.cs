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
    /// パーティクルシステムで使われるパーティクル単体の処理
    /// </summary>
    public class Particle : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private ParticleSystem master;//親のパーティクルシステム
        private Mesh mesh;
        public Mesh Mesh { get { return mesh; } set { mesh = value; } }
        private Game game;
        private Vector3 vector = Vector3.Zero;
        public Vector3 Vector { get { return vector; } set { vector = value; } }
        private float start = -1;
        private Vector3 startPos;
        private float duration;
        private float oslVelocity;
        private float startSize;
        private float endSize;
        private const float g = 9.80665f;
        private const float deltaCorrection = 10;

        public Particle(Game game, ParticleSystem master)
            : base(game)
        {
            // TODO: ここで子コンポーネントを作成します。
            this.master = master;
            this.game = game;
            Random r = new Random();

            //intを適切な値に変更する（durationの場合はミリ秒なので1000分の1）
            duration = r.Next(master.MinDuration, master.MaxDuration) * 0.001f;
            oslVelocity = r.Next(master.MinOSLVelocity, master.MaxOSLVelocity) * 0.01f;
            startSize = r.Next(master.MinStartSize, master.MaxStartSize) * 0.01f;
            endSize = r.Next(master.MinEndSize, master.MaxEndSize) * 0.01f;

            startPos = master.Position;

            vector.X = (float)r.Next(-10, 10) * 0.1f;
            vector.Y = (float)r.Next(-10, 10) * 0.1f;
            vector.Z = (float)r.Next(-10, 10) * 0.1f;
            mesh = new Mesh(game, master.GetMesh, master.GetCamera);
            mesh.Position = startPos;
            game.Components.Add(mesh);
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
            game.Components.Remove(mesh);
            mesh.Dispose();
        }
        /// <summary>
        /// ゲーム コンポーネントが自身を更新するためのメソッドです。
        /// </summary>
        /// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
        public override void Update(GameTime gameTime)
        {
            if (start == -1)
            {
                start = (float)gameTime.TotalGameTime.TotalSeconds;
            }
            else if (start + duration < gameTime.TotalGameTime.TotalSeconds)
            {
                //時間がたったら消滅させる
                Dispose();
            }

            float time = (float)gameTime.TotalGameTime.TotalSeconds - start;
            float rate = time / duration;
            SetPosition(time, rate);
            SetScale(rate);
            base.Update(gameTime);
        }
        
        //位置を時間とrateをもとに計算する
        private void SetPosition(float time,float rate)
        {
            Vector3 pos = mesh.Position;
            float gDistance = 0.5f * g * time * time;
            pos = startPos + (master.Gravity * gDistance) + Vector3.Lerp(Vector3.Zero, oslVelocity * vector, rate);
            mesh.Position = pos;
        }

        //rateをもとにScaleを計算
        private void SetScale(float rate)
        {
            float size = MathHelper.Lerp(startSize, endSize, rate);
            Vector3 vSize = master.Scale * size;
            mesh.Scale = vSize;
        }
    }
}
