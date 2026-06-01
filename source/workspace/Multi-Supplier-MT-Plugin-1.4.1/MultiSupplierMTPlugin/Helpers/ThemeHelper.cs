using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MultiSupplierMTPlugin.Helpers
{
    /// <summary>
    /// 统一主题样式工具类 — 扁平化圆角 + 明亮低饱和暖色调
    /// </summary>
    public static class ThemeHelper
    {
        #region 配色常量 — 白色基底现代风格

        public static Color FormBg       => ColorTranslator.FromHtml("#FFFFFF");  // 纯白
        public static Color CardBg       => ColorTranslator.FromHtml("#FFFFFF");  // 纯白
        public static Color TabBg        => ColorTranslator.FromHtml("#F5F5F5");  // 浅灰标签栏
        public static Color Accent       => ColorTranslator.FromHtml("#F59E6B");  // 暖橙强调
        public static Color AccentHover  => ColorTranslator.FromHtml("#F3844C");  // 悬停橙
        public static Color AccentLight  => ColorTranslator.FromHtml("#FFF5F0");  // 极浅橙底
        public static Color InfoBlue     => ColorTranslator.FromHtml("#5B9BD5");  // 信息蓝
        public static Color InfoLight    => ColorTranslator.FromHtml("#F0F6FC");
        public static Color BorderColor  => ColorTranslator.FromHtml("#E0E0E0");  // 浅灰边框
        public static Color TextPrimary  => ColorTranslator.FromHtml("#1E1E1E");  // 主文字
        public static Color TextSecondary=> ColorTranslator.FromHtml("#888888");  // 次级文字
        public static Color Success      => ColorTranslator.FromHtml("#5CB85C");  // 成功绿
        public static Color SuccessLight => ColorTranslator.FromHtml("#F0F9F0");
        public static Color ErrorColor   => ColorTranslator.FromHtml("#D9534F");  // 错误红
        public static Color ErrorLight   => ColorTranslator.FromHtml("#FDF5F5");
        public static Color CancelBg     => ColorTranslator.FromHtml("#F0F0F0");  // 次按钮浅灰
        public static Color CancelHover  => ColorTranslator.FromHtml("#E5E5E5");

        #endregion

        #region Win32 API (圆角窗体)

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWCP_ROUND = 2;
        private const int DWMWCP_DONOTROUND = 1;

        /// <summary>
        /// 给窗体应用圆角。Win11 使用 DWM 原生圆角，Win10 降级为无效果（不报错）。
        /// </summary>
        public static void ApplyRoundedCorners(Form form, int radius = 8)
        {
            form.Shown += (s, e) =>
            {
                var preference = DWMWCP_ROUND;
                DwmSetWindowAttribute(form.Handle, DWMWA_WINDOW_CORNER_PREFERENCE,
                    ref preference, sizeof(int));
            };
        }

        #endregion

        #region 自绘扁平 TabControl

        /// <summary>
        /// 应用自绘扁平 TabControl 样式：
        /// - 标签头无边框，浅卡其底色
        /// - 激活标签底部淡橙色指示条
        /// - 悬停标签淡橙色浅底
        /// </summary>
        public static void ApplyFlatTabStyle(TabControl tabControl)
        {
            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.SizeMode = TabSizeMode.Fixed;
            tabControl.BackColor = CardBg;

            // 根据文字长度计算标签宽度，中文不截断但也不过度加宽
            using (var g = tabControl.CreateGraphics())
            {
                var font = new Font(tabControl.Font.FontFamily, 9f);
                int maxWidth = 80;
                foreach (TabPage page in tabControl.TabPages)
                {
                    var textSize = TextRenderer.MeasureText(g, page.Text, font);
                    int w = textSize.Width + 32;
                    if (w > maxWidth) maxWidth = w;
                }
                // 上限防止子标签太宽需要箭头滚动
                if (maxWidth > 110) maxWidth = 110;
                tabControl.ItemSize = new Size(maxWidth, 32);
                font.Dispose();
            }

            tabControl.DrawItem += (sender, e) =>
            {
                var tab = (TabControl)sender;
                var tabPage = tab.TabPages[e.Index];
                var rect = tab.GetTabRect(e.Index);
                var isSelected = e.Index == tab.SelectedIndex;

                // 标签背景（悬停效果通过鼠标检测）
                var mousePos = tab.PointToClient(Cursor.Position);
                var isHovered = rect.Contains(mousePos) && !isSelected;
                using (var bgBrush = new SolidBrush(isSelected ? CardBg : (isHovered ? AccentLight : TabBg)))
                {
                    e.Graphics.FillRectangle(bgBrush, rect);
                }

                // 激活态底部淡橙指示条
                if (isSelected)
                {
                    using (var accentPen = new Pen(Accent, 3))
                    {
                        e.Graphics.DrawLine(accentPen,
                            rect.Left + 10, rect.Bottom - 2,
                            rect.Right - 10, rect.Bottom - 2);
                    }
                }

                // 标签文字（留足padding）
                var textRect = new Rectangle(rect.Left + 6, rect.Y, rect.Width - 12, rect.Height);
                var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                    | TextFormatFlags.EndEllipsis;
                TextRenderer.DrawText(e.Graphics, tabPage.Text,
                    new Font(tab.Font.FontFamily, 9f, FontStyle.Regular),
                    textRect,
                    isSelected ? TextPrimary : TextSecondary,
                    flags);
            };

            // 重绘标签栏底色 + 平直1px边框（覆盖系统3D阴影）
            tabControl.Paint += (sender, e) =>
            {
                int w = tabControl.Width, h = tabControl.Height;

                if (tabControl.TabPages.Count > 0)
                {
                    var headerRect = tabControl.GetTabRect(0);
                    // 标签栏底色
                    using (var bgBrush = new SolidBrush(TabBg))
                        e.Graphics.FillRectangle(bgBrush, 0, 0, w, headerRect.Bottom + 2);
                    // 标签栏下划线
                    using (var borderPen = new Pen(BorderColor, 1))
                        e.Graphics.DrawLine(borderPen, 0, headerRect.Bottom + 1, w - 1, headerRect.Bottom + 1);
                }

                // 覆盖系统3D阴影：外侧3px画纯白/浅灰底色
                using (var erasePen = new Pen(CardBg, 4))
                {
                    e.Graphics.DrawRectangle(erasePen, 0, 0, w - 1, h - 1);
                }
                // 再画1px平直灰边框
                using (var flatBorder = new Pen(BorderColor, 1))
                {
                    e.Graphics.DrawRectangle(flatBorder, 0, 0, w - 1, h - 1);
                }
            };
        }

        #endregion

        #region 扁平按钮

        /// <summary>
        /// 应用扁平按钮样式 — 纯 FlatStyle，不自绘，避免和系统绘制冲突
        /// </summary>
        /// <param name="isPrimary">true = 主按钮（淡橙底白字），false = 次按钮（暖灰底）</param>
        public static void ApplyFlatButtonStyle(Button button, bool isPrimary = false)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font(button.Font.FontFamily, 9f, FontStyle.Regular);
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;

            if (isPrimary)
            {
                button.BackColor = Accent;
                button.ForeColor = Color.White;
                button.FlatAppearance.BorderColor = Accent;
                button.FlatAppearance.MouseOverBackColor = AccentHover;
                button.FlatAppearance.MouseDownBackColor = AccentHover;
            }
            else
            {
                button.BackColor = CancelBg;
                button.ForeColor = TextPrimary;
                button.FlatAppearance.BorderColor = CancelBg;
                button.FlatAppearance.MouseOverBackColor = CancelHover;
                button.FlatAppearance.MouseDownBackColor = CancelHover;
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 设置窗体为可缩放并应用最小尺寸
        /// </summary>
        public static void MakeResizable(Form form, int minWidth, int minHeight)
        {
            form.FormBorderStyle = FormBorderStyle.Sizable;
            form.MinimumSize = new Size(minWidth, minHeight);
            form.MaximizeBox = false;
        }

        /// <summary>
        /// 应用主题背景到窗体，输入控件用极浅灰白
        /// </summary>
        public static void ApplyFormTheme(Form form)
        {
            form.BackColor = FormBg;
            SoftenControls(form.Controls);
        }

        /// <summary>
        /// 输入控件用极浅灰白 #FCFCFC，视觉柔和但和纯白底协调
        /// </summary>
        private static readonly Color InputBg = ColorTranslator.FromHtml("#F8F8F8");

        private static void SoftenControls(Control.ControlCollection controls)
        {
            foreach (Control c in controls)
            {
                if (c is TextBox || c is ComboBox || c is RichTextBox || c is ListBox)
                    c.BackColor = InputBg;
                else if (c is NumericUpDown nud)
                    nud.BackColor = InputBg;
                else if (c is DataGridView dgv)
                {
                    dgv.BackgroundColor = CardBg;
                    dgv.DefaultCellStyle.BackColor = CardBg;
                }
                else if (c is TabPage || c is Panel || c is GroupBox)
                {
                    if (c.HasChildren) SoftenControls(c.Controls);
                }
                else if (c.HasChildren)
                {
                    SoftenControls(c.Controls);
                }
            }
        }


        /// <summary>
        /// 获取圆角矩形 GraphicsPath
        /// </summary>
        public static GraphicsPath GetRoundedRectPath(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;

            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }

        #endregion
    }
}
