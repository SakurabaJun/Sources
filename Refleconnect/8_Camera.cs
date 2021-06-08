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
    /// カメラの処理をするクラス
    /// </summary>
    public class Camera : Microsoft.Xna.Framework.GameComponent
    {
        /// <summary>
        /// カメラ位置
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
        }
		private Vector3 position = new Vector3(0.0f, 10.0f, -10.0f);

        /// <summary>
        /// 注視点
        /// </summary>
        public Vector3 Target
        {
            get { return target; }
            set { target = value; isCalc = true; }
        }
        private Vector3 target = Vector3.Zero;

        /// <summary>
        /// アスペクト比
        /// </summary>
        public float AspectRatio
        {
            get { return aspectRatio; }
            set { aspectRatio = value; isCalc = true; }
        }
        float aspectRatio = 4.0f / 3.0f;

        /// <summary>
        /// 視野角　(fov)
        /// </summary>
        public float FieldOfView
        {
            get { return fieldOfView; }
            set { fieldOfView = value; isCalc = true; }
        }
        float fieldOfView = MathHelper.ToRadians(45.0f);

        /// <summary>
        /// ニアクリップ面の距離
        /// </summary>
        public float NearPlaneDistance
        {
            get { return nearPlaneDistance; }
            set { nearPlaneDistance = value; isCalc = true; }
        }
        private float nearPlaneDistance = 1.0f;

        /// <summary>
        /// ファークリップ面の距離
        /// </summary>
        public float FarPlaneDistance
        {
            get { return farPlaneDistance; }
            set { farPlaneDistance = value; isCalc = true; }
        }
        private float farPlaneDistance = 10000.0f;

        /// <summary>
        /// ビュー行列
        /// </summary>
        public Matrix View
        {
            get { return view; }
        }
        private Matrix view;

        /// <summary>
        /// 射影行列
        /// </summary>
        public Matrix Projection
        {
            get { return projection; }
        }
        private Matrix projection;

        /// <summary>
        /// 回転量
        /// </summary>
        private Vector3 rotation = Vector3.Zero;

        /// <summary>
        /// カメラの回転行列
        /// </summary>
        private Matrix rotationMatrix = Matrix.Identity;

        /// <summary>
        /// 更新が必要かどうか
        /// </summary>
        private bool isCalc;

        public Camera(Game game)
            : base(game)
        {
            // TODO: ここで子コンポーネントを作成します。
        }

        /// <summary>
        /// ゲーム コンポーネントの初期化を行います。
        /// ここで、必要なサービスを照会して、使用するコンテンツを読み込むことができます。
        /// </summary>
        public override void Initialize()
        {
            // TODO: ここに初期化のコードを追加します。
            Calculate();
            isCalc = false;
            base.Initialize();
        }

        /// <summary>
        /// ゲーム コンポーネントが自身を更新するためのメソッドです。
        /// </summary>
        /// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: ここにアップデートのコードを追加します。
            if (isCalc)
            {
                Calculate();
                isCalc = false;
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// マトリクスを更新
        /// </summary>
        private void Calculate()
        {
            // 回転行列を計算する
            rotationMatrix = Matrix.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);

            // ビュー行列を更新する
            view = Matrix.CreateLookAt(position, target, rotationMatrix.Up);

            // 射影行列を更新する
            projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance);
        }
		/// <summary>
		/// カメラの位置を設定
		/// </summary>
		/// <param name="vec">移動位置</param>
		public void SetPosition(Vector3 vec)
		{
			// 回転角度を設定する
			position = vec;

			isCalc = true;
		}
        /// <summary>
        /// カメラ移動
        /// </summary>
        /// <param name="vec">移動距離</param>
        public void Move(Vector3 vec)
        {
            // X軸について移動
            target += rotationMatrix.Right * vec.X;
            // Y軸について移動
            target += rotationMatrix.Up * vec.Y;
            // Z軸について移動
            target += rotationMatrix.Forward * vec.Z;

            isCalc = true;
        }

        /// <summary>
        /// カメラの平行移動
        /// </summary>
        /// <param name="vec"></param>
        public void MoveAxis(Vector3 vec)
        {
            target += vec;
            isCalc = true;
        }

        /// <summary>
        /// カメラの回転
        /// </summary>
        /// <param name="x">X軸周りの回転角度(ラジアン)</param>
        /// <param name="y">Y軸周りの回転角度(ラジアン)</param>
        /// <param name="z">Z軸周りの回転角度(ラジアン)</param>
        public void Rotation(float x, float y, float z)
        {
            // 回転角度を加算する
            rotation.X += x;
            rotation.Y += y;
            rotation.Z += z;

            isCalc = true;
        }

        /// <summary>
        /// カメラの回転
        /// </summary>
        /// <param name="rot">X,Y,Z軸の回転角度(ラジアン)</param>
        public void Rotation(Vector3 rot)
        {
            rotation += rot;

            isCalc = true;
        }

        /// <summary>
        /// カメラの回転角度を設定する
        /// </summary>
        /// <param name="vec">X,Y,Z軸周りの回転角度(ラジアン)</param>
        public void SetRotation(Vector3 vec)
        {
            rotation = vec;

            isCalc = true;
        }
    }
}
