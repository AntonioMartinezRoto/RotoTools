namespace RotoTools
{
    partial class OptionsMenu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsMenu));
            btn_SaveOptions = new Button();
            cmb_Idioma = new ComboBox();
            lbl_Idioma = new Label();
            chk_PermitirTraduccion = new CheckBox();
            groupBox_Opciones = new GroupBox();
            groupBox_Idioma = new GroupBox();
            btn_ExportarResources = new Button();
            btn_ImportarResources = new Button();
            groupBox_Opciones.SuspendLayout();
            groupBox_Idioma.SuspendLayout();
            SuspendLayout();
            // 
            // btn_SaveOptions
            // 
            btn_SaveOptions.Image = (Image)resources.GetObject("btn_SaveOptions.Image");
            btn_SaveOptions.Location = new Point(333, 195);
            btn_SaveOptions.Margin = new Padding(3, 2, 3, 2);
            btn_SaveOptions.Name = "btn_SaveOptions";
            btn_SaveOptions.Size = new Size(87, 41);
            btn_SaveOptions.TabIndex = 7;
            btn_SaveOptions.Text = "Guardar";
            btn_SaveOptions.TextAlign = ContentAlignment.MiddleRight;
            btn_SaveOptions.TextImageRelation = TextImageRelation.ImageBeforeText;
            btn_SaveOptions.UseVisualStyleBackColor = true;
            btn_SaveOptions.Click += btn_SaveOptions_Click;
            // 
            // cmb_Idioma
            // 
            cmb_Idioma.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb_Idioma.FormattingEnabled = true;
            cmb_Idioma.Location = new Point(169, 16);
            cmb_Idioma.Name = "cmb_Idioma";
            cmb_Idioma.Size = new Size(191, 23);
            cmb_Idioma.TabIndex = 14;
            // 
            // lbl_Idioma
            // 
            lbl_Idioma.BackColor = Color.Transparent;
            lbl_Idioma.Location = new Point(15, 19);
            lbl_Idioma.Name = "lbl_Idioma";
            lbl_Idioma.Size = new Size(148, 20);
            lbl_Idioma.TabIndex = 13;
            lbl_Idioma.Text = "Seleccionar idioma";
            // 
            // chk_PermitirTraduccion
            // 
            chk_PermitirTraduccion.AutoSize = true;
            chk_PermitirTraduccion.BackColor = Color.Transparent;
            chk_PermitirTraduccion.Location = new Point(15, 32);
            chk_PermitirTraduccion.Name = "chk_PermitirTraduccion";
            chk_PermitirTraduccion.Size = new Size(337, 19);
            chk_PermitirTraduccion.TabIndex = 24;
            chk_PermitirTraduccion.Text = "Permitir traducciones en Escandallos y Conector de Herraje";
            chk_PermitirTraduccion.UseVisualStyleBackColor = false;
            // 
            // groupBox_Opciones
            // 
            groupBox_Opciones.BackColor = Color.Transparent;
            groupBox_Opciones.Controls.Add(chk_PermitirTraduccion);
            groupBox_Opciones.Location = new Point(36, 95);
            groupBox_Opciones.Name = "groupBox_Opciones";
            groupBox_Opciones.Size = new Size(384, 72);
            groupBox_Opciones.TabIndex = 25;
            groupBox_Opciones.TabStop = false;
            // 
            // groupBox_Idioma
            // 
            groupBox_Idioma.BackColor = Color.Transparent;
            groupBox_Idioma.Controls.Add(lbl_Idioma);
            groupBox_Idioma.Controls.Add(cmb_Idioma);
            groupBox_Idioma.Location = new Point(36, 38);
            groupBox_Idioma.Name = "groupBox_Idioma";
            groupBox_Idioma.Size = new Size(384, 51);
            groupBox_Idioma.TabIndex = 26;
            groupBox_Idioma.TabStop = false;
            // 
            // btn_ExportarResources
            // 
            btn_ExportarResources.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_ExportarResources.BackColor = Color.Transparent;
            btn_ExportarResources.BackgroundImage = (Image)resources.GetObject("btn_ExportarResources.BackgroundImage");
            btn_ExportarResources.BackgroundImageLayout = ImageLayout.Stretch;
            btn_ExportarResources.Location = new Point(437, 48);
            btn_ExportarResources.Margin = new Padding(3, 2, 3, 2);
            btn_ExportarResources.Name = "btn_ExportarResources";
            btn_ExportarResources.Size = new Size(32, 32);
            btn_ExportarResources.TabIndex = 27;
            btn_ExportarResources.UseVisualStyleBackColor = false;
            btn_ExportarResources.Visible = false;
            btn_ExportarResources.Click += btn_ExportarResources_Click;
            // 
            // btn_ImportarResources
            // 
            btn_ImportarResources.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_ImportarResources.BackColor = Color.Transparent;
            btn_ImportarResources.BackgroundImage = (Image)resources.GetObject("btn_ImportarResources.BackgroundImage");
            btn_ImportarResources.BackgroundImageLayout = ImageLayout.Stretch;
            btn_ImportarResources.Location = new Point(475, 48);
            btn_ImportarResources.Margin = new Padding(3, 2, 3, 2);
            btn_ImportarResources.Name = "btn_ImportarResources";
            btn_ImportarResources.Size = new Size(32, 32);
            btn_ImportarResources.TabIndex = 28;
            btn_ImportarResources.UseVisualStyleBackColor = false;
            btn_ImportarResources.Visible = false;
            btn_ImportarResources.Click += btn_ImportarResources_Click;
            // 
            // OptionsMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(534, 270);
            Controls.Add(btn_ImportarResources);
            Controls.Add(btn_ExportarResources);
            Controls.Add(groupBox_Idioma);
            Controls.Add(groupBox_Opciones);
            Controls.Add(btn_SaveOptions);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "OptionsMenu";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Opciones";
            Load += OptionsMenu_Load;
            groupBox_Opciones.ResumeLayout(false);
            groupBox_Opciones.PerformLayout();
            groupBox_Idioma.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Button btn_SaveOptions;
        private ComboBox cmb_Idioma;
        private Label lbl_Idioma;
        private CheckBox chk_PermitirTraduccion;
        private GroupBox groupBox_Opciones;
        private GroupBox groupBox_Idioma;
        private Button btn_ExportarResources;
        private Button btn_ImportarResources;
    }
}