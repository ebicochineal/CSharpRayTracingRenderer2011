using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace renderer {
    #region Import
    class Import {
        public static Object3d[] objfile (string path, string file) {
            string[] mtl = file.Split('.');

            List<Vector3> vertex = new List<Vector3>();
            List<int[]> index = new List<int[]>();
            List<Vector2> uvtvertex = new List<Vector2>();
            List<int[]> uvtindex = new List<int[]>();

            List<int> objrangeV = new List<int>();
            List<int> objrangeI = new List<int>();
            List<int> objrangeT = new List<int>();

            List<int> matrange = new List<int>();
            List<int> objmatrange = new List<int>();

            List<string> objname = new List<string>();
            List<string> matname = new List<string>();

            int Icount = 0;
            int Vcount = 0;
            int VTcount = 0;
            int VIcount = 0;
            int Mcount = 0;

            string st;
            using (StreamReader objfile = new StreamReader(path + file, Encoding.GetEncoding("Shift_JIS"))) {
                while ((st = objfile.ReadLine()) != null) {
                    if (st[0] == 'o' && st[1] == ' ') {
                        string[] name = st.Split(' ');
                        objname.Add(name[1]);
                        objrangeV.Add(Vcount);
                        objrangeI.Add(Icount);
                        objrangeT.Add(VTcount);
                        objmatrange.Add(Mcount);
                    }

                    if (st[0] == 'u' && st[1] == 's') {
                        string[] filename = st.Split(' ');
                        if (filename.Length == 2) {
                            matname.Add(filename[1]);
                        } else {
                            matname.Add(" ");
                        }
                        matrange.Add(Icount);
                        Mcount += 1;
                    }


                    if (st[0] == 'v' && st[1] == ' ') {
                        Vcount += 1;
                        string[] v4 = st.Split(' ');
                        vertex.Add(new Vector3(float.Parse(v4[1]), float.Parse(v4[2]), float.Parse(v4[3])));
                    }

                    if (st[0] == 'v' && st[1] == 't') {
                        VTcount += 1;
                        string[] v3 = st.Split(' ');
                        uvtvertex.Add(new Vector2(float.Parse(v3[1]), float.Parse(v3[2])));
                    }

                    if (st[0] == 'f' && st[1] == ' ') {
                        Icount += 1;
                        string[] ft4 = st.Split(' ');
                        string[] f21 = ft4[1].Split('/');
                        string[] f22 = ft4[2].Split('/');
                        string[] f23 = ft4[3].Split('/');
                        index.Add(new int[] { int.Parse(f21[0]) - 1 - objrangeV[objname.Count - 1], int.Parse(f22[0]) - 1 - objrangeV[objname.Count - 1], int.Parse(f23[0]) - 1 - objrangeV[objname.Count - 1] });
                        if (f21.Length == 2) {
                            VIcount += 1;
                            uvtindex.Add(new int[] { int.Parse(f21[1]) - 1 - objrangeT[objname.Count - 1], int.Parse(f22[1]) - 1 - objrangeT[objname.Count - 1], int.Parse(f23[1]) - 1 - objrangeT[objname.Count - 1] });
                        }
                    }
                }
                objrangeV.Add(Vcount);
                objrangeI.Add(Icount);
                objrangeT.Add(VTcount);
                matrange.Add(Icount);
                objmatrange.Add(Mcount);
            }

            bool material = false;
            List<string> texname = new List<string>();
            List<string> materialname = new List<string>();

            using (StreamReader mtlfile = new StreamReader(path + mtl[0] + ".mtl", Encoding.GetEncoding("Shift_JIS"))) {
                while ((st = mtlfile.ReadLine()) != null) {
                    if (st.Length > 0 || material == true) {
                        if (st.Length > 0) {
                            if (st[0] == 'n' && st[1] == 'e') {
                                material = true;
                                string[] mat = st.Split(' ');
                                materialname.Add(mat[1]);
                            }
                            if (st[0] == 'm' && st[1] == 'a') {
                                string[] tex = st.Split(' ');
                                texname.Add(tex[1]);
                                Console.WriteLine(tex[1]);
                            }
                        } else {
                            if (materialname.Count != texname.Count) {
                                texname.Add(" ");
                            }
                            material = false;
                        }
                    }
                }
            }

            List<Bitmap> texture = new List<Bitmap>();
            foreach (string name in matname)//
            {
                for (int i = 0; i < materialname.Count; i++) {
                    if (name == materialname[i]) {
                        if (texname[i] != " ") {
                            using (Bitmap bm = new Bitmap(path + texname[i])) {
                                texture.Add(new Bitmap(bm));
                            }
                        } else {
                            texture.Add(new Bitmap(1, 1));
                            texture[texture.Count - 1].SetPixel(0, 0, Color.White);
                        }
                    }
                }
            }

            Object3d[] objlist = new Object3d[objname.Count];
            for (int i = 0; i < objname.Count; i++) {
                if (objrangeT[i + 1] - objrangeT[i] > 0) {
                    Vector3[] vertexl = new Vector3[objrangeV[i + 1] - objrangeV[i]];
                    int[][] indexl = new int[objrangeI[i + 1] - objrangeI[i]][];
                    Vector2[] uvtvertexl = new Vector2[objrangeT[i + 1] - objrangeT[i]];
                    int[][] uvtindexl = new int[objrangeI[i + 1] - objrangeI[i]][];

                    vertex.CopyTo(objrangeV[i], vertexl, 0, objrangeV[i + 1] - objrangeV[i]);
                    index.CopyTo(objrangeI[i], indexl, 0, objrangeI[i + 1] - objrangeI[i]);
                    uvtvertex.CopyTo(objrangeT[i], uvtvertexl, 0, objrangeT[i + 1] - objrangeT[i]);
                    uvtindex.CopyTo(objrangeI[i], uvtindexl, 0, objrangeI[i + 1] - objrangeI[i]);

                    string[] matnamel = new string[objmatrange[i + 1] - objmatrange[i]];
                    int[] matl = new int[objmatrange[i + 1] - objmatrange[i] + 1];

                    matname.CopyTo(objmatrange[i], matnamel, 0, objmatrange[i + 1] - objmatrange[i]);
                    matrange.CopyTo(objmatrange[i], matl, 0, objmatrange[i + 1] - objmatrange[i] + 1);

                    Bitmap[] texturel = new Bitmap[objmatrange[i + 1] - objmatrange[i]];
                    texture.CopyTo(objmatrange[i], texturel, 0, objmatrange[i + 1] - objmatrange[i]);

                    int matz = matl[0];
                    for (int ii = 0; ii < matl.Length; ++ii) {
                        matl[ii] -= matz;
                    }

                    objlist[i] = new Object3d(vertexl, indexl, uvtvertexl, uvtindexl, texturel, objname[i], matnamel, matl);
                } else {
                    Vector3[] vertexl = new Vector3[objrangeV[i + 1] - objrangeV[i]];
                    int[][] indexl = new int[objrangeI[i + 1] - objrangeI[i]][];
                    vertex.CopyTo(objrangeV[i], vertexl, 0, objrangeV[i + 1] - objrangeV[i]);
                    index.CopyTo(objrangeI[i], indexl, 0, objrangeI[i + 1] - objrangeI[i]);
                    objlist[i] = new Object3d(vertexl, indexl);
                }
            }
            return objlist;
        }
    }
    #endregion

    #region Object3d
    class Object3d {
        public List<Vector3> vertex = new List<Vector3>();
        public List<int[]> index = new List<int[]>();
        public List<Vector2> uvtvertex = new List<Vector2>();
        public List<int[]> uvtindex = new List<int[]>();
        public List<Vector3> tNormal = new List<Vector3>();
        public List<Vector3> vNormal = new List<Vector3>();

        private List<Vector3> _vertex = new List<Vector3>();

        public List<Bitmap> texture = new List<Bitmap>();
        public List<Bitmap> normaltexture = new List<Bitmap>();

        public bool shadeless = false;
        public bool Reflect = false;
        public bool Texture = false;
        public bool NormalTexture = false;

        public string objname = " ";
        public List<string> matname = new List<string>();
        public List<int> Mat = new List<int>();

        public Octree octree;


        public Object3d (Vector3[] vertex, int[][] index) {
            for (int a = 0; a < vertex.Length; ++a) {
                this._vertex.Add(vertex[a]);
                this.vertex.Add(vertex[a]);
                this.vNormal.Add(new Vector3(0f, 0f, 0f));
            }
            for (int a = 0; a < index.Length; ++a) {
                this.index.Add(index[a]);
                this.tNormal.Add(new Vector3(0f, 0f, 0f));
            }

            this.Reflect = true;

            Console.WriteLine("complete!");
        }

        public Object3d (Vector3[] vertex, int[][] index, Vector2[] uvtvertex, int[][] uvtindex, Bitmap texture) {
            for (int a = 0; a < vertex.Length; ++a) {
                this._vertex.Add(vertex[a]);
                this.vertex.Add(vertex[a]);
                this.vNormal.Add(new Vector3(0f, 0f, 0f));
            }
            for (int a = 0; a < index.Length; ++a) {
                this.index.Add(index[a]);
                this.tNormal.Add(new Vector3(0f, 0f, 0f));
            }
            for (int a = 0; a < uvtvertex.Length; ++a) {
                this.uvtvertex.Add(uvtvertex[a]);
            }
            for (int a = 0; a < uvtvertex.Length; ++a) {
                this.uvtvertex.Add(uvtvertex[a]);
            }
            for (int a = 0; a < uvtindex.Length; ++a) {
                this.uvtindex.Add(uvtindex[a]);
            }

            this.texture.Add(texture);
            this.Mat.Add(0);
            this.Mat.Add(index.Length);
            this.Texture = true;

            Console.WriteLine("uv complete!");
        }

        public Object3d (Vector3[] vertex, int[][] index, Vector2[] uvtvertex, int[][] uvtindex, Bitmap texture, Bitmap normaltexture) {
            for (int a = 0; a < vertex.Length; ++a) {
                this._vertex.Add(vertex[a]);
                this.vertex.Add(vertex[a]);
                this.vNormal.Add(new Vector3(0f, 0f, 0f));
            }
            for (int a = 0; a < index.Length; ++a) {
                this.index.Add(index[a]);
                this.tNormal.Add(new Vector3(0f, 0f, 0f));
            }
            for (int a = 0; a < uvtvertex.Length; ++a) {
                this.uvtvertex.Add(uvtvertex[a]);
            }
            for (int a = 0; a < uvtindex.Length; ++a) {
                this.uvtindex.Add(uvtindex[a]);
            }

            this.texture.Add(texture);
            this.normaltexture.Add(normaltexture);

            this.Mat.Add(0);
            this.Mat.Add(index.Length);

            this.Texture = true;
            this.NormalTexture = true;

            Console.WriteLine("uv complete!");
        }

        public Object3d (Vector3[] vertex, int[][] index, Vector2[] uvtvertex, int[][] uvtindex, Bitmap[] texture, string objname, string[] matname, int[] Mat) {
            this.objname = objname;
            for (int a = 0; a < vertex.Length; ++a) {
                this._vertex.Add(vertex[a]);
                this.vertex.Add(vertex[a]);
                this.vNormal.Add(new Vector3(0f, 0f, 0f));
            }
            for (int a = 0; a < index.Length; ++a) {
                this.index.Add(index[a]);
                this.tNormal.Add(new Vector3(0f, 0f, 0f));
            }
            for (int a = 0; a < uvtvertex.Length; ++a) {
                this.uvtvertex.Add(uvtvertex[a]);
            }

            for (int a = 0; a < uvtindex.Length; ++a) {
                this.uvtindex.Add(uvtindex[a]);
            }

            foreach (int m in Mat) {
                this.Mat.Add(m);
                Console.WriteLine(m);
            }
            foreach (Bitmap tex in texture) {
                this.texture.Add(tex);
            }

            this.Texture = true;

            Console.WriteLine("uvm complete!");
        }

        public void SetNormal () {
            for (int a = 0; a < this.tNormal.Count; ++a) {
                this.tNormal[a] = new Vector3(0f, 0f, 0f);
            }
            for (int a = 0; a < this.vNormal.Count; ++a) {
                this.vNormal[a] = new Vector3(0f, 0f, 0f);
            }

            for (int a = 0; a < this.tNormal.Count; ++a) {
                this.tNormal[a] = Vector3.Normalize(Vector3.Cross(
                  new Vector3(this.vertex[this.index[a][1]].x - this.vertex[this.index[a][0]].x
                            , this.vertex[this.index[a][1]].y - this.vertex[this.index[a][0]].y
                            , this.vertex[this.index[a][1]].z - this.vertex[this.index[a][0]].z)
                , new Vector3(this.vertex[this.index[a][2]].x - this.vertex[this.index[a][0]].x
                            , this.vertex[this.index[a][2]].y - this.vertex[this.index[a][0]].y
                            , this.vertex[this.index[a][2]].z - this.vertex[this.index[a][0]].z)));
            }

            for (int a = 0; a < this.index.Count; ++a) {
                this.vNormal[this.index[a][0]] = this.vNormal[this.index[a][0]] + this.tNormal[a];
                this.vNormal[this.index[a][1]] = this.vNormal[this.index[a][1]] + this.tNormal[a];
                this.vNormal[this.index[a][2]] = this.vNormal[this.index[a][2]] + this.tNormal[a];
            }
            for (int a = 0; a < this.vNormal.Count; ++a) {
                this.vNormal[a] = Vector3.Normalize(this.vNormal[a]);
            }
        }

        public void SetOctree () {
            this.octree = new Octree(this.vertex, this.index);
        }

        public void Transform (Matrix m) {
            for (int a = 0; a < this.vertex.Count; ++a) {
                this.vertex[a] = Matrix.matmul14(this._vertex[a], m);
            }

            //this.octree = new Octree(this.vertex, this.index);
        }

        public void SetRenderSetting () {
            SetNormal();
            SetOctree();
            GC.Collect();
        }


    }
    #endregion

    #region Matrix
    class Matrix {
        float[,] array = new float[4, 4];

        public Matrix () {
            this.array = new float[,]{{1f,0f,0f,0f},
                                      {0f,1f,0f,0f},
                                      {0f,0f,1f,0f},
                                      {0f,0f,0f,1f}};
        }

        public Matrix (float a00, float a01, float a02, float a03,
                      float a10, float a11, float a12, float a13,
                      float a20, float a21, float a22, float a23,
                      float a30, float a31, float a32, float a33) {
            this.array[0, 0] = a00;
            this.array[0, 1] = a01;
            this.array[0, 2] = a02;
            this.array[0, 3] = a03;
            this.array[1, 0] = a10;
            this.array[1, 1] = a11;
            this.array[1, 2] = a12;
            this.array[1, 3] = a13;
            this.array[2, 0] = a20;
            this.array[2, 1] = a21;
            this.array[2, 2] = a22;
            this.array[2, 3] = a23;
            this.array[3, 0] = a30;
            this.array[3, 1] = a31;
            this.array[3, 2] = a32;
            this.array[3, 3] = a33;
        }

        public Matrix (float[,] array) {
            this.array = array;
        }



        public float this[int i, int j] {
            set { this.array[i, j] = value; }
            get { return this.array[i, j]; }
        }


        public static Matrix inversematrix44 (Matrix a) {
            Matrix b = new Matrix(0f, 0f, 0f, 0f,
                                   0f, 0f, 0f, 0f,
                                   0f, 0f, 0f, 0f,
                                   0f, 0f, 0f, 0f);

            b[0, 0] = a[1, 1] * a[2, 2] * a[3, 3] + a[1, 2] * a[2, 3] * a[3, 1] + a[1, 3] * a[2, 1] * a[3, 2] - a[1, 1] * a[2, 3] * a[3, 2] - a[1, 2] * a[2, 1] * a[3, 3] - a[1, 3] * a[2, 2] * a[3, 1];
            b[0, 1] = a[0, 1] * a[2, 3] * a[3, 2] + a[0, 2] * a[2, 1] * a[3, 3] + a[0, 3] * a[2, 2] * a[3, 1] - a[0, 1] * a[2, 2] * a[3, 3] - a[0, 2] * a[2, 3] * a[3, 1] - a[0, 3] * a[2, 1] * a[3, 2];
            b[0, 2] = a[0, 1] * a[1, 2] * a[3, 3] + a[0, 2] * a[1, 3] * a[3, 1] + a[0, 3] * a[1, 1] * a[3, 2] - a[0, 1] * a[1, 3] * a[3, 2] - a[0, 2] * a[1, 1] * a[3, 3] - a[0, 3] * a[1, 2] * a[3, 1];
            b[0, 3] = a[0, 1] * a[1, 3] * a[2, 2] + a[0, 2] * a[1, 1] * a[2, 3] + a[0, 3] * a[1, 2] * a[2, 1] - a[0, 1] * a[1, 2] * a[2, 3] - a[0, 2] * a[1, 3] * a[2, 1] - a[0, 3] * a[1, 1] * a[2, 2];

            b[1, 0] = a[1, 0] * a[2, 3] * a[3, 2] + a[1, 2] * a[2, 0] * a[3, 3] + a[1, 3] * a[2, 2] * a[3, 0] - a[1, 0] * a[2, 2] * a[3, 3] - a[1, 2] * a[2, 3] * a[3, 0] - a[1, 3] * a[2, 0] * a[3, 2];
            b[1, 1] = a[0, 0] * a[2, 2] * a[3, 3] + a[0, 2] * a[2, 3] * a[3, 0] + a[0, 3] * a[2, 0] * a[3, 2] - a[0, 0] * a[2, 3] * a[3, 2] - a[0, 2] * a[2, 0] * a[3, 3] - a[0, 3] * a[2, 2] * a[3, 0];
            b[1, 2] = a[0, 0] * a[1, 3] * a[3, 2] + a[0, 2] * a[1, 0] * a[3, 3] + a[0, 3] * a[1, 2] * a[3, 0] - a[0, 0] * a[1, 2] * a[3, 3] - a[0, 2] * a[1, 3] * a[3, 0] - a[0, 3] * a[1, 0] * a[3, 2];
            b[1, 3] = a[0, 0] * a[1, 2] * a[2, 3] + a[0, 2] * a[1, 3] * a[2, 0] + a[0, 3] * a[1, 0] * a[2, 2] - a[0, 0] * a[1, 3] * a[2, 2] - a[0, 2] * a[1, 0] * a[2, 3] - a[0, 3] * a[1, 2] * a[2, 0];

            b[2, 0] = a[1, 0] * a[2, 1] * a[3, 3] + a[1, 1] * a[2, 3] * a[3, 0] + a[1, 3] * a[2, 0] * a[3, 1] - a[1, 0] * a[2, 3] * a[3, 1] - a[1, 1] * a[2, 0] * a[3, 3] - a[1, 3] * a[2, 1] * a[3, 0];
            b[2, 1] = a[0, 0] * a[2, 3] * a[3, 1] + a[0, 1] * a[2, 0] * a[3, 3] + a[0, 3] * a[2, 1] * a[3, 0] - a[0, 0] * a[2, 1] * a[3, 3] - a[0, 1] * a[2, 3] * a[3, 0] - a[0, 3] * a[2, 0] * a[3, 1];
            b[2, 2] = a[0, 0] * a[1, 1] * a[3, 3] + a[0, 1] * a[1, 3] * a[3, 0] + a[0, 3] * a[1, 0] * a[3, 1] - a[0, 0] * a[1, 3] * a[3, 1] - a[0, 1] * a[1, 0] * a[3, 3] - a[0, 3] * a[1, 1] * a[3, 0];
            b[2, 3] = a[0, 0] * a[1, 3] * a[2, 1] + a[0, 1] * a[1, 0] * a[2, 3] + a[0, 3] * a[1, 1] * a[2, 0] - a[0, 0] * a[1, 1] * a[2, 3] - a[0, 1] * a[1, 3] * a[2, 0] - a[0, 3] * a[1, 0] * a[2, 1];

            b[3, 0] = a[1, 0] * a[2, 2] * a[3, 1] + a[1, 1] * a[2, 0] * a[3, 2] + a[1, 2] * a[2, 1] * a[3, 0] - a[1, 0] * a[2, 1] * a[3, 2] - a[1, 1] * a[2, 2] * a[3, 0] - a[1, 2] * a[2, 0] * a[3, 1];
            b[3, 1] = a[0, 0] * a[2, 1] * a[3, 2] + a[0, 1] * a[2, 2] * a[3, 0] + a[0, 2] * a[2, 0] * a[3, 1] - a[0, 0] * a[2, 2] * a[3, 1] - a[0, 1] * a[2, 0] * a[3, 2] - a[0, 2] * a[2, 1] * a[3, 0];
            b[3, 2] = a[0, 0] * a[1, 2] * a[3, 1] + a[0, 1] * a[1, 0] * a[3, 2] + a[0, 2] * a[1, 1] * a[3, 0] - a[0, 0] * a[1, 1] * a[3, 2] - a[0, 1] * a[1, 2] * a[3, 0] - a[0, 2] * a[1, 0] * a[3, 1];
            b[3, 3] = a[0, 0] * a[1, 1] * a[2, 2] + a[0, 1] * a[1, 2] * a[2, 0] + a[0, 2] * a[1, 0] * a[2, 1] - a[0, 0] * a[1, 2] * a[2, 1] - a[0, 1] * a[1, 0] * a[2, 2] - a[0, 2] * a[1, 1] * a[2, 0];

            float deta = a[0, 0] * a[1, 1] * a[2, 2] * a[3, 3] + a[0, 0] * a[1, 2] * a[2, 3] * a[3, 1] + a[0, 0] * a[1, 3] * a[2, 1] * a[3, 2]
                        + a[0, 1] * a[1, 0] * a[2, 3] * a[3, 2] + a[0, 1] * a[1, 2] * a[2, 0] * a[3, 3] + a[0, 1] * a[1, 3] * a[2, 2] * a[3, 0]
                        + a[0, 2] * a[1, 0] * a[2, 1] * a[3, 3] + a[0, 2] * a[1, 1] * a[2, 3] * a[3, 0] + a[0, 2] * a[1, 3] * a[2, 0] * a[3, 1]
                        + a[0, 3] * a[1, 0] * a[2, 2] * a[3, 1] + a[0, 3] * a[1, 1] * a[2, 0] * a[3, 2] + a[0, 3] * a[1, 2] * a[2, 1] * a[3, 0]
                        - a[0, 0] * a[1, 1] * a[2, 3] * a[3, 2] - a[0, 0] * a[1, 2] * a[2, 1] * a[3, 3] - a[0, 0] * a[1, 3] * a[2, 2] * a[3, 1]
                        - a[0, 1] * a[1, 0] * a[2, 2] * a[3, 3] - a[0, 1] * a[1, 2] * a[2, 3] * a[3, 0] - a[0, 1] * a[1, 3] * a[2, 0] * a[3, 2]
                        - a[0, 2] * a[1, 0] * a[2, 3] * a[3, 1] - a[0, 2] * a[1, 1] * a[2, 0] * a[3, 3] - a[0, 2] * a[1, 3] * a[2, 1] * a[3, 0]
                        - a[0, 3] * a[1, 0] * a[2, 1] * a[3, 2] - a[0, 3] * a[1, 1] * a[2, 2] * a[3, 0] - a[0, 3] * a[1, 2] * a[2, 0] * a[3, 1];

            if (deta == 0f) {
                return a;
            }

            return new Matrix(b[0, 0] / deta, b[0, 1] / deta, b[0, 2] / deta, b[0, 3] / deta,
                              b[1, 0] / deta, b[1, 1] / deta, b[1, 2] / deta, b[1, 3] / deta,
                              b[2, 0] / deta, b[2, 1] / deta, b[2, 2] / deta, b[2, 3] / deta,
                              b[3, 0] / deta, b[3, 1] / deta, b[3, 2] / deta, b[3, 3] / deta);
        }

        public static Vector3 matmul14proj (Vector3 v1, Matrix m) {
            float[] an = { 0f, 0f, 0f, 0f };
            float[] v = { v1.x, v1.y, v1.z, 1f };

            for (int k = 0; k < 4; ++k) {
                for (int o = 0; o < 4; ++o) {
                    an[k] += v[o] * m[o, k];
                }
            }

            if (an[3] > 0f) {
                return new Vector3(an[0] / an[3], an[1] / an[3], an[2] / an[3]);
            } else {
                return new Vector3(an[0], an[1], an[2]);
            }

        }

        public static Vector3 matmul14 (Vector3 v1, Matrix m) {
            float[] an = { 0f, 0f, 0f, 0f };
            float[] v = { v1.x, v1.y, v1.z, 1f };
            for (int k = 0; k < 4; ++k) {
                for (int o = 0; o < 4; ++o) {
                    an[k] += v[o] * m[o, k];
                }
            }

            return new Vector3(an[0], an[1], an[2]);
        }

        public static Matrix matmul44 (Matrix m1, Matrix m2) {
            Matrix an = new Matrix(0f, 0f, 0f, 0f,
                                   0f, 0f, 0f, 0f,
                                   0f, 0f, 0f, 0f,
                                   0f, 0f, 0f, 0f);
            for (int i = 0; i < 4; ++i) {
                for (int k = 0; k < 4; ++k) {
                    for (int o = 0; o < 4; ++o) {
                        an[i, k] += m1[i, o] * m2[o, k];
                    }
                }
            }
            return an;
        }

        public static Matrix PerspectiveFovLH (int cvx, int cvy, float fovh) {
            float aspect = (float)cvy / (float)cvx;
            float fov = (float)(fovh / 180f * Math.PI);
            float near = 4.0f;
            float far = 400.0f;
            float sx = aspect * ((float)Math.Cos(fov * 0.5f) / (float)Math.Sin(fov * 0.5f));
            float sy = 1.0f * ((float)Math.Cos(fov * 0.5f) / (float)Math.Sin(fov * 0.5f));
            float sz = far / (far - near);
            return new Matrix(sx, 0f, 0f, 0f,
                               0f, sy, 0f, 0f,
                               0f, 0f, sz, 1f,
                               0f, 0f, -sz * near, 0f);
        }

        public static Matrix PerspectiveFovRH (int cvx, int cvy, float fovh) {
            float aspect = (float)cvy / (float)cvx;
            float fov = (float)(fovh / 180f * Math.PI);
            float near = 4.0f;
            float far = 400.0f;
            float sx = aspect * ((float)Math.Cos(fov * 0.5f) / (float)Math.Sin(fov * 0.5f));
            float sy = 1.0f * ((float)Math.Cos(fov * 0.5f) / (float)Math.Sin(fov * 0.5f));
            float sz = far / (near - far);
            return new Matrix(sx, 0f, 0f, 0f,
                               0f, sy, 0f, 0f,
                               0f, 0f, sz, -1f,
                               0f, 0f, sz * near, 0f);
        }

        public static Matrix ViewPort (int cvx, int cvy) {
            return new Matrix((float)cvx / 2, 0f, 0f, 0f,
                               0f, (float)-cvy / 2, 0f, 0f,
                               0f, 0f, 1f, 0f,
                               (float)cvx / 2, (float)cvy / 2, 0f, 1f);
        }
    }
    #endregion

    #region Ray
    class Ray {
        public Vector3 Position = new Vector3(0f, 0f, 0f);
        public Vector3 Direction = new Vector3(0f, 0f, -1f);
        public float Distance = 0f;
        public float U = 0f;
        public float V = 0f;

        public Vector3 HitRayPosition {
            get {
                return this.Position + (Vector3.Normalize(this.Direction) * this.Distance);
            }
        }

        public bool RayTriangle (Vector3 vertex1, Vector3 vertex2, Vector3 vertex3) {

            Vector3 edge1 = vertex2 - vertex1;
            Vector3 edge2 = vertex3 - vertex1;
            Vector3 pvec = Vector3.Cross(this.Direction, edge2);
            float det = Vector3.Dot(edge1, pvec);
            if (det < 0.0001f) {
                return false;
            }
            Vector3 tvec = this.Position - vertex1;
            this.U = Vector3.Dot(tvec, pvec);
            if (this.U < 0f || this.U > det) {
                return false;
            }
            Vector3 qvec = Vector3.Cross(tvec, edge1);
            this.V = Vector3.Dot(this.Direction, qvec);

            if (this.V < 0f || (this.U + this.V) > det) {
                return false;
            }
            this.Distance = Vector3.Dot(edge2, qvec);
            float fInvDet = 1f / det;
            this.Distance *= fInvDet;
            this.U *= fInvDet;
            this.V *= fInvDet;
            return true;
        }

        public static int[] RayTracing (List<Object3d> objlist, Ray rayy, int sam, Light[] light, object syncObject) {
            int[] color = { 0, 0, 0 };
            float d = 400f;
            int ob = 0;//hitobject
            int a = 0;//hitindex
            bool hit = false;
            Ray ray = new Ray();
            ray.Distance = 400f;
            for (int o = 0; o < objlist.Count; ++o) {
                foreach (int aa in objlist[o].octree.OctreeRayHitindex(rayy)) {
                    if (rayy.RayTriangle(objlist[o].vertex[objlist[o].index[aa][0]], objlist[o].vertex[objlist[o].index[aa][1]], objlist[o].vertex[objlist[o].index[aa][2]])) {
                        if (rayy.Distance < d && rayy.Distance > 0.001f) {
                            d = rayy.Distance;
                            ray.Position = rayy.Position;
                            ray.Distance = rayy.Distance;
                            ray.Direction = rayy.Direction;
                            ray.U = rayy.U;
                            ray.V = rayy.V;
                            a = aa;
                            ob = o;
                            hit = true;
                        }
                    }
                }
            }

            if (hit == false) {
                return color;
            }


            Vector3 pv = (objlist[ob].vNormal[objlist[ob].index[a][0]] * (1f - (ray.U + ray.V))) + (objlist[ob].vNormal[objlist[ob].index[a][1]] * ray.U) + (objlist[ob].vNormal[objlist[ob].index[a][2]] * ray.V);
            pv = Vector3.Normalize(pv);

            Vector2 t = new Vector2(0, 0);
            Vector2 ta = new Vector2(0, 0);
            Color co = Color.White;

            Monitor.Enter(syncObject);
            try {
                if (objlist[ob].Texture == true) {
                    for (int i = 0; i < objlist[ob].Mat.Count - 1; ++i) {
                        if (objlist[ob].Mat[i] <= a && a < objlist[ob].Mat[i + 1]) {
                            co = Texture.TexColor(objlist[ob], ray, a, i);
                        }
                    }
                }
                if (objlist[ob].NormalTexture == true) {

                    for (int i = 0; i < objlist[ob].Mat.Count - 1; ++i) {
                        if (objlist[ob].Mat[i] <= a && a < objlist[ob].Mat[i + 1]) {
                            pv = Texture.TexNormal(objlist[ob], ray, a, i, pv);
                        }
                    }

                }

            } finally {
                Monitor.Exit(syncObject);
            }

            int di = 0;
            if (objlist[ob].shadeless == true) {
                di = 255;
            } else {
                for (int i = 0; i < light.Length; i++) {
                    Vector3 lv = Vector3.Normalize(ray.HitRayPosition - light[i].position);
                    lv = new Vector3(0, 0, 0) - lv;
                    if (Ray.Rayshadow(objlist, lv, ray.HitRayPosition, Vector3.Distance(ray.HitRayPosition - light[i].position)) != true) {
                        if (Vector3.Dot(pv, lv) > 0f) {
                            di += (int)((255 * Vector3.Dot(pv, lv)) * 0.8f);
                        }
                    }
                }
            }

            if (di > 255) {
                di = 255;
            }

            color[0] = (int)(co.R * di / 255f);
            color[1] = (int)(co.G * di / 255f);
            color[2] = (int)(co.B * di / 255f);

            //normalcolor
            //color[0] = (int)(127.5 + (pv.x * 127.5));
            //color[1] = (int)(127.5 + (pv.y * 127.5));
            //color[2] = (int)(127.5 + (pv.z * 127.5));
            if (objlist[ob].Reflect == true && sam > 0) {
                Ray refray = new Ray();
                refray.Position = ray.HitRayPosition;
                refray.Direction = Vector3.Normalize(Vector3.Reflect(ray.Direction, pv));
                int[] color2 = Ray.RayTracing(objlist, refray, sam - 1, light, syncObject);
                color[0] += color2[0];
                color[1] += color2[1];
                color[2] += color2[2];
                color[0] /= 2;
                color[1] /= 2;
                color[2] /= 2;
            }



            return color;
        }

        public static int[] RayTracingOlder (List<Object3d> objlist, Ray ray, int sam, Light[] light, object syncObject) {
            int[] color = { 0, 0, 0 };
            float d = 400f;
            List<Object3d> objlistb = new List<Object3d>();

            foreach (Object3d obj in objlist) {
                //Console.WriteLine(obj.octree.OctreeRayHitindex(ray).Length);
                foreach (int a in obj.octree.OctreeRayHitindex(ray)) {

                    if (ray.RayTriangle(obj.vertex[obj.index[a][0]], obj.vertex[obj.index[a][1]], obj.vertex[obj.index[a][2]])) {
                        if (ray.Distance < d && ray.Distance > 0.001f) {
                            d = ray.Distance;
                            Vector3 pv = (obj.vNormal[obj.index[a][0]] * (1f - (ray.U + ray.V))) + (obj.vNormal[obj.index[a][1]] * ray.U) + (obj.vNormal[obj.index[a][2]] * ray.V);
                            pv = Vector3.Normalize(pv);

                            Vector2 t = new Vector2(0, 0);
                            Vector2 ta = new Vector2(0, 0);
                            Color co = Color.White;
                            Color noco = Color.White;
                            Monitor.Enter(syncObject);
                            try {
                                if (obj.Texture == true) {
                                    for (int i = 0; i < obj.Mat.Count - 1; ++i) {
                                        if (obj.Mat[i] <= a && a < obj.Mat[i + 1]) {
                                            co = Texture.TexColor(obj, ray, a, i);
                                        }
                                    }
                                }
                                if (obj.NormalTexture == true) {

                                    for (int i = 0; i < obj.Mat.Count - 1; ++i) {
                                        if (obj.Mat[i] <= a && a < obj.Mat[i + 1]) {
                                            pv = Texture.TexNormal(obj, ray, a, i, pv);
                                        }
                                    }

                                }

                            } finally {
                                Monitor.Exit(syncObject);
                            }

                            int di = 0;
                            if (obj.shadeless == true) {
                                di = 255;
                            } else {
                                for (int i = 0; i < light.Length; i++) {
                                    Vector3 lv = Vector3.Normalize(ray.HitRayPosition - light[i].position);
                                    lv = new Vector3(0, 0, 0) - lv;
                                    if (Ray.Rayshadow(objlist, lv, ray.HitRayPosition, Vector3.Distance(ray.HitRayPosition - light[i].position)) != true) {
                                        if (Vector3.Dot(pv, lv) > 0f) {
                                            di += (int)((255 * Vector3.Dot(pv, lv)) * 0.8f);
                                        }
                                    }
                                }
                            }

                            if (di > 255) {
                                di = 255;
                            }

                            color[0] = (int)(co.R * di / 255f);
                            color[1] = (int)(co.G * di / 255f);
                            color[2] = (int)(co.B * di / 255f);

                            //color[0] = (int)(127.5 + (pv.x * 127.5));
                            //color[1] = (int)(127.5 + (pv.y * 127.5));
                            //color[2] = (int)(127.5 + (pv.z * 127.5));
                            if (obj.Reflect == true && sam > 0) {
                                Ray refray = new Ray();
                                refray.Position = ray.HitRayPosition;
                                refray.Direction = Vector3.Normalize(Vector3.Reflect(ray.Direction, pv));
                                int[] color2 = Ray.RayTracing(objlist, refray, sam - 1, light, syncObject);
                                color[0] += color2[0];
                                color[1] += color2[1];
                                color[2] += color2[2];
                                color[0] /= 2;
                                color[1] /= 2;
                                color[2] /= 2;
                            }

                        }
                    }
                }
            }
            return color;
        }

        public static bool Rayshadow (List<Object3d> objlist, Vector3 lv, Vector3 hpos, float Distance) {
            Ray sray = new Ray();
            sray.Position = hpos;
            sray.Direction = lv;

            foreach (Object3d obj in objlist) {
                foreach (int a in obj.octree.OctreeRayHitindex(sray)) {
                    if (sray.RayTriangle(obj.vertex[obj.index[a][0]], obj.vertex[obj.index[a][1]], obj.vertex[obj.index[a][2]])) {
                        if (sray.Distance > 0.001f && sray.Distance <= Distance) {
                            return true;
                        }
                    }
                    if (sray.RayTriangle(obj.vertex[obj.index[a][0]], obj.vertex[obj.index[a][2]], obj.vertex[obj.index[a][1]])) {
                        if (sray.Distance > 0.001f && sray.Distance <= Distance) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
    #endregion

    #region Vector3
    struct Vector3 {
        public float x;
        public float y;
        public float z;

        public Vector3 (float x, float y, float z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3 operator + (Vector3 v1, Vector3 v2) {
            return new Vector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }

        public static Vector3 operator - (Vector3 v1, Vector3 v2) {
            return new Vector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static Vector3 operator * (Vector3 v1, float m) {
            return new Vector3(v1.x * m, v1.y * m, v1.z * m);
        }

        public static Vector3 operator / (Vector3 v1, float d) {
            return new Vector3(v1.x / d, v1.y / d, v1.z / d);
        }

        public static float Dot (Vector3 v1, Vector3 v2) {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }

        public static Vector3 Cross (Vector3 v1, Vector3 v2) {
            return new Vector3(v1.y * v2.z - v1.z * v2.y, v1.z * v2.x - v1.x * v2.z, v1.x * v2.y - v1.y * v2.x);
        }

        public static Vector3 Normalize (Vector3 v1) {
            if (Math.Sqrt(v1.x * v1.x + v1.y * v1.y + v1.z * v1.z) != 0) {
                return new Vector3(v1.x / (float)Math.Sqrt(v1.x * v1.x + v1.y * v1.y + v1.z * v1.z), v1.y / (float)Math.Sqrt(v1.x * v1.x + v1.y * v1.y + v1.z * v1.z), v1.z / (float)Math.Sqrt(v1.x * v1.x + v1.y * v1.y + v1.z * v1.z));
            }
            return new Vector3(0f, 0f, 0f);
        }

        public static float Distance (Vector3 v1) {
            return (float)Math.Sqrt(v1.x * v1.x + v1.y * v1.y + v1.z * v1.z);
        }

        public static Vector3 Reflect (Vector3 v1, Vector3 n) {
            return n * (2f * (-v1.x * n.x + -v1.y * n.y + -v1.z * n.z)) + v1;

        }
    }
    #endregion

    #region Vector2
    struct Vector2 {
        public float x;
        public float y;

        public Vector2 (float x, float y) {
            this.x = x;
            this.y = y;
        }

        public static Vector2 operator + (Vector2 v1, Vector2 v2) {
            return new Vector2(v1.x + v2.x, v1.y + v2.y);
        }

        public static Vector2 operator - (Vector2 v1, Vector2 v2) {
            return new Vector2(v1.x - v2.x, v1.y - v2.y);
        }

        public static Vector2 operator * (Vector2 v1, float m) {
            return new Vector2(v1.x * m, v1.y * m);
        }

        public static Vector2 operator / (Vector2 v1, float d) {
            return new Vector2(v1.x / d, v1.y / d);
        }

        public static Vector2 Normalize (Vector2 v1) {
            if (Math.Sqrt(v1.x * v1.x + v1.y * v1.y) != 0) {
                return new Vector2(v1.x / (float)Math.Sqrt(v1.x * v1.x + v1.y * v1.y), v1.y / (float)Math.Sqrt(v1.x * v1.x + v1.y * v1.y));
            }
            return new Vector2(0f, 0f);
        }
    }
    #endregion

    #region Texture
    class Texture {
        public static Color TexColor (Object3d obj, Ray ray, int indexnumber, int texnumber) {
            Vector2 t;
            Vector2 ta;
            Color texcolor = new Color();

            Vector2 wh = new Vector2((float)obj.texture[texnumber].Width - 1, (float)obj.texture[texnumber].Height - 1);

            Vector2 point = obj.uvtvertex[obj.uvtindex[indexnumber][0]];
            Vector2 v01 = obj.uvtvertex[obj.uvtindex[indexnumber][1]] - point;
            Vector2 v02 = obj.uvtvertex[obj.uvtindex[indexnumber][2]] - point;

            t = point + ((v01 * ray.U) + (v02 * ray.V));
            ta.x = t.x * wh.x;
            ta.y = t.y * wh.y;
            ta.y = wh.y - ta.y;
            texcolor = obj.texture[texnumber].GetPixel((int)ta.x, (int)ta.y);

            return texcolor;
        }

        public static Vector3 TexNormal (Object3d obj, Ray ray, int indexnumber, int texnumber, Vector3 pv) {


            Vector2 t;
            Vector2 ta;
            Color noco = new Color();

            Vector2 wh = new Vector2((float)obj.normaltexture[texnumber].Width - 1, (float)obj.normaltexture[texnumber].Height - 1);

            Vector2 point = obj.uvtvertex[obj.uvtindex[indexnumber][0]];
            Vector2 v01 = obj.uvtvertex[obj.uvtindex[indexnumber][1]] - point;
            Vector2 v02 = obj.uvtvertex[obj.uvtindex[indexnumber][2]] - point;

            t = point + ((v01 * ray.U) + (v02 * ray.V));
            ta.x = t.x * wh.x;
            ta.y = t.y * wh.y;
            ta.y = wh.y - ta.y;

            noco = obj.normaltexture[texnumber].GetPixel((int)ta.x, (int)ta.y);
            Vector3 tangent = new Vector3(0f, 0f, 0f);
            Vector3 binormal = new Vector3(0f, 0f, 0f);
            Texture.tangent(obj.vertex[obj.index[indexnumber][0]], obj.vertex[obj.index[indexnumber][1]], obj.vertex[obj.index[indexnumber][2]]
                          , obj.uvtvertex[obj.uvtindex[indexnumber][0]], obj.uvtvertex[obj.uvtindex[indexnumber][1]], obj.uvtvertex[obj.uvtindex[indexnumber][2]]
                          , ref tangent, ref binormal);

            float r = 0f;
            float g = 0f;
            float b = 0f;

            r = ((float)noco.R / (float)128) - 1f;
            g = ((float)noco.G / (float)128) - 1f;
            b = ((float)noco.B / (float)128) - 1f;

            return Vector3.Normalize((tangent * r) + (binormal * g) + (pv * b));
        }


        public static void tangent (Vector3 p0, Vector3 p1, Vector3 p2, Vector2 uv0, Vector2 uv1, Vector2 uv2, ref Vector3 tangent, ref Vector3 binormal) {
            Vector3[] CP0 = {
            new Vector3( p0.x, uv0.x, uv0.y ),
            new Vector3( p0.y, uv0.x, uv0.y ),
            new Vector3( p0.z, uv0.x, uv0.y ),
            };
            Vector3[] CP1 = {
            new Vector3( p1.x, uv1.x, uv1.y ),
            new Vector3( p1.y, uv1.x, uv1.y ),
            new Vector3( p1.z, uv1.x, uv1.y ),
            };
            Vector3[] CP2 = {
            new Vector3( p2.x, uv2.x, uv2.y ),
            new Vector3( p2.y, uv2.x, uv2.y ),
            new Vector3( p2.z, uv2.x, uv2.y ),
            };
            float[] u = new float[3];
            float[] v = new float[3];

            for (int i = 0; i < 3; ++i) {
                Vector3 V1 = CP1[i] - CP0[i];
                Vector3 V2 = CP2[i] - CP1[i];
                Vector3 ABC;
                ABC = Vector3.Cross(V1, V2);

                if (ABC.x == 0.0f) {
                    tangent = new Vector3(0f, 0f, 0f);
                    binormal = new Vector3(0f, 0f, 0f);
                    return;

                }
                u[i] = -ABC.y / ABC.x;
                v[i] = -ABC.z / ABC.x;
            }

            tangent = Vector3.Normalize(new Vector3(u[0], u[1], u[2]));
            binormal = Vector3.Normalize(new Vector3(v[0], v[1], v[2]));

        }
    }
    #endregion

    #region BoundingBox
    class BoundingBox {
        public float max_x = 0f;
        public float min_x = 0f;
        public float max_y = 0f;
        public float min_y = 0f;
        public float max_z = 0f;
        public float min_z = 0f;

        public Vector3[] vertex = new Vector3[8];

        public BoundingBox (List<Vector3> v) {
            this.max_x = v[0].x;
            this.min_x = v[0].x;
            this.max_y = v[0].y;
            this.min_y = v[0].y;
            this.max_z = v[0].z;
            this.min_z = v[0].z;
            foreach (Vector3 a in v) {
                if (this.max_x < a.x) {
                    this.max_x = a.x;
                }
                if (this.min_x > a.x) {
                    this.min_x = a.x;
                }
                if (this.max_y < a.y) {
                    this.max_y = a.y;
                }
                if (this.min_y > a.y) {
                    this.min_y = a.y;
                }
                if (this.max_z < a.z) {
                    this.max_z = a.z;
                }
                if (this.min_z > a.z) {
                    this.min_z = a.z;
                }
            }

            this.vertex[0] = new Vector3(this.min_x, this.min_y, this.min_z);
            this.vertex[1] = new Vector3(this.min_x, this.max_y, this.min_z);
            this.vertex[2] = new Vector3(this.min_x, this.min_y, this.max_z);
            this.vertex[3] = new Vector3(this.min_x, this.max_y, this.max_z);
            this.vertex[4] = new Vector3(this.max_x, this.min_y, this.min_z);
            this.vertex[5] = new Vector3(this.max_x, this.max_y, this.min_z);
            this.vertex[6] = new Vector3(this.max_x, this.min_y, this.max_z);
            this.vertex[7] = new Vector3(this.max_x, this.max_y, this.max_z);
        }

        public BoundingBox (float max_x, float min_x, float max_y, float min_y, float max_z, float min_z) {
            this.max_x = max_x;
            this.min_x = min_x;
            this.max_y = max_y;
            this.min_y = min_y;
            this.max_z = max_z;
            this.min_z = min_z;

            this.vertex[0] = new Vector3(this.min_x, this.min_y, this.min_z);
            this.vertex[1] = new Vector3(this.min_x, this.max_y, this.min_z);
            this.vertex[2] = new Vector3(this.min_x, this.min_y, this.max_z);
            this.vertex[3] = new Vector3(this.min_x, this.max_y, this.max_z);
            this.vertex[4] = new Vector3(this.max_x, this.min_y, this.min_z);
            this.vertex[5] = new Vector3(this.max_x, this.max_y, this.min_z);
            this.vertex[6] = new Vector3(this.max_x, this.min_y, this.max_z);
            this.vertex[7] = new Vector3(this.max_x, this.max_y, this.max_z);
        }

        public static BoundingBox[] Split (BoundingBox BBox) {
            float half_x = (BBox.min_x + BBox.max_x) / 2f;
            float half_y = (BBox.min_y + BBox.max_y) / 2f;
            float half_z = (BBox.min_z + BBox.max_z) / 2f;
            BoundingBox[] SplitBBox = new BoundingBox[8];

            SplitBBox[0] = new BoundingBox(half_x, BBox.min_x, half_y, BBox.min_y, half_z, BBox.min_z);
            SplitBBox[1] = new BoundingBox(BBox.max_x, half_x, half_y, BBox.min_y, half_z, BBox.min_z);
            SplitBBox[2] = new BoundingBox(half_x, BBox.min_x, BBox.max_y, half_y, half_z, BBox.min_z);
            SplitBBox[3] = new BoundingBox(BBox.max_x, half_x, BBox.max_y, half_y, half_z, BBox.min_z);

            SplitBBox[4] = new BoundingBox(half_x, BBox.min_x, half_y, BBox.min_y, BBox.max_z, half_z);
            SplitBBox[5] = new BoundingBox(BBox.max_x, half_x, half_y, BBox.min_y, BBox.max_z, half_z);
            SplitBBox[6] = new BoundingBox(half_x, BBox.min_x, BBox.max_y, half_y, BBox.max_z, half_z);
            SplitBBox[7] = new BoundingBox(BBox.max_x, half_x, BBox.max_y, half_y, BBox.max_z, half_z);

            return SplitBBox;
        }

        public static bool AABBTriangle (BoundingBox BBox, List<Vector3> v) {
            if (BoundingBox.AABBAABB(BBox, new BoundingBox(new List<Vector3> { v[0], v[1], v[2] }))
               && BoundingBox.AABBPlane(BBox, new List<Vector3> { v[0], v[1], v[2] })) {
                return true;
            } else {
                return false;
            }
        }

        public static bool AABBAABB (BoundingBox BBoxA, BoundingBox BBoxB) {
            if ((BBoxB.min_x <= BBoxA.max_x && BBoxB.max_x >= BBoxA.min_x)
               && (BBoxB.min_y <= BBoxA.max_y && BBoxB.max_y >= BBoxA.min_y)
               && (BBoxB.min_z <= BBoxA.max_z && BBoxB.max_z >= BBoxA.min_z)
               ) {
                return true;
            } else {
                return false;
            }
        }

        public static bool AABBPlane (BoundingBox BBox, List<Vector3> v) {
            float[] d = new float[8];

            Vector3 PlaneVector = Vector3.Normalize(Vector3.Cross(
                            new Vector3(v[1].x - v[0].x
                                      , v[1].y - v[0].y
                                      , v[1].z - v[0].z)
                          , new Vector3(v[2].x - v[0].x
                                      , v[2].y - v[0].y
                                      , v[2].z - v[0].z)));

            Vector3 PlanePoint = new Vector3((v[0].x + v[1].x + v[2].x) / 3
                                            , (v[0].y + v[1].y + v[2].y) / 3
                                            , (v[0].z + v[1].z + v[2].z) / 3);

            d[0] = Vector3.Dot(Vector3.Normalize(BBox.vertex[0] - PlanePoint), PlaneVector);
            d[1] = Vector3.Dot(Vector3.Normalize(BBox.vertex[1] - PlanePoint), PlaneVector);
            d[2] = Vector3.Dot(Vector3.Normalize(BBox.vertex[2] - PlanePoint), PlaneVector);
            d[3] = Vector3.Dot(Vector3.Normalize(BBox.vertex[3] - PlanePoint), PlaneVector);
            d[4] = Vector3.Dot(Vector3.Normalize(BBox.vertex[4] - PlanePoint), PlaneVector);
            d[5] = Vector3.Dot(Vector3.Normalize(BBox.vertex[5] - PlanePoint), PlaneVector);
            d[6] = Vector3.Dot(Vector3.Normalize(BBox.vertex[6] - PlanePoint), PlaneVector);
            d[7] = Vector3.Dot(Vector3.Normalize(BBox.vertex[7] - PlanePoint), PlaneVector);

            if (d[0] > 0f) {
                for (int i = 1; i < 8; ++i) {
                    if (d[i] < 0f) {
                        return true;
                    }
                }

            } else {
                for (int i = 1; i < 8; ++i) {
                    if (d[i] > 0f) {
                        return true;
                    }
                }
            }
            return false;

        }

        public static bool RayAABB (Ray ray, BoundingBox BBox) {
            float min_x = 0f;
            float min_y = 0f;
            float min_z = 0f;
            float max_x = 0f;
            float max_y = 0f;
            float max_z = 0f;
            bool xy = false;
            bool zx = false;
            bool yz = false;

            if (ray.Direction.x != 0f) {
                min_y = ray.Position.y + ((ray.Direction.y / ray.Direction.x) * (BBox.min_x - ray.Position.x));
                max_y = ray.Position.y + ((ray.Direction.y / ray.Direction.x) * (BBox.max_x - ray.Position.x));
                if (min_y >= BBox.min_y && min_y <= BBox.max_y) { xy = true; }
                if (max_y >= BBox.min_y && max_y <= BBox.max_y) { xy = true; }

            }
            if (ray.Direction.y != 0f) {
                min_x = ray.Position.x + ((ray.Direction.x / ray.Direction.y) * (BBox.min_y - ray.Position.y));
                max_x = ray.Position.x + ((ray.Direction.x / ray.Direction.y) * (BBox.max_y - ray.Position.y));
                if (min_x >= BBox.min_x && min_x <= BBox.max_x) { xy = true; }
                if (max_x >= BBox.min_x && max_x <= BBox.max_x) { xy = true; }

            }


            if (ray.Direction.z != 0f) {
                min_x = ray.Position.x + ((ray.Direction.x / ray.Direction.z) * (BBox.min_z - ray.Position.z));
                max_x = ray.Position.x + ((ray.Direction.x / ray.Direction.z) * (BBox.max_z - ray.Position.z));
                if (min_x >= BBox.min_x && min_x <= BBox.max_x) { zx = true; }
                if (max_x >= BBox.min_x && max_x <= BBox.max_x) { zx = true; }

            }
            if (ray.Direction.x != 0f) {
                min_z = ray.Position.z + ((ray.Direction.z / ray.Direction.x) * (BBox.min_x - ray.Position.x));
                max_z = ray.Position.z + ((ray.Direction.z / ray.Direction.x) * (BBox.max_x - ray.Position.x));
                if (min_z >= BBox.min_z && min_z <= BBox.max_z) { zx = true; }
                if (max_z >= BBox.min_z && max_z <= BBox.max_z) { zx = true; }

            }


            if (ray.Direction.y != 0f) {
                min_z = ray.Position.z + ((ray.Direction.z / ray.Direction.y) * (BBox.min_y - ray.Position.y));
                max_z = ray.Position.z + ((ray.Direction.z / ray.Direction.y) * (BBox.max_y - ray.Position.y));
                if (min_z >= BBox.min_z && min_z <= BBox.max_z) { yz = true; }
                if (max_z >= BBox.min_z && max_z <= BBox.max_z) { yz = true; }

            }


            if (ray.Direction.z != 0f) {
                min_y = ray.Position.y + ((ray.Direction.y / ray.Direction.z) * (BBox.min_z - ray.Position.z));
                max_y = ray.Position.y + ((ray.Direction.y / ray.Direction.z) * (BBox.max_z - ray.Position.z));
                if (min_y >= BBox.min_y && min_y <= BBox.max_y) { yz = true; }
                if (max_y >= BBox.min_y && max_y <= BBox.max_y) { yz = true; }

            }

            if (ray.Direction.x == 0f && ray.Direction.y == 0f) {
                if (ray.Position.x >= BBox.min_x && ray.Position.x <= BBox.max_x && ray.Position.y >= BBox.min_y && ray.Position.y <= BBox.max_y) { xy = true; }
            }
            if (ray.Direction.z == 0f && ray.Direction.x == 0f) {
                if (ray.Position.z >= BBox.min_z && ray.Position.z <= BBox.max_z && ray.Position.x >= BBox.min_x && ray.Position.x <= BBox.max_x) { zx = true; }
            }
            if (ray.Direction.y == 0f && ray.Direction.z == 0f) {
                if (ray.Position.y >= BBox.min_y && ray.Position.y <= BBox.max_y && ray.Position.z >= BBox.min_z && ray.Position.z <= BBox.max_z) { yz = true; }
            }

            if (xy == true && zx == true && yz == true) {
                return true;
            } else {
                return false;
            }
        }

    }
    #endregion

    #region Octree
    class Octree {
        int Level;
        public BoundingBox BBox;

        public Octree[] child = new Octree[8];

        public int[] index;

        public Octree (List<Vector3> vertex, List<int[]> index) {
            this.Level = 0;
            this.BBox = new BoundingBox(vertex);
            BoundingBox[] SplitBBox = BoundingBox.Split(BBox);
            int[] indexnum = new int[index.Count];
            foreach (int a in Enumerable.Range(0, index.Count)) {
                indexnum[a] = a;
            }
            if (indexnum.Length > 8) {
                for (int i = 0; i < 8; i++) {
                    List<int> _numlist = new List<int>();
                    foreach (int j in indexnum) {
                        if (BoundingBox.AABBTriangle(SplitBBox[i], new List<Vector3> { vertex[index[j][0]], vertex[index[j][1]], vertex[index[j][2]] })) {
                            _numlist.Add(j);
                        }
                    }
                    if (_numlist.Count > 0) {
                        int[] num = new int[_numlist.Count];
                        _numlist.CopyTo(num);
                        this.child[i] = new Octree(SplitBBox[i], this.Level, vertex, index, num);
                    }
                }
            } else {
                this.index = indexnum;
            }
        }

        public Octree (BoundingBox BBox, int Level, List<Vector3> vertex, List<int[]> index, int[] indexnum) {
            this.Level = Level + 1;
            this.BBox = BBox;
            BoundingBox[] SplitBBox = BoundingBox.Split(BBox);


            if (this.Level < 8 && indexnum.Length > 8) {
                for (int i = 0; i < 8; i++) {
                    List<int> _numlist = new List<int>();
                    foreach (int j in indexnum) {
                        if (BoundingBox.AABBTriangle(SplitBBox[i], new List<Vector3> { vertex[index[j][0]], vertex[index[j][1]], vertex[index[j][2]] })) {
                            _numlist.Add(j);
                        }
                    }
                    if (_numlist.Count > 0) {
                        int[] num = new int[_numlist.Count];
                        _numlist.CopyTo(num);
                        this.child[i] = new Octree(SplitBBox[i], this.Level, vertex, index, num);
                    }
                }
            } else {
                this.index = indexnum;
            }
        }

        public BoundingBox[] BoxList () {

            List<BoundingBox[]> lists = new List<BoundingBox[]>();
            List<BoundingBox> list = new List<BoundingBox>();
            for (int i = 0; i < 8; i++) {
                if (this.child[i] != null) {
                    lists.Add(this.child[i].BoxList());
                }
            }

            foreach (BoundingBox[] a in lists) {
                foreach (BoundingBox b in a) {
                    if (a != null) {
                        list.Add(b);
                    }
                }
            }
            if (this.BBox != null) {
                list.Add(this.BBox);
            }

            BoundingBox[] re = new BoundingBox[list.Count];
            for (int i = 0; i < list.Count; i++) {
                re[i] = list[i];
            }
            return re;
        }

        public int[] OctreeRayHitindex (Ray ray) {
            List<int[]> lists = new List<int[]>();
            List<int> list = new List<int>();

            for (int i = 0; i < 8; i++) {
                if (this.child[i] != null) {
                    if (BoundingBox.RayAABB(ray, this.child[i].BBox)) {
                        lists.Add(this.child[i].OctreeRayHitindex(ray));
                    }
                }
            }

            if (lists.Count > 0) {
                foreach (int[] a in lists) {
                    foreach (int b in a) {
                        if (!list.Contains(b)) {
                            list.Add(b);
                        }
                    }
                }

            }

            if (this.index != null) {
                foreach (int a in this.index)
                    list.Add(a);
            }

            int[] re = new int[list.Count];
            for (int i = 0; i < list.Count; i++) {
                re[i] = list[i];
            }
            return re;
        }
    }
    #endregion

    #region Wireframe
    class Wireframe {
        public static void LineDraw (Bitmap bm, List<Object3d> objlist, Matrix projscreen) {
            Graphics g = Graphics.FromImage(bm);
            Pen pen = new Pen(Color.Blue, 1f);
            Pen penr = new Pen(Color.Red, 1f);
            Pen peng = new Pen(Color.Green, 1f);
            foreach (Object3d obj in objlist) {
                for (int i = 0; i < obj.index.Count; ++i) {
                    Vector3 v1 = Matrix.matmul14proj(obj.vertex[obj.index[i][0]], projscreen);
                    Vector3 v2 = Matrix.matmul14proj(obj.vertex[obj.index[i][1]], projscreen);
                    Vector3 v3 = Matrix.matmul14proj(obj.vertex[obj.index[i][2]], projscreen);
                    if (v1.z < 1 && v1.z > 0 || v2.z < 1 && v2.z > 0 || v3.z < 1 && v3.z > 0) {
                        g.DrawLine(pen, new Point((int)v1.x, (int)v1.y), new Point((int)v2.x, (int)v2.y));
                        g.DrawLine(pen, new Point((int)v1.x, (int)v1.y), new Point((int)v3.x, (int)v3.y));
                        g.DrawLine(pen, new Point((int)v2.x, (int)v2.y), new Point((int)v3.x, (int)v3.y));
                    }
                }


            }
            /*
            foreach (Object3d obj in objlist)
            {
                if (obj.octree != null)
                {
                    Vector3[] bv = new Vector3[8];
                    int[][] ind = new int[][] { new int[] { 0, 1 },
                                            new int[] { 2, 3 }, 
                                            new int[] { 0, 2 }, 
                                            new int[] { 1, 3 },
                                            new int[] { 4, 5 }, 
                                            new int[] { 6, 7 },
                                            new int[] { 4, 6 }, 
                                            new int[] { 5, 7 },
                                            new int[] { 0, 4 }, 
                                            new int[] { 1, 5 },
                                            new int[] { 2, 6 }, 
                                            new int[] { 3, 7 } };

                    foreach (BoundingBox a in obj.octree.BoxList())
                    {
                        for (int i = 0; i < 8; ++i)
                        {
                            bv[i] = Matrix.matmul14proj(a.vertex[i], projscreen);
                        }
                        foreach (int[] i in ind)
                        {
                            if (bv[i[0]].z < 0 && bv[i[0]].z > -1 || bv[i[1]].z < 0 && bv[i[1]].z > -1)
                            {
                                g.DrawLine(peng, new Point((int)bv[i[0]].x, (int)bv[i[0]].y), new Point((int)bv[i[1]].x, (int)bv[i[1]].y));
                            }
                        }
                    }
                }
            }
            */
            g.Dispose();

        }


    }
    #endregion

    #region Light
    class Light {
        public Vector3 position = new Vector3(0f, 10f, -6f);
    }
    #endregion
}

