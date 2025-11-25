namespace RotoTools
{
    partial class TraduccionMenu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TraduccionMenu));
            lbl_Xml = new Label();
            btn_LoadXml = new Button();
            lbl_GenerarPlantilla = new Label();
            btn_GenerarPlantillaExcel = new Button();
            lbl_TraducirXML = new Label();
            btn_Traducir = new Button();
            SuspendLayout();
            // 
            // lbl_Xml
            // 
            lbl_Xml.BackColor = Color.Transparent;
            lbl_Xml.Location = new Point(89, 50);
            lbl_Xml.Name = "lbl_Xml";
            lbl_Xml.Size = new Size(519, 37);
            lbl_Xml.TabIndex = 6;
            lbl_Xml.Text = "Seleccionar XML";
            lbl_Xml.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btn_LoadXml
            // 
            btn_LoadXml.BackgroundImage = (Image)resources.GetObject("btn_LoadXml.BackgroundImage");
            btn_LoadXml.BackgroundImageLayout = ImageLayout.Stretch;
            btn_LoadXml.Location = new Point(36, 47);
            btn_LoadXml.Name = "btn_LoadXml";
            btn_LoadXml.Size = new Size(47, 40);
            btn_LoadXml.TabIndex = 5;
            btn_LoadXml.UseVisualStyleBackColor = true;
            btn_LoadXml.Click += btn_LoadXml_Click;
            // 
            // lbl_GenerarPlantilla
            // 
            lbl_GenerarPlantilla.BackColor = Color.Transparent;
            lbl_GenerarPlantilla.Location = new Point(89, 176);
            lbl_GenerarPlantilla.Name = "lbl_GenerarPlantilla";
            lbl_GenerarPlantilla.Size = new Size(278, 16);
            lbl_GenerarPlantilla.TabIndex = 16;
            lbl_GenerarPlantilla.Text = "Generar plantilla de traducción";
            // 
            // btn_GenerarPlantillaExcel
            // 
            btn_GenerarPlantillaExcel.BackgroundImage = (Image)resources.GetObject("btn_GenerarPlantillaExcel.BackgroundImage");
            btn_GenerarPlantillaExcel.BackgroundImageLayout = ImageLayout.Stretch;
            btn_GenerarPlantillaExcel.Location = new Point(36, 163);
            btn_GenerarPlantillaExcel.Name = "btn_GenerarPlantillaExcel";
            btn_GenerarPlantillaExcel.Size = new Size(47, 40);
            btn_GenerarPlantillaExcel.TabIndex = 15;
            btn_GenerarPlantillaExcel.UseVisualStyleBackColor = true;
            btn_GenerarPlantillaExcel.Click += btn_GenerarPlantillaExcel_Click;
            // 
            // lbl_TraducirXML
            // 
            lbl_TraducirXML.BackColor = Color.Transparent;
            lbl_TraducirXML.Location = new Point(89, 118);
            lbl_TraducirXML.Name = "lbl_TraducirXML";
            lbl_TraducirXML.Size = new Size(240, 12);
            lbl_TraducirXML.TabIndex = 18;
            lbl_TraducirXML.Text = "Traducir XML";
            // 
            // btn_Traducir
            // 
            btn_Traducir.BackgroundImage = (Image)resources.GetObject("btn_Traducir.BackgroundImage");
            btn_Traducir.BackgroundImageLayout = ImageLayout.Stretch;
            btn_Traducir.Location = new Point(36, 105);
            btn_Traducir.Name = "btn_Traducir";
            btn_Traducir.Size = new Size(47, 40);
            btn_Traducir.TabIndex = 17;
            btn_Traducir.UseVisualStyleBackColor = true;
            btn_Traducir.Click += btn_Traducir_Click;
            // 
            // TraduccionMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(624, 265);
            Controls.Add(lbl_TraducirXML);
            Controls.Add(btn_Traducir);
            Controls.Add(lbl_GenerarPlantilla);
            Controls.Add(btn_GenerarPlantillaExcel);
            Controls.Add(lbl_Xml);
            Controls.Add(btn_LoadXml);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "TraduccionMenu";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Traducción";
            Load += TraduccionMenu_Load;
            ResumeLayout(false);
        }

        #endregion

        private Label lbl_Xml;
        private Button btn_LoadXml;
        private Label lbl_GenerarPlantilla;
        private Button btn_GenerarPlantillaExcel;
        private Label lbl_TraducirXML;
        private Button btn_Traducir;
    }
}