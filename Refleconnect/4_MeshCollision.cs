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
    /// メッシュをもとに当たり判定を生成するクラス
    /// </summary>
    public class MeshCollision : Microsoft.Xna.Framework.GameComponent
    {
        private struct Hash
        {
            public int i1;
            public int i0;
            public int i2;

            public Hash(int i1, int i0, int i2)
            {
                this.i1 = i1;
                this.i0 = i0;
                this.i2 = i2;
            }
        };
        private Hash[] hash = new Hash[7];

        private Model model;
		private Camera camera;
        private List<Vector3> pos;
		private List<Line> line;
        private List<VertexPositionColor[]> allbuffers;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="game">コンポーネントを追加する元となるゲーム</param>
        /// <param name="model">当たり判定に使用するモデル（メッシュ）</param>
        /// <param name="camera">描画するカメラ</param>
        public MeshCollision(Game game, Model model,Camera camera)
            : base(game)
        {
			this.model = model;
			this.camera = camera;
            pos = new List<Vector3>();
            line = new List<Line>();
			GetVerteces();
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
            for (int i = 0; i < allbuffers.Count; i++)
            {

            }
            base.Update(gameTime);
        }

        //頂点の取得
        void GetVerteces()
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                allbuffers = new List<VertexPositionColor[]>(mesh.MeshParts.Count);
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    VertexDeclaration declaration = part.VertexBuffer.VertexDeclaration;
                    VertexElement[] vertexElements = declaration.GetVertexElements();
                    VertexElement vertexPosition = new VertexElement();
                    Vector3[] allVertex = new Vector3[part.NumVertices];
                    part.VertexBuffer.GetData<Vector3>(part.VertexOffset * declaration.VertexStride + vertexPosition.Offset, allVertex, 0, part.NumVertices, declaration.VertexStride);
                    short[] indexElements = new short[part.PrimitiveCount * 3];
                    part.IndexBuffer.GetData<short>(part.StartIndex * 2, indexElements, 0, part.PrimitiveCount * 3);
                    VertexPositionColor[] borrowVerts = new VertexPositionColor[indexElements.Length];
                    for (int i = 0; i < indexElements.Length; i++)
                    {
                        borrowVerts[i].Position.X = allVertex[indexElements[i]].X;
                        borrowVerts[i].Position.Y = allVertex[indexElements[i]].Y;
                        borrowVerts[i].Position.Z = allVertex[indexElements[i]].Z;
                        borrowVerts[i].Color = Color.Black;
                    }
                    allbuffers.Add(borrowVerts);
                }
            }
            for (int i = 0; i < allbuffers.Count; i++)
            {
                for (int j = 0; j < allbuffers[i].Length; j++)
                {
                    Vector3 v1, v2;
                    if ((j + 1) % 3 == 0)
                    {
                        v1 = allbuffers[i][j].Position;
                        v2 = allbuffers[i][j - 2].Position;
                    }
                    else
                    {
                        v1 = allbuffers[i][j].Position;
                        v2 = allbuffers[i][j + 1].Position;
                    }

                    //当たり判定の可視化
                    Line l = new Line(Game, v1, v2, camera);
                    Game.Components.Add(l);
                }
            }
        }

        //外積を求める
        private float Cross2D(Vector2 u, Vector2 v)
        {
            return u.Y * v.X - u.X * v.Y;
        }


        private bool PointInTriangle2D(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float pab = Cross2D(p - a, b - a);
            float pbc = Cross2D(p - b, c - b);
            if (pab * pbc < 0)
            {
                return false;
            }
            float pca = Cross2D(p - c, a - c);
            if (pab * pca < 0)
            {
                return false;
            }
            return true;
        }

        private float Signed2DTriArea(Vector2 a, Vector2 b, Vector2 c)
        {
            return (a.X - c.X) * (b.Y - c.Y) - (a.Y - c.Y) * (b.X - c.X);
        }

        private bool Test2DSegmentSegment(Vector2 a, Vector2 b, Vector2 c, Vector2 d, ref float t, ref Vector2 p)
        {
            float a1 = Signed2DTriArea(a, b, d);
            float a2 = Signed2DTriArea(a, b, c);
            if (a1 * a2 < 0.0f)
            {
                float a3 = Signed2DTriArea(c, d, a);
                float a4 = a3 + a2 - a1;
                if (a3 * a4 < 0.0f)
                {
                    t = a3 / (a3 - a4);
                    p = a + t * (b - a);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 三角ポリゴンと三角ポリゴンの衝突判定
        /// </summary>
        /// <param name="t1">頂点座標1</param>
        /// <param name="t2">頂点座標2</param>
        /// <returns>衝突したらTrue</returns>
        private bool IntersectTriangleTriangle(Vector3[] t1, Vector3[] t2)
        {
            Vector3 n1, n2;
            n1 = Vector3.Cross(t1[1] - t1[0], t1[2] - t1[0]);
            n1 = Vector3.Normalize(n1);
            n2 = Vector3.Cross(t2[1] - t2[0], t2[2] - t2[0]);
            n2 = Vector3.Normalize(n2);
            float d1 = Vector3.Dot(-n1, t1[0]);
            float d2 = Vector3.Dot(-n2, t2[0]);
            float[] dist1 = new float[3];
            float[] dist2 = new float[3];
            int sign1 = 0, sign2 = 0;
            for (int i = 0; i < 3; i++)
            {
                dist1[i] = Vector3.Dot(n2, t1[i]) + d2;
                if (dist1[i] > 0)
                {
                    sign1 |= 1 << i;
                }
                dist2[i] = Vector3.Dot(n1, t2[i]) + d1;
                if (dist2[i] > 0)
                {
                    sign2 |= 1 << i;
                }
            }
            if (sign1 == 0 || sign1 == 7 || sign2 == 0 || sign2 == 7)
            {
                if (Math.Abs(dist1[0]) >= 0.00001f || Math.Abs(dist1[1]) >= 0.00001f || Math.Abs(dist1[2]) >= 0.00001f)
                {
                    return false;
                }
                Vector2[] t1_2D = new Vector2[3];
                Vector2[] t2_2D = new Vector2[3];
                if (Math.Abs(n1.X) >= Math.Abs(n1.Y) && Math.Abs(n1.X) >= Math.Abs(n1.Z))
                {
                    // YZ
                    for (int i = 0; i < 3; i++)
                    {
                        t1_2D[i].X = t1[i].Y; t1_2D[i].Y = t1[i].Z;
                        t2_2D[i].X = t2[i].Y; t2_2D[i].Y = t2[i].Z;
                    }
                }
                else if (Math.Abs(n1.Y) >= Math.Abs(n1.Z))
                {
                    // XZ
                    for (int i = 0; i < 3; i++)
                    {
                        t1_2D[i].X = t1[i].Z; t1_2D[i].Y = t1[i].Z;
                        t2_2D[i].X = t2[i].X; t2_2D[i].Y = t2[i].Z;
                    }
                }
                else
                {
                    // XY
                    for (int i = 0; i < 3; i++)
                    {
                        t1_2D[i].X = t1[i].X; t1_2D[i].Y = t1[i].Y;
                        t2_2D[i].Y = t2[i].X; t2_2D[i].Y = t2[i].Y;
                    }
                }

                //線分間衝突
                float t = 0;
                Vector2 p = Vector2.Zero;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (Test2DSegmentSegment(t1_2D[i], t1_2D[(i + 1) % 3], t2_2D[j], t2_2D[(j + 1) % 3], ref t, ref p))
                        {
                            return true;
                        }
                    }
                }

                //点含有
                for (int i = 0; i < 3; i++)
                {
                    if (PointInTriangle2D(t1_2D[i], t2_2D[0], t2_2D[1], t2_2D[2]))
                    {
                        return true;
                    }
                }
                for (int i = 0; i < 3; i++)
                {
                    if (PointInTriangle2D(t2_2D[i], t1_2D[0], t1_2D[1], t1_2D[2]))
                    {
                        return true;
                    }
                }
                return false;
            }
            hash[0] = new Hash(0, 0, 0);
            hash[1] = new Hash(0, 1, 2);
            hash[2] = new Hash(1, 0, 2);
            hash[3] = new Hash(2, 0, 1);
            hash[4] = new Hash(2, 0, 1);
            hash[5] = new Hash(1, 0, 2);
            hash[6] = new Hash(0, 1, 2);
            {
                // 2三角形の共通線Lで交わっているか
                Vector3 d = Vector3.Cross(n1, n2);
                d = Vector3.Normalize(d);
                float[] p1 = new float[3];
                p1[0] = Vector3.Dot(d, t1[hash[sign1].i0]);
                p1[1] = Vector3.Dot(d, t1[hash[sign1].i1]);
                p1[2] = Vector3.Dot(d, t1[hash[sign1].i2]);
                float[] p2 = new float[3];
                p2[0] = Vector3.Dot(d, t2[hash[sign2].i0]);
                p2[1] = Vector3.Dot(d, t2[hash[sign2].i1]);
                p2[2] = Vector3.Dot(d, t2[hash[sign2].i2]);
                float[] di1 = new float[3];
                di1[0] = dist1[hash[sign1].i0];
                di1[1] = dist1[hash[sign1].i1];
                di1[2] = dist1[hash[sign1].i2];
                float[] di2 = new float[3];
                di2[0] = dist2[hash[sign2].i0];
                di2[1] = dist2[hash[sign2].i1];
                di2[2] = dist2[hash[sign2].i2];

                float t1_1 = p1[0] + (p1[1] - p1[0]) * di1[0] / (di1[0] - di1[1]);
                float t1_2 = p1[2] + (p1[1] - p1[2]) * di1[2] / (di1[2] - di1[1]);
                float t2_1 = p2[0] + (p2[1] - p2[0]) * di2[0] / (di2[0] - di2[1]);
                float t2_2 = p2[2] + (p2[1] - p2[2]) * di2[2] / (di2[2] - di2[1]);

                if (t1_1 < t1_2)
                {
                    if (t2_1 < t2_2)
                    {
                        if (t2_2 < t1_1 || t1_2 < t2_1)
                            return false;
                    }
                    else
                    {
                        if (t2_1 < t1_1 || t1_2 < t2_2)
                            return false;
                    }
                }
                else
                {
                    if (t2_1 < t2_2)
                    {
                        if (t2_2 < t1_2 || t1_1 < t2_1)
                            return false;
                    }
                    else
                    {
                        if (t2_1 < t1_2 || t1_1 < t2_2)
                            return false;
                    }
                }
            }
            return true;
        }
    }
}
