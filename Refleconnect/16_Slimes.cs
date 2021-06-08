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
    /// スライムを統括するクラス
    /// </summary>
    public class Slimes : Microsoft.Xna.Framework.GameComponent
    {
        private Game game;
        private CharacterMaster chara;
        private Plane[] w;
        private Camera camera;
        private List<Slime> slimes;
        private const int interval = 2;//時間の感覚
        private int isDeath = 0;
        private int maxDeath = 10;
        private bool canSpawn = true;
        private Obstacle[] obs;
        private Sphere sphere;
        private Random rnd=new Random();
        private SlimeValue slimeValue;
        private const int offset1Range = 50;
        private const int offset2Range = 200;
        private int normal;
        private int ice;

        //死んだ数
        public int IsDeath
        {
            get { return isDeath;}
            set{isDeath=value;}
        
        }

        //最大死亡数
        public int MaxDeath
        {
            get { return maxDeath; }

        }

        //スライムを保持するList
        public List<Slime> GetSlimes
        {
            get
            {
                return slimes;
            }
        }
        private int stage;

        public Slimes(Game game, int normal, int ice,Plane[] w, CharacterMaster chara,int stage, Camera camera)
            : base(game)
        {
            // TODO: ここで子コンポーネントを作成します。
            this.game = game;
            this.w = w;
            this.chara = chara;
            this.camera = camera;
            this.stage = stage;
            slimeValue = new SlimeValue(game, this);
            if (!game.Components.Contains(slimeValue))
            {
                game.Components.Add(slimeValue);
            }
            this.normal = normal;
            this.ice = ice;
            EnemyInit();
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

            int time = gameTime.TotalGameTime.Seconds;

            //秒数とスライムの数によってスポーンできるかを決めている
            if (time % interval == 0 && slimes.Count < normal+ice&&isDeath<maxDeath)
            {
                if (canSpawn)
                {
                    AddSlime(Slime.SlimeType.normal);
                    canSpawn = false;
                }
            }
            else
            {
                canSpawn = true;
            }

            //最大死亡数に達した場合は
            if (isDeath == maxDeath)
            {
                for (int i = 0; i < slimes.Count; i++)
                {
                    slimes[i].Dispose();
                }
                for (int i = 0; i < slimes.Count; i++)
                {
                    slimes.Remove(slimes[i]);
                }
                switch(stage)
                {
                    case 1:
                        Boss1 boss1 = new Boss1(Game, chara, obs, sphere, camera);
                        game.Components.Add(boss1);
                        sphere.SetBoss = boss1.GetBoss;
                        break;
                    case 2:
                        Boss2 boss2 = new Boss2(Game, chara, obs, sphere, camera);
                        game.Components.Add(boss2);
                        sphere.SetBoss = boss2.GetBoss;
                        break;
                    case 3:
                        Boss3 boss3 = new Boss3(Game, chara, obs, sphere, camera);
                        game.Components.Add(boss3);
                        sphere.SetBoss = boss3.GetBoss;
                        break;
                }
                
                //ステージ2に移行
                chara.GetField.GetMesh.SetModel("Model\\Stage\\stage2");
                if (game.Components.Contains(slimeValue))
                {
                    game.Components.Remove(slimeValue);
                }
                if (game.Components.Contains(this))
                {
                    game.Components.Remove(this);
                }

            }
            if (InputManager.IsJustKeyDown(Keys.B))
            {
                if (slimes.Count > 0) 
                {
                    slimes[0].ApplyDamage();
                }
            }
            base.Update(gameTime);
        }

        //敵の初期化
        private void EnemyInit()
        {
            slimes = new List<Slime>();
            for (int i = 0; i < normal; i++)
            {
                AddSlime(Slime.SlimeType.normal);
            }
            for (int i = 0; i < ice; i++)
            {
                AddSlime(Slime.SlimeType.ice);
            }
        }

        //スライムの追加
        private void AddSlime(Slime.SlimeType slimeType)
        {
            float d = MathHelper.ToRadians(rnd.Next(360));
            Vector3 pos = new Vector3(rnd.Next(-100, 190), 25, rnd.Next(-100, 100));
            Vector3 offset1 = new Vector3(rnd.Next(-offset1Range, offset1Range), 0, rnd.Next(-offset1Range, offset1Range));
            Vector3 offset2 = new Vector3(rnd.Next(-offset2Range, offset2Range), 0, rnd.Next(-offset2Range, offset2Range));

            //スライムをインスタンス化してコンポーネントに追加
            Slime a = new Slime(game, this, slimeType, w, chara, d, pos, offset1, offset2, camera);
            slimes.Add(a);
            if (!game.Components.Contains(a))
            {
                game.Components.Add(a);
            }
            a.SetObstacle = obs;
        }

        //障害物の追加
        public Obstacle[] SetObstacle
        {
            set 
            {
                obs = value;
            }
        }

        //ダメージ適応
        public void ApplyDamage(Slime s)
        {
            slimes.Remove(s);
            isDeath++;
        }

        //Sphereのセット
        public Sphere SetSphere
        {
            set { sphere = value; }
        }
    }
}
