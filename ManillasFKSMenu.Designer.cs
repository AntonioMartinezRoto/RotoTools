namespace RotoTools
{
    partial class ManillasFKSMenu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManillasFKSMenu));
            statusStrip1 = new StatusStrip();
            lbl_Conexion = new ToolStripStatusLabel();
            btn_SaveFKS = new Button();
            groupBoxFKS = new GroupBox();
            rb_NormalizadaYFks = new RadioButton();
            rb_SoloFks = new RadioButton();
            rb_Normalizada = new RadioButton();
            cmb_HardwareSupplier = new ComboBox();
            label1 = new Label();
            progress_Export = new ProgressBar();
            statusStrip1.SuspendLayout();
            groupBoxFKS.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.Transparent;
            statusStrip1.Items.AddRange(new ToolStripItem[] { lbl_Conexion });
            statusStrip1.Location = new Point(0, 269);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(519, 22);
            statusStrip1.SizingGrip = false;
            statusStrip1.TabIndex = 0;
            // 
            // lbl_Conexion
            // 
            lbl_Conexion.Name = "lbl_Conexion";
            lbl_Conexion.Size = new Size(118, 17);
            lbl_Conexion.Text = "toolStripStatusLabel1";
            // 
            // btn_SaveFKS
            // 
            btn_SaveFKS.Image = (Image)resources.GetObject("btn_SaveFKS.Image");
            btn_SaveFKS.ImageAlign = ContentAlignment.MiddleLeft;
            btn_SaveFKS.Location = new Point(316, 214);
            btn_SaveFKS.Margin = new Padding(9, 3, 3, 3);
            btn_SaveFKS.Name = "btn_SaveFKS";
            btn_SaveFKS.Padding = new Padding(2, 0, 0, 0);
            btn_SaveFKS.Size = new Size(87, 41);
            btn_SaveFKS.TabIndex = 2;
            btn_SaveFKS.Text = "Guardar";
            btn_SaveFKS.TextAlign = ContentAlignment.MiddleRight;
            btn_SaveFKS.TextImageRelation = TextImageRelation.ImageBeforeText;
            btn_SaveFKS.UseVisualStyleBackColor = true;
            btn_SaveFKS.Click += btn_SaveFKS_Click;
            // 
            // groupBoxFKS
            // 
            groupBoxFKS.BackColor = Color.Transparent;
            groupBoxFKS.Controls.Add(rb_NormalizadaYFks);
            groupBoxFKS.Controls.Add(rb_SoloFks);
            groupBoxFKS.Controls.Add(rb_Normalizada);
            groupBoxFKS.Location = new Point(47, 79);
            groupBoxFKS.Name = "groupBoxFKS";
            groupBoxFKS.Size = new Size(358, 122);
            groupBoxFKS.TabIndex = 3;
            groupBoxFKS.TabStop = false;
            groupBoxFKS.Text = "Seleccionar configuración";
            // 
            // rb_NormalizadaYFks
            // 
            rb_NormalizadaYFks.AutoSize = true;
            rb_NormalizadaYFks.Location = new Point(32, 82);
            rb_NormalizadaYFks.Name = "rb_NormalizadaYFks";
            rb_NormalizadaYFks.Size = new Size(249, 19);
            rb_NormalizadaYFks.TabIndex = 2;
            rb_NormalizadaYFks.TabStop = true;
            rb_NormalizadaYFks.Text = "Configuración normalizada + manillas FKS";
            rb_NormalizadaYFks.UseVisualStyleBackColor = true;
            // 
            // rb_SoloFks
            // 
            rb_SoloFks.AutoSize = true;
            rb_SoloFks.Location = new Point(32, 57);
            rb_SoloFks.Name = "rb_SoloFks";
            rb_SoloFks.Size = new Size(117, 19);
            rb_SoloFks.TabIndex = 1;
            rb_SoloFks.TabStop = true;
            rb_SoloFks.Text = "Solo manillas FKS";
            rb_SoloFks.UseVisualStyleBackColor = true;
            // 
            // rb_Normalizada
            // 
            rb_Normalizada.AutoSize = true;
            rb_Normalizada.Location = new Point(32, 32);
            rb_Normalizada.Name = "rb_Normalizada";
            rb_Normalizada.Size = new Size(169, 19);
            rb_Normalizada.TabIndex = 0;
            rb_Normalizada.TabStop = true;
            rb_Normalizada.Text = "Configuración normalizada";
            rb_Normalizada.UseVisualStyleBackColor = true;
            // 
            // cmb_HardwareSupplier
            // 
            cmb_HardwareSupplier.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb_HardwareSupplier.FormattingEnabled = true;
            cmb_HardwareSupplier.Location = new Point(159, 41);
            cmb_HardwareSupplier.Name = "cmb_HardwareSupplier";
            cmb_HardwareSupplier.Size = new Size(165, 23);
            cmb_HardwareSupplier.TabIndex = 12;
            cmb_HardwareSupplier.SelectedValueChanged += cmb_HardwareSupplier_SelectedValueChanged;
            // 
            // label1
            // 
            label1.BackColor = Color.Transparent;
            label1.Location = new Point(47, 44);
            label1.Name = "label1";
            label1.Size = new Size(106, 20);
            label1.TabIndex = 11;
            label1.Text = "Hardware Supplier";
            // 
            // progress_Export
            // 
            progress_Export.Location = new Point(47, 224);
            progress_Export.Margin = new Padding(3, 2, 3, 2);
            progress_Export.Name = "progress_Export";
            progress_Export.Size = new Size(257, 22);
            progress_Export.TabIndex = 14;
            progress_Export.Visible = false;
            // 
            // ManillasFKSMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(519, 291);
            Controls.Add(progress_Export);
            Controls.Add(cmb_HardwareSupplier);
            Controls.Add(label1);
            Controls.Add(groupBoxFKS);
            Controls.Add(btn_SaveFKS);
            Controls.Add(statusStrip1);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "ManillasFKSMenu";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Configuración Manillas FKS";
            Load += ManillasFKSMenu_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            groupBoxFKS.ResumeLayout(false);
            groupBoxFKS.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lbl_Conexion;
        private Button btn_SaveFKS;
        private GroupBox groupBoxFKS;
        private RadioButton rb_SoloFks;
        private RadioButton rb_Normalizada;
        private RadioButton rb_NormalizadaYFks;
        private ComboBox cmb_HardwareSupplier;
        private Label label1;
        private ProgressBar progress_Export;
    }
}