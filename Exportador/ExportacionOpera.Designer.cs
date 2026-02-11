namespace RotoTools
{
    partial class ExportacionOpera
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportacionOpera));
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
            SuspendLayout();
            // 
            // lbl_Profile
            // 
            lbl_Profile.AutoSize = true;
            lbl_Profile.BackColor = Color.Transparent;
            lbl_Profile.Location = new Point(38, 29);
            lbl_Profile.Name = "lbl_Profile";
            lbl_Profile.Size = new Size(34, 15);
            lbl_Profile.TabIndex = 0;
            lbl_Profile.Text = "Perfil";
            // 
            // lbl_Colour
            // 
            lbl_Colour.AutoSize = true;
            lbl_Colour.BackColor = Color.Transparent;
            lbl_Colour.Location = new Point(38, 80);
            lbl_Colour.Name = "lbl_Colour";
            lbl_Colour.Size = new Size(36, 15);
            lbl_Colour.TabIndex = 1;
            lbl_Colour.Text = "Color";
            // 
            // lbl_System
            // 
            lbl_System.AutoSize = true;
            lbl_System.BackColor = Color.Transparent;
            lbl_System.Location = new Point(38, 55);
            lbl_System.Name = "lbl_System";
            lbl_System.Size = new Size(48, 15);
            lbl_System.TabIndex = 2;
            lbl_System.Text = "Sistema";
            // 
            // cmb_Perfil
            // 
            cmb_Perfil.FormattingEnabled = true;
            cmb_Perfil.Location = new Point(109, 27);
            cmb_Perfil.Name = "cmb_Perfil";
            cmb_Perfil.Size = new Size(311, 23);
            cmb_Perfil.TabIndex = 3;
            // 
            // cmb_Sistema
            // 
            cmb_Sistema.FormattingEnabled = true;
            cmb_Sistema.Location = new Point(109, 52);
            cmb_Sistema.Name = "cmb_Sistema";
            cmb_Sistema.Size = new Size(311, 23);
            cmb_Sistema.TabIndex = 4;
            // 
            // cmb_Color
            // 
            cmb_Color.FormattingEnabled = true;
            cmb_Color.Location = new Point(109, 78);
            cmb_Color.Name = "cmb_Color";
            cmb_Color.Size = new Size(311, 23);
            cmb_Color.TabIndex = 5;
            // 
            // chk_All
            // 
            chk_All.AutoSize = true;
            chk_All.BackColor = Color.Transparent;
            chk_All.Font = new Font("Segoe UI", 9F);
            chk_All.Location = new Point(38, 113);
            chk_All.Margin = new Padding(3, 2, 3, 2);
            chk_All.Name = "chk_All";
            chk_All.Size = new Size(119, 19);
            chk_All.TabIndex = 8;
            chk_All.Text = "Seleccionar todos";
            chk_All.UseVisualStyleBackColor = false;
            chk_All.CheckedChanged += chk_All_CheckedChanged;
            // 
            // txt_filter
            // 
            txt_filter.Location = new Point(229, 111);
            txt_filter.Margin = new Padding(3, 2, 3, 2);
            txt_filter.Name = "txt_filter";
            txt_filter.Size = new Size(190, 23);
            txt_filter.TabIndex = 11;
            txt_filter.TextChanged += txt_filter_TextChanged;
            // 
            // lbl_Busqueda
            // 
            lbl_Busqueda.AutoSize = true;
            lbl_Busqueda.BackColor = Color.Transparent;
            lbl_Busqueda.Location = new Point(184, 113);
            lbl_Busqueda.Name = "lbl_Busqueda";
            lbl_Busqueda.Size = new Size(42, 15);
            lbl_Busqueda.TabIndex = 10;
            lbl_Busqueda.Text = "Buscar";
            // 
            // btn_ExportSets
            // 
            btn_ExportSets.BackColor = Color.Transparent;
            btn_ExportSets.BackgroundImage = (Image)resources.GetObject("btn_ExportSets.BackgroundImage");
            btn_ExportSets.BackgroundImageLayout = ImageLayout.Center;
            btn_ExportSets.Location = new Point(437, 26);
            btn_ExportSets.Margin = new Padding(3, 2, 3, 2);
            btn_ExportSets.Name = "btn_ExportSets";
            btn_ExportSets.Size = new Size(91, 106);
            btn_ExportSets.TabIndex = 12;
            btn_ExportSets.UseVisualStyleBackColor = false;
            btn_ExportSets.Click += btn_ExportSets_Click;
            // 
            // chkList_Sets
            // 
            chkList_Sets.FormattingEnabled = true;
            chkList_Sets.Location = new Point(39, 171);
            chkList_Sets.Margin = new Padding(3, 2, 3, 2);
            chkList_Sets.Name = "chkList_Sets";
            chkList_Sets.Size = new Size(489, 562);
            chkList_Sets.TabIndex = 14;
            // 
            // progress_Export
            // 
            progress_Export.Location = new Point(38, 138);
            progress_Export.Margin = new Padding(3, 2, 3, 2);
            progress_Export.Name = "progress_Export";
            progress_Export.Size = new Size(490, 22);
            progress_Export.TabIndex = 13;
            // 
            // ExportacionOpera
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(570, 768);
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
            Name = "ExportacionOpera";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Exportación a Opera";
            Load += ExportacionOpera_Load;
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
    }
}