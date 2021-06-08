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
    /// プレイヤー同市で受け渡しができるSphere
    /// </summary>
    public class Sphere : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private float delta;
        private Game game;
        private CharacterMaster chara;
        private Mesh sphere;

        //メッシュの取得
        public Mesh GetMesh
        {
            get { return sphere; }
        }
        private Vector3 vector;

        //進行方向の取得
        public Vector3 Vector
        {
            get { return vector; }
            set { vector = value; }
        }
        private Vector3 oldPos;
        private float moveSpeed = 200;
        private const float normalSpeed = 200;
        private const float smashSpeed = 300;

        //スマッシュボールかどうか
        public bool NowSmashBall
        {
            get { return (moveSpeed == smashSpeed); }
        }
        private Field field;

        //フィールドの取得
        public Field GetField
        {
            get { return field; }
        }
        private Obstacle[] obs;
        private int energy = 0;
        private const int multiple = 6;
        private const int maxEnergy = 18;

        //スマッシュボールに移行できるかどうか
        public bool EnableSmash
        {
            get { return energy >= multiple + 2; }
        }

        //打てるかどうか
        public bool EnableShot
        {
            get { return energy > multiple * 2; }
        }
        private Temperature temperature;
        private Slimes slimes;
        private Boss1 boss1;

        //ボス１のセット
        public Boss1 SetBoss1
        {
            set { boss1 = value; }
        }
        private Boss2 boss2;

        //ボス２のセット
        public Boss2 SetBoss2
        {
            set { boss2 = value; }
        }
        private Boss3 boss3;

        //ボス３のセット
        public Boss3 SetBoss3
        {
            set { boss3 = value; }
        }
        private Boss boss;

        //ボスのセット
        public Boss SetBoss
        {
            set { boss = value; }
        }
        private bool nowSmash = false;
        private bool hitDirection = false;

        //ヒットした向き
        public bool HitDirection
        {
            get { return hitDirection; }
            set { hitDirection = value; }
        }

        //スライムのセット
        public Slimes SetSlimes
        {
            set { slimes = value; }
        }
        private float smashStartTime = -1;
        private bool canDecay = true;
        private ParticleSystem particleSystem;
        private float particleStartTime = -1f;
        private float interval = 1f;
        private SoundEffect reflection;
        private SoundEffect beShot;

        public Sphere(Game game, int defEnergy, Vector3 position, Vector3 vector, Field field, CharacterMaster chara, Camera camera)
            : base(game)
        {
            // TODO: ここで子コンポーネントを作成します。
            this.game = game;
            sphere = new Mesh(game, "Model\\Ball\\ball1", camera);
            sphere.SSize = 25;
            sphere.Position = position;
            sphere.Scale = new Vector3(3, 3, 3);
            this.vector = vector;
            this.field = field;
            this.chara = chara;
            this.energy = defEnergy;

            //メッシュの追加
            if (!game.Components.Contains(sphere))
            {
                game.Components.Add(sphere);
            }

            //温度の追加
            temperature = new Temperature(game, this);
            if (!game.Components.Contains(temperature))
            {
                game.Components.Add(temperature);
            }

            //パーティクルシステムの追加
            particleSystem = new ParticleSystem(game, "Model\\Ice\\cube", camera);
            particleSystem.MaxParticles = 5;
            particleSystem.MinStartSize = 5;
            particleSystem.MaxStartSize = 10;
            particleSystem.MinOSLVelocity = 5000;
            particleSystem.MaxOSLVelocity = 10000;
            particleSystem.MinDuration = 200;
            particleSystem.MaxDuration = 400;
            Game.Components.Add(particleSystem);
            reflection = Game.Content.Load<SoundEffect>("Sounds\\reflection");
            beShot = Game.Content.Load<SoundEffect>("Sounds\\beShot");
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
            sphere.Dispose();
            Game.Components.Remove(this);
        }
        /// <summary>
        /// ゲーム コンポーネントが自身を更新するためのメソッドです。
        /// </summary>
        /// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: ここにアップデートのコードを追加します。
            delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            oldPos = sphere.Position;
            sphere.Position += vector * moveSpeed * delta;
            HitJudgment(gameTime);
            if (NowSmashBall)
            {
                sphere.EmissiveColor = new Vector3(1, 0, 0);
            }
            if (!EnableSmash)
            {
                EndSmash();
            }
            base.Update(gameTime);
        }

        //スマッシュの減衰処理
        private void SmashDecay(GameTime gameTime)
        {
            if (!NowSmashBall)
            {
                return;
            }
            if (smashStartTime == -1)
            {
                smashStartTime = (float)gameTime.TotalGameTime.TotalSeconds;
            }
            float nowTime = (float)gameTime.TotalGameTime.TotalSeconds - smashStartTime;
            if ((int)nowTime % 1 == 0)
            {
                if (canDecay)
                {
                    canDecay = false;
                    energy--;
                }
            }
            else
            {
                canDecay = true;
            }

        }

        //当たり判定
        private void HitJudgment(GameTime gameTime)
        {
            HitChara(chara.GetChara1, chara.GetChara2);
            HitChara(chara.GetChara2, chara.GetChara1);
            HitObstacle(oldPos);
            HitWall();
            HitSlime(oldPos);
            HitBoss(gameTime);
        }

        //障害物との当たり判定
        private void HitObstacle(Vector3 oldPos)
        {
            if (obs == null)
            {
                return;
            }
            foreach (Obstacle o in obs)
            {
                if (o != null)
                {
                    if (sphere.BSphere.Intersects(o.GetMesh.BSphere))
                    {
                        hitDirection = false;
                        SetCharaCollision(true);
                        sphere.Position = oldPos;
                        Calculation.HitSphere(o.GetMesh.Position, sphere.Position, ref vector);
                    }
                }
            }
        }

        //キャラクターとの当たり判定
        private void HitChara(MainCharacter chara, MainCharacter opponent)
        { 
            if (!EnableSmash)
            {
                moveSpeed = normalSpeed;
            }

            //モデルの変換
            if (energy / multiple == 0)
            {
                sphere.SetModel("Model\\Ball\\ball1");
                sphere.EmissiveColor = new Vector3(0, 0, 0);
            }
            if (energy / multiple == 1)
            {
                sphere.SetModel("Model\\Ball\\ball2");
                sphere.EmissiveColor = new Vector3(0.1f, 0, 0);
            }
            if (energy / multiple == 2)
            {
                sphere.SetModel("Model\\Ball\\ball3");
                sphere.EmissiveColor = new Vector3(0.2f, 0, 0);
            }

            //当たり判定
            if (sphere.BSphere.Intersects(chara.GetBackwardSphere))
            {
                //コリジョンが有効な時のみ
                if (chara.EnableCollision)
                {
                    chara.EnableCollision = false;
                    sphere.Position = oldPos;
                    chara.SetImpact(Calculation.HitSphere(chara.GetMesh.Position, sphere.Position, ref vector));
                    if (beShot != null) 
                    {
                        beShot.Play();
                    }
                    if (NowSmashBall)
                    {
                        this.chara.ApplyDamage(1);
                    }
                }
            }
            else if (!chara.EnableCollision)
            {
                chara.EnableCollision = true;
            }

            //ボールとの当たり判定
            if (sphere.BSphere.Intersects(chara.GetMesh.BSphere))
            {
                if (chara.EnableCollision)
                {
                    reflection.Play();
                    chara.EnableCollision = false;
                    if (chara.Skill == Skill.direction)
                    {
                        hitDirection = true;
                    }
                    if (chara.Skill == Skill.shot && hitDirection)
                    {
                        hitDirection = false;
                        nowSmash = true;
                        moveSpeed = smashSpeed;
                        sphere.Position = oldPos;
                        vector = -chara.GetMesh.GetForwardVector;
                    }
                    else if (chara.CheckLookingForward())
                    {
                        vector = Vector3.Normalize(opponent.GetMesh.Position - chara.GetMesh.Position);
                        if (chara.AddEnergy)
                        {
                            energy += 2;
                            chara.AddEnergy = false;
                            opponent.AddEnergy = true;
                            energy = Calculation.Clamp<int>(energy, 0, maxEnergy);
                        }
                    }
                    else
                    {
                        sphere.Position = oldPos;
                        chara.SetImpact(Calculation.HitSphere(chara.GetMesh.Position, sphere.Position, ref vector));
                        if (moveSpeed == smashSpeed)
                        {
                            this.chara.ApplyDamage(1);
                        }
                    }
                }
            }
            else if (!chara.EnableCollision)
            {
                chara.EnableCollision = true;
            }
        }

        //スマッシュ終了処理
        private void EndSmash()
        {
            chara.GetChara1.Skill = Skill.normal;
            chara.GetChara2.Skill = Skill.normal;
            nowSmash = false;
            smashStartTime = -1;
        }

        //Characterの当たり判定を有効にする、または無効に
        public void SetCharaCollision(bool e)
        {
            chara.GetChara1.EnableCollision = e;
            chara.GetChara2.EnableCollision = e;
        }

        //壁との当たり判定
        private void HitWall()
        {
            foreach (Plane w in field.GetWall)
            {
                if (w.Intersects(sphere.BSphere) == PlaneIntersectionType.Intersecting)
                {
                    hitDirection = false;
                    sphere.Position = oldPos;
                    vector = Vector3.Reflect(vector, w.Normal);
                }
            }
        }

        //スライムとの当たり判定
        private void HitSlime(Vector3 oldPos)
        {
            if (slimes == null)
            {
                return;
            }

            //スライムを一つずつ検証
            foreach (Slime s in slimes.GetSlimes)
            {
                if (sphere.BSphere.Intersects(s.GetMesh.BSphere))
                {
                    if (s.EnableCollision)
                    {
                        hitDirection = false;
                        s.EnableCollision = false;
                        if (nowSmash)
                        {
                            s.ApplyDamage();
                            energy -= 2;
                            Calculation.Clamp<int>(energy, 0, maxEnergy);
                        }
                        else
                        {
                            sphere.Position = oldPos;
                            s.Vector = Calculation.HitSphere(s.GetMesh.Position, sphere.Position, ref vector);
                        }
                        break;
                    }
                }
                else if (!s.EnableCollision)
                {
                    s.EnableCollision = true;
                }
            }
        }

        //ボスとの当たり判定
        private void HitBoss(GameTime gameTime)
        {
            if (boss == null)
            {
                return;
            }

            if (boss.GetMesh.BSphere.Intersects(sphere.BSphere))
            {
                //ボスの当たり判定が有効な時のみ
                if (boss.EnableCollsion)
                {
                    hitDirection = false;
                    boss.EnableCollsion = false;

                    //スマッシュボールの時のみ
                    if (NowSmashBall)
                    {
                        boss.ApplyDamage(gameTime, sphere.Position);
                    }
                    energy -= 2;
                    Calculation.Clamp<int>(energy, 0, maxEnergy);
                    SetCharaCollision(true);
                    sphere.Position = oldPos;
                    Calculation.HitSphere(boss.GetMesh.Position, sphere.Position, ref vector);
                }
            }
            else if (!boss.EnableCollsion)
            {
                boss.EnableCollsion = true;
            }
        }

        //障害物のセット
        public Obstacle[] SetObstacle
        {
            set { obs = value; }
        }

        //エネルギー
        public int Energy
        {
            get { return energy; }
            set { energy = value; }
        }

        //最大エネルギーのゲット
        public int GetMaxEnergy
        {
            get { return maxEnergy; }
        }

        //倍率の取得
        public int GetMultiple
        {
            get { return multiple; }
        }
        
        //一フレーム前の位置
        public void SetOldPosition()
        {
            sphere.Position = oldPos;
        }
    }
}
