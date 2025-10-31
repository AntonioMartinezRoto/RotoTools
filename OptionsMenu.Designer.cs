﻿namespace RotoTools
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
            groupBox_Opciones.SuspendLayout();
            SuspendLayout();
            // 
            // btn_SaveOptions
            // 
            btn_SaveOptions.Image = (Image)resources.GetObject("btn_SaveOptions.Image");
            btn_SaveOptions.Location = new Point(322, 195);
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
            cmb_Idioma.Location = new Point(213, 29);
            cmb_Idioma.Name = "cmb_Idioma";
            cmb_Idioma.Size = new Size(175, 23);
            cmb_Idioma.TabIndex = 14;
            cmb_Idioma.Visible = false;
            // 
            // lbl_Idioma
            // 
            lbl_Idioma.BackColor = Color.Transparent;
            lbl_Idioma.Location = new Point(58, 32);
            lbl_Idioma.Name = "lbl_Idioma";
            lbl_Idioma.Size = new Size(132, 20);
            lbl_Idioma.TabIndex = 13;
            lbl_Idioma.Text = "Seleccionar idioma";
            lbl_Idioma.Visible = false;
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
            groupBox_Opciones.Location = new Point(36, 84);
            groupBox_Opciones.Name = "groupBox_Opciones";
            groupBox_Opciones.Size = new Size(384, 72);
            groupBox_Opciones.TabIndex = 25;
            groupBox_Opciones.TabStop = false;
            // 
            // OptionsMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(534, 270);
            Controls.Add(groupBox_Opciones);
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
            groupBox_Opciones.ResumeLayout(false);
            groupBox_Opciones.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button btn_SaveOptions;
        private ComboBox cmb_Idioma;
        private Label lbl_Idioma;
        private CheckBox chk_PermitirTraduccion;
        private GroupBox groupBox_Opciones;
    }
}