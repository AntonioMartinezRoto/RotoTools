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
            SuspendLayout();
            // 
            // btn_SaveOptions
            // 
            btn_SaveOptions.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btn_SaveOptions.Image = (Image)resources.GetObject("btn_SaveOptions.Image");
            btn_SaveOptions.Location = new Point(261, 113);
            btn_SaveOptions.Margin = new Padding(3, 2, 3, 2);
            btn_SaveOptions.Name = "btn_SaveOptions";
            btn_SaveOptions.Size = new Size(98, 30);
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
            cmb_Idioma.Location = new Point(186, 52);
            cmb_Idioma.Name = "cmb_Idioma";
            cmb_Idioma.Size = new Size(175, 23);
            cmb_Idioma.TabIndex = 14;
            // 
            // lbl_Idioma
            // 
            lbl_Idioma.BackColor = Color.Transparent;
            lbl_Idioma.Location = new Point(31, 55);
            lbl_Idioma.Name = "lbl_Idioma";
            lbl_Idioma.Size = new Size(132, 20);
            lbl_Idioma.TabIndex = 13;
            lbl_Idioma.Text = "Seleccionar idioma";
            // 
            // OptionsMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(470, 174);
            Controls.Add(cmb_Idioma);
            Controls.Add(lbl_Idioma);
            Controls.Add(btn_SaveOptions);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "OptionsMenu";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Opciones";
            Load += OptionsMenu_Load;
            ResumeLayout(false);
        }

        #endregion

        private Button btn_SaveOptions;
        private ComboBox cmb_Idioma;
        private Label lbl_Idioma;
    }
}