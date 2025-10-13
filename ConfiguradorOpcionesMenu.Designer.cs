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
            label2 = new Label();
            btn_Restore = new Button();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // lbl_ConfigOpciones
            // 
            lbl_ConfigOpciones.AutoSize = true;
            lbl_ConfigOpciones.BackColor = Color.Transparent;
            lbl_ConfigOpciones.Location = new Point(109, 106);
            lbl_ConfigOpciones.Name = "lbl_ConfigOpciones";
            lbl_ConfigOpciones.Size = new Size(212, 20);
            lbl_ConfigOpciones.TabIndex = 5;
            lbl_ConfigOpciones.Text = "Configurar y guardar Opciones";
            // 
            // btn_ConfigOpciones
            // 
            btn_ConfigOpciones.BackgroundImage = (Image)resources.GetObject("btn_ConfigOpciones.BackgroundImage");
            btn_ConfigOpciones.BackgroundImageLayout = ImageLayout.Stretch;
            btn_ConfigOpciones.Location = new Point(49, 89);
            btn_ConfigOpciones.Margin = new Padding(3, 4, 3, 4);
            btn_ConfigOpciones.Name = "btn_ConfigOpciones";
            btn_ConfigOpciones.Size = new Size(54, 54);
            btn_ConfigOpciones.TabIndex = 4;
            btn_ConfigOpciones.UseVisualStyleBackColor = true;
            btn_ConfigOpciones.Click += btn_ConfigOpciones_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.Transparent;
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { lbl_Conexion });
            statusStrip1.Location = new Point(0, 315);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new Padding(1, 0, 16, 0);
            statusStrip1.Size = new Size(583, 26);
            statusStrip1.SizingGrip = false;
            statusStrip1.TabIndex = 6;
            statusStrip1.Text = "statusStrip1";
            // 
            // lbl_Conexion
            // 
            lbl_Conexion.Name = "lbl_Conexion";
            lbl_Conexion.Size = new Size(151, 20);
            lbl_Conexion.Text = "toolStripStatusLabel1";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Location = new Point(109, 184);
            label2.Name = "label2";
            label2.Size = new Size(166, 20);
            label2.TabIndex = 10;
            label2.Text = "Restaurar configuración";
            // 
            // btn_Restore
            // 
            btn_Restore.BackgroundImage = (Image)resources.GetObject("btn_Restore.BackgroundImage");
            btn_Restore.BackgroundImageLayout = ImageLayout.Stretch;
            btn_Restore.Location = new Point(49, 167);
            btn_Restore.Margin = new Padding(3, 4, 3, 4);
            btn_Restore.Name = "btn_Restore";
            btn_Restore.Size = new Size(54, 54);
            btn_Restore.TabIndex = 9;
            btn_Restore.UseVisualStyleBackColor = true;
            btn_Restore.Click += btn_Restore_Click;
            // 
            // ConfiguradorOpcionesMenu
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(583, 341);
            Controls.Add(label2);
            Controls.Add(btn_Restore);
            Controls.Add(statusStrip1);
            Controls.Add(lbl_ConfigOpciones);
            Controls.Add(btn_ConfigOpciones);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
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
        private Label label2;
        private Button btn_Restore;
    }
}