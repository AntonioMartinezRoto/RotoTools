namespace RotoTools
{
    partial class TariffsImporterMenu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TariffsImporterMenu));
            statusStrip1 = new StatusStrip();
            lbl_Conexion = new ToolStripStatusLabel();
            lbl_Tarifa = new Label();
            cmb_Tarifas = new ComboBox();
            lbl_Fichero = new Label();
            btn_LoadTariff = new Button();
            btn_ImportTariff = new Button();
            progress_Install = new ProgressBar();
            btn_AddTariff = new Button();
            groupBox1 = new GroupBox();
            groupBox2 = new GroupBox();
            statusStrip1.SuspendLayout();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.Transparent;
            statusStrip1.Items.AddRange(new ToolStripItem[] { lbl_Conexion });
            statusStrip1.Location = new Point(0, 317);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(640, 22);
            statusStrip1.SizingGrip = false;
            statusStrip1.TabIndex = 0;
            statusStrip1.Text = "statusStrip";
            // 
            // lbl_Conexion
            // 
            lbl_Conexion.Name = "lbl_Conexion";
            lbl_Conexion.Size = new Size(0, 17);
            // 
            // lbl_Tarifa
            // 
            lbl_Tarifa.AutoSize = true;
            lbl_Tarifa.BackColor = Color.Transparent;
            lbl_Tarifa.Location = new Point(10, 32);
            lbl_Tarifa.Name = "lbl_Tarifa";
            lbl_Tarifa.Size = new Size(35, 15);
            lbl_Tarifa.TabIndex = 4;
            lbl_Tarifa.Text = "Tarifa";
            // 
            // cmb_Tarifas
            // 
            cmb_Tarifas.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb_Tarifas.Font = new Font("Calibri", 9F);
            cmb_Tarifas.FormattingEnabled = true;
            cmb_Tarifas.Location = new Point(87, 28);
            cmb_Tarifas.Name = "cmb_Tarifas";
            cmb_Tarifas.Size = new Size(212, 22);
            cmb_Tarifas.TabIndex = 6;
            // 
            // lbl_Fichero
            // 
            lbl_Fichero.BackColor = Color.Transparent;
            lbl_Fichero.Location = new Point(53, 25);
            lbl_Fichero.Name = "lbl_Fichero";
            lbl_Fichero.Size = new Size(417, 37);
            lbl_Fichero.TabIndex = 8;
            lbl_Fichero.Text = "Seleccionar archivo";
            lbl_Fichero.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btn_LoadTariff
            // 
            btn_LoadTariff.BackgroundImage = (Image)resources.GetObject("btn_LoadTariff.BackgroundImage");
            btn_LoadTariff.BackgroundImageLayout = ImageLayout.Stretch;
            btn_LoadTariff.Location = new Point(7, 22);
            btn_LoadTariff.Name = "btn_LoadTariff";
            btn_LoadTariff.Size = new Size(40, 40);
            btn_LoadTariff.TabIndex = 7;
            btn_LoadTariff.UseVisualStyleBackColor = true;
            btn_LoadTariff.Click += btn_LoadTariff_Click;
            // 
            // btn_ImportTariff
            // 
            btn_ImportTariff.Image = (Image)resources.GetObject("btn_ImportTariff.Image");
            btn_ImportTariff.Location = new Point(424, 262);
            btn_ImportTariff.Margin = new Padding(0);
            btn_ImportTariff.Name = "btn_ImportTariff";
            btn_ImportTariff.Size = new Size(87, 41);
            btn_ImportTariff.TabIndex = 9;
            btn_ImportTariff.Text = "Guardar";
            btn_ImportTariff.TextAlign = ContentAlignment.MiddleRight;
            btn_ImportTariff.TextImageRelation = TextImageRelation.ImageBeforeText;
            btn_ImportTariff.UseVisualStyleBackColor = true;
            btn_ImportTariff.Click += btn_ImportTariff_Click;
            // 
            // progress_Install
            // 
            progress_Install.Location = new Point(28, 223);
            progress_Install.Margin = new Padding(3, 2, 3, 2);
            progress_Install.Name = "progress_Install";
            progress_Install.Size = new Size(483, 22);
            progress_Install.TabIndex = 31;
            progress_Install.Visible = false;
            // 
            // btn_AddTariff
            // 
            btn_AddTariff.BackgroundImage = (Image)resources.GetObject("btn_AddTariff.BackgroundImage");
            btn_AddTariff.BackgroundImageLayout = ImageLayout.Stretch;
            btn_AddTariff.Location = new Point(305, 28);
            btn_AddTariff.Name = "btn_AddTariff";
            btn_AddTariff.Size = new Size(25, 22);
            btn_AddTariff.TabIndex = 32;
            btn_AddTariff.UseVisualStyleBackColor = true;
            btn_AddTariff.Click += btn_AddTariff_Click;
            // 
            // groupBox1
            // 
            groupBox1.BackColor = Color.Transparent;
            groupBox1.Controls.Add(btn_LoadTariff);
            groupBox1.Controls.Add(lbl_Fichero);
            groupBox1.Location = new Point(26, 52);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(485, 79);
            groupBox1.TabIndex = 33;
            groupBox1.TabStop = false;
            groupBox1.Text = "Archivo";
            // 
            // groupBox2
            // 
            groupBox2.BackColor = Color.Transparent;
            groupBox2.Controls.Add(cmb_Tarifas);
            groupBox2.Controls.Add(lbl_Tarifa);
            groupBox2.Controls.Add(btn_AddTariff);
            groupBox2.Location = new Point(28, 142);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(483, 68);
            groupBox2.TabIndex = 34;
            groupBox2.TabStop = false;
            // 
            // TariffsImporterMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(640, 339);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(progress_Install);
            Controls.Add(btn_ImportTariff);
            Controls.Add(statusStrip1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "TariffsImporterMenu";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Cargar precios";
            Load += TariffsImporterMenu_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lbl_Conexion;
        private Label lbl_Tarifa;
        private ComboBox cmb_Tarifas;
        private Label lbl_Fichero;
        private Button btn_LoadTariff;
        private Button btn_ImportTariff;
        private ProgressBar progress_Install;
        private Button btn_AddTariff;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
    }
}