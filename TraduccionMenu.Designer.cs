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
            label4 = new Label();
            btn_GenerarPlantillaExcel = new Button();
            label1 = new Label();
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
            // label4
            // 
            label4.BackColor = Color.Transparent;
            label4.Location = new Point(89, 176);
            label4.Name = "label4";
            label4.Size = new Size(189, 15);
            label4.TabIndex = 16;
            label4.Text = "Generar plantilla de traducción";
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
            // label1
            // 
            label1.BackColor = Color.Transparent;
            label1.Location = new Point(89, 118);
            label1.Name = "label1";
            label1.Size = new Size(189, 15);
            label1.TabIndex = 18;
            label1.Text = "Traducir XML";
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
            Controls.Add(label1);
            Controls.Add(btn_Traducir);
            Controls.Add(label4);
            Controls.Add(btn_GenerarPlantillaExcel);
            Controls.Add(lbl_Xml);
            Controls.Add(btn_LoadXml);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "TraduccionMenu";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Traducción";
            ResumeLayout(false);
        }

        #endregion

        private Label lbl_Xml;
        private Button btn_LoadXml;
        private Label label4;
        private Button btn_GenerarPlantillaExcel;
        private Label label1;
        private Button btn_Traducir;
    }
}