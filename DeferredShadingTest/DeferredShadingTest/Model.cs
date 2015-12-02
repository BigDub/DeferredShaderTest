using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredShadingTest
{
    class Mesh
    {
        private CVF[] vertices;
        private int[] indices;
        public void Draw(GraphicsDevice gd)
        {
            gd.DrawUserIndexedPrimitives<CVF>(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3, CVF.VertexDeclaration);
        }
        public Mesh(CVF[] v, int[] i)
        {
            vertices = v;
            indices = i;
        }
        public static Mesh createPlane(int w, int h)
        {
            if (w <= 0 || h <= 0)
                return null;
            CVF[] v = new CVF[(w + 1) * (h + 1)];
            int[] i = new int[w * h * 6];
            for (int x = 0; x <= w; x++)
            {
                for (int y = 0; y <= h; y++)
                {
                    v[x * (h + 1) + y] = new CVF(new Vector3((1.0f / w) * x - 0.5f, 0, (1.0f / h) * y - 0.5f), Vector3.UnitY, new Vector2((1.0f / w) * x, (1.0f / h) * y));
                }
            }
            int a = 0;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    i[a++] = (h + 1) * x + y;
                    i[a++] = (h + 1) * (x + 1) + y;
                    i[a++] = (h + 1) * (x + 1) + y + 1;

                    i[a++] = (h + 1) * x + y;
                    i[a++] = (h + 1) * (x + 1) + y + 1;
                    i[a++] = (h + 1) * x + y + 1;
                }
            }
            return new Mesh(v, i);
        }
        public static Mesh CreateCubeMesh(float round)
        {
            CVF[] v = new CVF[24];
            int num = 0;
            Vector2 a = new Vector2(0), b = new Vector2(0, 1), c = new Vector2(1), d = new Vector2(1, 0);
            Vector3 norm = new Vector3(0, 0, -1);//Front
            v[num++] = new CVF(new Vector3(-1, -1, -1), norm, a);//0
            v[num++] = new CVF(new Vector3(1, -1, -1), norm, b);//1
            v[num++] = new CVF(new Vector3(1, 1, -1), norm, c);//2
            v[num++] = new CVF(new Vector3(-1, 1, -1), norm, d);//3
            norm = new Vector3(-1, 0, 0);//Left
            v[num++] = new CVF(new Vector3(-1, -1, 1), norm, a);//4
            v[num++] = new CVF(new Vector3(-1, -1, -1), norm, b);//5
            v[num++] = new CVF(new Vector3(-1, 1, -1), norm, c);//6
            v[num++] = new CVF(new Vector3(-1, 1, 1), norm, d);//7
            norm = new Vector3(0, 0, 1);//Back
            v[num++] = new CVF(new Vector3(1, -1, 1), norm, a);//8
            v[num++] = new CVF(new Vector3(-1, -1, 1), norm, b);//9
            v[num++] = new CVF(new Vector3(-1, 1, 1), norm, c);//10
            v[num++] = new CVF(new Vector3(1, 1, 1), norm, d);//11
            norm = new Vector3(1, 0, 0);//Right
            v[num++] = new CVF(new Vector3(1, -1, -1), norm, a);//12
            v[num++] = new CVF(new Vector3(1, -1, 1), norm, b);//13
            v[num++] = new CVF(new Vector3(1, 1, 1), norm, c);//14
            v[num++] = new CVF(new Vector3(1, 1, -1), norm, d);//15
            norm = new Vector3(0, 1, 0);//Top
            v[num++] = new CVF(new Vector3(-1, 1, -1), norm, a);//16
            v[num++] = new CVF(new Vector3(1, 1, -1), norm, b);//17
            v[num++] = new CVF(new Vector3(1, 1, 1), norm, c);//18
            v[num++] = new CVF(new Vector3(-1, 1, 1), norm, d);//19
            norm = new Vector3(0, -1, 0);//Bottom
            v[num++] = new CVF(new Vector3(-1, -1, 1), norm, a);//20
            v[num++] = new CVF(new Vector3(1, -1, 1), norm, b);//21
            v[num++] = new CVF(new Vector3(1, -1, -1), norm, c);//22
            v[num++] = new CVF(new Vector3(-1, -1, -1), norm, d);//23

            for (num = 0; num < 24; num++)
            {
                Vector3 normal0 = v[num].Normal, normal1 = v[num].Position;
                normal1.Normalize();
                round = MathHelper.Clamp(round, 0, 1);
                v[num].Normal = normal0 * (1 - round) + normal1 * round;
            }

            int[] i = new int[36];
            for (int i0 = 0; i0 < 6; i0++)
            {
                i[(6 * i0)] = 4 * i0;
                i[(6 * i0) + 1] = 4 * i0 + 2;
                i[(6 * i0) + 2] = 4 * i0 + 3;
                i[(6 * i0) + 3] = 4 * i0;
                i[(6 * i0) + 4] = 4 * i0 + 1;
                i[(6 * i0) + 5] = 4 * i0 + 2;
            }
            //0,2,3,
            //0,1,2,
            return new Mesh(v, i);
        }
    } 
}
