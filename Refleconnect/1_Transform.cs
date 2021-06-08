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
    public class Transform
    {
        private Vector3 position;
        private Vector3 rotation;
        private Vector3 scale;

        /// <summary>
        /// 位置
        /// </summary>
        public Vector3 Position { get { return position; } set { position = value; } }

        /// <summary>
        /// 回転
        /// </summary>
        public Vector3 Rotation { get { return rotation; } set { rotation = value; } }

        /// <summary>
        /// スケール
        /// </summary>
        public Vector3 Scale { get { return scale; } set { scale = value; } }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Transform()
        {
            position = Vector3.Zero;
            rotation = Vector3.Zero;
            scale = Vector3.One;
        }

        /// <summary>
        /// コンストラクタ（位置指定）
        /// </summary>
        /// <param name="x">位置情報（x）</param>
        /// <param name="y">位置情報（y）</param>
        /// <param name="z">位置情報（z）</param>
        public Transform(float x,float y,float z)
		{
			position = new Vector3(x, y, z);
			rotation = Vector3.Zero;
			scale = Vector3.One;
		}
    }
}
