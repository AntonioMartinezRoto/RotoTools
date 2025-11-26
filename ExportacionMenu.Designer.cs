namespace RotoTools
{
    partial class ExportacionMenu
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportacionMenu));
            btn_Xml = new Button();
            lbl_info = new Label();
            btn_ExportWinPerfil = new Button();
            btn_ExportOrgadata = new Button();
            lbl_ExportWinPerfil = new Label();
            lbl_ExportOrgadata = new Label();
            lbl_ExportOpera = new Label();
            btn_ExportOpera = new Button();
            SuspendLayout();
            // 
            // btn_Xml
            // 
            btn_Xml.BackColor = SystemColors.Control;
            btn_Xml.BackgroundImage = (Image)resources.GetObject("btn_Xml.BackgroundImage");
            btn_Xml.BackgroundImageLayout = ImageLayout.Stretch;
            btn_Xml.Location = new Point(42, 44);
            btn_Xml.Name = "btn_Xml";
            btn_Xml.Size = new Size(47, 40);
            btn_Xml.TabIndex = 0;
            btn_Xml.UseVisualStyleBackColor = false;
            btn_Xml.Click += btn_Xml_Click;
            // 
            // lbl_info
            // 
            lbl_info.BackColor = Color.Transparent;
            lbl_info.Font = new Font("Segoe UI", 8F);
            lbl_info.Location = new Point(95, 44);
            lbl_info.Name = "lbl_info";
            lbl_info.Size = new Size(425, 40);
            lbl_info.TabIndex = 1;
            lbl_info.Text = "Seleccionar XML";
            lbl_info.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btn_ExportWinPerfil
            // 
            btn_ExportWinPerfil.BackColor = SystemColors.Control;
            btn_ExportWinPerfil.BackgroundImage = (Image)resources.GetObject("btn_ExportWinPerfil.BackgroundImage");
            btn_ExportWinPerfil.BackgroundImageLayout = ImageLayout.Stretch;
            btn_ExportWinPerfil.Enabled = false;
            btn_ExportWinPerfil.Location = new Point(42, 129);
            btn_ExportWinPerfil.Name = "btn_ExportWinPerfil";
            btn_ExportWinPerfil.Size = new Size(47, 40);
            btn_ExportWinPerfil.TabIndex = 2;
            btn_ExportWinPerfil.UseVisualStyleBackColor = false;
            btn_ExportWinPerfil.Click += btn_ExportWinPerfil_Click;
            // 
            // btn_ExportOrgadata
            // 
            btn_ExportOrgadata.BackColor = SystemColors.Control;
            btn_ExportOrgadata.BackgroundImage = (Image)resources.GetObject("btn_ExportOrgadata.BackgroundImage");
            btn_ExportOrgadata.BackgroundImageLayout = ImageLayout.Stretch;
            btn_ExportOrgadata.Enabled = false;
            btn_ExportOrgadata.Location = new Point(42, 204);
            btn_ExportOrgadata.Name = "btn_ExportOrgadata";
            btn_ExportOrgadata.Size = new Size(47, 40);
            btn_ExportOrgadata.TabIndex = 3;
            btn_ExportOrgadata.UseVisualStyleBackColor = false;
            btn_ExportOrgadata.Click += btn_ExportOrgadata_Click;
            // 
            // lbl_ExportWinPerfil
            // 
            lbl_ExportWinPerfil.AutoSize = true;
            lbl_ExportWinPerfil.BackColor = Color.Transparent;
            lbl_ExportWinPerfil.Location = new Point(95, 145);
            lbl_ExportWinPerfil.Name = "lbl_ExportWinPerfil";
            lbl_ExportWinPerfil.Size = new Size(111, 15);
            lbl_ExportWinPerfil.TabIndex = 4;
            lbl_ExportWinPerfil.Text = "Exportar a WinPerfil";
            // 
            // lbl_ExportOrgadata
            // 
            lbl_ExportOrgadata.AutoSize = true;
            lbl_ExportOrgadata.BackColor = Color.Transparent;
            lbl_ExportOrgadata.Location = new Point(95, 220);
            lbl_ExportOrgadata.Name = "lbl_ExportOrgadata";
            lbl_ExportOrgadata.Size = new Size(112, 15);
            lbl_ExportOrgadata.TabIndex = 5;
            lbl_ExportOrgadata.Text = "Exportar a Orgadata";
            // 
            // lbl_ExportOpera
            // 
            lbl_ExportOpera.AutoSize = true;
            lbl_ExportOpera.BackColor = Color.Transparent;
            lbl_ExportOpera.Location = new Point(355, 145);
            lbl_ExportOpera.Name = "lbl_ExportOpera";
            lbl_ExportOpera.Size = new Size(95, 15);
            lbl_ExportOpera.TabIndex = 7;
            lbl_ExportOpera.Text = "Exportar a Opera";
            lbl_ExportOpera.Visible = false;
            // 
            // btn_ExportOpera
            // 
            btn_ExportOpera.BackColor = SystemColors.Control;
            btn_ExportOpera.BackgroundImage = (Image)resources.GetObject("btn_ExportOpera.BackgroundImage");
            btn_ExportOpera.BackgroundImageLayout = ImageLayout.Zoom;
            btn_ExportOpera.Enabled = false;
            btn_ExportOpera.Location = new Point(302, 129);
            btn_ExportOpera.Name = "btn_ExportOpera";
            btn_ExportOpera.Size = new Size(47, 40);
            btn_ExportOpera.TabIndex = 6;
            btn_ExportOpera.UseVisualStyleBackColor = false;
            btn_ExportOpera.Visible = false;
            btn_ExportOpera.Click += btn_ExportOpera_Click;
            // 
            // ExportacionMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(612, 291);
            Controls.Add(lbl_ExportOpera);
            Controls.Add(btn_ExportOpera);
            Controls.Add(lbl_ExportOrgadata);
            Controls.Add(lbl_ExportWinPerfil);
            Controls.Add(btn_ExportOrgadata);
            Controls.Add(btn_ExportWinPerfil);
            Controls.Add(lbl_info);
            Controls.Add(btn_Xml);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "ExportacionMenu";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Menú exportar";
            Load += ExportacionMenu_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btn_Xml;
        private Label lbl_info;
        private Button btn_ExportWinPerfil;
        private Button btn_ExportOrgadata;
        private Label lbl_ExportWinPerfil;
        private Label lbl_ExportOrgadata;
        private Label lbl_ExportOpera;
        private Button btn_ExportOpera;
    }
}
