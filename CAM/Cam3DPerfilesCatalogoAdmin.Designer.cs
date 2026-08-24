namespace RotoTools
{
    partial class Cam3DPerfilesCatalogoAdmin
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Cam3DPerfilesCatalogoAdmin));
            lbl_TituloBiblioteca = new Label();
            btn_NuevoPerfil = new Button();
            lbl_BuscarBiblioteca = new Label();
            txt_BuscarBiblioteca = new TextBox();
            dataGridViewBiblioteca = new DataGridView();
            btn_Guardar = new Button();
            btn_Volver = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridViewBiblioteca).BeginInit();
            SuspendLayout();
            // 
            // lbl_TituloBiblioteca
            // 
            lbl_TituloBiblioteca.AutoSize = true;
            lbl_TituloBiblioteca.BackColor = Color.Transparent;
            lbl_TituloBiblioteca.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lbl_TituloBiblioteca.Location = new Point(12, 15);
            lbl_TituloBiblioteca.Name = "lbl_TituloBiblioteca";
            lbl_TituloBiblioteca.Size = new Size(196, 25);
            lbl_TituloBiblioteca.TabIndex = 0;
            lbl_TituloBiblioteca.Text = "Biblioteca de perfiles";
            // 
            // btn_NuevoPerfil
            // 
            btn_NuevoPerfil.BackColor = Color.White;
            btn_NuevoPerfil.Location = new Point(260, 10);
            btn_NuevoPerfil.Name = "btn_NuevoPerfil";
            btn_NuevoPerfil.Size = new Size(140, 32);
            btn_NuevoPerfil.TabIndex = 1;
            btn_NuevoPerfil.Text = "Nuevo perfil";
            btn_NuevoPerfil.UseVisualStyleBackColor = false;
            btn_NuevoPerfil.Click += btn_NuevoPerfil_Click;
            // 
            // lbl_BuscarBiblioteca
            // 
            lbl_BuscarBiblioteca.AutoSize = true;
            lbl_BuscarBiblioteca.BackColor = Color.Transparent;
            lbl_BuscarBiblioteca.Location = new Point(1000, 18);
            lbl_BuscarBiblioteca.Name = "lbl_BuscarBiblioteca";
            lbl_BuscarBiblioteca.Size = new Size(52, 20);
            lbl_BuscarBiblioteca.TabIndex = 2;
            lbl_BuscarBiblioteca.Text = "Buscar";
            // 
            // txt_BuscarBiblioteca
            // 
            txt_BuscarBiblioteca.Location = new Point(1060, 15);
            txt_BuscarBiblioteca.Name = "txt_BuscarBiblioteca";
            txt_BuscarBiblioteca.Size = new Size(130, 27);
            txt_BuscarBiblioteca.TabIndex = 3;
            txt_BuscarBiblioteca.TextChanged += txt_BuscarBiblioteca_TextChanged;
            // 
            // dataGridViewBiblioteca
            // 
            dataGridViewBiblioteca.AllowUserToAddRows = false;
            dataGridViewBiblioteca.AllowUserToDeleteRows = false;
            dataGridViewBiblioteca.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewBiblioteca.Location = new Point(12, 50);
            dataGridViewBiblioteca.MultiSelect = false;
            dataGridViewBiblioteca.Name = "dataGridViewBiblioteca";
            dataGridViewBiblioteca.RowHeadersWidth = 51;
            dataGridViewBiblioteca.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewBiblioteca.Size = new Size(1176, 500);
            dataGridViewBiblioteca.TabIndex = 4;
            //
            // btn_Guardar
            //
            btn_Guardar.BackColor = Color.White;
            btn_Guardar.Image = (Image)resources.GetObject("btn_Guardar.Image");
            btn_Guardar.ImageAlign = ContentAlignment.MiddleLeft;
            btn_Guardar.Location = new Point(1037, 565);
            btn_Guardar.Margin = new Padding(3, 4, 3, 4);
            btn_Guardar.Name = "btn_Guardar";
            btn_Guardar.Padding = new Padding(10, 0, 20, 0);
            btn_Guardar.Size = new Size(151, 53);
            btn_Guardar.TabIndex = 6;
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
            btn_Volver.Location = new Point(872, 565);
            btn_Volver.Margin = new Padding(3, 4, 3, 4);
            btn_Volver.Name = "btn_Volver";
            btn_Volver.Padding = new Padding(15, 0, 20, 0);
            btn_Volver.Size = new Size(151, 53);
            btn_Volver.TabIndex = 5;
            btn_Volver.Text = "Volver";
            btn_Volver.TextAlign = ContentAlignment.MiddleRight;
            btn_Volver.UseVisualStyleBackColor = false;
            btn_Volver.Click += btn_Volver_Click;
            // 
            // Cam3DPerfilesCatalogoAdmin
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1200, 630);
            Controls.Add(lbl_TituloBiblioteca);
            Controls.Add(btn_NuevoPerfil);
            Controls.Add(lbl_BuscarBiblioteca);
            Controls.Add(txt_BuscarBiblioteca);
            Controls.Add(dataGridViewBiblioteca);
            Controls.Add(btn_Guardar);
            Controls.Add(btn_Volver);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Cam3DPerfilesCatalogoAdmin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Biblioteca de perfiles";
            Load += Cam3DPerfilesCatalogoAdmin_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridViewBiblioteca).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lbl_TituloBiblioteca;
        private Button btn_NuevoPerfil;
        private Label lbl_BuscarBiblioteca;
        private TextBox txt_BuscarBiblioteca;
        private DataGridView dataGridViewBiblioteca;
        private Button btn_Guardar;
        private Button btn_Volver;
    }
}
