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
            listBox_Condiciones = new ListBox();
            btn_ExportConditions = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // btn_Save
            // 
            btn_Save.Image = (Image)resources.GetObject("btn_Save.Image");
            btn_Save.Location = new Point(581, 427);
            btn_Save.Margin = new Padding(0);
            btn_Save.Name = "btn_Save";
            btn_Save.Size = new Size(99, 55);
            btn_Save.TabIndex = 8;
            btn_Save.Text = "Guardar";
            btn_Save.TextAlign = ContentAlignment.MiddleRight;
            btn_Save.TextImageRelation = TextImageRelation.ImageBeforeText;
            btn_Save.UseVisualStyleBackColor = true;
            btn_Save.Visible = false;
            btn_Save.Click += btn_Save_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(215, 71);
            dataGridView1.Margin = new Padding(3, 4, 3, 4);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.Size = new Size(465, 325);
            dataGridView1.TabIndex = 31;
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
            // 
            // cmb_Primitivas
            // 
            cmb_Primitivas.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb_Primitivas.FormattingEnabled = true;
            cmb_Primitivas.Location = new Point(409, 32);
            cmb_Primitivas.Margin = new Padding(3, 4, 3, 4);
            cmb_Primitivas.Name = "cmb_Primitivas";
            cmb_Primitivas.Size = new Size(235, 28);
            cmb_Primitivas.TabIndex = 33;
            cmb_Primitivas.Visible = false;
            // 
            // label2
            // 
            label2.BackColor = Color.Transparent;
            label2.Location = new Point(294, 36);
            label2.Name = "label2";
            label2.Size = new Size(98, 27);
            label2.TabIndex = 32;
            label2.Text = "Operación";
            label2.Visible = false;
            // 
            // btn_AddPrimitiva
            // 
            btn_AddPrimitiva.BackgroundImage = (Image)resources.GetObject("btn_AddPrimitiva.BackgroundImage");
            btn_AddPrimitiva.BackgroundImageLayout = ImageLayout.Stretch;
            btn_AddPrimitiva.Location = new Point(651, 33);
            btn_AddPrimitiva.Margin = new Padding(3, 4, 3, 4);
            btn_AddPrimitiva.Name = "btn_AddPrimitiva";
            btn_AddPrimitiva.Size = new Size(29, 29);
            btn_AddPrimitiva.TabIndex = 34;
            btn_AddPrimitiva.UseVisualStyleBackColor = true;
            btn_AddPrimitiva.Visible = false;
            btn_AddPrimitiva.Click += btn_AddPrimitiva_Click;
            // 
            // listBox_Condiciones
            // 
            listBox_Condiciones.FormattingEnabled = true;
            listBox_Condiciones.Location = new Point(31, 71);
            listBox_Condiciones.Margin = new Padding(3, 4, 3, 4);
            listBox_Condiciones.Name = "listBox_Condiciones";
            listBox_Condiciones.Size = new Size(177, 324);
            listBox_Condiciones.TabIndex = 35;
            listBox_Condiciones.SelectedIndexChanged += listBox_Condiciones_SelectedIndexChanged;
            // 
            // btn_ExportConditions
            // 
            btn_ExportConditions.BackgroundImage = (Image)resources.GetObject("btn_ExportConditions.BackgroundImage");
            btn_ExportConditions.BackgroundImageLayout = ImageLayout.Stretch;
            btn_ExportConditions.Location = new Point(31, 36);
            btn_ExportConditions.Margin = new Padding(3, 4, 3, 4);
            btn_ExportConditions.Name = "btn_ExportConditions";
            btn_ExportConditions.Size = new Size(29, 29);
            btn_ExportConditions.TabIndex = 36;
            btn_ExportConditions.UseVisualStyleBackColor = true;
            btn_ExportConditions.Visible = false;
            btn_ExportConditions.Click += btn_ExportConditions_Click;
            // 
            // CamConfigurarGeometria
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(845, 513);
            Controls.Add(btn_ExportConditions);
            Controls.Add(listBox_Condiciones);
            Controls.Add(btn_AddPrimitiva);
            Controls.Add(cmb_Primitivas);
            Controls.Add(label2);
            Controls.Add(dataGridView1);
            Controls.Add(btn_Save);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
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
        private ListBox listBox_Condiciones;
        private Button btn_ExportConditions;
    }
}