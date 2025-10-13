namespace RotoTools
{
    partial class ControlCambiosFiltroItems
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlCambiosFiltroItems));
            chk_SelectAll = new CheckBox();
            lbl_Buscar = new Label();
            txt_filter = new TextBox();
            chkList_Sets = new CheckedListBox();
            btn_GuardarFiltro = new Button();
            lbl_NumeroComunes = new Label();
            chk_SoloFiltrados = new CheckBox();
            SuspendLayout();
            // 
            // chk_SelectAll
            // 
            chk_SelectAll.AutoSize = true;
            chk_SelectAll.BackColor = Color.Transparent;
            chk_SelectAll.Location = new Point(41, 41);
            chk_SelectAll.Margin = new Padding(3, 2, 3, 2);
            chk_SelectAll.Name = "chk_SelectAll";
            chk_SelectAll.Size = new Size(119, 19);
            chk_SelectAll.TabIndex = 1;
            chk_SelectAll.Text = "Seleccionar todos";
            chk_SelectAll.UseVisualStyleBackColor = false;
            chk_SelectAll.CheckedChanged += chk_SelectAll_CheckedChanged;
            // 
            // lbl_Buscar
            // 
            lbl_Buscar.AutoSize = true;
            lbl_Buscar.BackColor = Color.Transparent;
            lbl_Buscar.Location = new Point(312, 42);
            lbl_Buscar.Name = "lbl_Buscar";
            lbl_Buscar.Size = new Size(42, 15);
            lbl_Buscar.TabIndex = 2;
            lbl_Buscar.Text = "Buscar";
            // 
            // txt_filter
            // 
            txt_filter.Location = new Point(364, 37);
            txt_filter.Name = "txt_filter";
            txt_filter.Size = new Size(149, 23);
            txt_filter.TabIndex = 3;
            txt_filter.TextChanged += txt_filter_TextChanged;
            // 
            // chkList_Sets
            // 
            chkList_Sets.FormattingEnabled = true;
            chkList_Sets.Location = new Point(38, 68);
            chkList_Sets.Name = "chkList_Sets";
            chkList_Sets.Size = new Size(475, 220);
            chkList_Sets.TabIndex = 4;
            chkList_Sets.ItemCheck += chkList_Sets_ItemCheck;
            // 
            // btn_GuardarFiltro
            // 
            btn_GuardarFiltro.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btn_GuardarFiltro.Image = (Image)resources.GetObject("btn_GuardarFiltro.Image");
            btn_GuardarFiltro.Location = new Point(554, 302);
            btn_GuardarFiltro.Margin = new Padding(3, 2, 3, 2);
            btn_GuardarFiltro.Name = "btn_GuardarFiltro";
            btn_GuardarFiltro.Size = new Size(98, 30);
            btn_GuardarFiltro.TabIndex = 6;
            btn_GuardarFiltro.Text = "Guardar";
            btn_GuardarFiltro.TextAlign = ContentAlignment.MiddleRight;
            btn_GuardarFiltro.TextImageRelation = TextImageRelation.ImageBeforeText;
            btn_GuardarFiltro.UseVisualStyleBackColor = true;
            btn_GuardarFiltro.Click += btn_GuardarFiltro_Click;
            // 
            // lbl_NumeroComunes
            // 
            lbl_NumeroComunes.AutoSize = true;
            lbl_NumeroComunes.BackColor = Color.Transparent;
            lbl_NumeroComunes.Location = new Point(38, 302);
            lbl_NumeroComunes.Name = "lbl_NumeroComunes";
            lbl_NumeroComunes.Size = new Size(92, 15);
            lbl_NumeroComunes.TabIndex = 7;
            lbl_NumeroComunes.Text = "Sets comunes: 0";
            // 
            // chk_SoloFiltrados
            // 
            chk_SoloFiltrados.AutoSize = true;
            chk_SoloFiltrados.BackColor = Color.Transparent;
            chk_SoloFiltrados.Location = new Point(174, 41);
            chk_SoloFiltrados.Margin = new Padding(3, 2, 3, 2);
            chk_SoloFiltrados.Name = "chk_SoloFiltrados";
            chk_SoloFiltrados.Size = new Size(126, 19);
            chk_SoloFiltrados.TabIndex = 8;
            chk_SoloFiltrados.Text = "Solo seleccionados";
            chk_SoloFiltrados.UseVisualStyleBackColor = false;
            chk_SoloFiltrados.CheckedChanged += chk_SoloFiltrados_CheckedChanged;
            // 
            // ControlCambiosFiltroItems
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(664, 343);
            Controls.Add(chk_SoloFiltrados);
            Controls.Add(lbl_NumeroComunes);
            Controls.Add(btn_GuardarFiltro);
            Controls.Add(chkList_Sets);
            Controls.Add(txt_filter);
            Controls.Add(lbl_Buscar);
            Controls.Add(chk_SelectAll);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "ControlCambiosFiltroItems";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Filtro de elementos comunes";
            Load += FiltroSets_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox chk_SelectAll;
        private Label lbl_Buscar;
        private TextBox txt_filter;
        private CheckedListBox chkList_Sets;
        private Button btn_GuardarFiltro;
        private Label lbl_NumeroComunes;
        private CheckBox chk_SoloFiltrados;
    }
}