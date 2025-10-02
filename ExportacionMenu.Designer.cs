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
            label1 = new Label();
            label2 = new Label();
            SuspendLayout();
            // 
            // btn_Xml
            // 
            btn_Xml.BackColor = SystemColors.Control;
            btn_Xml.BackgroundImage = (Image)resources.GetObject("btn_Xml.BackgroundImage");
            btn_Xml.BackgroundImageLayout = ImageLayout.Stretch;
            btn_Xml.Location = new Point(43, 53);
            btn_Xml.Name = "btn_Xml";
            btn_Xml.Size = new Size(47, 46);
            btn_Xml.TabIndex = 0;
            btn_Xml.UseVisualStyleBackColor = false;
            btn_Xml.Click += btn_Xml_Click;
            // 
            // lbl_info
            // 
            lbl_info.BackColor = Color.Transparent;
            lbl_info.Font = new Font("Segoe UI", 8F);
            lbl_info.Location = new Point(96, 70);
            lbl_info.Name = "lbl_info";
            lbl_info.Size = new Size(563, 29);
            lbl_info.TabIndex = 1;
            lbl_info.Text = "Seleccionar XML";
            // 
            // btn_ExportWinPerfil
            // 
            btn_ExportWinPerfil.BackColor = SystemColors.Control;
            btn_ExportWinPerfil.BackgroundImage = (Image)resources.GetObject("btn_ExportWinPerfil.BackgroundImage");
            btn_ExportWinPerfil.BackgroundImageLayout = ImageLayout.Stretch;
            btn_ExportWinPerfil.Enabled = false;
            btn_ExportWinPerfil.Location = new Point(43, 147);
            btn_ExportWinPerfil.Name = "btn_ExportWinPerfil";
            btn_ExportWinPerfil.Size = new Size(47, 46);
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
            btn_ExportOrgadata.Location = new Point(43, 222);
            btn_ExportOrgadata.Name = "btn_ExportOrgadata";
            btn_ExportOrgadata.Size = new Size(47, 46);
            btn_ExportOrgadata.TabIndex = 3;
            btn_ExportOrgadata.UseVisualStyleBackColor = false;
            btn_ExportOrgadata.Click += btn_ExportOrgadata_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Location = new Point(96, 163);
            label1.Name = "label1";
            label1.Size = new Size(111, 15);
            label1.TabIndex = 4;
            label1.Text = "Exportar a WinPerfil";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Location = new Point(96, 238);
            label2.Name = "label2";
            label2.Size = new Size(112, 15);
            label2.TabIndex = 5;
            label2.Text = "Exportar a Orgadata";
            // 
            // ExportacionMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            ClientSize = new Size(684, 354);
            Controls.Add(label2);
            Controls.Add(label1);
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
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btn_Xml;
        private Label lbl_info;
        private Button btn_ExportWinPerfil;
        private Button btn_ExportOrgadata;
        private Label label1;
        private Label label2;
    }
}
