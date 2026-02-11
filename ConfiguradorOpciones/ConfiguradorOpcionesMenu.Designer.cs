namespace RotoTools
{
    partial class ConfiguradorOpcionesMenu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfiguradorOpcionesMenu));
            lbl_ConfigOpciones = new Label();
            btn_ConfigOpciones = new Button();
            statusStrip1 = new StatusStrip();
            lbl_Conexion = new ToolStripStatusLabel();
            lbl_RestoreOptions = new Label();
            btn_Restore = new Button();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // lbl_ConfigOpciones
            // 
            lbl_ConfigOpciones.AutoSize = true;
            lbl_ConfigOpciones.BackColor = Color.Transparent;
            lbl_ConfigOpciones.Location = new Point(95, 80);
            lbl_ConfigOpciones.Name = "lbl_ConfigOpciones";
            lbl_ConfigOpciones.Size = new Size(170, 15);
            lbl_ConfigOpciones.TabIndex = 5;
            lbl_ConfigOpciones.Text = "Configurar y guardar Opciones";
            // 
            // btn_ConfigOpciones
            // 
            btn_ConfigOpciones.BackgroundImage = (Image)resources.GetObject("btn_ConfigOpciones.BackgroundImage");
            btn_ConfigOpciones.BackgroundImageLayout = ImageLayout.Stretch;
            btn_ConfigOpciones.Location = new Point(43, 67);
            btn_ConfigOpciones.Name = "btn_ConfigOpciones";
            btn_ConfigOpciones.Size = new Size(47, 40);
            btn_ConfigOpciones.TabIndex = 4;
            btn_ConfigOpciones.UseVisualStyleBackColor = true;
            btn_ConfigOpciones.Click += btn_ConfigOpciones_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.Transparent;
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { lbl_Conexion });
            statusStrip1.Location = new Point(0, 234);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(510, 22);
            statusStrip1.SizingGrip = false;
            statusStrip1.TabIndex = 6;
            statusStrip1.Text = "statusStrip1";
            // 
            // lbl_Conexion
            // 
            lbl_Conexion.Name = "lbl_Conexion";
            lbl_Conexion.Size = new Size(118, 17);
            lbl_Conexion.Text = "toolStripStatusLabel1";
            // 
            // lbl_RestoreOptions
            // 
            lbl_RestoreOptions.AutoSize = true;
            lbl_RestoreOptions.BackColor = Color.Transparent;
            lbl_RestoreOptions.Location = new Point(95, 138);
            lbl_RestoreOptions.Name = "lbl_RestoreOptions";
            lbl_RestoreOptions.Size = new Size(133, 15);
            lbl_RestoreOptions.TabIndex = 10;
            lbl_RestoreOptions.Text = "Restaurar configuración";
            // 
            // btn_Restore
            // 
            btn_Restore.BackgroundImage = (Image)resources.GetObject("btn_Restore.BackgroundImage");
            btn_Restore.BackgroundImageLayout = ImageLayout.Stretch;
            btn_Restore.Location = new Point(43, 125);
            btn_Restore.Name = "btn_Restore";
            btn_Restore.Size = new Size(47, 40);
            btn_Restore.TabIndex = 9;
            btn_Restore.UseVisualStyleBackColor = true;
            btn_Restore.Click += btn_Restore_Click;
            // 
            // ConfiguradorOpcionesMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(510, 256);
            Controls.Add(lbl_RestoreOptions);
            Controls.Add(btn_Restore);
            Controls.Add(statusStrip1);
            Controls.Add(lbl_ConfigOpciones);
            Controls.Add(btn_ConfigOpciones);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "ConfiguradorOpcionesMenu";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Menú de configurador de opciones";
            Load += ConfiguradorOpciones_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lbl_ConfigOpciones;
        private Button btn_ConfigOpciones;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lbl_Conexion;
        private Label lbl_RestoreOptions;
        private Button btn_Restore;
    }
}