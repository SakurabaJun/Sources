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


namespace Components
{
    /// <summary>
    /// IUpdateable インターフェイスを実装したゲーム コンポーネントです。
    /// </summary>
    public class Mesh : Microsoft.Xna.Framework.GameComponent
    {
        /// <summary>
        /// MeshRenderに割り当てられるMesh
        /// </summary>
        public Model model;

        /// <summary>
        /// MeshRenderに割り当てられるMaterial
        /// </summary>
        public Material material;

        /// <summary>
        /// 位置、角度、スケールを保有するクラス
        /// </summary>
        public Transform transform;

        /// <summary>
        /// メッシュレンダラー
        /// </summary>
        public MeshRenderer render;

        public Mesh(Game game, string name, LightParameter light, Camera camera)
            : base(game)
        {
            transform = new Transform();
			model = Game.Content.Load<Model>(name);
            transform.Position = new Vector3(0, 0, 0);
            material = new Material(model);
            render = new MeshRenderer(game, transform, model, material, light, camera);
            Game.Components.Add(render);
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
