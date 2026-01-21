namespace RotoTools
{
    partial class CamMenu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CamMenu));
            lbl_Xml = new Label();
            btn_LoadXml = new Button();
            btn_CargarOperations = new Button();
            btn_ClearOperations = new Button();
            btn_InstalarMacros = new Button();
            btn_ExportMacros = new Button();
            chkList_Sets = new CheckedListBox();
            txt_filter = new TextBox();
            lbl_Busqueda = new Label();
            chk_All = new CheckBox();
            chk_AllOperations = new CheckBox();
            txt_FilterOperations = new TextBox();
            lbl_BusquedaOp = new Label();
            group_Sets = new GroupBox();
            group_Operaciones = new GroupBox();
            dataGridView2 = new DataGridView();
            rb_NoExists = new RadioButton();
            rb_All = new RadioButton();
            btn_InstallOperation = new Button();
            dataGridView1 = new DataGridView();
            statusStrip1 = new StatusStrip();
            lbl_Conexion = new ToolStripStatusLabel();
            progress_Install = new ProgressBar();
            group_Sets.SuspendLayout();
            group_Operaciones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // lbl_Xml
            // 
            lbl_Xml.BackColor = Color.Transparent;
            lbl_Xml.Location = new Point(94, 44);
            lbl_Xml.Name = "lbl_Xml";
            lbl_Xml.Size = new Size(978, 49);
            lbl_Xml.TabIndex = 6;
            lbl_Xml.Text = "Seleccionar XML";
            lbl_Xml.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btn_LoadXml
            // 
            btn_LoadXml.BackgroundImage = (Image)resources.GetObject("btn_LoadXml.BackgroundImage");
            btn_LoadXml.BackgroundImageLayout = ImageLayout.Stretch;
            btn_LoadXml.Location = new Point(41, 41);
            btn_LoadXml.Margin = new Padding(3, 4, 3, 4);
            btn_LoadXml.Name = "btn_LoadXml";
            btn_LoadXml.Size = new Size(46, 53);
            btn_LoadXml.TabIndex = 5;
            btn_LoadXml.UseVisualStyleBackColor = true;
            btn_LoadXml.Click += btn_LoadXml_Click;
            // 
            // btn_CargarOperations
            // 
            btn_CargarOperations.BackgroundImage = (Image)resources.GetObject("btn_CargarOperations.BackgroundImage");
            btn_CargarOperations.BackgroundImageLayout = ImageLayout.Stretch;
            btn_CargarOperations.Location = new Point(41, 124);
            btn_CargarOperations.Margin = new Padding(3, 4, 3, 4);
            btn_CargarOperations.Name = "btn_CargarOperations";
            btn_CargarOperations.Size = new Size(29, 37);
            btn_CargarOperations.TabIndex = 12;
            btn_CargarOperations.UseVisualStyleBackColor = true;
            btn_CargarOperations.Click += btn_CargarOperations_Click;
            // 
            // btn_ClearOperations
            // 
            btn_ClearOperations.BackgroundImage = (Image)resources.GetObject("btn_ClearOperations.BackgroundImage");
            btn_ClearOperations.BackgroundImageLayout = ImageLayout.Stretch;
            btn_ClearOperations.Location = new Point(77, 124);
            btn_ClearOperations.Margin = new Padding(3, 4, 3, 4);
            btn_ClearOperations.Name = "btn_ClearOperations";
            btn_ClearOperations.Size = new Size(33, 37);
            btn_ClearOperations.TabIndex = 13;
            btn_ClearOperations.UseVisualStyleBackColor = true;
            btn_ClearOperations.Click += btn_ClearOperations_Click;
            // 
            // btn_InstalarMacros
            // 
            btn_InstalarMacros.BackgroundImage = (Image)resources.GetObject("btn_InstalarMacros.BackgroundImage");
            btn_InstalarMacros.BackgroundImageLayout = ImageLayout.Stretch;
            btn_InstalarMacros.Location = new Point(117, 124);
            btn_InstalarMacros.Margin = new Padding(3, 4, 3, 4);
            btn_InstalarMacros.Name = "btn_InstalarMacros";
            btn_InstalarMacros.Size = new Size(33, 37);
            btn_InstalarMacros.TabIndex = 15;
            btn_InstalarMacros.UseVisualStyleBackColor = true;
            btn_InstalarMacros.Click += btn_InstalarMacros_Click;
            // 
            // btn_ExportMacros
            // 
            btn_ExportMacros.BackgroundImage = (Image)resources.GetObject("btn_ExportMacros.BackgroundImage");
            btn_ExportMacros.BackgroundImageLayout = ImageLayout.Stretch;
            btn_ExportMacros.Location = new Point(157, 124);
            btn_ExportMacros.Margin = new Padding(3, 4, 3, 4);
            btn_ExportMacros.Name = "btn_ExportMacros";
            btn_ExportMacros.Size = new Size(33, 37);
            btn_ExportMacros.TabIndex = 16;
            btn_ExportMacros.UseVisualStyleBackColor = true;
            btn_ExportMacros.Click += btn_ExportMacros_Click;
            // 
            // chkList_Sets
            // 
            chkList_Sets.FormattingEnabled = true;
            chkList_Sets.Location = new Point(7, 57);
            chkList_Sets.Margin = new Padding(3, 4, 3, 4);
            chkList_Sets.Name = "chkList_Sets";
            chkList_Sets.Size = new Size(439, 268);
            chkList_Sets.TabIndex = 17;
            // 
            // txt_filter
            // 
            txt_filter.Location = new Point(287, 20);
            txt_filter.Name = "txt_filter";
            txt_filter.Size = new Size(159, 27);
            txt_filter.TabIndex = 20;
            txt_filter.TextChanged += txt_filter_TextChanged;
            // 
            // lbl_Busqueda
            // 
            lbl_Busqueda.AutoSize = true;
            lbl_Busqueda.BackColor = Color.Transparent;
            lbl_Busqueda.Location = new Point(232, 27);
            lbl_Busqueda.Name = "lbl_Busqueda";
            lbl_Busqueda.Size = new Size(52, 20);
            lbl_Busqueda.TabIndex = 19;
            lbl_Busqueda.Text = "Buscar";
            // 
            // chk_All
            // 
            chk_All.AutoSize = true;
            chk_All.BackColor = Color.Transparent;
            chk_All.Font = new Font("Segoe UI", 9F);
            chk_All.Location = new Point(11, 25);
            chk_All.Name = "chk_All";
            chk_All.Size = new Size(149, 24);
            chk_All.TabIndex = 18;
            chk_All.Text = "Seleccionar todos";
            chk_All.UseVisualStyleBackColor = false;
            chk_All.CheckedChanged += chk_All_CheckedChanged;
            // 
            // chk_AllOperations
            // 
            chk_AllOperations.AutoSize = true;
            chk_AllOperations.BackColor = Color.Transparent;
            chk_AllOperations.Font = new Font("Segoe UI", 9F);
            chk_AllOperations.Location = new Point(11, 25);
            chk_AllOperations.Name = "chk_AllOperations";
            chk_AllOperations.Size = new Size(149, 24);
            chk_AllOperations.TabIndex = 21;
            chk_AllOperations.Text = "Seleccionar todos";
            chk_AllOperations.UseVisualStyleBackColor = false;
            chk_AllOperations.CheckedChanged += chk_AllOperations_CheckedChanged;
            // 
            // txt_FilterOperations
            // 
            txt_FilterOperations.Location = new Point(499, 23);
            txt_FilterOperations.Name = "txt_FilterOperations";
            txt_FilterOperations.Size = new Size(113, 27);
            txt_FilterOperations.TabIndex = 24;
            txt_FilterOperations.TextChanged += txt_FilterOperations_TextChanged;
            // 
            // lbl_BusquedaOp
            // 
            lbl_BusquedaOp.AutoSize = true;
            lbl_BusquedaOp.BackColor = Color.Transparent;
            lbl_BusquedaOp.Location = new Point(445, 27);
            lbl_BusquedaOp.Name = "lbl_BusquedaOp";
            lbl_BusquedaOp.Size = new Size(52, 20);
            lbl_BusquedaOp.TabIndex = 23;
            lbl_BusquedaOp.Text = "Buscar";
            // 
            // group_Sets
            // 
            group_Sets.BackColor = Color.Transparent;
            group_Sets.Controls.Add(chk_All);
            group_Sets.Controls.Add(chkList_Sets);
            group_Sets.Controls.Add(txt_filter);
            group_Sets.Controls.Add(lbl_Busqueda);
            group_Sets.Location = new Point(41, 169);
            group_Sets.Margin = new Padding(3, 4, 3, 4);
            group_Sets.Name = "group_Sets";
            group_Sets.Padding = new Padding(3, 4, 3, 4);
            group_Sets.Size = new Size(461, 341);
            group_Sets.TabIndex = 25;
            group_Sets.TabStop = false;
            group_Sets.Text = "Sets";
            // 
            // group_Operaciones
            // 
            group_Operaciones.BackColor = Color.Transparent;
            group_Operaciones.Controls.Add(dataGridView2);
            group_Operaciones.Controls.Add(rb_NoExists);
            group_Operaciones.Controls.Add(rb_All);
            group_Operaciones.Controls.Add(chk_AllOperations);
            group_Operaciones.Controls.Add(txt_FilterOperations);
            group_Operaciones.Controls.Add(lbl_BusquedaOp);
            group_Operaciones.Location = new Point(511, 169);
            group_Operaciones.Margin = new Padding(3, 4, 3, 4);
            group_Operaciones.Name = "group_Operaciones";
            group_Operaciones.Padding = new Padding(3, 4, 3, 4);
            group_Operaciones.Size = new Size(621, 339);
            group_Operaciones.TabIndex = 26;
            group_Operaciones.TabStop = false;
            group_Operaciones.Text = "Operaciones";
            // 
            // dataGridView2
            // 
            dataGridView2.AllowUserToAddRows = false;
            dataGridView2.AllowUserToDeleteRows = false;
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.EditMode = DataGridViewEditMode.EditOnEnter;
            dataGridView2.Location = new Point(7, 60);
            dataGridView2.Margin = new Padding(3, 4, 3, 4);
            dataGridView2.MultiSelect = false;
            dataGridView2.Name = "dataGridView2";
            dataGridView2.ReadOnly = true;
            dataGridView2.RowHeadersWidth = 51;
            dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView2.Size = new Size(606, 267);
            dataGridView2.TabIndex = 30;
            dataGridView2.CellContentClick += dataGridView2_CellContentClick;
            dataGridView2.CellMouseUp += dataGridView2_CellMouseUp;
            dataGridView2.SelectionChanged += dataGridView2_SelectionChanged;
            // 
            // rb_NoExists
            // 
            rb_NoExists.AutoSize = true;
            rb_NoExists.Location = new Point(313, 24);
            rb_NoExists.Margin = new Padding(3, 4, 3, 4);
            rb_NoExists.Name = "rb_NoExists";
            rb_NoExists.Size = new Size(100, 24);
            rb_NoExists.TabIndex = 26;
            rb_NoExists.TabStop = true;
            rb_NoExists.Text = "No existen";
            rb_NoExists.UseVisualStyleBackColor = true;
            rb_NoExists.CheckedChanged += rb_NoExists_CheckedChanged;
            // 
            // rb_All
            // 
            rb_All.AutoSize = true;
            rb_All.Location = new Point(214, 24);
            rb_All.Margin = new Padding(3, 4, 3, 4);
            rb_All.Name = "rb_All";
            rb_All.Size = new Size(69, 24);
            rb_All.TabIndex = 25;
            rb_All.TabStop = true;
            rb_All.Text = "Todas";
            rb_All.UseVisualStyleBackColor = true;
            rb_All.CheckedChanged += rb_All_CheckedChanged;
            // 
            // btn_InstallOperation
            // 
            btn_InstallOperation.BackgroundImage = (Image)resources.GetObject("btn_InstallOperation.BackgroundImage");
            btn_InstallOperation.BackgroundImageLayout = ImageLayout.Stretch;
            btn_InstallOperation.Location = new Point(1098, 124);
            btn_InstallOperation.Margin = new Padding(3, 4, 3, 4);
            btn_InstallOperation.Name = "btn_InstallOperation";
            btn_InstallOperation.Size = new Size(33, 37);
            btn_InstallOperation.TabIndex = 27;
            btn_InstallOperation.UseVisualStyleBackColor = true;
            btn_InstallOperation.Click += btn_InstallOperation_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(42, 521);
            dataGridView1.Margin = new Padding(3, 4, 3, 4);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(1090, 308);
            dataGridView1.TabIndex = 28;
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.Transparent;
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { lbl_Conexion });
            statusStrip1.Location = new Point(0, 862);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new Padding(1, 0, 16, 0);
            statusStrip1.Size = new Size(1415, 26);
            statusStrip1.SizingGrip = false;
            statusStrip1.TabIndex = 29;
            statusStrip1.Text = "statusStrip1";
            // 
            // lbl_Conexion
            // 
            lbl_Conexion.Name = "lbl_Conexion";
            lbl_Conexion.Size = new Size(151, 20);
            lbl_Conexion.Text = "toolStripStatusLabel1";
            // 
            // progress_Install
            // 
            progress_Install.Location = new Point(798, 129);
            progress_Install.Name = "progress_Install";
            progress_Install.Size = new Size(294, 29);
            progress_Install.TabIndex = 30;
            progress_Install.Visible = false;
            // 
            // CamMenu
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1415, 888);
            Controls.Add(progress_Install);
            Controls.Add(statusStrip1);
            Controls.Add(dataGridView1);
            Controls.Add(btn_InstallOperation);
            Controls.Add(group_Operaciones);
            Controls.Add(group_Sets);
            Controls.Add(btn_ExportMacros);
            Controls.Add(btn_InstalarMacros);
            Controls.Add(btn_ClearOperations);
            Controls.Add(btn_CargarOperations);
            Controls.Add(lbl_Xml);
            Controls.Add(btn_LoadXml);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            Name = "CamMenu";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CAM";
            Load += CamMenu_Load;
            group_Sets.ResumeLayout(false);
            group_Sets.PerformLayout();
            group_Operaciones.ResumeLayout(false);
            group_Operaciones.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lbl_Xml;
        private Button btn_LoadXml;
        private Button btn_CargarOperations;
        private Button btn_ClearOperations;
        private Button btn_InstalarMacros;
        private Button btn_ExportMacros;
        private CheckedListBox chkList_Sets;
        private TextBox txt_filter;
        private Label lbl_Busqueda;
        private CheckBox chk_All;
        private CheckBox chk_AllOperations;
        private TextBox txt_FilterOperations;
        private Label lbl_BusquedaOp;
        private GroupBox group_Sets;
        private GroupBox group_Operaciones;
        private RadioButton rb_NoExists;
        private RadioButton rb_All;
        private Button btn_InstallOperation;
        private DataGridView dataGridView1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lbl_Conexion;
        private DataGridView dataGridView2;
        private ProgressBar progress_Install;
    }
}