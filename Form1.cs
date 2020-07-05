using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using ControlEngine.Extended;
using ControlEngine;
using System.Reflection;
using Newtonsoft.Json;

namespace CollisionCreatorSoft
{
    public partial class Form1 : Form
    {
        Image SelectImage = null;
        string ProjectName = "";
        Vector Shift = Vector.Empty;
        Vector LastShift = Vector.Empty;
        const float MovementK = 3;
        float ScaleValue = 1;
        float LastScaleValue = 1;
        int FPSDeley = 32;
        Color PointColor = Color.Red;
        Size PointSize = new Size(6, 6);
        List<Panel> PointMass = new List<Panel>();

        public Form1()
        {
            Size = new Size(1280, 720);
            MouseWheel += Scaling;
            SizeChanged += (s, e) => Refresh();
            MouseClick += SetPointClickHandler;
            InitializeComponent();
        }

        private void открытьИзображениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;
            int dot = openFileDialog1.FileName.LastIndexOf('.');
            int slash = openFileDialog1.FileName.LastIndexOf('\\');
            ProjectName = openFileDialog1.FileName.Substring(slash + 1, dot - slash) + "json";
            using (var s = openFileDialog1.OpenFile())
            {
                SelectImage = Image.FromStream(s);
            }
            Refresh();
        }
        private void Scaling(object sender, MouseEventArgs e)
        {
            if (ScaleValue + e.Delta * 0.0048f < 0) return;
            double temp = e.Delta * 0.0048f;
            temp = (float)Math.Ceiling(Math.Abs(temp)) * Math.Sign(temp);
            if (ScaleValue + temp == 0) return;
            ScaleValue += (float)temp;
            Refresh();
        }
        private void сохранитьМодельToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = ProjectName;
            saveFileDialog1.Filter = "JSON (*.json)|*.json|JSONFormatFile (*.txt)|*.txt";
            if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;
            using (var s = saveFileDialog1.OpenFile())
            {
                var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(PointMass.Select((p) => new Vector(p.Location.X + (double)PointSize.Width / 2, p.Location.Y + (double)PointSize.Height / 2)).ToArray()));
                s.Write(buffer, 0, buffer.Length);
            }
        }
        private void RemovePointClickHandler(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            PointMass.Remove((Panel)sender);
            Controls.Remove((Control)sender);
            Refresh();
        }
        private void SetPointClickHandler(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            var temp = new Panel() { BackColor = PointColor, Size = PointSize, Location = new Point(e.Location.X - PointSize.Width / 2, e.Location.Y - PointSize.Height / 2) };
            PointMass.Add(temp);
            temp.MouseClick += RemovePointClickHandler;
            temp.MouseDown += TranslateCatchPointClickHandler;
            Controls.Add(temp);
            Refresh();
        }

        delegate Point GetPoint();
        private void TranslateCatchPointClickHandler(object sender, MouseEventArgs e) //привязывает точку к курсору мыши, пока кнопка не будет отпущена
        {
            if (e.Button != MouseButtons.Left) return;
            var th = new Thread(new ParameterizedThreadStart((form) =>
            {
                GetPoint CursorToClient = () =>
                {
                    return PointToClient(Cursor.Position);
                };
                while (true)
                {
                    var Ps = (Panel)sender;
                    Point now = (Point)((Form1)form).Invoke(CursorToClient);
                    Ps.Invoke((MethodInvoker)(() => { Ps.Location = new Point(now.X - Ps.Width / 2, now.Y - Ps.Height / 2); }));
                    ((Form1)form).Invoke(new Action(Refresh));
                    Thread.Sleep(FPSDeley);
                }
            }));
            ((Panel)sender).MouseUp += (s, ev) => TranslateSetPointClickHandler(s, ev, th);
            th.Start(this);
        }
        private void TranslateSetPointClickHandler(object sender, MouseEventArgs e, Thread th)
        {
            th.Abort();
            ((Panel)sender).MouseUp -= (s, ev) => TranslateSetPointClickHandler(s, ev, th);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            if (SelectImage != null)
            {
                g.ScaleTransform(ScaleValue, ScaleValue);
                g.TranslateTransform((int)Shift.X, (int)Shift.Y);
                var tempLoc = new Vector((Width / ScaleValue - SelectImage.Width) / 2, (Height / ScaleValue - SelectImage.Height) / 2);
                g.DrawImage(SelectImage, tempLoc);
                g.DrawRectangle(new Pen(Color.Black), new Rectangle(new Point((int)tempLoc.X, (int)tempLoc.Y), SelectImage.Size));
                g.TranslateTransform(-(int)Shift.X, -(int)Shift.Y);
                g.ScaleTransform(1 / ScaleValue, 1 / ScaleValue);
            }
            if (PointMass.Count > 0)
            {
                var tempLineArr = new List<Point>();
                foreach (var p in PointMass)
                {
                    p.Location = (Shift - LastShift) * (int)ScaleValue + p.Location;
                    p.Location = new Point((int)Math.Round((p.Location.X - Width / 2) * ScaleValue / LastScaleValue + Width / 2), (int)Math.Round((p.Location.Y - Height / 2) * ScaleValue / LastScaleValue + Height / 2));
                    tempLineArr.Add(new Point(p.Location.X + PointSize.Width / 2, p.Location.Y + PointSize.Height / 2));
                }
                tempLineArr.Add(tempLineArr[0]);
                g.DrawLines(new Pen(PointColor), tempLineArr.ToArray());
            }
            LastScaleValue = ScaleValue;
            LastShift = Shift;
            g.Dispose();
        }

        private void отчиститьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectImage = null;
            ScaleValue = 1;
            LastScaleValue = 1;
            foreach(var p in PointMass)
            {
                Controls.Remove(p);
            }
            PointMass.Clear();
            Refresh();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            Vector Dir = MovementK / ScaleValue;
            switch (e.KeyCode)
            {
                case Keys.Up: Dir *= Vector.Up; break;
                case Keys.Down: Dir *= Vector.Down; break;
                case Keys.Left: Dir *= Vector.Left; break;
                case Keys.Right: Dir *= Vector.Right; break;
                default: return;
            }
            Shift += Dir.Ceiling();
            Refresh();
        }
    }
}
