namespace RotoTools
{
    partial class ActualizadorInstalarEscandallos
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ActualizadorInstalarEscandallos));
            btn_InstalarEscandallos = new Button();
            chk_Alu = new CheckBox();
            chk_PVC = new CheckBox();
            chk_Manillas = new CheckBox();
            chk_Bombillos = new CheckBox();
            chk_Customizations = new CheckBox();
            chk_GestionGeneral = new CheckBox();
            toolTipEscandallos = new ToolTip(components);
            groupBoxEscandallos = new GroupBox();
            chk_SelectAll = new CheckBox();
            groupBoxManual = new GroupBox();
            lbl_SelectScripts = new Label();
            btn_FiltrarEscandallos = new Button();
            groupBoxEscandallos.SuspendLayout();
            groupBoxManual.SuspendLayout();
            SuspendLayout();
            // 
            // btn_InstalarEscandallos
            // 
            btn_InstalarEscandallos.BackColor = Color.White;
            btn_InstalarEscandallos.Image = (Image)resources.GetObject("btn_InstalarEscandallos.Image");
            btn_InstalarEscandallos.ImageAlign = ContentAlignment.MiddleLeft;
            btn_InstalarEscandallos.Location = new Point(306, 248);
            btn_InstalarEscandallos.Name = "btn_InstalarEscandallos";
            btn_InstalarEscandallos.Padding = new Padding(0, 0, 5, 0);
            btn_InstalarEscandallos.Size = new Size(102, 40);
            btn_InstalarEscandallos.TabIndex = 10;
            btn_InstalarEscandallos.Text = "Instalar";
            btn_InstalarEscandallos.TextAlign = ContentAlignment.MiddleRight;
            btn_InstalarEscandallos.UseVisualStyleBackColor = false;
            btn_InstalarEscandallos.Click += btn_InstalarEscandallos_Click;
            // 
            // chk_Alu
            // 
            chk_Alu.AutoSize = true;
            chk_Alu.BackColor = Color.Transparent;
            chk_Alu.Location = new Point(26, 81);
            chk_Alu.Name = "chk_Alu";
            chk_Alu.Size = new Size(149, 19);
            chk_Alu.TabIndex = 11;
            chk_Alu.Text = "Constructivos aluminio";
            chk_Alu.UseVisualStyleBackColor = false;
            // 
            // chk_PVC
            // 
            chk_PVC.AutoSize = true;
            chk_PVC.BackColor = Color.Transparent;
            chk_PVC.Location = new Point(26, 56);
            chk_PVC.Name = "chk_PVC";
            chk_PVC.Size = new Size(124, 19);
            chk_PVC.TabIndex = 12;
            chk_PVC.Text = "Constructivos PVC";
            chk_PVC.UseVisualStyleBackColor = false;
            // 
            // chk_Manillas
            // 
            chk_Manillas.AutoSize = true;
            chk_Manillas.BackColor = Color.Transparent;
            chk_Manillas.Location = new Point(217, 31);
            chk_Manillas.Name = "chk_Manillas";
            chk_Manillas.Size = new Size(113, 19);
            chk_Manillas.TabIndex = 13;
            chk_Manillas.Text = "Gestión Manillas";
            chk_Manillas.UseVisualStyleBackColor = false;
            // 
            // chk_Bombillos
            // 
            chk_Bombillos.AutoSize = true;
            chk_Bombillos.BackColor = Color.Transparent;
            chk_Bombillos.Location = new Point(217, 56);
            chk_Bombillos.Name = "chk_Bombillos";
            chk_Bombillos.Size = new Size(122, 19);
            chk_Bombillos.TabIndex = 14;
            chk_Bombillos.Text = "Gestión Bombillos";
            chk_Bombillos.UseVisualStyleBackColor = false;
            // 
            // chk_Customizations
            // 
            chk_Customizations.AutoSize = true;
            chk_Customizations.BackColor = Color.Transparent;
            chk_Customizations.Location = new Point(217, 81);
            chk_Customizations.Name = "chk_Customizations";
            chk_Customizations.Size = new Size(151, 19);
            chk_Customizations.TabIndex = 15;
            chk_Customizations.Text = "Personalización clientes";
            chk_Customizations.UseVisualStyleBackColor = false;
            // 
            // chk_GestionGeneral
            // 
            chk_GestionGeneral.AutoSize = true;
            chk_GestionGeneral.BackColor = Color.Transparent;
            chk_GestionGeneral.Location = new Point(26, 31);
            chk_GestionGeneral.Name = "chk_GestionGeneral";
            chk_GestionGeneral.Size = new Size(108, 19);
            chk_GestionGeneral.TabIndex = 16;
            chk_GestionGeneral.Text = "Gestión general";
            chk_GestionGeneral.UseVisualStyleBackColor = false;
            // 
            // groupBoxEscandallos
            // 
            groupBoxEscandallos.BackColor = Color.Transparent;
            groupBoxEscandallos.Controls.Add(chk_Customizations);
            groupBoxEscandallos.Controls.Add(chk_GestionGeneral);
            groupBoxEscandallos.Controls.Add(chk_Alu);
            groupBoxEscandallos.Controls.Add(chk_PVC);
            groupBoxEscandallos.Controls.Add(chk_Bombillos);
            groupBoxEscandallos.Controls.Add(chk_Manillas);
            groupBoxEscandallos.Location = new Point(22, 46);
            groupBoxEscandallos.Name = "groupBoxEscandallos";
            groupBoxEscandallos.Size = new Size(386, 113);
            groupBoxEscandallos.TabIndex = 17;
            groupBoxEscandallos.TabStop = false;
            groupBoxEscandallos.Text = "Grupos";
            // 
            // chk_SelectAll
            // 
            chk_SelectAll.AutoSize = true;
            chk_SelectAll.BackColor = Color.Transparent;
            chk_SelectAll.Location = new Point(28, 46);
            chk_SelectAll.Name = "chk_SelectAll";
            chk_SelectAll.Size = new Size(177, 19);
            chk_SelectAll.TabIndex = 17;
            chk_SelectAll.Text = "Seleccionar todos los grupos";
            chk_SelectAll.UseVisualStyleBackColor = false;
            chk_SelectAll.CheckedChanged += chk_SelectAll_CheckedChanged;
            // 
            // groupBoxManual
            // 
            groupBoxManual.BackColor = Color.Transparent;
            groupBoxManual.Controls.Add(lbl_SelectScripts);
            groupBoxManual.Controls.Add(btn_FiltrarEscandallos);
            groupBoxManual.Location = new Point(23, 175);
            groupBoxManual.Name = "groupBoxManual";
            groupBoxManual.Size = new Size(384, 57);
            groupBoxManual.TabIndex = 18;
            groupBoxManual.TabStop = false;
            groupBoxManual.Text = "Instalación individualizada";
            // 
            // lbl_SelectScripts
            // 
            lbl_SelectScripts.AutoSize = true;
            lbl_SelectScripts.Location = new Point(52, 27);
            lbl_SelectScripts.Name = "lbl_SelectScripts";
            lbl_SelectScripts.Size = new Size(226, 15);
            lbl_SelectScripts.TabIndex = 15;
            lbl_SelectScripts.Text = "Seleccionar manualmente los escandallos";
            // 
            // btn_FiltrarEscandallos
            // 
            btn_FiltrarEscandallos.BackColor = Color.Transparent;
            btn_FiltrarEscandallos.BackgroundImage = (Image)resources.GetObject("btn_FiltrarEscandallos.BackgroundImage");
            btn_FiltrarEscandallos.BackgroundImageLayout = ImageLayout.Center;
            btn_FiltrarEscandallos.Location = new Point(16, 21);
            btn_FiltrarEscandallos.Margin = new Padding(3, 2, 3, 2);
            btn_FiltrarEscandallos.Name = "btn_FiltrarEscandallos";
            btn_FiltrarEscandallos.Size = new Size(30, 26);
            btn_FiltrarEscandallos.TabIndex = 14;
            btn_FiltrarEscandallos.UseVisualStyleBackColor = false;
            btn_FiltrarEscandallos.Click += btn_FiltrarEscandallos_Click;
            // 
            // ActualizadorInstalarEscandallos
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(518, 307);
            Controls.Add(groupBoxManual);
            Controls.Add(chk_SelectAll);
            Controls.Add(groupBoxEscandallos);
            Controls.Add(btn_InstalarEscandallos);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "ActualizadorInstalarEscandallos";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Instalar Escandallos";
            Load += ActualizadorInstalarEscandallos_Load;
            groupBoxEscandallos.ResumeLayout(false);
            groupBoxEscandallos.PerformLayout();
            groupBoxManual.ResumeLayout(false);
            groupBoxManual.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btn_InstalarEscandallos;
        private CheckBox chk_Alu;
        private CheckBox chk_PVC;
        private CheckBox chk_Manillas;
        private CheckBox chk_Bombillos;
        private CheckBox chk_Customizations;
        private CheckBox chk_GestionGeneral;
        private ToolTip toolTipEscandallos;
        private GroupBox groupBoxEscandallos;
        private CheckBox chk_SelectAll;
        private GroupBox groupBoxManual;
        private Button btn_FiltrarEscandallos;
        private Label lbl_SelectScripts;
    }
}