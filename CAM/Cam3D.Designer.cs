namespace RotoTools
{
    partial class Cam3D
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Cam3D));
            btn_InstalarOperaciones = new Button();
            btn_Volver = new Button();
            btn_CatalogoOperaciones = new Button();
            btn_CatalogoPerfiles = new Button();
            statusStrip1 = new StatusStrip();
            lbl_Conexion = new ToolStripStatusLabel();
            treeViewMateriales = new TreeView();
            imageList1 = new ImageList(components);
            dataGridViewMateriales = new DataGridView();
            dataGridViewResultado = new DataGridView();
            txt_Buscar = new TextBox();
            lbl_Buscar = new Label();
            lbl_TodosPerfiles = new Label();
            lbl_Resultado = new Label();
            btn_LimpiarResultado = new Button();
            grp_OperacionesInfo = new GroupBox();
            lst_OperacionesInfo = new ListBox();
            progress_Instalar3D = new ProgressBar();
            statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewMateriales).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewResultado).BeginInit();
            grp_OperacionesInfo.SuspendLayout();
            SuspendLayout();
            // 
            // btn_InstalarOperaciones
            // 
            btn_InstalarOperaciones.BackColor = Color.White;
            btn_InstalarOperaciones.Image = (Image)resources.GetObject("btn_InstalarOperaciones.Image");
            btn_InstalarOperaciones.ImageAlign = ContentAlignment.MiddleLeft;
            btn_InstalarOperaciones.Location = new Point(1687, 884);
            btn_InstalarOperaciones.Margin = new Padding(3, 4, 3, 4);
            btn_InstalarOperaciones.Name = "btn_InstalarOperaciones";
            btn_InstalarOperaciones.Padding = new Padding(5, 0, 20, 0);
            btn_InstalarOperaciones.Size = new Size(151, 53);
            btn_InstalarOperaciones.TabIndex = 11;
            btn_InstalarOperaciones.Text = "Instalar";
            btn_InstalarOperaciones.TextAlign = ContentAlignment.MiddleRight;
            btn_InstalarOperaciones.UseVisualStyleBackColor = false;
            btn_InstalarOperaciones.Click += btn_InstalarOperaciones_Click;
            // 
            // btn_Volver
            // 
            btn_Volver.BackColor = Color.White;
            btn_Volver.Image = (Image)resources.GetObject("btn_Volver.Image");
            btn_Volver.ImageAlign = ContentAlignment.MiddleLeft;
            btn_Volver.Location = new Point(1521, 884);
            btn_Volver.Margin = new Padding(3, 4, 3, 4);
            btn_Volver.Name = "btn_Volver";
            btn_Volver.Padding = new Padding(15, 0, 20, 0);
            btn_Volver.Size = new Size(151, 53);
            btn_Volver.TabIndex = 40;
            btn_Volver.Text = "Volver";
            btn_Volver.TextAlign = ContentAlignment.MiddleRight;
            btn_Volver.UseVisualStyleBackColor = false;
            btn_Volver.Click += btn_Volver_Click;
            // 
            // btn_CatalogoOperaciones
            // 
            btn_CatalogoOperaciones.BackColor = Color.White;
            btn_CatalogoOperaciones.Image = (Image)resources.GetObject("btn_CatalogoOperaciones.Image");
            btn_CatalogoOperaciones.ImageAlign = ContentAlignment.MiddleLeft;
            btn_CatalogoOperaciones.Location = new Point(1355, 884);
            btn_CatalogoOperaciones.Margin = new Padding(3, 4, 3, 4);
            btn_CatalogoOperaciones.Name = "btn_CatalogoOperaciones";
            btn_CatalogoOperaciones.Padding = new Padding(5, 0, 10, 0);
            btn_CatalogoOperaciones.Size = new Size(151, 53);
            btn_CatalogoOperaciones.TabIndex = 41;
            btn_CatalogoOperaciones.Text = "Operaciones";
            btn_CatalogoOperaciones.TextAlign = ContentAlignment.MiddleRight;
            btn_CatalogoOperaciones.UseVisualStyleBackColor = false;
            btn_CatalogoOperaciones.Click += btn_CatalogoOperaciones_Click;
            // 
            // btn_CatalogoPerfiles
            // 
            btn_CatalogoPerfiles.BackColor = Color.White;
            btn_CatalogoPerfiles.Image = (Image)resources.GetObject("btn_CatalogoPerfiles.Image");
            btn_CatalogoPerfiles.ImageAlign = ContentAlignment.MiddleLeft;
            btn_CatalogoPerfiles.Location = new Point(1189, 884);
            btn_CatalogoPerfiles.Margin = new Padding(3, 4, 3, 4);
            btn_CatalogoPerfiles.Name = "btn_CatalogoPerfiles";
            btn_CatalogoPerfiles.Padding = new Padding(5, 0, 20, 0);
            btn_CatalogoPerfiles.Size = new Size(151, 53);
            btn_CatalogoPerfiles.TabIndex = 43;
            btn_CatalogoPerfiles.Text = "Perfiles";
            btn_CatalogoPerfiles.TextAlign = ContentAlignment.MiddleRight;
            btn_CatalogoPerfiles.UseVisualStyleBackColor = false;
            btn_CatalogoPerfiles.Click += btn_CatalogoPerfiles_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.Transparent;
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { lbl_Conexion });
            statusStrip1.Location = new Point(0, 941);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new Padding(1, 0, 16, 0);
            statusStrip1.Size = new Size(1850, 26);
            statusStrip1.SizingGrip = false;
            statusStrip1.TabIndex = 12;
            statusStrip1.Text = "statusStrip1";
            // 
            // lbl_Conexion
            // 
            lbl_Conexion.Name = "lbl_Conexion";
            lbl_Conexion.Size = new Size(151, 20);
            lbl_Conexion.Text = "toolStripStatusLabel1";
            // 
            // treeViewMateriales
            // 
            treeViewMateriales.Location = new Point(12, 172);
            treeViewMateriales.Name = "treeViewMateriales";
            treeViewMateriales.Size = new Size(647, 320);
            treeViewMateriales.TabIndex = 13;
            treeViewMateriales.DoubleClick += treeViewMateriales_DoubleClick;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageStream = (ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = Color.Transparent;
            imageList1.Images.SetKeyName(0, "folder.png");
            imageList1.Images.SetKeyName(1, "steel.png");
            // 
            // dataGridViewMateriales
            // 
            dataGridViewMateriales.AllowUserToAddRows = false;
            dataGridViewMateriales.AllowUserToDeleteRows = false;
            dataGridViewMateriales.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewMateriales.EditMode = DataGridViewEditMode.EditOnEnter;
            dataGridViewMateriales.Location = new Point(665, 172);
            dataGridViewMateriales.Margin = new Padding(3, 4, 3, 4);
            dataGridViewMateriales.MultiSelect = false;
            dataGridViewMateriales.Name = "dataGridViewMateriales";
            dataGridViewMateriales.ReadOnly = true;
            dataGridViewMateriales.RowHeadersWidth = 51;
            dataGridViewMateriales.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewMateriales.Size = new Size(1173, 320);
            dataGridViewMateriales.TabIndex = 31;
            dataGridViewMateriales.CellDoubleClick += dataGridViewMateriales_CellDoubleClick;
            dataGridViewMateriales.SelectionChanged += dataGridViewMateriales_SelectionChanged;
            // 
            // dataGridViewResultado
            // 
            dataGridViewResultado.AllowUserToAddRows = false;
            dataGridViewResultado.AllowUserToDeleteRows = false;
            dataGridViewResultado.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewResultado.EditMode = DataGridViewEditMode.EditOnEnter;
            dataGridViewResultado.Location = new Point(12, 538);
            dataGridViewResultado.Margin = new Padding(3, 4, 3, 4);
            dataGridViewResultado.MultiSelect = false;
            dataGridViewResultado.Name = "dataGridViewResultado";
            dataGridViewResultado.RowHeadersWidth = 51;
            dataGridViewResultado.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewResultado.Size = new Size(1826, 338);
            dataGridViewResultado.TabIndex = 36;
            // 
            // txt_Buscar
            // 
            txt_Buscar.Location = new Point(1678, 141);
            txt_Buscar.Name = "txt_Buscar";
            txt_Buscar.Size = new Size(160, 27);
            txt_Buscar.TabIndex = 33;
            txt_Buscar.TextChanged += txt_Buscar_TextChanged;
            // 
            // lbl_Buscar
            // 
            lbl_Buscar.AutoSize = true;
            lbl_Buscar.BackColor = Color.Transparent;
            lbl_Buscar.Location = new Point(1580, 144);
            lbl_Buscar.Name = "lbl_Buscar";
            lbl_Buscar.Size = new Size(52, 20);
            lbl_Buscar.TabIndex = 32;
            lbl_Buscar.Text = "Buscar";
            // 
            // lbl_TodosPerfiles
            // 
            lbl_TodosPerfiles.AutoSize = true;
            lbl_TodosPerfiles.BackColor = Color.Transparent;
            lbl_TodosPerfiles.Location = new Point(665, 144);
            lbl_TodosPerfiles.Name = "lbl_TodosPerfiles";
            lbl_TodosPerfiles.Size = new Size(343, 20);
            lbl_TodosPerfiles.TabIndex = 37;
            lbl_TodosPerfiles.Text = "Todos los perfiles (doble clic para añadir a la lista)";
            // 
            // lbl_Resultado
            // 
            lbl_Resultado.AutoSize = true;
            lbl_Resultado.BackColor = Color.Transparent;
            lbl_Resultado.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            lbl_Resultado.Location = new Point(12, 511);
            lbl_Resultado.Name = "lbl_Resultado";
            lbl_Resultado.Size = new Size(146, 23);
            lbl_Resultado.TabIndex = 38;
            lbl_Resultado.Text = "Perfiles a instalar";
            // 
            // btn_LimpiarResultado
            // 
            btn_LimpiarResultado.BackColor = Color.White;
            btn_LimpiarResultado.Location = new Point(175, 506);
            btn_LimpiarResultado.Name = "btn_LimpiarResultado";
            btn_LimpiarResultado.Size = new Size(110, 30);
            btn_LimpiarResultado.TabIndex = 42;
            btn_LimpiarResultado.Text = "Limpiar";
            btn_LimpiarResultado.UseVisualStyleBackColor = false;
            btn_LimpiarResultado.Click += btn_LimpiarResultado_Click;
            // 
            // grp_OperacionesInfo
            // 
            grp_OperacionesInfo.BackColor = Color.Gainsboro;
            grp_OperacionesInfo.Controls.Add(lst_OperacionesInfo);
            grp_OperacionesInfo.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            grp_OperacionesInfo.Location = new Point(12, 12);
            grp_OperacionesInfo.Name = "grp_OperacionesInfo";
            grp_OperacionesInfo.Size = new Size(1826, 120);
            grp_OperacionesInfo.TabIndex = 39;
            grp_OperacionesInfo.TabStop = false;
            grp_OperacionesInfo.Text = "Operaciones a instalar en 3D";
            // 
            // lst_OperacionesInfo
            // 
            lst_OperacionesInfo.BackColor = Color.White;
            lst_OperacionesInfo.Font = new Font("Segoe UI", 10.5F);
            lst_OperacionesInfo.FormattingEnabled = true;
            lst_OperacionesInfo.ItemHeight = 23;
            lst_OperacionesInfo.Location = new Point(10, 26);
            lst_OperacionesInfo.Name = "lst_OperacionesInfo";
            lst_OperacionesInfo.SelectionMode = SelectionMode.None;
            lst_OperacionesInfo.Size = new Size(1810, 73);
            lst_OperacionesInfo.TabIndex = 0;
            // 
            // progress_Instalar3D
            // 
            progress_Instalar3D.Location = new Point(12, 899);
            progress_Instalar3D.Name = "progress_Instalar3D";
            progress_Instalar3D.Size = new Size(1156, 27);
            progress_Instalar3D.TabIndex = 35;
            progress_Instalar3D.Visible = false;
            // 
            // Cam3D
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1850, 967);
            Controls.Add(txt_Buscar);
            Controls.Add(lbl_Buscar);
            Controls.Add(lbl_TodosPerfiles);
            Controls.Add(lbl_Resultado);
            Controls.Add(btn_LimpiarResultado);
            Controls.Add(grp_OperacionesInfo);
            Controls.Add(progress_Instalar3D);
            Controls.Add(dataGridViewMateriales);
            Controls.Add(dataGridViewResultado);
            Controls.Add(treeViewMateriales);
            Controls.Add(statusStrip1);
            Controls.Add(btn_InstalarOperaciones);
            Controls.Add(btn_Volver);
            Controls.Add(btn_CatalogoOperaciones);
            Controls.Add(btn_CatalogoPerfiles);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Cam3D";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Cam3D";
            Load += Cam3D_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewMateriales).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewResultado).EndInit();
            grp_OperacionesInfo.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btn_InstalarOperaciones;
        private Button btn_Volver;
        private Button btn_CatalogoOperaciones;
        private Button btn_CatalogoPerfiles;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lbl_Conexion;
        private TreeView treeViewMateriales;
        private ImageList imageList1;
        private DataGridView dataGridViewMateriales;
        private DataGridView dataGridViewResultado;
        private TextBox txt_Buscar;
        private Label lbl_Buscar;
        private Label lbl_TodosPerfiles;
        private Label lbl_Resultado;
        private Button btn_LimpiarResultado;
        private GroupBox grp_OperacionesInfo;
        private ListBox lst_OperacionesInfo;
        private ProgressBar progress_Instalar3D;
    }
}
