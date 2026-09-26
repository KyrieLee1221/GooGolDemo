using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YunkongDemo_GooGol
{
    public partial class LED : Panel
    {
        public LED()
        {
            InitializeComponent();
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            Size = new Size(30, 30);
            BackColor = Color.LawnGreen;
            UpdateCircleRegion();
        }

        [DefaultValue(typeof(Color), "LawnGreen")]
        public override Color BackColor
        {
            get => base.BackColor;
            set
            {
                base.BackColor = value;
                Invalidate();
            }
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            UpdateCircleRegion();
        }

        private void UpdateCircleRegion()
        {
            int diameter = Math.Min(Width, Height);

            if (diameter <= 0)
                return;

            int x = (Width - diameter) / 2;
            int y = (Height - diameter) / 2;

            using (var path = new GraphicsPath())
            {
                path.AddEllipse(x, y, diameter, diameter);

                Region oldRegion = Region;
                Region = new Region(path);
                oldRegion?.Dispose();
            }

            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // 不绘制 Panel 原本的矩形背景。
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int diameter = Math.Min(ClientSize.Width, ClientSize.Height);
            if (diameter <= 0)
                return;

            float x = (ClientSize.Width - diameter) / 2f;
            float y = (ClientSize.Height - diameter) / 2f;

            using (var brush = new SolidBrush(BackColor))
            {
                e.Graphics.FillEllipse(
                    brush, x, y, diameter - 1f, diameter - 1f);
            }
        }
    }
}
