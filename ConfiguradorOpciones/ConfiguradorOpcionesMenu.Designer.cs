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
            label1 = new Label();
            btn_ImportConfigCliente = new Button();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // lbl_ConfigOpciones
            // 
            lbl_ConfigOpciones.AutoSize = true;
            lbl_ConfigOpciones.BackColor = Color.Transparent;
            lbl_ConfigOpciones.Location = new Point(109, 70);
            lbl_ConfigOpciones.Name = "lbl_ConfigOpciones";
            lbl_ConfigOpciones.Size = new Size(212, 20);
            lbl_ConfigOpciones.TabIndex = 5;
            lbl_ConfigOpciones.Text = "Configurar y guardar Opciones";
            // 
            // btn_ConfigOpciones
            // 
            btn_ConfigOpciones.BackgroundImage = (Image)resources.GetObject("btn_ConfigOpciones.BackgroundImage");
            btn_ConfigOpciones.BackgroundImageLayout = ImageLayout.Stretch;
            btn_ConfigOpciones.Location = new Point(49, 52);
            btn_ConfigOpciones.Margin = new Padding(3, 4, 3, 4);
            btn_ConfigOpciones.Name = "btn_ConfigOpciones";
            btn_ConfigOpciones.Size = new Size(54, 53);
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
            // lbl_RestoreOptions
            // 
            lbl_RestoreOptions.AutoSize = true;
            lbl_RestoreOptions.BackColor = Color.Transparent;
            lbl_RestoreOptions.Location = new Point(109, 151);
            lbl_RestoreOptions.Name = "lbl_RestoreOptions";
            lbl_RestoreOptions.Size = new Size(166, 20);
            lbl_RestoreOptions.TabIndex = 10;
            lbl_RestoreOptions.Text = "Restaurar configuración";
            // 
            // btn_Restore
            // 
            btn_Restore.BackgroundImage = (Image)resources.GetObject("btn_Restore.BackgroundImage");
            btn_Restore.BackgroundImageLayout = ImageLayout.Stretch;
            btn_Restore.Location = new Point(49, 134);
            btn_Restore.Margin = new Padding(3, 4, 3, 4);
            btn_Restore.Name = "btn_Restore";
            btn_Restore.Size = new Size(54, 53);
            btn_Restore.TabIndex = 9;
            btn_Restore.UseVisualStyleBackColor = true;
            btn_Restore.Click += btn_Restore_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Location = new Point(109, 233);
            label1.Name = "label1";
            label1.Size = new Size(231, 20);
            label1.TabIndex = 12;
            label1.Text = "Importar configuración de cliente";
            // 
            // btn_ImportConfigCliente
            // 
            btn_ImportConfigCliente.BackgroundImage = (Image)resources.GetObject("btn_ImportConfigCliente.BackgroundImage");
            btn_ImportConfigCliente.BackgroundImageLayout = ImageLayout.Stretch;
            btn_ImportConfigCliente.Location = new Point(49, 215);
            btn_ImportConfigCliente.Margin = new Padding(3, 4, 3, 4);
            btn_ImportConfigCliente.Name = "btn_ImportConfigCliente";
            btn_ImportConfigCliente.Size = new Size(54, 53);
            btn_ImportConfigCliente.TabIndex = 11;
            btn_ImportConfigCliente.UseVisualStyleBackColor = true;
            btn_ImportConfigCliente.Click += btn_ImportConfigCliente_Click;
            // 
            // ConfiguradorOpcionesMenu
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(583, 341);
            Controls.Add(label1);
            Controls.Add(btn_ImportConfigCliente);
            Controls.Add(lbl_RestoreOptions);
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
        private Label lbl_RestoreOptions;
        private Button btn_Restore;
        private Label label1;
        private Button btn_ImportConfigCliente;
    }
}