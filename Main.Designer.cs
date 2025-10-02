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
            label1 = new Label();
            btn_Export = new Button();
            lbl_Export = new Label();
            btn_Conector = new Button();
            lbl_Conector = new Label();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.Transparent;
            statusStrip1.Items.AddRange(new ToolStripItem[] { lbl_Conexion });
            statusStrip1.Location = new Point(0, 287);
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
            btn_Refresh.Location = new Point(12, 12);
            btn_Refresh.Name = "btn_Refresh";
            btn_Refresh.Size = new Size(32, 29);
            btn_Refresh.TabIndex = 1;
            btn_Refresh.UseVisualStyleBackColor = true;
            btn_Refresh.Click += btn_Refresh_Click;
            // 
            // btn_ConfigOpciones
            // 
            btn_ConfigOpciones.BackgroundImage = (Image)resources.GetObject("btn_ConfigOpciones.BackgroundImage");
            btn_ConfigOpciones.BackgroundImageLayout = ImageLayout.Stretch;
            btn_ConfigOpciones.Location = new Point(54, 89);
            btn_ConfigOpciones.Name = "btn_ConfigOpciones";
            btn_ConfigOpciones.Size = new Size(40, 40);
            btn_ConfigOpciones.TabIndex = 2;
            btn_ConfigOpciones.UseVisualStyleBackColor = true;
            btn_ConfigOpciones.Click += btn_ConfigOpciones_Click;
            // 
            // lbl_ConfigOpciones
            // 
            lbl_ConfigOpciones.AutoSize = true;
            lbl_ConfigOpciones.BackColor = Color.Transparent;
            lbl_ConfigOpciones.Location = new Point(100, 102);
            lbl_ConfigOpciones.Name = "lbl_ConfigOpciones";
            lbl_ConfigOpciones.Size = new Size(145, 15);
            lbl_ConfigOpciones.TabIndex = 3;
            lbl_ConfigOpciones.Text = "Configurador de opciones";
            // 
            // btn_Actualizador
            // 
            btn_Actualizador.BackgroundImage = (Image)resources.GetObject("btn_Actualizador.BackgroundImage");
            btn_Actualizador.BackgroundImageLayout = ImageLayout.Stretch;
            btn_Actualizador.Location = new Point(54, 148);
            btn_Actualizador.Name = "btn_Actualizador";
            btn_Actualizador.Size = new Size(40, 40);
            btn_Actualizador.TabIndex = 4;
            btn_Actualizador.UseVisualStyleBackColor = true;
            btn_Actualizador.Click += btn_Actualizador_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Location = new Point(100, 161);
            label1.Name = "label1";
            label1.Size = new Size(78, 15);
            label1.TabIndex = 5;
            label1.Text = "Actualización";
            // 
            // btn_Export
            // 
            btn_Export.BackgroundImage = (Image)resources.GetObject("btn_Export.BackgroundImage");
            btn_Export.BackgroundImageLayout = ImageLayout.Stretch;
            btn_Export.Location = new Point(313, 89);
            btn_Export.Name = "btn_Export";
            btn_Export.Size = new Size(40, 40);
            btn_Export.TabIndex = 6;
            btn_Export.UseVisualStyleBackColor = true;
            // 
            // lbl_Export
            // 
            lbl_Export.AutoSize = true;
            lbl_Export.BackColor = Color.Transparent;
            lbl_Export.Location = new Point(359, 102);
            lbl_Export.Name = "lbl_Export";
            lbl_Export.Size = new Size(83, 15);
            lbl_Export.TabIndex = 7;
            lbl_Export.Text = "Exportar datos";
            // 
            // btn_Conector
            // 
            btn_Conector.BackgroundImage = (Image)resources.GetObject("btn_Conector.BackgroundImage");
            btn_Conector.BackgroundImageLayout = ImageLayout.Stretch;
            btn_Conector.Location = new Point(313, 148);
            btn_Conector.Name = "btn_Conector";
            btn_Conector.Size = new Size(40, 40);
            btn_Conector.TabIndex = 8;
            btn_Conector.UseVisualStyleBackColor = true;
            btn_Conector.Click += btn_Conector_Click;
            // 
            // lbl_Conector
            // 
            lbl_Conector.AutoSize = true;
            lbl_Conector.BackColor = Color.Transparent;
            lbl_Conector.Location = new Point(359, 161);
            lbl_Conector.Name = "lbl_Conector";
            lbl_Conector.Size = new Size(195, 15);
            lbl_Conector.TabIndex = 9;
            lbl_Conector.Text = "Herramientas del Conector Herrajes";
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(696, 309);
            Controls.Add(lbl_Conector);
            Controls.Add(btn_Conector);
            Controls.Add(lbl_Export);
            Controls.Add(btn_Export);
            Controls.Add(label1);
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
        private Label label1;
        private Button btn_Export;
        private Label lbl_Export;
        private Button btn_Conector;
        private Label lbl_Conector;
    }
}
