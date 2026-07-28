namespace RotoTools
{
    partial class Cam3DCatalogoAdmin
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Cam3DCatalogoAdmin));
            lbl_TituloCatalogo = new Label();
            lbl_FiltroExterior = new Label();
            cmb_FiltroExterior = new ComboBox();
            lbl_FiltroRol = new Label();
            cmb_FiltroRol = new ComboBox();
            lbl_BuscarCatalogo = new Label();
            txt_BuscarCatalogo = new TextBox();
            dataGridViewCatalogo = new DataGridView();
            lbl_Faltantes = new Label();
            lbl_BuscarFaltantes = new Label();
            txt_BuscarFaltantes = new TextBox();
            dataGridViewFaltantes = new DataGridView();
            btn_Guardar = new Button();
            btn_Volver = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridViewCatalogo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewFaltantes).BeginInit();
            SuspendLayout();
            // 
            // lbl_TituloCatalogo
            // 
            lbl_TituloCatalogo.AutoSize = true;
            lbl_TituloCatalogo.BackColor = Color.Transparent;
            lbl_TituloCatalogo.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lbl_TituloCatalogo.Location = new Point(12, 41);
            lbl_TituloCatalogo.Name = "lbl_TituloCatalogo";
            lbl_TituloCatalogo.Size = new Size(425, 25);
            lbl_TituloCatalogo.TabIndex = 0;
            lbl_TituloCatalogo.Text = "Catálogo actual (CatalogoOperaciones3D.json)";
            //
            // lbl_FiltroExterior
            //
            lbl_FiltroExterior.AutoSize = true;
            lbl_FiltroExterior.BackColor = Color.Transparent;
            lbl_FiltroExterior.Location = new Point(480, 41);
            lbl_FiltroExterior.Name = "lbl_FiltroExterior";
            lbl_FiltroExterior.Size = new Size(65, 20);
            lbl_FiltroExterior.TabIndex = 11;
            lbl_FiltroExterior.Text = "Exterior:";
            //
            // cmb_FiltroExterior
            //
            cmb_FiltroExterior.Location = new Point(550, 38);
            cmb_FiltroExterior.Name = "cmb_FiltroExterior";
            cmb_FiltroExterior.Size = new Size(110, 28);
            cmb_FiltroExterior.TabIndex = 12;
            //
            // lbl_FiltroRol
            //
            lbl_FiltroRol.AutoSize = true;
            lbl_FiltroRol.BackColor = Color.Transparent;
            lbl_FiltroRol.Location = new Point(675, 41);
            lbl_FiltroRol.Name = "lbl_FiltroRol";
            lbl_FiltroRol.Size = new Size(35, 20);
            lbl_FiltroRol.TabIndex = 13;
            lbl_FiltroRol.Text = "Rol:";
            //
            // cmb_FiltroRol
            //
            cmb_FiltroRol.Location = new Point(715, 38);
            cmb_FiltroRol.Name = "cmb_FiltroRol";
            cmb_FiltroRol.Size = new Size(180, 28);
            cmb_FiltroRol.TabIndex = 14;
            //
            // lbl_BuscarCatalogo
            // 
            lbl_BuscarCatalogo.AutoSize = true;
            lbl_BuscarCatalogo.BackColor = Color.Transparent;
            lbl_BuscarCatalogo.Location = new Point(1416, 41);
            lbl_BuscarCatalogo.Name = "lbl_BuscarCatalogo";
            lbl_BuscarCatalogo.Size = new Size(52, 20);
            lbl_BuscarCatalogo.TabIndex = 7;
            lbl_BuscarCatalogo.Text = "Buscar";
            // 
            // txt_BuscarCatalogo
            // 
            txt_BuscarCatalogo.Location = new Point(1512, 38);
            txt_BuscarCatalogo.Name = "txt_BuscarCatalogo";
            txt_BuscarCatalogo.Size = new Size(160, 27);
            txt_BuscarCatalogo.TabIndex = 8;
            txt_BuscarCatalogo.TextChanged += txt_BuscarCatalogo_TextChanged;
            // 
            // dataGridViewCatalogo
            // 
            dataGridViewCatalogo.AllowUserToAddRows = false;
            dataGridViewCatalogo.AllowUserToDeleteRows = false;
            dataGridViewCatalogo.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCatalogo.Location = new Point(12, 74);
            dataGridViewCatalogo.MultiSelect = false;
            dataGridViewCatalogo.Name = "dataGridViewCatalogo";
            dataGridViewCatalogo.ReadOnly = true;
            dataGridViewCatalogo.RowHeadersWidth = 51;
            dataGridViewCatalogo.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewCatalogo.Size = new Size(1660, 420);
            dataGridViewCatalogo.TabIndex = 1;
            // 
            // lbl_Faltantes
            // 
            lbl_Faltantes.AutoSize = true;
            lbl_Faltantes.BackColor = Color.Transparent;
            lbl_Faltantes.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lbl_Faltantes.Location = new Point(12, 551);
            lbl_Faltantes.Name = "lbl_Faltantes";
            lbl_Faltantes.Size = new Size(500, 25);
            lbl_Faltantes.TabIndex = 2;
            lbl_Faltantes.Text = "Operaciones seleccionadas sin definición en el catálogo";
            // 
            // lbl_BuscarFaltantes
            // 
            lbl_BuscarFaltantes.AutoSize = true;
            lbl_BuscarFaltantes.BackColor = Color.Transparent;
            lbl_BuscarFaltantes.Location = new Point(1416, 554);
            lbl_BuscarFaltantes.Name = "lbl_BuscarFaltantes";
            lbl_BuscarFaltantes.Size = new Size(52, 20);
            lbl_BuscarFaltantes.TabIndex = 9;
            lbl_BuscarFaltantes.Text = "Buscar";
            // 
            // txt_BuscarFaltantes
            // 
            txt_BuscarFaltantes.Location = new Point(1512, 551);
            txt_BuscarFaltantes.Name = "txt_BuscarFaltantes";
            txt_BuscarFaltantes.Size = new Size(160, 27);
            txt_BuscarFaltantes.TabIndex = 10;
            txt_BuscarFaltantes.TextChanged += txt_BuscarFaltantes_TextChanged;
            // 
            // dataGridViewFaltantes
            // 
            dataGridViewFaltantes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewFaltantes.EditMode = DataGridViewEditMode.EditOnEnter;
            dataGridViewFaltantes.Location = new Point(12, 584);
            dataGridViewFaltantes.Name = "dataGridViewFaltantes";
            dataGridViewFaltantes.RowHeadersWidth = 51;
            dataGridViewFaltantes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewFaltantes.Size = new Size(1660, 330);
            dataGridViewFaltantes.TabIndex = 3;
            //
            // btn_Guardar
            //
            btn_Guardar.BackColor = Color.White;
            btn_Guardar.Image = (Image)resources.GetObject("btn_Guardar.Image");
            btn_Guardar.ImageAlign = ContentAlignment.MiddleLeft;
            btn_Guardar.Location = new Point(1521, 944);
            btn_Guardar.Margin = new Padding(3, 4, 3, 4);
            btn_Guardar.Name = "btn_Guardar";
            btn_Guardar.Padding = new Padding(10, 0, 20, 0);
            btn_Guardar.Size = new Size(151, 53);
            btn_Guardar.TabIndex = 5;
            btn_Guardar.Text = "Guardar";
            btn_Guardar.TextAlign = ContentAlignment.MiddleRight;
            btn_Guardar.UseVisualStyleBackColor = false;
            btn_Guardar.Click += btn_Guardar_Click;
            // 
            // btn_Volver
            // 
            btn_Volver.BackColor = Color.White;
            btn_Volver.Image = (Image)resources.GetObject("btn_Volver.Image");
            btn_Volver.ImageAlign = ContentAlignment.MiddleLeft;
            btn_Volver.Location = new Point(1355, 944);
            btn_Volver.Margin = new Padding(3, 4, 3, 4);
            btn_Volver.Name = "btn_Volver";
            btn_Volver.Padding = new Padding(15, 0, 20, 0);
            btn_Volver.Size = new Size(151, 53);
            btn_Volver.TabIndex = 6;
            btn_Volver.Text = "Volver";
            btn_Volver.TextAlign = ContentAlignment.MiddleRight;
            btn_Volver.UseVisualStyleBackColor = false;
            btn_Volver.Click += btn_Volver_Click;
            // 
            // Cam3DCatalogoAdmin
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1729, 1011);
            Controls.Add(lbl_TituloCatalogo);
            Controls.Add(lbl_FiltroExterior);
            Controls.Add(cmb_FiltroExterior);
            Controls.Add(lbl_FiltroRol);
            Controls.Add(cmb_FiltroRol);
            Controls.Add(lbl_BuscarCatalogo);
            Controls.Add(txt_BuscarCatalogo);
            Controls.Add(dataGridViewCatalogo);
            Controls.Add(lbl_Faltantes);
            Controls.Add(lbl_BuscarFaltantes);
            Controls.Add(txt_BuscarFaltantes);
            Controls.Add(dataGridViewFaltantes);
            Controls.Add(btn_Guardar);
            Controls.Add(btn_Volver);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Cam3DCatalogoAdmin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Catálogo de operaciones 3D";
            Load += Cam3DCatalogoAdmin_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridViewCatalogo).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewFaltantes).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lbl_TituloCatalogo;
        private Label lbl_FiltroExterior;
        private ComboBox cmb_FiltroExterior;
        private Label lbl_FiltroRol;
        private ComboBox cmb_FiltroRol;
        private Label lbl_BuscarCatalogo;
        private TextBox txt_BuscarCatalogo;
        private DataGridView dataGridViewCatalogo;
        private Label lbl_Faltantes;
        private Label lbl_BuscarFaltantes;
        private TextBox txt_BuscarFaltantes;
        private DataGridView dataGridViewFaltantes;
        private Button btn_Guardar;
        private Button btn_Volver;
    }
}
