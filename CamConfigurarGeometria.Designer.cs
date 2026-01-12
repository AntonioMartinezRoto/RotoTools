namespace RotoTools
{
    partial class CamConfigurarGeometria
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CamConfigurarGeometria));
            btn_Save = new Button();
            dataGridView1 = new DataGridView();
            cmb_Primitivas = new ComboBox();
            label2 = new Label();
            btn_AddPrimitiva = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // btn_Save
            // 
            btn_Save.Image = (Image)resources.GetObject("btn_Save.Image");
            btn_Save.Location = new Point(429, 294);
            btn_Save.Margin = new Padding(0);
            btn_Save.Name = "btn_Save";
            btn_Save.Size = new Size(87, 41);
            btn_Save.TabIndex = 8;
            btn_Save.Text = "Guardar";
            btn_Save.TextAlign = ContentAlignment.MiddleRight;
            btn_Save.TextImageRelation = TextImageRelation.ImageBeforeText;
            btn_Save.UseVisualStyleBackColor = true;
            btn_Save.Click += btn_Save_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(35, 85);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(481, 192);
            dataGridView1.TabIndex = 31;
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
            // 
            // cmb_Primitivas
            // 
            cmb_Primitivas.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb_Primitivas.FormattingEnabled = true;
            cmb_Primitivas.Location = new Point(172, 42);
            cmb_Primitivas.Name = "cmb_Primitivas";
            cmb_Primitivas.Size = new Size(206, 23);
            cmb_Primitivas.TabIndex = 33;
            // 
            // label2
            // 
            label2.BackColor = Color.Transparent;
            label2.Location = new Point(35, 45);
            label2.Name = "label2";
            label2.Size = new Size(86, 20);
            label2.TabIndex = 32;
            label2.Text = "Operación";
            // 
            // btn_AddPrimitiva
            // 
            btn_AddPrimitiva.BackgroundImage = (Image)resources.GetObject("btn_AddPrimitiva.BackgroundImage");
            btn_AddPrimitiva.BackgroundImageLayout = ImageLayout.Stretch;
            btn_AddPrimitiva.Location = new Point(384, 43);
            btn_AddPrimitiva.Name = "btn_AddPrimitiva";
            btn_AddPrimitiva.Size = new Size(25, 22);
            btn_AddPrimitiva.TabIndex = 34;
            btn_AddPrimitiva.UseVisualStyleBackColor = true;
            btn_AddPrimitiva.Click += btn_AddPrimitiva_Click;
            // 
            // CamConfigurarGeometria
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(664, 360);
            Controls.Add(btn_AddPrimitiva);
            Controls.Add(cmb_Primitivas);
            Controls.Add(label2);
            Controls.Add(dataGridView1);
            Controls.Add(btn_Save);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "CamConfigurarGeometria";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CAM";
            Load += CamConfigurarGeometria_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button btn_Save;
        private DataGridView dataGridView1;
        private ComboBox cmb_Primitivas;
        private Label label2;
        private Button btn_AddPrimitiva;
    }
}