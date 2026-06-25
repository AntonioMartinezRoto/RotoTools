namespace RotoTools
{
    partial class ExportacionWinPerfil
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportacionWinPerfil));
            lbl_Profile = new Label();
            lbl_Colour = new Label();
            lbl_System = new Label();
            cmb_Perfil = new ComboBox();
            cmb_Sistema = new ComboBox();
            cmb_Color = new ComboBox();
            chk_All = new CheckBox();
            txt_filter = new TextBox();
            lbl_Busqueda = new Label();
            btn_ExportSets = new Button();
            chkList_Sets = new CheckedListBox();
            progress_Export = new ProgressBar();
            btn_FiltrarPerfil = new Button();
            btn_FiltrarPerfilAlu = new Button();
            cmb_PerfilAlu = new ComboBox();
            lbl_ProfileAlu = new Label();
            SuspendLayout();
            // 
            // lbl_Profile
            // 
            lbl_Profile.AutoSize = true;
            lbl_Profile.BackColor = Color.Transparent;
            lbl_Profile.Location = new Point(43, 39);
            lbl_Profile.Name = "lbl_Profile";
            lbl_Profile.Size = new Size(42, 20);
            lbl_Profile.TabIndex = 0;
            lbl_Profile.Text = "Perfil";
            // 
            // lbl_Colour
            // 
            lbl_Colour.AutoSize = true;
            lbl_Colour.BackColor = Color.Transparent;
            lbl_Colour.Location = new Point(43, 143);
            lbl_Colour.Name = "lbl_Colour";
            lbl_Colour.Size = new Size(45, 20);
            lbl_Colour.TabIndex = 1;
            lbl_Colour.Text = "Color";
            // 
            // lbl_System
            // 
            lbl_System.AutoSize = true;
            lbl_System.BackColor = Color.Transparent;
            lbl_System.Location = new Point(43, 109);
            lbl_System.Name = "lbl_System";
            lbl_System.Size = new Size(61, 20);
            lbl_System.TabIndex = 2;
            lbl_System.Text = "Sistema";
            // 
            // cmb_Perfil
            // 
            cmb_Perfil.FormattingEnabled = true;
            cmb_Perfil.Location = new Point(125, 36);
            cmb_Perfil.Margin = new Padding(3, 4, 3, 4);
            cmb_Perfil.Name = "cmb_Perfil";
            cmb_Perfil.Size = new Size(313, 28);
            cmb_Perfil.TabIndex = 3;
            // 
            // cmb_Sistema
            // 
            cmb_Sistema.FormattingEnabled = true;
            cmb_Sistema.Location = new Point(125, 105);
            cmb_Sistema.Margin = new Padding(3, 4, 3, 4);
            cmb_Sistema.Name = "cmb_Sistema";
            cmb_Sistema.Size = new Size(354, 28);
            cmb_Sistema.TabIndex = 4;
            // 
            // cmb_Color
            // 
            cmb_Color.FormattingEnabled = true;
            cmb_Color.Location = new Point(125, 140);
            cmb_Color.Margin = new Padding(3, 4, 3, 4);
            cmb_Color.Name = "cmb_Color";
            cmb_Color.Size = new Size(355, 28);
            cmb_Color.TabIndex = 5;
            // 
            // chk_All
            // 
            chk_All.AutoSize = true;
            chk_All.BackColor = Color.Transparent;
            chk_All.Font = new Font("Segoe UI", 9F);
            chk_All.Location = new Point(43, 187);
            chk_All.Name = "chk_All";
            chk_All.Size = new Size(149, 24);
            chk_All.TabIndex = 8;
            chk_All.Text = "Seleccionar todos";
            chk_All.UseVisualStyleBackColor = false;
            chk_All.CheckedChanged += chk_All_CheckedChanged;
            // 
            // txt_filter
            // 
            txt_filter.Location = new Point(262, 184);
            txt_filter.Name = "txt_filter";
            txt_filter.Size = new Size(217, 27);
            txt_filter.TabIndex = 11;
            txt_filter.TextChanged += txt_filter_TextChanged;
            // 
            // lbl_Busqueda
            // 
            lbl_Busqueda.AutoSize = true;
            lbl_Busqueda.BackColor = Color.Transparent;
            lbl_Busqueda.Location = new Point(210, 187);
            lbl_Busqueda.Name = "lbl_Busqueda";
            lbl_Busqueda.Size = new Size(52, 20);
            lbl_Busqueda.TabIndex = 10;
            lbl_Busqueda.Text = "Buscar";
            // 
            // btn_ExportSets
            // 
            btn_ExportSets.BackColor = Color.Transparent;
            btn_ExportSets.BackgroundImage = (Image)resources.GetObject("btn_ExportSets.BackgroundImage");
            btn_ExportSets.BackgroundImageLayout = ImageLayout.Center;
            btn_ExportSets.Location = new Point(499, 35);
            btn_ExportSets.Name = "btn_ExportSets";
            btn_ExportSets.Size = new Size(104, 141);
            btn_ExportSets.TabIndex = 12;
            btn_ExportSets.UseVisualStyleBackColor = false;
            btn_ExportSets.Click += btn_ExportSets_Click;
            // 
            // chkList_Sets
            // 
            chkList_Sets.FormattingEnabled = true;
            chkList_Sets.Location = new Point(45, 250);
            chkList_Sets.Name = "chkList_Sets";
            chkList_Sets.Size = new Size(558, 708);
            chkList_Sets.TabIndex = 14;
            // 
            // progress_Export
            // 
            progress_Export.Location = new Point(43, 217);
            progress_Export.Name = "progress_Export";
            progress_Export.Size = new Size(560, 29);
            progress_Export.TabIndex = 13;
            // 
            // btn_FiltrarPerfil
            // 
            btn_FiltrarPerfil.BackColor = Color.Transparent;
            btn_FiltrarPerfil.BackgroundImage = (Image)resources.GetObject("btn_FiltrarPerfil.BackgroundImage");
            btn_FiltrarPerfil.BackgroundImageLayout = ImageLayout.Center;
            btn_FiltrarPerfil.Location = new Point(445, 35);
            btn_FiltrarPerfil.Name = "btn_FiltrarPerfil";
            btn_FiltrarPerfil.Size = new Size(34, 32);
            btn_FiltrarPerfil.TabIndex = 15;
            btn_FiltrarPerfil.UseVisualStyleBackColor = false;
            btn_FiltrarPerfil.Click += btn_FiltrarPerfil_Click;
            // 
            // btn_FiltrarPerfilAlu
            // 
            btn_FiltrarPerfilAlu.BackColor = Color.Transparent;
            btn_FiltrarPerfilAlu.BackgroundImage = (Image)resources.GetObject("btn_FiltrarPerfilAlu.BackgroundImage");
            btn_FiltrarPerfilAlu.BackgroundImageLayout = ImageLayout.Center;
            btn_FiltrarPerfilAlu.Location = new Point(445, 70);
            btn_FiltrarPerfilAlu.Name = "btn_FiltrarPerfilAlu";
            btn_FiltrarPerfilAlu.Size = new Size(34, 32);
            btn_FiltrarPerfilAlu.TabIndex = 18;
            btn_FiltrarPerfilAlu.UseVisualStyleBackColor = false;
            btn_FiltrarPerfilAlu.Click += btn_FiltrarPerfilAlu_Click;
            // 
            // cmb_PerfilAlu
            // 
            cmb_PerfilAlu.FormattingEnabled = true;
            cmb_PerfilAlu.Location = new Point(125, 71);
            cmb_PerfilAlu.Margin = new Padding(3, 4, 3, 4);
            cmb_PerfilAlu.Name = "cmb_PerfilAlu";
            cmb_PerfilAlu.Size = new Size(313, 28);
            cmb_PerfilAlu.TabIndex = 17;
            // 
            // lbl_ProfileAlu
            // 
            lbl_ProfileAlu.AutoSize = true;
            lbl_ProfileAlu.BackColor = Color.Transparent;
            lbl_ProfileAlu.Location = new Point(43, 74);
            lbl_ProfileAlu.Name = "lbl_ProfileAlu";
            lbl_ProfileAlu.Size = new Size(73, 20);
            lbl_ProfileAlu.TabIndex = 16;
            lbl_ProfileAlu.Text = "Perfil ALU";
            // 
            // ExportacionWinPerfil
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(651, 1024);
            Controls.Add(btn_FiltrarPerfilAlu);
            Controls.Add(cmb_PerfilAlu);
            Controls.Add(lbl_ProfileAlu);
            Controls.Add(btn_FiltrarPerfil);
            Controls.Add(chkList_Sets);
            Controls.Add(progress_Export);
            Controls.Add(btn_ExportSets);
            Controls.Add(txt_filter);
            Controls.Add(lbl_Busqueda);
            Controls.Add(chk_All);
            Controls.Add(cmb_Color);
            Controls.Add(cmb_Sistema);
            Controls.Add(cmb_Perfil);
            Controls.Add(lbl_System);
            Controls.Add(lbl_Colour);
            Controls.Add(lbl_Profile);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            Name = "ExportacionWinPerfil";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Exportación a WinPerfil";
            Load += WinPerfilExportForm_Load;
            KeyDown += ExportacionWinPerfil_KeyDown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lbl_Profile;
        private Label lbl_Colour;
        private Label lbl_System;
        private ComboBox cmb_Perfil;
        private ComboBox cmb_Sistema;
        private ComboBox cmb_Color;
        private CheckBox chk_All;
        private TextBox txt_filter;
        private Label lbl_Busqueda;
        private Button btn_ExportSets;
        private CheckedListBox chkList_Sets;
        private ProgressBar progress_Export;
        private Button btn_FiltrarPerfil;
        private Button btn_FiltrarPerfilAlu;
        private ComboBox cmb_PerfilAlu;
        private Label lbl_ProfileAlu;
    }
}