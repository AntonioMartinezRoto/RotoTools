namespace RotoTools
{
    partial class TariffsImporterAddTariff
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TariffsImporterAddTariff));
            btn_Aceptar = new Button();
            txt_TariffName = new TextBox();
            lbl_Tarifa = new Label();
            cmb_Monedas = new ComboBox();
            lbl_Monedas = new Label();
            SuspendLayout();
            // 
            // btn_Aceptar
            // 
            btn_Aceptar.Image = (Image)resources.GetObject("btn_Aceptar.Image");
            btn_Aceptar.Location = new Point(232, 155);
            btn_Aceptar.Margin = new Padding(0);
            btn_Aceptar.Name = "btn_Aceptar";
            btn_Aceptar.Size = new Size(87, 41);
            btn_Aceptar.TabIndex = 10;
            btn_Aceptar.Text = "Guardar";
            btn_Aceptar.TextAlign = ContentAlignment.MiddleRight;
            btn_Aceptar.TextImageRelation = TextImageRelation.ImageBeforeText;
            btn_Aceptar.UseVisualStyleBackColor = true;
            btn_Aceptar.Click += btn_Aceptar_Click;
            // 
            // txt_TariffName
            // 
            txt_TariffName.Location = new Point(154, 52);
            txt_TariffName.MaxLength = 20;
            txt_TariffName.Name = "txt_TariffName";
            txt_TariffName.Size = new Size(165, 23);
            txt_TariffName.TabIndex = 23;
            // 
            // lbl_Tarifa
            // 
            lbl_Tarifa.AutoSize = true;
            lbl_Tarifa.BackColor = Color.Transparent;
            lbl_Tarifa.Location = new Point(20, 55);
            lbl_Tarifa.Name = "lbl_Tarifa";
            lbl_Tarifa.Size = new Size(35, 15);
            lbl_Tarifa.TabIndex = 24;
            lbl_Tarifa.Text = "Tarifa";
            // 
            // cmb_Monedas
            // 
            cmb_Monedas.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb_Monedas.Font = new Font("Calibri", 9F);
            cmb_Monedas.FormattingEnabled = true;
            cmb_Monedas.Location = new Point(154, 99);
            cmb_Monedas.Name = "cmb_Monedas";
            cmb_Monedas.Size = new Size(165, 22);
            cmb_Monedas.TabIndex = 25;
            // 
            // lbl_Monedas
            // 
            lbl_Monedas.AutoSize = true;
            lbl_Monedas.BackColor = Color.Transparent;
            lbl_Monedas.Location = new Point(20, 101);
            lbl_Monedas.Name = "lbl_Monedas";
            lbl_Monedas.Size = new Size(100, 15);
            lbl_Monedas.TabIndex = 26;
            lbl_Monedas.Text = "Moneda asociada";
            // 
            // TariffsImporterAddTariff
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(417, 230);
            Controls.Add(lbl_Monedas);
            Controls.Add(cmb_Monedas);
            Controls.Add(lbl_Tarifa);
            Controls.Add(txt_TariffName);
            Controls.Add(btn_Aceptar);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "TariffsImporterAddTariff";
            StartPosition = FormStartPosition.CenterScreen;
            Load += TariffsImporterAddTariff_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btn_Aceptar;
        private TextBox txt_TariffName;
        private Label lbl_Tarifa;
        private ComboBox cmb_Monedas;
        private Label lbl_Monedas;
    }
}