namespace RotoTools
{
    partial class ActualizadorInstalarManualEscandallos
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ActualizadorInstalarManualEscandallos));
            chkList_Escandallos = new CheckedListBox();
            txt_filter = new TextBox();
            lbl_Buscar = new Label();
            chk_SelectAll = new CheckBox();
            btn_InstalarEscandallos = new Button();
            SuspendLayout();
            // 
            // chkList_Escandallos
            // 
            chkList_Escandallos.FormattingEnabled = true;
            chkList_Escandallos.Location = new Point(40, 64);
            chkList_Escandallos.Name = "chkList_Escandallos";
            chkList_Escandallos.Size = new Size(475, 220);
            chkList_Escandallos.TabIndex = 12;
            // 
            // txt_filter
            // 
            txt_filter.Location = new Point(366, 33);
            txt_filter.Name = "txt_filter";
            txt_filter.Size = new Size(149, 23);
            txt_filter.TabIndex = 11;
            txt_filter.TextChanged += txt_filter_TextChanged;
            // 
            // lbl_Buscar
            // 
            lbl_Buscar.AutoSize = true;
            lbl_Buscar.BackColor = Color.Transparent;
            lbl_Buscar.Location = new Point(314, 38);
            lbl_Buscar.Name = "lbl_Buscar";
            lbl_Buscar.Size = new Size(42, 15);
            lbl_Buscar.TabIndex = 10;
            lbl_Buscar.Text = "Buscar";
            // 
            // chk_SelectAll
            // 
            chk_SelectAll.AutoSize = true;
            chk_SelectAll.BackColor = Color.Transparent;
            chk_SelectAll.Location = new Point(43, 37);
            chk_SelectAll.Margin = new Padding(3, 2, 3, 2);
            chk_SelectAll.Name = "chk_SelectAll";
            chk_SelectAll.Size = new Size(119, 19);
            chk_SelectAll.TabIndex = 9;
            chk_SelectAll.Text = "Seleccionar todos";
            chk_SelectAll.UseVisualStyleBackColor = false;
            chk_SelectAll.CheckedChanged += chk_SelectAll_CheckedChanged;
            // 
            // btn_InstalarEscandallos
            // 
            btn_InstalarEscandallos.BackColor = Color.White;
            btn_InstalarEscandallos.Image = (Image)resources.GetObject("btn_InstalarEscandallos.Image");
            btn_InstalarEscandallos.ImageAlign = ContentAlignment.MiddleLeft;
            btn_InstalarEscandallos.Location = new Point(413, 299);
            btn_InstalarEscandallos.Name = "btn_InstalarEscandallos";
            btn_InstalarEscandallos.Padding = new Padding(0, 0, 5, 0);
            btn_InstalarEscandallos.Size = new Size(102, 40);
            btn_InstalarEscandallos.TabIndex = 14;
            btn_InstalarEscandallos.Text = "Instalar";
            btn_InstalarEscandallos.TextAlign = ContentAlignment.MiddleRight;
            btn_InstalarEscandallos.UseVisualStyleBackColor = false;
            btn_InstalarEscandallos.Click += btn_InstalarEscandallos_Click;
            // 
            // ActualizadorInstalarManualEscandallos
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(664, 360);
            Controls.Add(btn_InstalarEscandallos);
            Controls.Add(chkList_Escandallos);
            Controls.Add(txt_filter);
            Controls.Add(lbl_Buscar);
            Controls.Add(chk_SelectAll);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "ActualizadorInstalarManualEscandallos";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Instalar escandallos individualmente";
            Load += ActualizadorInstalarManualEscandallos_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private CheckedListBox chkList_Escandallos;
        private TextBox txt_filter;
        private Label lbl_Buscar;
        private CheckBox chk_SelectAll;
        private Button btn_InstalarEscandallos;
    }
}