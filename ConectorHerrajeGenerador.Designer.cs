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
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // lbl_Filtro
            // 
            lbl_Filtro.AutoSize = true;
            lbl_Filtro.BackColor = Color.Transparent;
            lbl_Filtro.Location = new Point(894, 33);
            lbl_Filtro.Name = "lbl_Filtro";
            lbl_Filtro.Size = new Size(42, 15);
            lbl_Filtro.TabIndex = 25;
            lbl_Filtro.Text = "Buscar";
            // 
            // txt_Filtro
            // 
            txt_Filtro.Location = new Point(942, 30);
            txt_Filtro.Name = "txt_Filtro";
            txt_Filtro.Size = new Size(214, 23);
            txt_Filtro.TabIndex = 24;
            txt_Filtro.TextChanged += txt_Filtro_TextChanged;
            // 
            // chk_Predefinido
            // 
            chk_Predefinido.AutoSize = true;
            chk_Predefinido.BackColor = Color.Transparent;
            chk_Predefinido.Location = new Point(692, 32);
            chk_Predefinido.Name = "chk_Predefinido";
            chk_Predefinido.Size = new Size(155, 19);
            chk_Predefinido.TabIndex = 23;
            chk_Predefinido.Text = "Poner como predefinido";
            chk_Predefinido.UseVisualStyleBackColor = false;
            // 
            // txt_ConectorName
            // 
            txt_ConectorName.Location = new Point(512, 30);
            txt_ConectorName.Name = "txt_ConectorName";
            txt_ConectorName.Size = new Size(174, 23);
            txt_ConectorName.TabIndex = 22;
            // 
            // lbl_SaveBD
            // 
            lbl_SaveBD.AutoSize = true;
            lbl_SaveBD.BackColor = Color.Transparent;
            lbl_SaveBD.Location = new Point(400, 34);
            lbl_SaveBD.Name = "lbl_SaveBD";
            lbl_SaveBD.Size = new Size(98, 15);
            lbl_SaveBD.TabIndex = 29;
            lbl_SaveBD.Text = "Guardar en BBDD";
            // 
            // btn_InsertConector
            // 
            btn_InsertConector.BackgroundImage = (Image)resources.GetObject("btn_InsertConector.BackgroundImage");
            btn_InsertConector.BackgroundImageLayout = ImageLayout.Stretch;
            btn_InsertConector.Location = new Point(354, 21);
            btn_InsertConector.Margin = new Padding(3, 2, 3, 2);
            btn_InsertConector.Name = "btn_InsertConector";
            btn_InsertConector.Size = new Size(40, 40);
            btn_InsertConector.TabIndex = 28;
            btn_InsertConector.UseVisualStyleBackColor = true;
            btn_InsertConector.Click += btn_InsertConector_Click;
            // 
            // lbl_SaveXML
            // 
            lbl_SaveXML.AutoSize = true;
            lbl_SaveXML.BackColor = Color.Transparent;
            lbl_SaveXML.Location = new Point(121, 32);
            lbl_SaveXML.Name = "lbl_SaveXML";
            lbl_SaveXML.Size = new Size(92, 15);
            lbl_SaveXML.TabIndex = 27;
            lbl_SaveXML.Text = "Guardar en XML";
            // 
            // btn_GenerarConector
            // 
            btn_GenerarConector.BackgroundImage = (Image)resources.GetObject("btn_GenerarConector.BackgroundImage");
            btn_GenerarConector.BackgroundImageLayout = ImageLayout.Stretch;
            btn_GenerarConector.Location = new Point(75, 20);
            btn_GenerarConector.Margin = new Padding(3, 2, 3, 2);
            btn_GenerarConector.Name = "btn_GenerarConector";
            btn_GenerarConector.Size = new Size(40, 40);
            btn_GenerarConector.TabIndex = 26;
            btn_GenerarConector.UseVisualStyleBackColor = true;
            btn_GenerarConector.Click += btn_GenerarConector_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(41, 70);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(1115, 677);
            dataGridView1.TabIndex = 30;
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.Transparent;
            statusStrip1.Items.AddRange(new ToolStripItem[] { lbl_Conexion });
            statusStrip1.Location = new Point(0, 754);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1434, 22);
            statusStrip1.SizingGrip = false;
            statusStrip1.TabIndex = 31;
            statusStrip1.Text = "statusStrip1";
            // 
            // lbl_Conexion
            // 
            lbl_Conexion.Name = "lbl_Conexion";
            lbl_Conexion.Size = new Size(118, 17);
            lbl_Conexion.Text = "toolStripStatusLabel1";
            // 
            // ConectorHerrajeGenerador
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1434, 776);
            Controls.Add(statusStrip1);
            Controls.Add(dataGridView1);
            Controls.Add(lbl_SaveBD);
            Controls.Add(btn_InsertConector);
            Controls.Add(lbl_SaveXML);
            Controls.Add(btn_GenerarConector);
            Controls.Add(lbl_Filtro);
            Controls.Add(txt_Filtro);
            Controls.Add(chk_Predefinido);
            Controls.Add(txt_ConectorName);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "ConectorHerrajeGenerador";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Generar conector";
            Load += ConectorHerrajeGenerador_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
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
    }
}