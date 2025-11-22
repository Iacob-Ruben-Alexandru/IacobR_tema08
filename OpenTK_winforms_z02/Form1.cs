using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Linq;

namespace OpenTK_winforms_z02
{
    public partial class Form1 : Form
    {

        private int eyePosX, eyePosY, eyePosZ;


        private Point mousePos;
        private float camDepth;

        private bool statusControlMouse2D, statusControlMouse3D, statusMouseDown;

        private bool statusControlAxe;

        private bool lightON;
        private bool lightON_0;
        private bool lightON_1;

        private string statusCube;

        private int[,] arrVertex = new int[50, 3];
        private int nVertex;

        private int[] arrQuadsList = new int[100];
        private int nQuadsList;

        private int[] arrTrianglesList = new int[100];
        private int nTrianglesList;

        private string fileVertex = "vertexList.txt";
        private string fileQList = "quadsVertexList.txt";
        private string fileTList = "trianglesVertexList.txt";
        private bool statusFiles;

        private float[] valuesAmbientTemplate0 = new float[] { 0.2f, 0.4f, 0.6f, 1.0f };
        private float[] valuesDiffuseTemplate0 = new float[] { 0.8f, 0.9f, 0.5f, 1.0f };
        private float[] valuesSpecularTemplate0 = new float[] { 1.0f, 0.8f, 1.0f, 1.0f };
        private float[] valuesPositionTemplate0 = new float[] { 0.0f, 0.0f, 5.0f, 1.0f };

        private float[] valuesAmbientTemplate1 = new float[] { 0.2f, 0.2f, 0.5f, 1.0f };
        private float[] valuesDiffuseTemplate1 = new float[] { 0.5f, 0.5f, 1.0f, 1.0f };
        private float[] valuesSpecularTemplate1 = new float[] { 0.9f, 0.9f, 0.9f, 1.0f };
        private float[] valuesPositionTemplate1 = new float[] { 0.0f, 20.0f, 0.0f, 1.0f };

        private float[] valuesAmbient1 = new float[4];
        private float[] valuesDiffuse1 = new float[4];
        private float[] valuesSpecular1 = new float[4];
        private float[] valuesPosition1 = new float[4];

        private float[] valuesAmbient0 = new float[4];
        private float[] valuesDiffuse0 = new float[4];
        private float[] valuesSpecular0 = new float[4];
        private float[] valuesPosition0 = new float[4];

        private LightSource light0;
        private LightSource light1;

        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   ON_LOAD
        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(this.Form1_KeyDown);
            InitializeDummyComponents();
        }

        private void InitializeDummyComponents()
        {
            if (trackLight0PositionX == null) trackLight0PositionX = new TrackBar() { Minimum = -50, Maximum = 50, Value = 0 };
            if (trackLight0PositionY == null) trackLight0PositionY = new TrackBar() { Minimum = -50, Maximum = 50, Value = 0 };
            if (trackLight0PositionZ == null) trackLight0PositionZ = new TrackBar() { Minimum = -50, Maximum = 50, Value = 5 };
            if (trackLight1PositionX == null) trackLight1PositionX = new TrackBar() { Minimum = -50, Maximum = 50, Value = 0 };
            if (trackLight1PositionY == null) trackLight1PositionY = new TrackBar() { Minimum = -50, Maximum = 50, Value = 20 };
            if (trackLight1PositionZ == null) trackLight1PositionZ = new TrackBar() { Minimum = -50, Maximum = 50, Value = 0 };
        }

        private void btnMouseControl2D_Click(object sender, EventArgs e)
        {
            if (statusControlMouse2D == true)
            {
                setControlMouse2D(false);
            }
            else
            {
                setControlMouse3D(false);
                setControlMouse2D(true);
            }
        }
        public class LightSource
        {
            public OpenTK.Graphics.OpenGL.LightName Name { get; private set; }
            public float[] Ambient { get; set; }
            public float[] Diffuse { get; set; }
            public float[] Specular { get; set; }
            public float[] Position { get; set; }

            public bool IsOn { get; set; }
            private float speed = 1.0f;

            public LightSource(OpenTK.Graphics.OpenGL.LightName name, float[] ambient, float[] diffuse, float[] specular, float[] position)
            {
                Name = name;
                Ambient = (float[])ambient.Clone();
                Diffuse = (float[])diffuse.Clone();
                Specular = (float[])specular.Clone();
                Position = (float[])position.Clone();
                IsOn = false;
            }



            public void Move(char axis, int direction)
            {
                int index;
                if (axis == 'X') index = 0;
                else if (axis == 'Y') index = 1;
                else index = 2;

                Position[index] += direction * speed;
            }

            public void MoveMouse(float deltaX, float deltaY)
            {
                Position[0] += deltaX * 0.5f;
                Position[1] -= deltaY * 0.5f;
            }
        }
        private void btnMouseControl3D_Click(object sender, EventArgs e)
        {
            if (statusControlMouse3D == true)
            {
                setControlMouse3D(false);
            }
            else
            {
                setControlMouse2D(false);
                setControlMouse3D(true);
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            SetupValues();
            SetupWindowGUI();
        }
        public void setControlMouse2D(bool choose)
        {
            if (choose == false) { statusControlMouse2D = false; if (btnMouseControl2D != null) btnMouseControl2D.Text = "Mouse 2D OFF"; }
            else { statusControlMouse2D = true; if (btnMouseControl2D != null) btnMouseControl2D.Text = "Mouse 2D ON"; }
        }
        public void setControlMouse3D(bool choose)
        {
            if (choose == false) { statusControlMouse3D = false; if (btnMouseControl3D != null) btnMouseControl3D.Text = "Mouse 3D OFF"; }
            else { statusControlMouse3D = true; if (btnMouseControl3D != null) btnMouseControl3D.Text = "Mouse 3D ON"; }
        }

        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   SETARI INIȚIALE
        private void SetupValues()
        {
            eyePosX = 100;
            eyePosY = 100;
            eyePosZ = 50;
            camDepth = 1.04f;

            setLight0Values();
            setLight1Values();

            light0 = new LightSource(
                LightName.Light0, valuesAmbient0, valuesDiffuse0, valuesSpecular0, valuesPosition0
            );
            light1 = new LightSource(
                LightName.Light1, valuesAmbient1, valuesDiffuse1, valuesSpecular1, valuesPosition1
            );

            light0.IsOn = lightON_0;
            light1.IsOn = lightON_1;

            if (numericXeye != null) numericXeye.Value = eyePosX;
            if (numericYeye != null) numericYeye.Value = eyePosY;
            if (numericZeye != null) numericZeye.Value = eyePosZ;
        }


        private void SetupWindowGUI()
        {
            setControlMouse2D(false);
            setControlMouse3D(false);

            if (numericCameraDepth != null) numericCameraDepth.Value = (int)camDepth;

            setControlAxe(true);

            setCubeStatus("OFF");
            setIlluminationStatus(false);
            setSource0Status(false);
            setSource1Status(false);

            setTrackLigh0Default();
            setColorAmbientLigh0Default();
            setColorDifuseLigh0Default();
            setColorSpecularLigh0Default();

            setTrackLigh1Default();
            setColorAmbientLigh1Default();
            setColorDifuseLigh1Default();
            setColorSpecularLigh1Default();
        }

        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   MANIPULARE VERTEXURI ȘI LISTE DE COORDONATE.
        //Încărcarea coordonatelor vertexurilor și lista de compunere a obiectelor 3D.
        private void loadVertex()
        {
            if (nVertex == 0)
            {
                arrVertex[0, 0] = -10; arrVertex[0, 1] = -10; arrVertex[0, 2] = -10;
                arrVertex[1, 0] = 10; arrVertex[1, 1] = -10; arrVertex[1, 2] = -10;
                arrVertex[2, 0] = 10; arrVertex[2, 1] = 10; arrVertex[2, 2] = -10;
                arrVertex[3, 0] = -10; arrVertex[3, 1] = 10; arrVertex[3, 2] = -10;
                arrVertex[4, 0] = -10; arrVertex[4, 1] = -10; arrVertex[4, 2] = 10;
                arrVertex[5, 0] = 10; arrVertex[5, 1] = -10; arrVertex[5, 2] = 10;
                arrVertex[6, 0] = 10; arrVertex[6, 1] = 10; arrVertex[6, 2] = 10;
                arrVertex[7, 0] = -10; arrVertex[7, 1] = 10; arrVertex[7, 2] = 10;
                nVertex = 8;
            }
        }
        private void loadQList()
        {
            arrQuadsList = new int[] { 0, 3, 2, 1, 4, 5, 6, 7, 0, 1, 5, 4, 2, 3, 7, 6, 1, 2, 6, 5, 0, 4, 7, 3 };
            nQuadsList = arrQuadsList.Length;
        }
        private void loadTList()
        {
            arrTrianglesList = new int[] { 0, 3, 2, 0, 2, 1, 4, 5, 6, 4, 6, 7, 0, 1, 5, 0, 5, 4, 2, 3, 7, 2, 7, 6, 1, 2, 6, 1, 6, 5, 0, 4, 7, 0, 7, 3 };
            nTrianglesList = arrTrianglesList.Length;
        }

        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   CONTROL CAMERĂ
        private void numericXeye_ValueChanged(object sender, EventArgs e) { eyePosX = (int)numericXeye.Value; GlControl1.Invalidate(); }
        private void numericYeye_ValueChanged(object sender, EventArgs e) { eyePosY = (int)numericYeye.Value; GlControl1.Invalidate(); }
        private void numericZeye_ValueChanged(object sender, EventArgs e) { eyePosZ = (int)numericZeye.Value; GlControl1.Invalidate(); }
        private void numericCameraDepth_ValueChanged(object sender, EventArgs e) { camDepth = 1 + ((float)numericCameraDepth.Value) * 0.1f; GlControl1.Invalidate(); }

        private void GlControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (statusMouseDown)
            {
                Point currentPos = new Point(e.X, e.Y);

                if (lightON && light1.IsOn && e.Button == MouseButtons.Right)
                {
                    float deltaX = currentPos.X - mousePos.X;
                    float deltaY = currentPos.Y - mousePos.Y;

                    light1.MoveMouse(deltaX, deltaY);
                    setTrackLigh1Default();

                    Array.Copy(light1.Position, valuesPosition1, 4);
                }

                mousePos = currentPos;
                GlControl1.Invalidate();
            }
        }


        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   CONTROL MOUSE
        //Setăm variabila de stare pentru rotația în 2D a mouseului.
        private void GlControl1_MouseDown(object sender, MouseEventArgs e) { statusMouseDown = true; }
        private void GlControl1_MouseUp(object sender, MouseEventArgs e) { statusMouseDown = false; }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (lightON && light1.IsOn)
            {
                bool moved = true;

                switch (e.KeyCode)
                {
                    case Keys.Q: light1.Move('X', 1); break;
                    case Keys.A: light1.Move('X', -1); break;
                    case Keys.W: light1.Move('Y', 1); break;
                    case Keys.S: light1.Move('Y', -1); break;
                    case Keys.E: light1.Move('Z', 1); break;
                    case Keys.D: light1.Move('Z', -1); break;
                    default: moved = false; break;
                }

                if (moved)
                {
                    setTrackLigh1Default();
                    Array.Copy(light1.Position, valuesPosition1, 4);
                    GlControl1.Invalidate();
                }
            }
        }

        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   CONTROL ILUMINARE
        //Setăm variabila de stare pentru iluminare.
        private void setIlluminationStatus(bool status)
        {
            lightON = status;
            if (btnLights != null) btnLights.Text = lightON ? "Iluminare ON" : "Iluminare OFF";
        }

        private void btnLights_Click(object sender, EventArgs e)
        {
            setIlluminationStatus(!lightON);
            GlControl1.Invalidate();
        }

        private void btnLightsNo_Click(object sender, EventArgs e)
        {
            int nr = GL.GetInteger(GetPName.MaxLights);
            MessageBox.Show("Nr. maxim de luminii pentru aceasta implementare este <" + nr.ToString() + ">.");
        }

        private void setSource0Status(bool status)
        {
            lightON_0 = status;
            light0.IsOn = status;
            if (btnLight0 != null) btnLight0.Text = light0.IsOn ? "Sursa 0 ON" : "Sursa 0 OFF";
        }
        private void btnLight0_Click(object sender, EventArgs e)
        {
            if (lightON) { setSource0Status(!light0.IsOn); GlControl1.Invalidate(); }
        }
        private void setTrackLigh0Default()
        {
            if (trackLight0PositionX != null) trackLight0PositionX.Value = (int)light0.Position[0];
            if (trackLight0PositionY != null) trackLight0PositionY.Value = (int)light0.Position[1];
            if (trackLight0PositionZ != null) trackLight0PositionZ.Value = (int)light0.Position[2];
        }
        private void trackLight0PositionX_Scroll(object sender, EventArgs e) { light0.Position[0] = trackLight0PositionX.Value; Array.Copy(light0.Position, valuesPosition0, 4); GlControl1.Invalidate(); }
        private void trackLight0PositionY_Scroll(object sender, EventArgs e) { light0.Position[1] = trackLight0PositionY.Value; Array.Copy(light0.Position, valuesPosition0, 4); GlControl1.Invalidate(); }
        private void trackLight0PositionZ_Scroll(object sender, EventArgs e) { light0.Position[2] = trackLight0PositionZ.Value; Array.Copy(light0.Position, valuesPosition0, 4); GlControl1.Invalidate(); }
        private void setColorAmbientLigh0Default()
        {
            if (numericLight0Ambient_Red != null) numericLight0Ambient_Red.Value = (decimal)light0.Ambient[0] * 100;
            if (numericLight0Ambient_Green != null) numericLight0Ambient_Green.Value = (decimal)light0.Ambient[1] * 100;
            if (numericLight0Ambient_Blue != null) numericLight0Ambient_Blue.Value = (decimal)light0.Ambient[2] * 100;
        }
        private void numericLight0Ambient_Red_ValueChanged(object sender, EventArgs e) { light0.Ambient[0] = (float)numericLight0Ambient_Red.Value / 100; Array.Copy(light0.Ambient, valuesAmbient0, 4); GlControl1.Invalidate(); }
        private void numericLight0Ambient_Green_ValueChanged(object sender, EventArgs e) { light0.Ambient[1] = (float)numericLight0Ambient_Green.Value / 100; Array.Copy(light0.Ambient, valuesAmbient0, 4); GlControl1.Invalidate(); }
        private void numericLight0Ambient_Blue_ValueChanged(object sender, EventArgs e) { light0.Ambient[2] = (float)numericLight0Ambient_Blue.Value / 100; Array.Copy(light0.Ambient, valuesAmbient0, 4); GlControl1.Invalidate(); }


        private void setColorDifuseLigh0Default()
        {
            if (numericLight0Difuse_Red != null) numericLight0Difuse_Red.Value = (decimal)light0.Diffuse[0] * 100;
            if (numericLight0Difuse_Green != null) numericLight0Difuse_Green.Value = (decimal)light0.Diffuse[1] * 100;
            if (numericLight0Difuse_Blue != null) numericLight0Difuse_Blue.Value = (decimal)light0.Diffuse[2] * 100;
        }
        private void numericLight0Difuse_Red_ValueChanged(object sender, EventArgs e) { light0.Diffuse[0] = (float)numericLight0Difuse_Red.Value / 100; Array.Copy(light0.Diffuse, valuesDiffuse0, 4); GlControl1.Invalidate(); }
        private void numericLight0Difuse_Green_ValueChanged(object sender, EventArgs e) { light0.Diffuse[1] = (float)numericLight0Difuse_Green.Value / 100; Array.Copy(light0.Diffuse, valuesDiffuse0, 4); GlControl1.Invalidate(); }
        private void numericLight0Difuse_Blue_ValueChanged(object sender, EventArgs e) { light0.Diffuse[2] = (float)numericLight0Difuse_Blue.Value / 100; Array.Copy(light0.Diffuse, valuesDiffuse0, 4); GlControl1.Invalidate(); }
        private void setColorSpecularLigh0Default()
        {
            if (numericLight0Specular_Red != null) numericLight0Specular_Red.Value = (decimal)light0.Specular[0] * 100;
            if (numericLight0Specular_Green != null) numericLight0Specular_Green.Value = (decimal)light0.Specular[1] * 100;
            if (numericLight0Specular_Blue != null) numericLight0Specular_Blue.Value = (decimal)light0.Specular[2] * 100;
        }
        private void numericLight0Specular_Red_ValueChanged(object sender, EventArgs e) { light0.Specular[0] = (float)numericLight0Specular_Red.Value / 100; Array.Copy(light0.Specular, valuesSpecular0, 4); GlControl1.Invalidate(); }
        private void numericLight0Specular_Green_ValueChanged(object sender, EventArgs e) { light0.Specular[1] = (float)numericLight0Specular_Green.Value / 100; Array.Copy(light0.Specular, valuesSpecular0, 4); GlControl1.Invalidate(); }
        private void numericLight0Specular_Blue_ValueChanged(object sender, EventArgs e) { light0.Specular[2] = (float)numericLight0Specular_Blue.Value / 100; Array.Copy(light0.Specular, valuesSpecular0, 4); GlControl1.Invalidate(); }
        private void setLight0Values()
        {
            Array.Copy(valuesAmbientTemplate0, valuesAmbient0, 4);
            Array.Copy(valuesDiffuseTemplate0, valuesDiffuse0, 4);
            Array.Copy(valuesSpecularTemplate0, valuesSpecular0, 4);
            Array.Copy(valuesPositionTemplate0, valuesPosition0, 4);
            light0 = new LightSource(LightName.Light0, valuesAmbient0, valuesDiffuse0, valuesSpecular0, valuesPosition0);
        }
        private void btnLight0Reset_Click(object sender, EventArgs e)
        {
            setLight0Values();
            setTrackLigh0Default();
            setColorAmbientLigh0Default();
            setColorDifuseLigh0Default();
            setColorSpecularLigh0Default();
            setSource0Status(lightON_0);
            GlControl1.Invalidate();
        }

        private void setSource1Status(bool status)
        {
            lightON_1 = status;
            light1.IsOn = status;
            if (btnLight1 != null) btnLight1.Text = light1.IsOn ? "Sursa 1 ON" : "Sursa 1 OFF";
        }

        private void btnLight1_Click(object sender, EventArgs e)
        {
            if (lightON) { setSource1Status(!light1.IsOn); GlControl1.Invalidate(); }
        }

        private void setTrackLigh1Default()
        {
            if (light1 == null) return;
            if (trackLight1PositionX != null && trackLight1PositionX.Minimum <= (int)light1.Position[0] && (int)light1.Position[0] <= trackLight1PositionX.Maximum)
                trackLight1PositionX.Value = (int)light1.Position[0];
            if (trackLight1PositionY != null && trackLight1PositionY.Minimum <= (int)light1.Position[1] && (int)light1.Position[1] <= trackLight1PositionY.Maximum)
                trackLight1PositionY.Value = (int)light1.Position[1];
            if (trackLight1PositionZ != null && trackLight1PositionZ.Minimum <= (int)light1.Position[2] && (int)light1.Position[2] <= trackLight1PositionZ.Maximum)
                trackLight1PositionZ.Value = (int)light1.Position[2];
        }

        private void trackLight1PositionX_Scroll(object sender, EventArgs e) { light1.Position[0] = trackLight1PositionX.Value; Array.Copy(light1.Position, valuesPosition1, 4); GlControl1.Invalidate(); }
        private void trackLight1PositionY_Scroll(object sender, EventArgs e) { light1.Position[1] = trackLight1PositionY.Value; Array.Copy(light1.Position, valuesPosition1, 4); GlControl1.Invalidate(); }
        private void trackLight1PositionZ_Scroll(object sender, EventArgs e) { light1.Position[2] = trackLight1PositionZ.Value; Array.Copy(light1.Position, valuesPosition1, 4); GlControl1.Invalidate(); }

        private void setColorAmbientLigh1Default()
        {
            if (numericLight1Ambient_Red != null) numericLight1Ambient_Red.Value = (decimal)light1.Ambient[0] * 100;
            if (numericLight1Ambient_Green != null) numericLight1Ambient_Green.Value = (decimal)light1.Ambient[1] * 100;
            if (numericLight1Ambient_Blue != null) numericLight1Ambient_Blue.Value = (decimal)light1.Ambient[2] * 100;
        }
        private void numericLight1Ambient_Red_ValueChanged(object sender, EventArgs e) { light1.Ambient[0] = (float)numericLight1Ambient_Red.Value / 100; Array.Copy(light1.Ambient, valuesAmbient1, 4); GlControl1.Invalidate(); }
        private void numericLight1Ambient_Green_ValueChanged(object sender, EventArgs e) { light1.Ambient[1] = (float)numericLight1Ambient_Green.Value / 100; Array.Copy(light1.Ambient, valuesAmbient1, 4); GlControl1.Invalidate(); }
        private void numericLight1Ambient_Blue_ValueChanged(object sender, EventArgs e) { light1.Ambient[2] = (float)numericLight1Ambient_Blue.Value / 100; Array.Copy(light1.Ambient, valuesAmbient1, 4); GlControl1.Invalidate(); }

        private void setColorDifuseLigh1Default()
        {
            if (numericLight1Difuse_Red != null) numericLight1Difuse_Red.Value = (decimal)light1.Diffuse[0] * 100;
            if (numericLight1Difuse_Green != null) numericLight1Difuse_Green.Value = (decimal)light1.Diffuse[1] * 100;
            if (numericLight1Difuse_Blue != null) numericLight1Difuse_Blue.Value = (decimal)light1.Diffuse[2] * 100;
        }
        private void numericLight1Difuse_Red_ValueChanged(object sender, EventArgs e) { light1.Diffuse[0] = (float)numericLight1Difuse_Red.Value / 100; Array.Copy(light1.Diffuse, valuesDiffuse1, 4); GlControl1.Invalidate(); }
        private void numericLight1Difuse_Green_ValueChanged(object sender, EventArgs e) { light1.Diffuse[1] = (float)numericLight1Difuse_Green.Value / 100; Array.Copy(light1.Diffuse, valuesDiffuse1, 4); GlControl1.Invalidate(); }
        private void numericLight1Difuse_Blue_ValueChanged(object sender, EventArgs e) { light1.Diffuse[2] = (float)numericLight1Difuse_Blue.Value / 100; Array.Copy(light1.Diffuse, valuesDiffuse1, 4); GlControl1.Invalidate(); }

        private void setColorSpecularLigh1Default()
        {
            if (numericLight1Specular_Red != null) numericLight1Specular_Red.Value = (decimal)light1.Specular[0] * 100;
            if (numericLight1Specular_Green != null) numericLight1Specular_Green.Value = (decimal)light1.Specular[1] * 100;
            if (numericLight1Specular_Blue != null) numericLight1Specular_Blue.Value = (decimal)light1.Specular[2] * 100;
        }
        private void numericLight1Specular_Red_ValueChanged(object sender, EventArgs e) { light1.Specular[0] = (float)numericLight1Specular_Red.Value / 100; Array.Copy(light1.Specular, valuesSpecular1, 4); GlControl1.Invalidate(); }
        private void numericLight1Specular_Green_ValueChanged(object sender, EventArgs e) { light1.Specular[1] = (float)numericLight1Specular_Green.Value / 100; Array.Copy(light1.Specular, valuesSpecular1, 4); GlControl1.Invalidate(); }
        private void numericLight1Specular_Blue_ValueChanged(object sender, EventArgs e) { light1.Specular[2] = (float)numericLight1Specular_Blue.Value / 100; Array.Copy(light1.Specular, valuesSpecular1, 4); GlControl1.Invalidate(); }

        private void setLight1Values()
        {
            Array.Copy(valuesAmbientTemplate1, valuesAmbient1, 4);
            Array.Copy(valuesDiffuseTemplate1, valuesDiffuse1, 4);
            Array.Copy(valuesSpecularTemplate1, valuesSpecular1, 4);
            Array.Copy(valuesPositionTemplate1, valuesPosition1, 4);
            light1 = new LightSource(LightName.Light1, valuesAmbient1, valuesDiffuse1, valuesSpecular1, valuesPosition1);
        }
        private void btnLight1Reset_Click(object sender, EventArgs e)
        {
            setLight1Values();
            setTrackLigh1Default();
            setColorAmbientLigh1Default();
            setColorDifuseLigh1Default();
            setColorSpecularLigh1Default();
            setSource1Status(lightON_1);
            GlControl1.Invalidate();
        }
        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   CONTROL OBIECTE 3D
        //Setăm variabila de stare pentru afișarea/scunderea sistemului de coordonate.

        private void setControlAxe(bool status)
        {
            if (status == false) { statusControlAxe = false; if (btnShowAxes != null) btnShowAxes.Text = "Axe Oxyz OFF"; }
            else { statusControlAxe = true; if (btnShowAxes != null) btnShowAxes.Text = "Axe Oxyz ON"; }
        }
        private void btnShowAxes_Click(object sender, EventArgs e)
        {
            if (statusControlAxe == true) { setControlAxe(false); }
            else { setControlAxe(true); }
            GlControl1.Invalidate();
        }

        //Setăm variabila de stare pentru desenarea cubului. Valorile acceptabile sunt:
        //TRIANGLES = cubul este desenat, prin triunghiuri.
        //QUADS = cubul este desenat, prin quaduri.
        //OFF (sau orice altceva) = cubul nu este desenat.
        private void setCubeStatus(string status)
        {
            if (status.Trim().ToUpper().Equals("TRIANGLES")) { statusCube = "TRIANGLES"; }
            else if (status.Trim().ToUpper().Equals("QUADS")) { statusCube = "QUADS"; }
            else { statusCube = "OFF"; }
        }
        private void btnCubeQ_Click(object sender, EventArgs e)
        {
            statusFiles = true;
            loadVertex();
            loadQList();
            setCubeStatus("QUADS");
            GlControl1.Invalidate();
        }
        private void btnCubeT_Click(object sender, EventArgs e)
        {
            statusFiles = true;
            loadVertex();
            loadTList();
            setCubeStatus("TRIANGLES");
            GlControl1.Invalidate();
        }
        private void btnResetObjects_Click(object sender, EventArgs e)
        {
            setCubeStatus("OFF");
            GlControl1.Invalidate();
        }

        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   ADMINISTRARE MOD 3D (METODA PRINCIPALĂ)
        private void GlControl1_Paint(object sender, EventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            GL.ClearColor(Color.Black);

            // Setări Preliminare
            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(camDepth, 4f / 3f, 1, 10000);
            Matrix4 lookat = Matrix4.LookAt(eyePosX, eyePosY, eyePosZ, 0, 0, 0, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.LoadMatrix(ref perspective);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.LoadMatrix(ref lookat);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);


            GL.Light(LightName.Light0, LightParameter.Ambient, valuesAmbient0);
            GL.Light(LightName.Light0, LightParameter.Diffuse, valuesDiffuse0);
            GL.Light(LightName.Light0, LightParameter.Specular, valuesSpecular0);
            GL.Light(LightName.Light0, LightParameter.Position, valuesPosition0);

            GL.Light(LightName.Light1, LightParameter.Ambient, valuesAmbient1);
            GL.Light(LightName.Light1, LightParameter.Diffuse, valuesDiffuse1);
            GL.Light(LightName.Light1, LightParameter.Specular, valuesSpecular1);
            GL.Light(LightName.Light1, LightParameter.Position, valuesPosition1);

            if (lightON)
            {
                GL.Enable(EnableCap.Lighting);
                if (lightON_0) GL.Enable(EnableCap.Light0); else GL.Disable(EnableCap.Light0);
                if (lightON_1) GL.Enable(EnableCap.Light1); else GL.Disable(EnableCap.Light1);
            }
            else
            {
                GL.Disable(EnableCap.Lighting);
                GL.Disable(EnableCap.Light0);
                GL.Disable(EnableCap.Light1);
            }
            if (statusControlMouse2D == true) { GL.Rotate(mousePos.X, 0, 1, 0); }
            if (statusControlMouse3D == true) { GL.Rotate(mousePos.X, 0, 1, 1); }

            if (statusControlAxe == true) { DeseneazaAxe(); }

            if (statusCube.ToUpper().Equals("QUADS")) { DeseneazaCubQ(); }
            else if (statusCube.ToUpper().Equals("TRIANGLES")) { DeseneazaCubT(); }

            GlControl1.SwapBuffers();
        }

        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //   DESENARE OBIECTE 3D
        //Desenează axe XYZ.
        private void DeseneazaAxe()
        {
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Red);
            GL.Vertex3(0, 0, 0); GL.Vertex3(75, 0, 0); GL.End();
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Yellow);
            GL.Vertex3(0, 0, 0); GL.Vertex3(0, 75, 0); GL.End();
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Green);
            GL.Vertex3(0, 0, 0); GL.Vertex3(0, 0, 75); GL.End();
        }
        private void DeseneazaCubQ()
        {
            if (nQuadsList == 0 || nVertex == 0) return;
            GL.Begin(PrimitiveType.Quads);
            for (int i = 0; i < nQuadsList; i++)
            {
                switch (i % 4) { case 0: GL.Color3(Color.Blue); break; case 1: GL.Color3(Color.Red); break; case 2: GL.Color3(Color.Green); break; case 3: GL.Color3(Color.Yellow); break; }
                int x = arrQuadsList[i];
                GL.Vertex3(arrVertex[x, 0], arrVertex[x, 1], arrVertex[x, 2]);
            }
            GL.End();
        }
        private void DeseneazaCubT()
        {
            if (nTrianglesList == 0 || nVertex == 0) return;
            GL.Begin(PrimitiveType.Triangles);
            for (int i = 0; i < nTrianglesList; i++)
            {
                switch (i % 3) { case 0: GL.Color3(Color.Blue); break; case 1: GL.Color3(Color.Red); break; case 2: GL.Color3(Color.Green); break; }
                int x = arrTrianglesList[i];
                GL.Vertex3(arrVertex[x, 0], arrVertex[x, 1], arrVertex[x, 2]);
            }
            GL.End();
        }

    }
}