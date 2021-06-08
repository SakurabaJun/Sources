using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
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
    /// ボスのクラス
    /// </summary>
    public class Boss : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private ParticleSystem particleSystem;
        private Sphere sphere;
        public Sphere GetSphere { get { return sphere; } }
        private CharacterMaster chara;
        private Camera camera;
        private Mesh mesh;
        private BossUI ui;
        private Vector3 position;
        private int hp;
        private int maxHp = 15;
        private bool canHit = false;
        private const int interval = 7;
        private GameClear gameClear;
        private bool enableCollision = true;

        //当たり判定を有効にするかどうか
        public bool EnableCollsion
        {
            get { return enableCollision; }
            set { enableCollision = value; }
        }

        //体力
        public int HP
        {
            get { return hp; }
            set { hp = value; }
        }

        //最大の体力
        public int MaxHP
        {
            get { return maxHp; }
        }
        private float rotOffset = 0;
        public float RotOffset { get { return rotOffset; } set { rotOffset = value; } }
        private bool useCanHit = true;

        public Boss(Game game, int maxHp, string name, CharacterMaster characterMaster, Obstacle[] obs, Sphere sphere, Camera camera,string clip="none")
            : base(game)
        {
            this.chara = characterMaster;
            this.sphere = sphere;
            this.camera = camera;
            this.maxHp = maxHp;
            chara.SetBoss = this;
            position = new Vector3(0, -500, 0);
            hp = maxHp;
            MeshInit(name, clip, camera);
            UIInit(game);

            //ボスの力で障害物のモデルが氷に変わる
            if (obs != null)
            {
                foreach (Obstacle o in obs)
                {
                    Console.WriteLine(o.GetMesh.Position);
                    o.GetMesh.SetModel("Model\\Ice\\ice");
                }
            }

            //パーティクルの初期化
            ParticleInit(camera);
        }

        //UIの初期化
        private void UIInit(Game game)
        {
            ui = new BossUI(Game, this);
            game.Components.Add(ui);
        }

        //メッシュの初期化
        private void MeshInit(string name,string clip, Camera camera)
        {
            if (clip == "none")
            {
                //アニメーションクリップがない場合
                mesh = new Mesh(Game, name, camera);
                mesh.Scale = new Vector3(5.0f);
                mesh.SSize = 40;
            }
            else
            {
                //アニメーションクリップがある場合
                mesh = new Mesh(Game, name, clip, true, 0, camera);
                mesh.Scale = new Vector3(3.0f);
                mesh.SSize = 20;
                useCanHit = false;
                canHit = true;
            }
            mesh.Position = position;
            mesh.Rotation = new Vector3(0, 180, 0);
            Game.Components.Add(mesh);
        }

        //パーティクルの初期化
        private void ParticleInit(Camera camera)
        {
            particleSystem = new ParticleSystem(Game, "Model\\Ice\\cube", camera);
            particleSystem.MaxParticles = 10;
            particleSystem.MinStartSize = 1000;
            particleSystem.MaxStartSize = 2000;
            particleSystem.MinOSLVelocity = 5000;
            particleSystem.MaxOSLVelocity = 10000;
            particleSystem.MinDuration = 300;
            particleSystem.MaxDuration = 600;
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
            mesh.Dispose();
        }
        /// <summary>
        /// ゲーム コンポーネントが自身を更新するためのメソッドです。
        /// </summary>
        /// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: ここにアップデートのコードを追加します。
            
            //床判定
            if (position.Y<0)
            {
                position.Y += 1;
                mesh.Position = position;
            }

            //スマッシュボールではないときはヒットしない
            if (!sphere.NowSmashBall && canHit && useCanHit)   
            {
                canHit = false;
            }

            //ボスの回転
            BossRotation();

            //インプットなど
            if (InputManager.IsJustKeyDown(Keys.Enter))
            {
                if (!Game.Components.Contains(gameClear))
                {
                    gameClear = new GameClear(Game, chara);
                    Game.Components.Add(gameClear);
                }
            }
            if (InputManager.IsJustKeyDown(Keys.B))
            {
                canHit = true;
                ApplyDamage(gameTime, Vector3.Zero);
            }
            base.Update(gameTime);
        }

        //ダメージの適応
        public void ApplyDamage(GameTime gameTime,Vector3 pos)
        {
            //ダメージが入るときのみ
            if (canHit)
            {
                particleSystem.Position = pos;
                particleSystem.Start();
                hp--;
            }

            //HPが0以下になった場合
            if (hp <= 0 && gameClear == null) 
            {
                if (!Game.Components.Contains(gameClear))
                {
                    gameClear = new GameClear(Game, chara);
                    Game.Components.Add(gameClear);
                    sphere.Dispose();
                }
                Dispose();
            }
        }

        //ボスの回転
        private void BossRotation()
        {
            MainCharacter chara = Close();
            Vector3 rot = mesh.Rotation;
            float target = -GetAngle(chara) + 90 + rotOffset;
            target = Calculation.Degree360Conversion(target);
            Calculation.RotationShortes(ref rot.Y, ref target);
            rot.Y = Calculation.Finterp(rot.Y, target, 1.0f);
            mesh.Rotation = rot;
        }

        //一番近い方のキャラクターを返す
        private MainCharacter Close()
        {
            float chara1 = GetDistance(chara.GetChara1);
            float chara2 = GetDistance(chara.GetChara2);
            if (chara1 < chara2)
            {
                return chara.GetChara1;
            }
            else
            {
                return chara.GetChara2;
            }
        }

        //キャラクターとの角度を取得
        private float GetAngle(MainCharacter chara)
        {
            Vector3 myPos = mesh.Position;
            Vector3 charaPos = chara.GetMesh.Position;
            double r = Calculation.GetRadian(charaPos.X, charaPos.Z, myPos.X, myPos.Z);
            return MathHelper.ToDegrees((float)r);
        }

        //キャラクターとの距離を取得
        private float GetDistance(MainCharacter chara)
        {
            Vector3 myPos = mesh.Position;
            Vector3 charaPos = chara.GetMesh.Position;
            double d = Calculation.GetDistance(charaPos.X, charaPos.Z, myPos.X, myPos.Z);
            return (float)d;
        }

        //メッシュの取得
        public Mesh GetMesh
        {
            get { return mesh; }
        }

        //Hitすることができるか
        public bool GetCanHit
        {
            get { return canHit; }
        }

        //CanHitのセット
        public bool CanHit
        {
            get { return canHit; }
            set { canHit = value; }
        }
    }
}
