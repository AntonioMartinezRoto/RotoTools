namespace RotoTools
{
    partial class ConectorHerrajeGenerador
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConectorHerrajeGenerador));
            lbl_Filtro = new Label();
            txt_Filtro = new TextBox();
            chk_Predefinido = new CheckBox();
            txt_ConectorName = new TextBox();
            lbl_SaveBD = new Label();
            btn_InsertConector = new Button();
            lbl_SaveXML = new Label();
            btn_GenerarConector = new Button();
            dataGridView1 = new DataGridView();
            statusStrip1 = new StatusStrip();
            lbl_Conexion = new ToolStripStatusLabel();
            lbl_ConectorActivo = new ToolStripStatusLabel();
            groupSaveConector = new GroupBox();
            group_Buscar = new GroupBox();
            chk_Elevables = new CheckBox();
            chk_Plegables = new CheckBox();
            chk_Abatibles = new CheckBox();
            chk_Paralelas = new CheckBox();
            chk_Correderas = new CheckBox();
            chk_Puertas = new CheckBox();
            chk_Balconeras = new CheckBox();
            chk_Ventanas = new CheckBox();
            chk_SelectAll = new CheckBox();
            lbl_Total = new Label();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            statusStrip1.SuspendLayout();
            groupSaveConector.SuspendLayout();
            group_Buscar.SuspendLayout();
            SuspendLayout();
            // 
            // lbl_Filtro
            // 
            lbl_Filtro.AutoSize = true;
            lbl_Filtro.BackColor = Color.Transparent;
            lbl_Filtro.Location = new Point(1328, 292);
            lbl_Filtro.Name = "lbl_Filtro";
            lbl_Filtro.Size = new Size(52, 20);
            lbl_Filtro.TabIndex = 25;
            lbl_Filtro.Text = "Buscar";
            lbl_Filtro.Visible = false;
            // 
            // txt_Filtro
            // 
            txt_Filtro.Location = new Point(463, 47);
            txt_Filtro.Margin = new Padding(3, 4, 3, 4);
            txt_Filtro.Name = "txt_Filtro";
            txt_Filtro.Size = new Size(158, 27);
            txt_Filtro.TabIndex = 24;
            txt_Filtro.TextChanged += txt_Filtro_TextChanged;
            // 
            // chk_Predefinido
            // 
            chk_Predefinido.AutoSize = true;
            chk_Predefinido.BackColor = Color.Transparent;
            chk_Predefinido.Location = new Point(331, 68);
            chk_Predefinido.Margin = new Padding(3, 4, 3, 4);
            chk_Predefinido.Name = "chk_Predefinido";
            chk_Predefinido.Size = new Size(192, 24);
            chk_Predefinido.TabIndex = 23;
            chk_Predefinido.Text = "Poner como predefinido";
            chk_Predefinido.UseVisualStyleBackColor = false;
            // 
            // txt_ConectorName
            // 
            txt_ConectorName.Location = new Point(331, 29);
            txt_ConectorName.Margin = new Padding(3, 4, 3, 4);
            txt_ConectorName.MaxLength = 25;
            txt_ConectorName.Name = "txt_ConectorName";
            txt_ConectorName.Size = new Size(177, 27);
            txt_ConectorName.TabIndex = 22;
            // 
            // lbl_SaveBD
            // 
            lbl_SaveBD.AutoSize = true;
            lbl_SaveBD.BackColor = Color.Transparent;
            lbl_SaveBD.Location = new Point(1328, 387);
            lbl_SaveBD.Name = "lbl_SaveBD";
            lbl_SaveBD.Size = new Size(126, 20);
            lbl_SaveBD.TabIndex = 29;
            lbl_SaveBD.Text = "Guardar en BBDD";
            lbl_SaveBD.Visible = false;
            // 
            // btn_InsertConector
            // 
            btn_InsertConector.BackColor = Color.White;
            btn_InsertConector.BackgroundImageLayout = ImageLayout.Stretch;
            btn_InsertConector.Image = (Image)resources.GetObject("btn_InsertConector.Image");
            btn_InsertConector.ImageAlign = ContentAlignment.MiddleLeft;
            btn_InsertConector.Location = new Point(169, 29);
            btn_InsertConector.Name = "btn_InsertConector";
            btn_InsertConector.Size = new Size(101, 64);
            btn_InsertConector.TabIndex = 28;
            btn_InsertConector.Text = "BBDD";
            btn_InsertConector.TextAlign = ContentAlignment.MiddleRight;
            btn_InsertConector.UseVisualStyleBackColor = false;
            btn_InsertConector.Click += btn_InsertConector_Click;
            // 
            // lbl_SaveXML
            // 
            lbl_SaveXML.AutoSize = true;
            lbl_SaveXML.BackColor = Color.Transparent;
            lbl_SaveXML.Location = new Point(1328, 336);
            lbl_SaveXML.Name = "lbl_SaveXML";
            lbl_SaveXML.Size = new Size(115, 20);
            lbl_SaveXML.TabIndex = 27;
            lbl_SaveXML.Text = "Guardar en XML";
            lbl_SaveXML.Visible = false;
            // 
            // btn_GenerarConector
            // 
            btn_GenerarConector.BackColor = Color.White;
            btn_GenerarConector.BackgroundImageLayout = ImageLayout.Stretch;
            btn_GenerarConector.Image = (Image)resources.GetObject("btn_GenerarConector.Image");
            btn_GenerarConector.ImageAlign = ContentAlignment.MiddleLeft;
            btn_GenerarConector.Location = new Point(17, 28);
            btn_GenerarConector.Name = "btn_GenerarConector";
            btn_GenerarConector.Size = new Size(101, 64);
            btn_GenerarConector.TabIndex = 26;
            btn_GenerarConector.Text = "XML";
            btn_GenerarConector.TextAlign = ContentAlignment.MiddleRight;
            btn_GenerarConector.UseVisualStyleBackColor = false;
            btn_GenerarConector.Click += btn_GenerarConector_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(47, 155);
            dataGridView1.Margin = new Padding(3, 4, 3, 4);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.Size = new Size(1274, 963);
            dataGridView1.TabIndex = 30;
            dataGridView1.CellValueChanged += dataGridView1_CellValueChanged;
            dataGridView1.CurrentCellDirtyStateChanged += dataGridView1_CurrentCellDirtyStateChanged;
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.Transparent;
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { lbl_Conexion, lbl_ConectorActivo });
            statusStrip1.Location = new Point(0, 1122);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new Padding(1, 0, 16, 0);
            statusStrip1.Size = new Size(1639, 26);
            statusStrip1.SizingGrip = false;
            statusStrip1.TabIndex = 31;
            statusStrip1.Text = "statusStrip1";
            // 
            // lbl_Conexion
            // 
            lbl_Conexion.Name = "lbl_Conexion";
            lbl_Conexion.Size = new Size(1511, 20);
            lbl_Conexion.Spring = true;
            lbl_Conexion.Text = "toolStripStatusLabel1";
            lbl_Conexion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lbl_ConectorActivo
            // 
            lbl_ConectorActivo.ForeColor = Color.White;
            lbl_ConectorActivo.Name = "lbl_ConectorActivo";
            lbl_ConectorActivo.Size = new Size(111, 20);
            lbl_ConectorActivo.Text = "ConectorActivo";
            lbl_ConectorActivo.TextAlign = ContentAlignment.MiddleRight;
            // 
            // groupSaveConector
            // 
            groupSaveConector.BackColor = Color.Transparent;
            groupSaveConector.Controls.Add(btn_GenerarConector);
            groupSaveConector.Controls.Add(btn_InsertConector);
            groupSaveConector.Controls.Add(txt_ConectorName);
            groupSaveConector.Controls.Add(chk_Predefinido);
            groupSaveConector.Location = new Point(47, 16);
            groupSaveConector.Margin = new Padding(3, 4, 3, 4);
            groupSaveConector.Name = "groupSaveConector";
            groupSaveConector.Padding = new Padding(3, 4, 3, 4);
            groupSaveConector.Size = new Size(597, 103);
            groupSaveConector.TabIndex = 32;
            groupSaveConector.TabStop = false;
            groupSaveConector.Text = "Guardar";
            // 
            // group_Buscar
            // 
            group_Buscar.BackColor = Color.Transparent;
            group_Buscar.Controls.Add(chk_Elevables);
            group_Buscar.Controls.Add(chk_Plegables);
            group_Buscar.Controls.Add(chk_Abatibles);
            group_Buscar.Controls.Add(chk_Paralelas);
            group_Buscar.Controls.Add(chk_Correderas);
            group_Buscar.Controls.Add(chk_Puertas);
            group_Buscar.Controls.Add(chk_Balconeras);
            group_Buscar.Controls.Add(chk_Ventanas);
            group_Buscar.Controls.Add(txt_Filtro);
            group_Buscar.Location = new Point(663, 16);
            group_Buscar.Margin = new Padding(3, 4, 3, 4);
            group_Buscar.Name = "group_Buscar";
            group_Buscar.Padding = new Padding(3, 4, 3, 4);
            group_Buscar.Size = new Size(658, 103);
            group_Buscar.TabIndex = 33;
            group_Buscar.TabStop = false;
            group_Buscar.Text = "Buscar";
            // 
            // chk_Elevables
            // 
            chk_Elevables.AutoSize = true;
            chk_Elevables.Location = new Point(16, 63);
            chk_Elevables.Margin = new Padding(3, 4, 3, 4);
            chk_Elevables.Name = "chk_Elevables";
            chk_Elevables.Size = new Size(93, 24);
            chk_Elevables.TabIndex = 32;
            chk_Elevables.Text = "Elevables";
            chk_Elevables.UseVisualStyleBackColor = true;
            chk_Elevables.CheckedChanged += chk_Elevables_CheckedChanged;
            // 
            // chk_Plegables
            // 
            chk_Plegables.AutoSize = true;
            chk_Plegables.Location = new Point(329, 61);
            chk_Plegables.Margin = new Padding(3, 4, 3, 4);
            chk_Plegables.Name = "chk_Plegables";
            chk_Plegables.Size = new Size(95, 24);
            chk_Plegables.TabIndex = 31;
            chk_Plegables.Text = "Plegables";
            chk_Plegables.UseVisualStyleBackColor = true;
            chk_Plegables.CheckedChanged += chk_Plegables_CheckedChanged;
            // 
            // chk_Abatibles
            // 
            chk_Abatibles.AutoSize = true;
            chk_Abatibles.Location = new Point(239, 61);
            chk_Abatibles.Margin = new Padding(3, 4, 3, 4);
            chk_Abatibles.Name = "chk_Abatibles";
            chk_Abatibles.Size = new Size(94, 24);
            chk_Abatibles.TabIndex = 30;
            chk_Abatibles.Text = "Abatibles";
            chk_Abatibles.UseVisualStyleBackColor = true;
            chk_Abatibles.CheckedChanged += chk_Abatibles_CheckedChanged;
            // 
            // chk_Paralelas
            // 
            chk_Paralelas.AutoSize = true;
            chk_Paralelas.Location = new Point(120, 61);
            chk_Paralelas.Margin = new Padding(3, 4, 3, 4);
            chk_Paralelas.Name = "chk_Paralelas";
            chk_Paralelas.Size = new Size(132, 24);
            chk_Paralelas.TabIndex = 29;
            chk_Paralelas.Text = "Osciloparalelas";
            chk_Paralelas.UseVisualStyleBackColor = true;
            chk_Paralelas.CheckedChanged += chk_Paralelas_CheckedChanged;
            // 
            // chk_Correderas
            // 
            chk_Correderas.AutoSize = true;
            chk_Correderas.Location = new Point(329, 28);
            chk_Correderas.Margin = new Padding(3, 4, 3, 4);
            chk_Correderas.Name = "chk_Correderas";
            chk_Correderas.Size = new Size(103, 24);
            chk_Correderas.TabIndex = 28;
            chk_Correderas.Text = "Correderas";
            chk_Correderas.UseVisualStyleBackColor = true;
            chk_Correderas.CheckedChanged += chk_Correderas_CheckedChanged;
            // 
            // chk_Puertas
            // 
            chk_Puertas.AutoSize = true;
            chk_Puertas.Location = new Point(239, 28);
            chk_Puertas.Margin = new Padding(3, 4, 3, 4);
            chk_Puertas.Name = "chk_Puertas";
            chk_Puertas.Size = new Size(79, 24);
            chk_Puertas.TabIndex = 27;
            chk_Puertas.Text = "Puertas";
            chk_Puertas.UseVisualStyleBackColor = true;
            chk_Puertas.CheckedChanged += chk_Puertas_CheckedChanged;
            // 
            // chk_Balconeras
            // 
            chk_Balconeras.AutoSize = true;
            chk_Balconeras.Location = new Point(120, 28);
            chk_Balconeras.Margin = new Padding(3, 4, 3, 4);
            chk_Balconeras.Name = "chk_Balconeras";
            chk_Balconeras.Size = new Size(103, 24);
            chk_Balconeras.TabIndex = 26;
            chk_Balconeras.Text = "Balconeras";
            chk_Balconeras.UseVisualStyleBackColor = true;
            chk_Balconeras.CheckedChanged += chk_Balconeras_CheckedChanged;
            // 
            // chk_Ventanas
            // 
            chk_Ventanas.AutoSize = true;
            chk_Ventanas.Location = new Point(16, 28);
            chk_Ventanas.Margin = new Padding(3, 4, 3, 4);
            chk_Ventanas.Name = "chk_Ventanas";
            chk_Ventanas.Size = new Size(90, 24);
            chk_Ventanas.TabIndex = 25;
            chk_Ventanas.Text = "Ventanas";
            chk_Ventanas.UseVisualStyleBackColor = true;
            chk_Ventanas.CheckedChanged += chk_Ventanas_CheckedChanged;
            // 
            // chk_SelectAll
            // 
            chk_SelectAll.AutoSize = true;
            chk_SelectAll.BackColor = Color.Transparent;
            chk_SelectAll.Location = new Point(58, 127);
            chk_SelectAll.Margin = new Padding(3, 4, 3, 4);
            chk_SelectAll.Name = "chk_SelectAll";
            chk_SelectAll.Size = new Size(148, 24);
            chk_SelectAll.TabIndex = 34;
            chk_SelectAll.Text = "Seleccionar todas";
            chk_SelectAll.UseVisualStyleBackColor = false;
            chk_SelectAll.CheckedChanged += chk_SelectAll_CheckedChanged;
            // 
            // lbl_Total
            // 
            lbl_Total.AutoSize = true;
            lbl_Total.BackColor = Color.Transparent;
            lbl_Total.Location = new Point(1235, 132);
            lbl_Total.Name = "lbl_Total";
            lbl_Total.Size = new Size(62, 20);
            lbl_Total.TabIndex = 35;
            lbl_Total.Text = "0 Líneas";
            // 
            // ConectorHerrajeGenerador
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1639, 1148);
            Controls.Add(lbl_Total);
            Controls.Add(chk_SelectAll);
            Controls.Add(group_Buscar);
            Controls.Add(groupSaveConector);
            Controls.Add(lbl_SaveXML);
            Controls.Add(statusStrip1);
            Controls.Add(dataGridView1);
            Controls.Add(lbl_SaveBD);
            Controls.Add(lbl_Filtro);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            Name = "ConectorHerrajeGenerador";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Generar conector";
            Load += ConectorHerrajeGenerador_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            groupSaveConector.ResumeLayout(false);
            groupSaveConector.PerformLayout();
            group_Buscar.ResumeLayout(false);
            group_Buscar.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lbl_Filtro;
        private TextBox txt_Filtro;
        private CheckBox chk_Predefinido;
        private TextBox txt_ConectorName;
        private Label lbl_SaveBD;
        private Button btn_InsertConector;
        private Label lbl_SaveXML;
        private Button btn_GenerarConector;
        private DataGridView dataGridView1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lbl_Conexion;
        private ToolStripStatusLabel lbl_ConectorActivo;
        private GroupBox groupSaveConector;
        private GroupBox group_Buscar;
        private CheckBox chk_Balconeras;
        private CheckBox chk_Ventanas;
        private CheckBox chk_Correderas;
        private CheckBox chk_Puertas;
        private CheckBox chk_Abatibles;
        private CheckBox chk_Plegables;
        private CheckBox chk_Elevables;
        private CheckBox chk_Paralelas;
        private CheckBox chk_SelectAll;
        private Label lbl_Total;
    }
}