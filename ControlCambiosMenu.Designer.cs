namespace RotoTools
{
    partial class ControlCambiosMenu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlCambiosMenu));
            btn_SelectXml1 = new Button();
            btn_SelectXml2 = new Button();
            lbl_Xml1 = new Label();
            lbl_Xml2 = new Label();
            btn_Compare = new Button();
            lbl_ControlCambios = new Label();
            btn_Config = new Button();
            lbl_Configuracion = new Label();
            SuspendLayout();
            // 
            // btn_SelectXml1
            // 
            btn_SelectXml1.BackColor = Color.Transparent;
            btn_SelectXml1.BackgroundImage = (Image)resources.GetObject("btn_SelectXml1.BackgroundImage");
            btn_SelectXml1.BackgroundImageLayout = ImageLayout.Stretch;
            btn_SelectXml1.Location = new Point(38, 36);
            btn_SelectXml1.Margin = new Padding(3, 2, 3, 2);
            btn_SelectXml1.Name = "btn_SelectXml1";
            btn_SelectXml1.Size = new Size(47, 40);
            btn_SelectXml1.TabIndex = 0;
            btn_SelectXml1.UseVisualStyleBackColor = false;
            btn_SelectXml1.Click += btn_SelectXml1_Click;
            // 
            // btn_SelectXml2
            // 
            btn_SelectXml2.BackColor = Color.Transparent;
            btn_SelectXml2.BackgroundImage = (Image)resources.GetObject("btn_SelectXml2.BackgroundImage");
            btn_SelectXml2.BackgroundImageLayout = ImageLayout.Stretch;
            btn_SelectXml2.Location = new Point(38, 85);
            btn_SelectXml2.Margin = new Padding(3, 2, 3, 2);
            btn_SelectXml2.Name = "btn_SelectXml2";
            btn_SelectXml2.Size = new Size(47, 40);
            btn_SelectXml2.TabIndex = 1;
            btn_SelectXml2.UseVisualStyleBackColor = false;
            btn_SelectXml2.Click += btn_SelectXml2_Click;
            // 
            // lbl_Xml1
            // 
            lbl_Xml1.BackColor = Color.Transparent;
            lbl_Xml1.Location = new Point(91, 36);
            lbl_Xml1.Name = "lbl_Xml1";
            lbl_Xml1.Size = new Size(470, 40);
            lbl_Xml1.TabIndex = 2;
            lbl_Xml1.Text = "Seleccione XML anterior";
            lbl_Xml1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lbl_Xml2
            // 
            lbl_Xml2.BackColor = Color.Transparent;
            lbl_Xml2.Location = new Point(91, 85);
            lbl_Xml2.Name = "lbl_Xml2";
            lbl_Xml2.Size = new Size(470, 42);
            lbl_Xml2.TabIndex = 3;
            lbl_Xml2.Text = "Seleccione XML nuevo";
            lbl_Xml2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btn_Compare
            // 
            btn_Compare.BackColor = Color.Transparent;
            btn_Compare.BackgroundImage = (Image)resources.GetObject("btn_Compare.BackgroundImage");
            btn_Compare.BackgroundImageLayout = ImageLayout.Stretch;
            btn_Compare.Location = new Point(39, 218);
            btn_Compare.Margin = new Padding(3, 2, 3, 2);
            btn_Compare.Name = "btn_Compare";
            btn_Compare.Size = new Size(47, 40);
            btn_Compare.TabIndex = 4;
            btn_Compare.UseVisualStyleBackColor = false;
            btn_Compare.Click += btn_Compare_Click;
            // 
            // lbl_ControlCambios
            // 
            lbl_ControlCambios.AutoSize = true;
            lbl_ControlCambios.BackColor = Color.Transparent;
            lbl_ControlCambios.Location = new Point(92, 231);
            lbl_ControlCambios.Name = "lbl_ControlCambios";
            lbl_ControlCambios.Size = new Size(214, 15);
            lbl_ControlCambios.TabIndex = 5;
            lbl_ControlCambios.Text = "Generar informe de control de cambios";
            // 
            // btn_Config
            // 
            btn_Config.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_Config.BackColor = Color.Transparent;
            btn_Config.BackgroundImage = (Image)resources.GetObject("btn_Config.BackgroundImage");
            btn_Config.BackgroundImageLayout = ImageLayout.Stretch;
            btn_Config.Location = new Point(39, 168);
            btn_Config.Margin = new Padding(3, 2, 3, 2);
            btn_Config.Name = "btn_Config";
            btn_Config.Size = new Size(47, 40);
            btn_Config.TabIndex = 8;
            btn_Config.UseVisualStyleBackColor = false;
            btn_Config.Click += btn_Config_Click;
            // 
            // lbl_Configuracion
            // 
            lbl_Configuracion.AutoSize = true;
            lbl_Configuracion.BackColor = Color.Transparent;
            lbl_Configuracion.Location = new Point(93, 181);
            lbl_Configuracion.Name = "lbl_Configuracion";
            lbl_Configuracion.Size = new Size(147, 15);
            lbl_Configuracion.TabIndex = 9;
            lbl_Configuracion.Text = "Configuración del informe";
            // 
            // ControlCambiosMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(649, 292);
            Controls.Add(lbl_Configuracion);
            Controls.Add(btn_Config);
            Controls.Add(lbl_ControlCambios);
            Controls.Add(btn_Compare);
            Controls.Add(lbl_Xml2);
            Controls.Add(lbl_Xml1);
            Controls.Add(btn_SelectXml2);
            Controls.Add(btn_SelectXml1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            Name = "ControlCambiosMenu";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Menú de control de cambios";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btn_SelectXml1;
        private Button btn_SelectXml2;
        private Label lbl_Xml1;
        private Label lbl_Xml2;
        private Button btn_Compare;
        private Label lbl_ControlCambios;
        private Button btn_Config;
        private Label lbl_Configuracion;
    }
}
