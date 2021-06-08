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
    /// メインキャラクターのクラス
    /// </summary>
    
    //メインキャラクターが持つスキル
    public enum Skill
    {
        normal,
        shot,
        direction
    }
    public class MainCharacter : Microsoft.Xna.Framework.GameComponent
    {
        private float delta;
        private Mesh mesh;
        private Mesh circle;
        private const float circleDistance = 40;
        private Sphere sphere;
        public Sphere GetSphere { get { return sphere; } }
        private BoundingSphere backwardSphere;
        public BoundingSphere GetBackwardSphere
        {
            get { return backwardSphere; }
        }
        private Boss boss;
        private CharacterMaster master;
        private const float backwardDIstance = 5;
        private bool enableCollision = true;
        public bool EnableCollision
        {
            get { return enableCollision; }
            set { enableCollision = value; }
        }
        private Ray ray;
        private Obstacle[] obs;
        private Plane[] wall;
        private const float defaultMoveSpeed = 220;
        private const float slowMoveSpeed = 150;
        private const int releaseTime = 5;
        private float moveSpeed;
        private PlayerIndex playerIndex;
        private bool isMoving = false;
        private MainCharacter opponent;
        private Game game;
        private Mesh arrow;
        private Mesh dArrow;
        private Skill skill = Skill.normal;
        public Skill Skill
        {
            get { return skill; }
            set { skill = value; }
        }
        private enum Clip
        {
            stand,
            run,
        }
        private Clip clip = Clip.stand;
        private bool isImpact = false;
        private float impact = 0;
        private Vector3 impactVec;
        private const float maxImpact = 300;
        private const float impactSpeed = 500;
        private const float checkDistance = 20;
        private Vector3 vector = Vector3.Zero;
        private bool canMove = true;
        private float lf;
        private float rf;
        private bool lfFlag = false;
        private bool rfFlag = false;
        private bool nowSmash = false;
        private bool addEnergy = true;
        public bool AddEnergy
        {
            get { return addEnergy; }
            set { addEnergy = value; }
        }
        private const int mcDistance = 5;
        private int slowStart = -1;
        private SoundEffect freeze;


        public MainCharacter(Game game,string modelName,string circleName,PlayerIndex playerIndex,Plane[] wall,CharacterMaster master, Camera camera)
            : base(game)
        {
            // TODO: ここで子コンポーネントを作成します。

            //キャラクターメッシュの生成
            mesh = new Mesh(game, modelName, clip.ToString(), true, 0, camera);
            mesh.SSize = 15;
            mesh.Scale = new Vector3(1.5f, 1.5f, 1.5f);

            //キャラの後ろ側の判定
            backwardSphere = new BoundingSphere(Vector3.Zero, 15);

            ray = new Ray();
            this.playerIndex = playerIndex;
            this.wall = wall;
            this.game = game;
            if (!game.Components.Contains(mesh))
            {
                game.Components.Add(mesh);
            }

            //魔法陣の生成
            circle = new Mesh(game, circleName, camera);
            circle.Position = mesh.Position;
            circle.Scale = new Vector3(5, 5, 5);
            circle.EmissiveColor = new Vector3(0.3f, 0.3f, 0.3f);
            double r = Calculation.GetRadian(0, 0, mesh.GetForwardVector.X, mesh.GetForwardVector.Z);
            circle.Rotation = new Vector3(0, MathHelper.ToDegrees((float)-r) - 90, 0);
            if (!game.Components.Contains(circle))
            {
                game.Components.Add(circle);
            }
            arrow = new Mesh(game, "Model\\Arrow", camera);
            dArrow = new Mesh(game, "Model\\Arrow", camera);
            this.master = master;
            moveSpeed = defaultMoveSpeed;
            freeze = Game.Content.Load<SoundEffect>("Sounds\\freeze");
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
            delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            vector = Vector3.Zero;

            //コントローラーやキーボードからのインプットの処理
            Input(ref vector);

            //動けない場合はvectorを0にする
            if (!canMove)
            {
                vector = Vector3.Zero;
            }
            if (vector.Length() > 0)
            {
                isMoving = true;
            }
            else
            {
                isMoving = false;
            }

            //動いている場合は当たり判定などをする
            if (isMoving)
            {
                if (skill == Skill.normal || skill == Skill.direction) 
                {
                    Vector3 oldPos = mesh.Position;
                    mesh.Position += -mesh.GetForwardVector * moveSpeed * delta;
                    HitWall(oldPos);
                    HitObstacle(oldPos);
                    HitBoss(oldPos, ref vector, gameTime);
                }
                Vector3 vRot = mesh.Rotation;
                double dRot = Calculation.GetRadian(mesh.Position.Z, mesh.Position.X, mesh.Position.Z + vector.Z, mesh.Position.X + vector.X);
                float target = MathHelper.ToDegrees((float)dRot);
                target = Calculation.Degree360Conversion(target);
                Calculation.RotationShortes(ref vRot.Y, ref target);
                vRot.Y = Calculation.Finterp(vRot.Y, target, 20.0f);
                mesh.Rotation = vRot;
                clip = Clip.run;
            }
            else
            {
                clip = Clip.stand;
            }
            circle.Position = mesh.Position + new Vector3(0, 30, 0) - mesh.GetForwardVector * circleDistance;
            double r = Calculation.GetRadian(0, 0, mesh.GetForwardVector.X, mesh.GetForwardVector.Z);
            circle.Rotation = new Vector3(0, MathHelper.ToDegrees((float)-r) - 90, 0);
            backwardSphere.Center = mesh.BSphere.Center + mesh.GetForwardVector * backwardDIstance;

            //ボールにあたった場合はインパクトをする
            Impact();

            //スマッシュ
            Smash();

            //Directionモード時は矢印を表示する
            ShowDirectionArrow();
            mesh.PlayAnimation(clip.ToString(), true, 5);
            SlowMove(gameTime);
            base.Update(gameTime);
        }

        //動きを遅くする
        private void SlowMove(GameTime gameTime)
        {
            int nowTime=gameTime.TotalGameTime.Seconds;
            if (moveSpeed == slowMoveSpeed)
            {
                if (slowStart == -1)
                {
                    slowStart = nowTime;
                }
            }
            else
            {
                return;
            }
            if (slowStart + releaseTime <= nowTime)
            {
                SetMoveSpeed(true);
                slowStart = -1;
            }
        }

        //矢印を表示する
        private void ShowDirectionArrow()
        {
            //スキルがDirectionモードの時のみ
            if (skill == Skill.direction)
            {
                dArrow.Position = mesh.Position;
                Vector3 opPos = opponent.mesh.Position;
                Vector3 myPos = mesh.Position;
                double r = Calculation.GetRadian(opPos.X, opPos.Z, myPos.X, myPos.Z);
                Vector3 rot = dArrow.Rotation;
                rot.Y = -MathHelper.ToDegrees((float)r) - 90;
                dArrow.Rotation = rot;
                if (!game.Components.Contains(dArrow))
                {
                    game.Components.Add(dArrow);
                }
            }
            else
            {
                if (game.Components.Contains(dArrow))
                {
                    dArrow.Dispose();
                }
            }
        }

        //のけぞりをセットする
        public void SetImpact(Vector3 vec)
        {
            isImpact = true;
            impactVec = vec;
            impact = maxImpact;
        }

        //のけぞりの処理
        private void Impact()
        {
            if (isImpact)
            {
                if (impact > 0)
                {
                    impact -= impactSpeed * delta;
                    Vector3 oldPos = mesh.Position;
                    mesh.Position += impactVec * impact * delta;
                    HitWall(oldPos);
                    HitObstacle(oldPos);
                }
                else
                {
                    isImpact = false;
                }
            }
        }

        //スマッシュの処理
        private void Smash()
        {
            //スキルがShotの時だけ
            if (skill == Skill.shot)
            {
                arrow.Position = mesh.Position;
                arrow.Rotation = mesh.Rotation;
                if (!game.Components.Contains(arrow))
                {
                    game.Components.Add(arrow);
                }
                if (mesh.BSphere.Intersects(sphere.GetMesh.BSphere))
                {
                    sphere.Vector = -mesh.GetForwardVector;
                }
            }
            else
            {
                if (game.Components.Contains(arrow))
                {
                    game.Components.Remove(arrow);
                }
            }
        }

        //Sphereのセット
        public void SetSphere(Sphere s)
        {
            sphere = s;
        }

        //壁との当たり判定
        private void HitWall(Vector3 oldPos)
        {
            int count = 0;
            
            //すべての壁とのヒットを検出
            foreach (Plane w in wall)
            {
                if (w.Intersects(mesh.BSphere) == PlaneIntersectionType.Intersecting)
                {
                    count++;
                }
            }
            if (count == 1)
            {
                foreach (Plane w in wall)
                {
                    if (w.Intersects(mesh.BSphere) == PlaneIntersectionType.Intersecting)
                    {
                        mesh.Position = oldPos;
                        SetWallVector(w.Normal, moveSpeed, delta);
                    }
                }
            }
            else if (count != 0)
            {
                mesh.Position = oldPos;
            }
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
                    if (mesh.BSphere.Intersects(o.GetMesh.BSphere))
                    {
                        mesh.Position = oldPos;
                    }
                }
            }
        }

        //Nanになった場合は0を返す
        private Vector3 NanAvoidance(Vector3 value)
        {
            if (float.IsNaN(value.X))
            {
                value.X = 0;
            }
            if (float.IsNaN(value.Y))
            {
                value.Y = 0;
            }
            if (float.IsNaN(value.Z))
            {
                value.Z = 0;
            }
            return value;
        }

        //壁のVector計算
        public Vector3 CalculationWallVector(Vector3 flont, Vector3 normal)
        {
            normal = Vector3.Normalize(normal);
            normal = NanAvoidance(normal);
            return flont - Vector3.Dot(flont, normal) * normal;
        }

        //壁のVectorセット
        public void SetWallVector(Vector3 normal, float speed, float delta)
        {
            mesh.Position += -CalculationWallVector(mesh.GetForwardVector, normal) * speed * delta;
        }

        //コントローラーやキーボードからのインプット処理
        private void Input(ref Vector3 move)
        {
            if (playerIndex == PlayerIndex.One)
            {
                if (InputManager.IsKeyDown(Keys.Up))
                {
                    move.Z = -1;
                }
                if (InputManager.IsKeyDown(Keys.Down))
                {
                    move.Z = 1;
                }
                if (InputManager.IsKeyDown(Keys.Right))
                {
                    move.X = 1;
                }
                if (InputManager.IsKeyDown(Keys.Left))
                {
                    move.X = -1;
                }
            }
            else
            {
                if (InputManager.IsKeyDown(Keys.W))
                {
                    move.Z = -1;
                }
                if (InputManager.IsKeyDown(Keys.S))
                {
                    move.Z = 1;
                }
                if (InputManager.IsKeyDown(Keys.D))
                {
                    move.X = 1;
                }
                if (InputManager.IsKeyDown(Keys.A))
                {
                    move.X = -1;
                }
            }

            //接続されてる時のみ
            if (InputManager.IsConnected(playerIndex))
            {
                Vector2 input = InputManager.GetThumbSticksLeft(playerIndex);
                move.X = input.X;
                move.Z = -input.Y;
            }
            if (playerIndex == PlayerIndex.One && InputManager.IsJustKeyDown(Keys.Space))  
            {
                //Sphereが打てる時のみ
                if (sphere.EnableShot)
                {
                    skill = Skill.direction;
                    opponent.skill = Skill.shot;
                }
            }

            lf = InputManager.GetLeftTrigger(playerIndex);
            rf = InputManager.GetRightTrigger(playerIndex);
            if (lf > 0.5)
            {
                lfFlag = true;
            }
            else
            {
                lfFlag = false;
            }
            if (rf > 0.5)
            {
                rfFlag = true;
            }
            else
            {
                rfFlag = false;
            }
            if (lfFlag && rfFlag)
            {
                if (sphere.EnableShot && !nowSmash && skill != Skill.shot) 
                {
                    nowSmash = true;
                    skill = Skill.shot;
                    opponent.skill = Skill.direction;
                }
                else if (!nowSmash)
                {
                    nowSmash = true;
                    if (skill == Skill.shot)
                    {
                        skill = Skill.normal;
                        opponent.skill = Skill.normal;
                    }
                }
            }
            else
            {
                nowSmash = false;
            }

        }

        //メッシュの取得
        public Mesh GetMesh
        {
            get { return mesh; }
        }

        //前を向いているかを判定する処理
        public bool CheckLookingForward()
        {
            ray.Position = mesh.Position + mesh.GetForwardVector * checkDistance;
            ray.Direction = Vector3.Normalize(mesh.Position - opponent.GetMesh.Position);
            if (ray.Intersects(mesh.BSphere) == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //ボスとの当たり判定
        private void HitBoss(Vector3 oldPos,ref Vector3 move,GameTime gameTime)
        {
            if (boss != null)
            {
                if (boss.GetMesh.BSphere.Intersects(mesh.BSphere))
                {
                    mesh.Position = oldPos;
                    SetImpact(-Calculation.HitSphere(boss.GetMesh.Position, mesh.Position, ref move));
                    master.ApplyDamage(1);
                }
            }
        }

        //ボスのセット
        public Boss SetBoss
        {
            set { boss = value; }
        }

        //障害物のセット
        public Obstacle[] SetObstacle
        {
            set { obs = value; }
        }

        //相手のセット
        public void SetOpponent(MainCharacter opponent)
        {
            this.opponent = opponent;
        }

        //動けるかどうかをセットする（true=動ける）
        public bool SetMove
        {
            set { canMove = value; }
        }

        //MoveSpeedをセットする
        public void SetMoveSpeed(bool isDef)
        {
            if (isDef)
            {
                moveSpeed = defaultMoveSpeed;
            }
            else
            {
                freeze.Play();
                moveSpeed = slowMoveSpeed;
            }
        }
    }
}
