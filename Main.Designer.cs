namespace RotoTools
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            statusStrip1 = new StatusStrip();
            lbl_Conexion = new ToolStripStatusLabel();
            btn_Refresh = new Button();
            btn_ConfigOpciones = new Button();
            lbl_ConfigOpciones = new Label();
            btn_Actualizador = new Button();
            lbl_Actualizacion = new Label();
            btn_Export = new Button();
            lbl_Export = new Label();
            btn_Conector = new Button();
            lbl_Conector = new Label();
            lbl_ControlCambios = new Label();
            btn_ControlCambios = new Button();
            lbl_Traduccion = new Label();
            btn_Traduccion = new Button();
            btn_Config = new Button();
            lbl_ConfigFKS = new Label();
            btn_ManillasFKS = new Button();
            lbl_CAM = new Label();
            btn_CAM = new Button();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.Transparent;
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { lbl_Conexion });
            statusStrip1.Location = new Point(0, 336);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(696, 22);
            statusStrip1.SizingGrip = false;
            statusStrip1.TabIndex = 0;
            statusStrip1.Text = "statusStrip1";
            // 
            // lbl_Conexion
            // 
            lbl_Conexion.Name = "lbl_Conexion";
            lbl_Conexion.Size = new Size(118, 17);
            lbl_Conexion.Text = "toolStripStatusLabel1";
            // 
            // btn_Refresh
            // 
            btn_Refresh.BackgroundImage = (Image)resources.GetObject("btn_Refresh.BackgroundImage");
            btn_Refresh.BackgroundImageLayout = ImageLayout.Stretch;
            btn_Refresh.Location = new Point(50, 11);
            btn_Refresh.Name = "btn_Refresh";
            btn_Refresh.Size = new Size(32, 32);
            btn_Refresh.TabIndex = 1;
            btn_Refresh.UseVisualStyleBackColor = true;
            btn_Refresh.Click += btn_Refresh_Click;
            // 
            // btn_ConfigOpciones
            // 
            btn_ConfigOpciones.BackgroundImage = (Image)resources.GetObject("btn_ConfigOpciones.BackgroundImage");
            btn_ConfigOpciones.BackgroundImageLayout = ImageLayout.Stretch;
            btn_ConfigOpciones.Location = new Point(59, 86);
            btn_ConfigOpciones.Name = "btn_ConfigOpciones";
            btn_ConfigOpciones.Size = new Size(47, 40);
            btn_ConfigOpciones.TabIndex = 2;
            btn_ConfigOpciones.UseVisualStyleBackColor = true;
            btn_ConfigOpciones.Click += btn_ConfigOpciones_Click;
            // 
            // lbl_ConfigOpciones
            // 
            lbl_ConfigOpciones.AutoSize = true;
            lbl_ConfigOpciones.BackColor = Color.Transparent;
            lbl_ConfigOpciones.Location = new Point(111, 98);
            lbl_ConfigOpciones.Name = "lbl_ConfigOpciones";
            lbl_ConfigOpciones.Size = new Size(145, 15);
            lbl_ConfigOpciones.TabIndex = 3;
            lbl_ConfigOpciones.Text = "Configurador de opciones";
            // 
            // btn_Actualizador
            // 
            btn_Actualizador.BackgroundImage = (Image)resources.GetObject("btn_Actualizador.BackgroundImage");
            btn_Actualizador.BackgroundImageLayout = ImageLayout.Stretch;
            btn_Actualizador.Location = new Point(59, 144);
            btn_Actualizador.Name = "btn_Actualizador";
            btn_Actualizador.Size = new Size(47, 40);
            btn_Actualizador.TabIndex = 4;
            btn_Actualizador.UseVisualStyleBackColor = true;
            btn_Actualizador.Click += btn_Actualizador_Click;
            // 
            // lbl_Actualizacion
            // 
            lbl_Actualizacion.AutoSize = true;
            lbl_Actualizacion.BackColor = Color.Transparent;
            lbl_Actualizacion.Location = new Point(111, 158);
            lbl_Actualizacion.Name = "lbl_Actualizacion";
            lbl_Actualizacion.Size = new Size(78, 15);
            lbl_Actualizacion.TabIndex = 5;
            lbl_Actualizacion.Text = "Actualización";
            // 
            // btn_Export
            // 
            btn_Export.BackgroundImage = (Image)resources.GetObject("btn_Export.BackgroundImage");
            btn_Export.BackgroundImageLayout = ImageLayout.Center;
            btn_Export.Location = new Point(59, 269);
            btn_Export.Name = "btn_Export";
            btn_Export.Size = new Size(47, 40);
            btn_Export.TabIndex = 6;
            btn_Export.UseVisualStyleBackColor = true;
            btn_Export.Click += btn_Export_Click;
            // 
            // lbl_Export
            // 
            lbl_Export.AutoSize = true;
            lbl_Export.BackColor = Color.Transparent;
            lbl_Export.Location = new Point(111, 281);
            lbl_Export.Name = "lbl_Export";
            lbl_Export.Size = new Size(83, 15);
            lbl_Export.TabIndex = 7;
            lbl_Export.Text = "Exportar datos";
            // 
            // btn_Conector
            // 
            btn_Conector.BackgroundImage = (Image)resources.GetObject("btn_Conector.BackgroundImage");
            btn_Conector.BackgroundImageLayout = ImageLayout.Center;
            btn_Conector.Location = new Point(318, 144);
            btn_Conector.Name = "btn_Conector";
            btn_Conector.Size = new Size(47, 40);
            btn_Conector.TabIndex = 8;
            btn_Conector.UseVisualStyleBackColor = true;
            btn_Conector.Click += btn_Conector_Click;
            // 
            // lbl_Conector
            // 
            lbl_Conector.AutoSize = true;
            lbl_Conector.BackColor = Color.Transparent;
            lbl_Conector.Location = new Point(370, 157);
            lbl_Conector.Name = "lbl_Conector";
            lbl_Conector.Size = new Size(111, 15);
            lbl_Conector.TabIndex = 9;
            lbl_Conector.Text = "Conector de herraje";
            // 
            // lbl_ControlCambios
            // 
            lbl_ControlCambios.AutoSize = true;
            lbl_ControlCambios.BackColor = Color.Transparent;
            lbl_ControlCambios.Location = new Point(370, 100);
            lbl_ControlCambios.Name = "lbl_ControlCambios";
            lbl_ControlCambios.Size = new Size(111, 15);
            lbl_ControlCambios.TabIndex = 11;
            lbl_ControlCambios.Text = "Control de cambios";
            // 
            // btn_ControlCambios
            // 
            btn_ControlCambios.BackgroundImage = (Image)resources.GetObject("btn_ControlCambios.BackgroundImage");
            btn_ControlCambios.BackgroundImageLayout = ImageLayout.Center;
            btn_ControlCambios.Location = new Point(318, 86);
            btn_ControlCambios.Name = "btn_ControlCambios";
            btn_ControlCambios.Size = new Size(47, 40);
            btn_ControlCambios.TabIndex = 10;
            btn_ControlCambios.UseVisualStyleBackColor = true;
            btn_ControlCambios.Click += btn_ControlCambios_Click;
            // 
            // lbl_Traduccion
            // 
            lbl_Traduccion.AutoSize = true;
            lbl_Traduccion.BackColor = Color.Transparent;
            lbl_Traduccion.Location = new Point(371, 220);
            lbl_Traduccion.Name = "lbl_Traduccion";
            lbl_Traduccion.Size = new Size(65, 15);
            lbl_Traduccion.TabIndex = 13;
            lbl_Traduccion.Text = "Traducción";
            // 
            // btn_Traduccion
            // 
            btn_Traduccion.BackgroundImage = (Image)resources.GetObject("btn_Traduccion.BackgroundImage");
            btn_Traduccion.BackgroundImageLayout = ImageLayout.Stretch;
            btn_Traduccion.Location = new Point(318, 207);
            btn_Traduccion.Name = "btn_Traduccion";
            btn_Traduccion.Size = new Size(47, 40);
            btn_Traduccion.TabIndex = 12;
            btn_Traduccion.UseVisualStyleBackColor = true;
            btn_Traduccion.Click += btn_Traduccion_Click;
            // 
            // btn_Config
            // 
            btn_Config.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_Config.BackColor = Color.Transparent;
            btn_Config.BackgroundImage = (Image)resources.GetObject("btn_Config.BackgroundImage");
            btn_Config.BackgroundImageLayout = ImageLayout.Stretch;
            btn_Config.Location = new Point(12, 11);
            btn_Config.Margin = new Padding(3, 2, 3, 2);
            btn_Config.Name = "btn_Config";
            btn_Config.Size = new Size(32, 32);
            btn_Config.TabIndex = 14;
            btn_Config.UseVisualStyleBackColor = false;
            btn_Config.Click += btn_Config_Click;
            // 
            // lbl_ConfigFKS
            // 
            lbl_ConfigFKS.AutoSize = true;
            lbl_ConfigFKS.BackColor = Color.Transparent;
            lbl_ConfigFKS.Location = new Point(112, 220);
            lbl_ConfigFKS.Name = "lbl_ConfigFKS";
            lbl_ConfigFKS.Size = new Size(152, 15);
            lbl_ConfigFKS.TabIndex = 13;
            lbl_ConfigFKS.Text = "Configuración Manillas FKS";
            // 
            // btn_ManillasFKS
            // 
            btn_ManillasFKS.BackgroundImage = (Image)resources.GetObject("btn_ManillasFKS.BackgroundImage");
            btn_ManillasFKS.BackgroundImageLayout = ImageLayout.Stretch;
            btn_ManillasFKS.Location = new Point(59, 207);
            btn_ManillasFKS.Name = "btn_ManillasFKS";
            btn_ManillasFKS.Size = new Size(47, 40);
            btn_ManillasFKS.TabIndex = 12;
            btn_ManillasFKS.UseVisualStyleBackColor = true;
            btn_ManillasFKS.Click += btn_ManillasFKS_Click;
            // 
            // lbl_CAM
            // 
            lbl_CAM.AutoSize = true;
            lbl_CAM.BackColor = Color.Transparent;
            lbl_CAM.Location = new Point(371, 281);
            lbl_CAM.Name = "lbl_CAM";
            lbl_CAM.Size = new Size(34, 15);
            lbl_CAM.TabIndex = 16;
            lbl_CAM.Text = "CAM";
            // 
            // btn_CAM
            // 
            btn_CAM.BackgroundImage = (Image)resources.GetObject("btn_CAM.BackgroundImage");
            btn_CAM.BackgroundImageLayout = ImageLayout.Stretch;
            btn_CAM.Location = new Point(318, 268);
            btn_CAM.Name = "btn_CAM";
            btn_CAM.Size = new Size(47, 40);
            btn_CAM.TabIndex = 15;
            btn_CAM.UseVisualStyleBackColor = true;
            btn_CAM.Click += btn_CAM_Click;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(696, 358);
            Controls.Add(lbl_CAM);
            Controls.Add(btn_CAM);
            Controls.Add(lbl_ConfigFKS);
            Controls.Add(btn_ManillasFKS);
            Controls.Add(btn_Config);
            Controls.Add(lbl_Traduccion);
            Controls.Add(btn_Traduccion);
            Controls.Add(lbl_ControlCambios);
            Controls.Add(btn_ControlCambios);
            Controls.Add(lbl_Conector);
            Controls.Add(btn_Conector);
            Controls.Add(lbl_Export);
            Controls.Add(btn_Export);
            Controls.Add(lbl_Actualizacion);
            Controls.Add(btn_Actualizador);
            Controls.Add(lbl_ConfigOpciones);
            Controls.Add(btn_ConfigOpciones);
            Controls.Add(btn_Refresh);
            Controls.Add(statusStrip1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Main";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Menú";
            Load += Main_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lbl_Conexion;
        private Button btn_Refresh;
        private Button btn_ConfigOpciones;
        private Label lbl_ConfigOpciones;
        private Button btn_Actualizador;
        private Label lbl_Actualizacion;
        private Button btn_Export;
        private Label lbl_Export;
        private Button btn_Conector;
        private Label lbl_Conector;
        private Label lbl_ControlCambios;
        private Button btn_ControlCambios;
        private Label lbl_ConfigFKS;
        private Button btn_ManillasFKS;
        private Label lbl_Traduccion;
        private Button btn_Traduccion;
        private Button btn_Config;
        private Label lbl_CAM;
        private Button btn_CAM;
    }
}
