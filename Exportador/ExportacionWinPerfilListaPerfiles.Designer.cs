namespace RotoTools.Exportador
{
    partial class ExportacionWinPerfilListaPerfiles
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportacionWinPerfilListaPerfiles));
            btn_Save = new Button();
            chkList_Perfiles = new CheckedListBox();
            txt_filter = new TextBox();
            lbl_Buscar = new Label();
            SuspendLayout();
            // 
            // btn_Save
            // 
            btn_Save.Image = (Image)resources.GetObject("btn_Save.Image");
            btn_Save.Location = new Point(303, 225);
            btn_Save.Margin = new Padding(0);
            btn_Save.Name = "btn_Save";
            btn_Save.Size = new Size(87, 41);
            btn_Save.TabIndex = 9;
            btn_Save.Text = "Guardar";
            btn_Save.TextAlign = ContentAlignment.MiddleRight;
            btn_Save.TextImageRelation = TextImageRelation.ImageBeforeText;
            btn_Save.UseVisualStyleBackColor = true;
            btn_Save.Click += btn_Save_Click;
            // 
            // chkList_Perfiles
            // 
            chkList_Perfiles.FormattingEnabled = true;
            chkList_Perfiles.Location = new Point(35, 51);
            chkList_Perfiles.Name = "chkList_Perfiles";
            chkList_Perfiles.Size = new Size(355, 166);
            chkList_Perfiles.TabIndex = 11;
            chkList_Perfiles.ItemCheck += chkList_Perfiles_ItemCheck;
            // 
            // txt_filter
            // 
            txt_filter.Location = new Point(261, 22);
            txt_filter.Name = "txt_filter";
            txt_filter.Size = new Size(129, 23);
            txt_filter.TabIndex = 10;
            txt_filter.TextChanged += txt_filter_TextChanged;
            // 
            // lbl_Buscar
            // 
            lbl_Buscar.AutoSize = true;
            lbl_Buscar.BackColor = Color.Transparent;
            lbl_Buscar.Location = new Point(197, 25);
            lbl_Buscar.Name = "lbl_Buscar";
            lbl_Buscar.Size = new Size(42, 15);
            lbl_Buscar.TabIndex = 12;
            lbl_Buscar.Text = "Buscar";
            // 
            // ExportacionWinPerfilListaPerfiles
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(506, 286);
            Controls.Add(lbl_Buscar);
            Controls.Add(chkList_Perfiles);
            Controls.Add(txt_filter);
            Controls.Add(btn_Save);
            DoubleBuffered = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "ExportacionWinPerfilListaPerfiles";
            StartPosition = FormStartPosition.CenterScreen;
            Load += ExportacionWinPerfilListaPerfiles_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btn_Save;
        private CheckedListBox chkList_Perfiles;
        private TextBox txt_filter;
        private Label lbl_Buscar;
    }
}