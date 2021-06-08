using System;
using System.Timers;
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
    /// スライムの処理
    /// </summary>
    public class Slime : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private Game game;
        private CharacterMaster chara;
        private Random rnd = new Random();
        private Mesh mesh;
        private Plane[] wall;
        private Vector3 vector;
        private const float velocity = 100f;
        private Obstacle[] obs;
        private bool enableCollision = true;
        private const float followDistance = 100;
        private Vector3 offset1;
        private Vector3 offset2;
        private bool useOffset1 = true;
        private bool offsetChange = false;
        private const int interval = 10;
        private float defY;
        private const float jumpSpeed = 100;
        private const float g = 9.80665f;
        private const float gMult = 20;
        private bool isJump = false;
        private float jumpStartTime;
        private float defScaleY;
        private ParticleSystem particleSystem;

        //スライムのタイプ
        public enum SlimeType
        {
            normal,
            ice
        }
        private SlimeType slimeType;
        private Slimes master;
        private Camera camera;

        public Slime(Game game,Slimes master,SlimeType slimeType, Plane[] wall,CharacterMaster chara, float d, Vector3 pos,Vector3 offset1,Vector3 offset2, Camera cam)
            : base(game)
        {
            this.game = game;
            int seed = Environment.TickCount;
            this.chara = chara;
            this.offset1 = offset1;
            this.offset2 = offset2;
            this.defY = pos.Y;
            this.slimeType = slimeType;
            this.master = master;

            //スライムのタイプによってメッシュを変える
            switch (slimeType)
            {
                case SlimeType.normal:
                    mesh = new Mesh(game, "Model\\Character\\Slime\\Normal\\slmtpose", "slm", true, 0, cam);
                    break;
                case SlimeType.ice:
                    mesh = new Mesh(game, "Model\\Character\\Slime\\Ice\\slm1", cam);
                    break;
            }


            mesh.Scale = new Vector3(3.0f);//大きさ
            mesh.SSize = 12;//敵の当たり判定サイズ
            mesh.Position = pos;
            this.defScaleY = mesh.Scale.Y;
            vector.X = (float)(mesh.Position.X * Math.Cos(d));
            vector.Z = (float)(mesh.Position.Z * Math.Sin(d));
            vector.Normalize();
            vector = Vector3.Left;
            Vector3 rot = mesh.Rotation;
            float r = (float)Calculation.GetRadian(mesh.Position.Z, mesh.Position.X, mesh.Position.Z + vector.Z, mesh.Position.X + vector.X);
            rot.Y = MathHelper.ToDegrees(r);
            mesh.Rotation = rot;

            //メッシュの追加
            if (!game.Components.Contains(mesh))
            {
                game.Components.Add(mesh);
            }

            this.wall = wall;
            InitParticle(cam);
            this.camera = cam;
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
            particleSystem.Position = mesh.Position;
            particleSystem.Start();
            game.Components.Remove(this);
            mesh.SSize = 0;
            mesh.Dispose();
        }
        /// <summary>
        /// ゲーム コンポーネントが自身を更新するためのメソッドです。
        /// </summary>
        /// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: ここにアップデートのコードを追加します。
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector3 oldPos = mesh.Position;
            mesh.Position += vector * velocity * delta;
            HitObstacle(oldPos);
            HitWall(oldPos);
            Follow(gameTime);
            Jump(gameTime);
            base.Update(gameTime);
        }

        //パーティクルの初期化
        private void InitParticle(Camera camera)
        {
            particleSystem = new ParticleSystem(game, "Model\\Ice\\cube", camera);
            particleSystem.MaxParticles = 5;
            particleSystem.MinStartSize = 1000;
            particleSystem.MaxStartSize = 2000;
            particleSystem.MinOSLVelocity = 5000;
            particleSystem.MaxOSLVelocity = 10000;
            particleSystem.MinDuration = 300;
            particleSystem.MaxDuration = 600;
            particleSystem.Gravity = Vector3.Up;
            particleSystem.EndDispose = true;
        }

        //ダメージの適応
        public void ApplyDamage()
        {
            switch(slimeType)
            {
                case SlimeType.normal:
                    master.ApplyDamage(this);
                    Dispose();
                    break;
                case SlimeType.ice:

                    //アイスだった場合は通常スライムにメッシュを変更

                    slimeType = SlimeType.normal;
                    Vector3 pos = mesh.Position;
                    mesh.Dispose();
                    mesh = new Mesh(game, "Model\\Character\\Slime\\Normal\\slmtpose", "slm", true, 0, camera);
                    mesh.Scale = new Vector3(3.0f);
                    mesh.SSize = 12;
                    mesh.Position = pos;
                    Game.Components.Add(mesh);
                    break;
                default:
                    break;
            }
        }

        //ジャンプの処理
        private void Jump(GameTime gameTime)
        {
            float nowTime = (float)gameTime.TotalGameTime.TotalSeconds;
            if (!isJump)
            {
                isJump = true;
                jumpStartTime = nowTime;

            }

            //ジャンプしているときのみ
            if (isJump)
            {
                Vector3 pos = mesh.Position;
                Vector3 scale = mesh.Scale;
                float time = nowTime - jumpStartTime;
                float rate = time / 1;
                float gDistance = 0.5f * g * time * time;
                pos.Y = (defY - gDistance * gMult) + MathHelper.Lerp(0, jumpSpeed, rate);

                //少しだけ形を変える（スライムっぽく)
                scale.Y = MathHelper.Lerp(defScaleY * 0.6f, defScaleY, rate);
                if (defY > pos.Y)
                {
                    pos.Y = defY;
                    isJump = false;
                }
                mesh.Position = pos;
                mesh.Scale = scale;
            }
        }

        //壁との当たり判定
        private void HitWall(Vector3 oldPos)
        {
            foreach (Plane w in wall)
            {
                if (w.Intersects(mesh.BSphere) == PlaneIntersectionType.Intersecting)
                {
                    mesh.Position = oldPos;
                    vector = Vector3.Reflect(vector, w.Normal);//反射ベクトル
                }
            }
            float r = (float)Calculation.GetRadian(mesh.Position.Z, mesh.Position.X, mesh.Position.Z + vector.Z, mesh.Position.X + vector.X);
            Vector3 rot = mesh.Rotation;
            float target = MathHelper.ToDegrees((float)r);
            target = Calculation.Degree360Conversion(target);
            Calculation.RotationShortes(ref rot.Y, ref target);
            rot.Y = Calculation.Finterp(rot.Y, target, 5.0f);
            mesh.Rotation = rot;
        }

        //障害物との当たり判定
        private void HitObstacle(Vector3 oldPos)
        {
            if (obs != null)
            {
                foreach (Obstacle o in obs)
                {
                    if (o.GetMesh.BSphere.Intersects(mesh.BSphere))
                    {
                        mesh.Position = oldPos;
                        Calculation.HitSphere(o.GetMesh.Position, mesh.Position, ref vector);
                    }
                }
            }
        }

        //近いキャラクターについてくるようにする
        private void Follow(GameTime gameTime)
        {
            MainCharacter c = Close();
            Vector3 targetPos;

            //10秒に一度挙動を変える
            if (gameTime.TotalGameTime.Seconds % 10 == 0)
            {
                if (!offsetChange)
                {
                    offsetChange = true;
                    useOffset1 = !useOffset1;
                }
            }
            else
            {
                offsetChange = false;
            }
            if (useOffset1)
            {
                targetPos = (c.GetMesh.Position + c.GetMesh.GetForwardVector * followDistance) + offset1;
            }
            else
            {
                targetPos = (c.GetMesh.Position + c.GetMesh.GetForwardVector) + offset2;
            }

            Vector3 v = Vector3.Normalize(targetPos - mesh.Position);
            v.Y = vector.Y;
            vector = v;
            Vector3 rot = mesh.Rotation;
            float target = -GetAngle(c);
            target = Calculation.Degree360Conversion(target);
            Calculation.RotationShortes(ref rot.Y, ref target);
            rot.Y = Calculation.Finterp(rot.Y, target, 1.0f);
            mesh.Rotation = rot;
        }

        //近い方のキャラクターを返す
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

        //障害物のセット
        public Obstacle[] SetObstacle
        {
            set { obs = value; }
        }

        //メッシュの取得
        public Mesh GetMesh
        {
            get { return mesh; }
        }

        //移動方向の取得
        public Vector3 Vector
        {
            get { return vector; }
            set { vector = value; }
        }

        //当たり判定が有効かどうか
        public bool EnableCollision
        {
            get { return enableCollision; }
            set { enableCollision = value; }
        }
    }
}
