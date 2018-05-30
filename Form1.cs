using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using renderer;

namespace WindowsFormsApp1 {
    public partial class Form1 : Form {

        List<Object3d> objlist = new List<Object3d>();

        Bitmap bitmap;
        Light[] light = new Light[2];
        Matrix move;
        Matrix rot;
        Matrix projscreen;
        int renderline = 0;
        int x = 0;
        int z = 0;
        double rot_y = 0;
        int mode = 0;
        int window_x = 0;
        int window_y = 0;
        static float scale = 1f;
        Matrix pos = new Matrix(scale, 0f, 0f, 0f,
                                0f, scale, 0f, 0f,
                                0f, 0f, scale, 0f,
                                0f, -2f, -16f, 1f);
        Stopwatch st = new Stopwatch();

        public Form1 () {
            InitializeComponent();
        }

        private void pictureBox1_Click (object sender, EventArgs e) {

            this.bitmap = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
            pictureBox1.Image = this.bitmap;

            Matrix proj = Matrix.PerspectiveFovRH(pictureBox1.Size.Width, pictureBox1.Size.Height, 45f);
            Matrix screen = Matrix.ViewPort(pictureBox1.Size.Width, pictureBox1.Size.Height);
            this.projscreen = Matrix.matmul44(proj, screen);

            this.window_x = pictureBox1.Size.Width;
            this.window_y = pictureBox1.Size.Height;

            this.renderline = 0;
            this.mode = 1;

            this.st.Reset();
            this.st.Start();
            for (int i = 0; i < this.objlist.Count; i++) {
                this.objlist[i].SetRenderSetting();
            }
            this.st.Stop();
            Console.WriteLine("octree time " + this.st.ElapsedMilliseconds);

            this.st.Reset();
            this.st.Start();
            timer1.Start();

        }

        private void timer1_Tick (object sender, EventArgs e) {
            timer1.Stop();
            if (this.mode == 1) {
                var syncObject = new object();
                int thre = 4;
                for (; this.renderline < this.window_y;) {
                    Parallel.For(0, thre, id => {
                        foreach (int x in Enumerable.Range(0, this.window_x)) {
                            if (this.renderline + id < this.window_y) {

                                Vector3 rp1 = new Vector3((float)x, (float)this.renderline + id, 0f);
                                Vector3 rp2 = new Vector3((float)x, (float)this.renderline + id, 1f);
                                rp1 = Matrix.matmul14proj(rp1, Matrix.inversematrix44(projscreen));
                                rp2 = Matrix.matmul14proj(rp2, Matrix.inversematrix44(projscreen));
                                Ray ray1 = new Ray();
                                ray1.Position = rp1;
                                ray1.Direction = Vector3.Normalize(rp2 - rp1);
                                int sam = 4;
                                int[] color = { 0, 0, 0 };

                                color = Ray.RayTracing(objlist, ray1, sam, this.light, syncObject);
                                Monitor.Enter(syncObject);
                                try {

                                    this.bitmap.SetPixel(x, this.renderline + id, Color.FromArgb(color[0], color[1], color[2]));
                                } finally {
                                    Monitor.Exit(syncObject);
                                }
                            }
                        }
                    });
                    pictureBox1.Image = this.bitmap;

                    this.renderline += thre;
                }
                if (this.renderline < this.window_y) {
                    timer1.Start();
                } else {
                    this.st.Stop();
                    Console.WriteLine("render time " + this.st.ElapsedMilliseconds);

                }
            }
            if (this.mode == 2) {

                Matrix proj = Matrix.PerspectiveFovRH(pictureBox1.Size.Width, pictureBox1.Size.Height, 45f);
                Matrix screen = Matrix.ViewPort(pictureBox1.Size.Width, pictureBox1.Size.Height);
                this.projscreen = Matrix.matmul44(proj, screen);

                this.objlist[0].Transform(Matrix.matmul44(Matrix.matmul44(this.rot, this.pos), this.move));


                this.bitmap.Dispose();
                this.bitmap = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
                Wireframe.LineDraw(this.bitmap, objlist, this.projscreen);
                pictureBox1.Image = this.bitmap;
                timer1.Stop();
            }
        }

        private void Form1_Load (object sender, EventArgs e) {
            Object3d[] obs = Import.objfile("./", "zou22RUV2483.obj");
            this.objlist.Add(obs[0]);

            this.light[0] = new Light();
            this.light[1] = new Light();
            this.light[1].position.x = 2f;
            this.light[1].position.y = 0f;
            this.light[1].position.z = -10f;

            this.objlist[0].Transform(this.pos);

            Matrix proj = Matrix.PerspectiveFovRH(pictureBox1.Size.Width, pictureBox1.Size.Height, 45f);
            Matrix screen = Matrix.ViewPort(pictureBox1.Size.Width, pictureBox1.Size.Height);
            this.projscreen = Matrix.matmul44(proj, screen);

            this.bitmap = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
            Wireframe.LineDraw(this.bitmap, objlist, this.projscreen);
            pictureBox1.Image = this.bitmap;
        }

        private void Form1_KeyDown (object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.W) {
                this.z -= 1;
                this.mode = 2;
            }
            if (e.KeyCode == Keys.S) {
                this.z += 1;
                this.mode = 2;
            }
            if (e.KeyCode == Keys.A) {
                this.x -= 1;
                this.mode = 2;
            }
            if (e.KeyCode == Keys.D) {
                this.x += 1;
                this.mode = 2;

            }
            if (e.KeyCode == Keys.Q) {
                this.rot_y -= 1.0;
                this.mode = 2;

            }
            if (e.KeyCode == Keys.E) {
                this.rot_y += 1.0;
                this.mode = 2;

            }

            this.move = new Matrix(1f, 0f, 0f, 0f,
                                   0f, 1f, 0f, 0f,
                                   0f, 0f, 1f, 0f,
                                   0.2f * this.x, 0f, 0.2f * this.z, 1f);

            this.rot = new Matrix((float)Math.Cos(this.rot_y / 180 * Math.PI), 0f, (float)Math.Sin(this.rot_y / 180 * Math.PI), 0f,
                                  0f, 1f, 0f, 0f,
                                  -(float)Math.Sin(this.rot_y / 180 * Math.PI), 0f, (float)Math.Cos(this.rot_y / 180 * Math.PI), 0f,
                                  0f, 0f, 0f, 1f);

            timer1.Start();
        }
    }
}
