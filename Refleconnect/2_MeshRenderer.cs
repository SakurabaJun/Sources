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
    public class MeshRenderer : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private Transform transform;
        private Material material;
        private Camera camera;
        private LightParameter light;
        private Matrix[] modelTransform;
        private Matrix modelWorld;
        private Model model;

        public MeshRenderer(Game game, Transform transform, Model model, Material material,LightParameter light, Camera camera)
            : base(game)
        {
            this.transform = transform;
			this.model = model;
            this.material = material;
            this.light = light;
            this.camera = camera;
            ModelInitialize();
        }
        private void ModelInitialize()
        {
            modelTransform = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(modelTransform);
            modelWorld = Matrix.Identity;
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
            modelWorld = Matrix.Identity;
            modelWorld *= Matrix.CreateScale(transform.Scale);
            modelWorld *= Matrix.CreateRotationX(MathHelper.ToRadians(transform.Rotation.X));
            modelWorld *= Matrix.CreateRotationY(MathHelper.ToRadians(transform.Rotation.Y));
            modelWorld *= Matrix.CreateRotationZ(MathHelper.ToRadians(transform.Rotation.Z));
            modelWorld *= Matrix.CreateTranslation(transform.Position);
            base.Update(gameTime);
        }
        /// <summary>
        /// ゲームが自身を描画するためのメソッドです。
        /// </summary>
        /// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
        public override void Draw(GameTime gameTime)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    if (light != null)
                    {
                        DirectionalLight dLight = null;
                        dLight = effect.DirectionalLight0;
                        dLight.Enabled = light.Enable;
                        dLight.Direction = light.Direction;
                    }
                    effect.SpecularColor = material.SpecularColor;
                    effect.AmbientLightColor = material.AmbientLightColor;
                    effect.EmissiveColor = material.EmissiveColor;
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                    effect.World = modelTransform[mesh.ParentBone.Index] * modelWorld;
                }
                mesh.Draw();
            }
            base.Draw(gameTime);
        }
    }
}
