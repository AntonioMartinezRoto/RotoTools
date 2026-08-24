using System.Drawing.Drawing2D;

namespace RotoTools
{
    /// <summary>
    /// Motor de tema visual moderno para RotoTools.
    ///
    /// IMPORTANTE: esta clase NO modifica el diseñador (Designer.cs) ni los
    /// recursos (.resx) de ningún formulario más allá de lo estrictamente
    /// necesario para quitar el fondo antiguo (ver comentario en cada
    /// Designer.cs). Únicamente recolorea, redimensiona bordes y normaliza
    /// tipografías de los controles que el propio InitializeComponent() ya ha
    /// creado, una vez el formulario ha terminado de cargar sus datos. Ningún
    /// nombre de control, evento o tipo se modifica, por lo que el
    /// comportamiento existente de la aplicación no se ve afectado.
    ///
    /// Además, para no pisar colores que un formulario haya fijado a propósito,
    /// la mayoría de propiedades solo se cambian si su valor actual sigue siendo
    /// el valor "por defecto" de WinForms (SystemColors.Control, etc.).
    /// </summary>
    public static class ModernTheme
    {
        // ------------------------------------------------------------------
        // Paleta de marca RotoTools
        // ------------------------------------------------------------------
        public static readonly Color BrandRed = Color.FromArgb(216, 9, 16);
        public static readonly Color BrandRedDark = Color.FromArgb(176, 6, 12);
        public static readonly Color BrandRedLight = Color.FromArgb(253, 231, 231);

        public static readonly Color SurfaceWindow = Color.FromArgb(246, 247, 249);
        public static readonly Color SurfaceCard = Color.White;
        public static readonly Color SurfaceAlt = Color.FromArgb(248, 249, 250);
        public static readonly Color BorderColor = Color.FromArgb(222, 226, 230);

        public static readonly Color TextPrimary = Color.FromArgb(33, 37, 41);
        public static readonly Color TextSecondary = Color.FromArgb(108, 117, 125);

        public static readonly Font BaseFont = new("Segoe UI", 9.25f, FontStyle.Regular);

        // Familias de fuente "antiguas" que sustituimos por una tipografía
        // moderna. Si el control ya usa Segoe UI (el valor por defecto desde
        // .NET Core/5+), no se toca para no alterar tamaños/posiciones.
        private static readonly HashSet<string> LegacyFontFamilies = new(StringComparer.OrdinalIgnoreCase)
        {
            "Microsoft Sans Serif",
            "MS Sans Serif",
            "Tahoma",
            "Arial",
            "Calibri",
        };

        /// <summary>
        /// Aplica el tema moderno a un formulario (y a todos sus controles hijos,
        /// de forma recursiva) una vez ha terminado de cargar.
        /// </summary>
        public static void Apply(Control root)
        {
            if (root == null) return;
            StyleRecursive(root, isRoot: true);
        }

        private static void StyleRecursive(Control control, bool isRoot)
        {
            try
            {
                StyleSingle(control, isRoot);
            }
            catch
            {
                // Un fallo al estilizar un control puntual nunca debe impedir
                // que el formulario cargue con normalidad.
            }

            foreach (Control child in control.Controls.Cast<Control>().ToList())
            {
                StyleRecursive(child, false);
            }
        }

        private static void StyleSingle(Control control, bool isRoot)
        {
            switch (control)
            {
                case Form form when isRoot:
                    if (IsDefaultColor(form.BackColor, SystemColors.Control))
                        form.BackColor = SurfaceWindow;
                    ApplyFont(form);
                    break;

                case Button btn:
                    StyleButton(btn);
                    break;

                case Label lbl:
                    if (IsDefaultForeColor(lbl.ForeColor))
                        lbl.ForeColor = TextPrimary;
                    ApplyFont(lbl);
                    break;

                case DataGridView grid:
                    StyleDataGridView(grid);
                    break;

                case ListView lv:
                    if (IsDefaultColor(lv.BackColor, SystemColors.Window))
                        lv.BackColor = Color.White;
                    if (IsDefaultForeColor(lv.ForeColor))
                        lv.ForeColor = TextPrimary;
                    ApplyFont(lv);
                    break;

                case ComboBox cmb:
                    cmb.FlatStyle = FlatStyle.Flat;
                    if (IsDefaultColor(cmb.BackColor, SystemColors.Window))
                        cmb.BackColor = Color.White;
                    if (IsDefaultForeColor(cmb.ForeColor))
                        cmb.ForeColor = TextPrimary;
                    ApplyFont(cmb);
                    break;

                case CheckedListBox clb:
                    if (IsDefaultColor(clb.BackColor, SystemColors.Window))
                        clb.BackColor = Color.White;
                    if (IsDefaultForeColor(clb.ForeColor))
                        clb.ForeColor = TextPrimary;
                    ApplyFont(clb);
                    break;

                case CheckBox or RadioButton:
                    if (IsDefaultForeColor(control.ForeColor))
                        control.ForeColor = TextPrimary;
                    ApplyFont(control);
                    break;

                case TextBoxBase or NumericUpDown or MaskedTextBox:
                    if (IsDefaultColor(control.BackColor, SystemColors.Window))
                        control.BackColor = Color.White;
                    if (IsDefaultForeColor(control.ForeColor))
                        control.ForeColor = TextPrimary;
                    ApplyFont(control);
                    break;

                case GroupBox or Panel:
                    if (IsDefaultColor(control.BackColor, SystemColors.Control))
                        control.BackColor = SurfaceCard;
                    if (IsDefaultForeColor(control.ForeColor))
                        control.ForeColor = TextPrimary;
                    ApplyFont(control);
                    break;

                case TabControl tab:
                    ApplyFont(tab);
                    break;

                case ProgressBar pb:
                    // El estilo visual del ProgressBar nativo no admite
                    // recolorear sin owner-draw; se deja intacto para no
                    // arriesgar su comportamiento.
                    break;

                case ToolStrip strip: // cubre también MenuStrip y StatusStrip
                    ApplyFont(strip);
                    foreach (ToolStripItem item in strip.Items)
                        StyleToolStripItem(item);
                    break;
            }
        }

        private static void StyleToolStripItem(ToolStripItem item)
        {
            try
            {
                if (IsDefaultForeColor(item.ForeColor))
                    item.ForeColor = TextPrimary;
                var normalized = NormalizeFont(item.Font);
                if (normalized != null) item.Font = normalized;

                if (item is ToolStripDropDownItem dropDown)
                {
                    foreach (ToolStripItem sub in dropDown.DropDownItems)
                        StyleToolStripItem(sub);
                }
            }
            catch
            {
                // ignorar, no crítico
            }
        }

        private static void StyleButton(Button btn)
        {
            bool isDefaultBg = IsDefaultColor(btn.BackColor, SystemColors.Control);
            bool isPrimary = btn.DialogResult == DialogResult.OK;

            btn.FlatStyle = FlatStyle.Flat;
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.BorderSize = 1;

            if (isPrimary)
            {
                btn.BackColor = BrandRed;
                btn.ForeColor = Color.White;
                btn.FlatAppearance.BorderColor = BrandRedDark;
                btn.FlatAppearance.MouseOverBackColor = BrandRedDark;
                btn.FlatAppearance.MouseDownBackColor = BrandRedDark;
            }
            else
            {
                if (isDefaultBg) btn.BackColor = SurfaceCard;
                btn.FlatAppearance.BorderColor = BorderColor;
                btn.FlatAppearance.MouseOverBackColor = BrandRedLight;
                btn.FlatAppearance.MouseDownBackColor = BrandRedLight;
                if (IsDefaultForeColor(btn.ForeColor))
                    btn.ForeColor = TextPrimary;
            }

            ApplyFont(btn);
            ApplyRoundedRegion(btn, Math.Min(8, Math.Max(0, btn.Height / 2)));
        }

        private static void StyleDataGridView(DataGridView grid)
        {
            grid.BorderStyle = BorderStyle.None;
            grid.BackgroundColor = SurfaceCard;
            grid.GridColor = BorderColor;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.EnableHeadersVisualStyles = false;
            grid.RowHeadersDefaultCellStyle.BackColor = SurfaceCard;

            grid.ColumnHeadersDefaultCellStyle.BackColor = SurfaceAlt;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = SurfaceAlt;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextPrimary;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font(BaseFont, FontStyle.Bold);
            if (grid.ColumnHeadersHeightSizeMode != DataGridViewColumnHeadersHeightSizeMode.AutoSize)
                grid.ColumnHeadersHeight = Math.Max(grid.ColumnHeadersHeight, 30);

            grid.DefaultCellStyle.BackColor = SurfaceCard;
            grid.DefaultCellStyle.ForeColor = TextPrimary;
            grid.DefaultCellStyle.SelectionBackColor = BrandRedLight;
            grid.DefaultCellStyle.SelectionForeColor = TextPrimary;

            grid.AlternatingRowsDefaultCellStyle.BackColor = SurfaceAlt;
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = BrandRedLight;
            grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = TextPrimary;

            var normalized = NormalizeFont(grid.Font);
            if (normalized != null) grid.Font = normalized;
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static bool IsDefaultColor(Color current, Color defaultColor)
            => current.ToArgb() == defaultColor.ToArgb();

        private static bool IsDefaultForeColor(Color current)
            => current.ToArgb() == SystemColors.ControlText.ToArgb()
            || current.ToArgb() == SystemColors.WindowText.ToArgb()
            || current.ToArgb() == Color.Black.ToArgb();

        private static Font? NormalizeFont(Font current)
        {
            if (current != null && LegacyFontFamilies.Contains(current.FontFamily.Name))
                return new Font("Segoe UI", current.Size, current.Style);
            return null;
        }

        private static void ApplyFont(Control control)
        {
            var normalized = NormalizeFont(control.Font);
            if (normalized != null) control.Font = normalized;
        }

        private static void ApplyRoundedRegion(Control control, int radius)
        {
            void SetRegion()
            {
                if (control.Width <= 0 || control.Height <= 0) return;
                var path = RoundedRectPath(new Rectangle(0, 0, control.Width, control.Height), radius);
                control.Region?.Dispose();
                control.Region = new Region(path);
            }

            SetRegion();
            control.Resize += (_, _) => SetRegion();
        }

        private static GraphicsPath RoundedRectPath(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            if (d <= 0 || d >= bounds.Width || d >= bounds.Height)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.StartFigure();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    /// <summary>
    /// Paleta de colores para menús, barra de estado y toolbars, usada por
    /// <see cref="System.Windows.Forms.ToolStripProfessionalRenderer"/> para
    /// lograr un aspecto plano y consistente con la marca RotoTools.
    /// </summary>
    public sealed class ModernToolStripColorTable : ProfessionalColorTable
    {
        public override Color ToolStripGradientBegin => ModernTheme.SurfaceCard;
        public override Color ToolStripGradientMiddle => ModernTheme.SurfaceCard;
        public override Color ToolStripGradientEnd => ModernTheme.SurfaceCard;
        public override Color MenuStripGradientBegin => ModernTheme.SurfaceCard;
        public override Color MenuStripGradientEnd => ModernTheme.SurfaceCard;
        public override Color ImageMarginGradientBegin => ModernTheme.SurfaceCard;
        public override Color ImageMarginGradientMiddle => ModernTheme.SurfaceCard;
        public override Color ImageMarginGradientEnd => ModernTheme.SurfaceCard;
        public override Color MenuItemSelected => ModernTheme.BrandRedLight;
        public override Color MenuItemSelectedGradientBegin => ModernTheme.BrandRedLight;
        public override Color MenuItemSelectedGradientEnd => ModernTheme.BrandRedLight;
        public override Color MenuItemBorder => ModernTheme.BrandRed;
        public override Color MenuBorder => ModernTheme.BorderColor;
        public override Color SeparatorDark => ModernTheme.BorderColor;
        public override Color SeparatorLight => ModernTheme.SurfaceCard;
        public override Color StatusStripGradientBegin => ModernTheme.SurfaceWindow;
        public override Color StatusStripGradientEnd => ModernTheme.SurfaceWindow;
        public override Color ButtonSelectedHighlight => ModernTheme.BrandRedLight;
        public override Color ButtonSelectedBorder => ModernTheme.BrandRed;
    }
}
