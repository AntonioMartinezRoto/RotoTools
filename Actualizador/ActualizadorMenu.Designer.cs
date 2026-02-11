namespace RotoTools
{
    partial class ActualizadorMenu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ActualizadorMenu));
            statusStrip1 = new StatusStrip();
            lbl_Conexion = new ToolStripStatusLabel();
            cmb_IdPresupuestado = new ComboBox();
            lbl_IdPresupuestado = new Label();
            groupBox_Grupos = new GroupBox();
            txt_Presupuestado = new TextBox();
            txt_Produccion = new TextBox();
            lbl_IdProduccion = new Label();
            cmb_IdProduccion = new ComboBox();
            groupBox_Proveedor = new GroupBox();
            btn_AddProveedor = new Button();
            txt_Proveedor = new TextBox();
            lbl_Proveedor = new Label();
            cmb_Proveedor = new ComboBox();
            btn_EjecutarScripts = new Button();
            btn_EjecutarCarpeta = new Button();
            btn_OcultaOpciones = new Button();
            btn_ExportarEscandallos = new Button();
            btn_InstalarEscandallos = new Button();
            btn_ShowScripts = new Button();
            statusStrip1.SuspendLayout();
            groupBox_Grupos.SuspendLayout();
            groupBox_Proveedor.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.Transparent;
            statusStrip1.Items.AddRange(new ToolStripItem[] { lbl_Conexion });
            statusStrip1.Location = new Point(0, 324);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(780, 22);
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
            // cmb_IdPresupuestado
            // 
            cmb_IdPresupuestado.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb_IdPresupuestado.Font = new Font("Calibri", 9F);
            cmb_IdPresupuestado.FormattingEnabled = true;
            cmb_IdPresupuestado.Location = new Point(123, 33);
            cmb_IdPresupuestado.Name = "cmb_IdPresupuestado";
            cmb_IdPresupuestado.Size = new Size(202, 22);
            cmb_IdPresupuestado.TabIndex = 1;
            cmb_IdPresupuestado.SelectedIndexChanged += cmb_IdPresupuestado_SelectedIndexChanged;
            // 
            // lbl_IdPresupuestado
            // 
            lbl_IdPresupuestado.AutoSize = true;
            lbl_IdPresupuestado.Location = new Point(12, 36);
            lbl_IdPresupuestado.Name = "lbl_IdPresupuestado";
            lbl_IdPresupuestado.Size = new Size(85, 15);
            lbl_IdPresupuestado.TabIndex = 2;
            lbl_IdPresupuestado.Text = "Presupuestado";
            // 
            // groupBox_Grupos
            // 
            groupBox_Grupos.BackColor = Color.Transparent;
            groupBox_Grupos.Controls.Add(txt_Presupuestado);
            groupBox_Grupos.Controls.Add(txt_Produccion);
            groupBox_Grupos.Controls.Add(lbl_IdProduccion);
            groupBox_Grupos.Controls.Add(cmb_IdProduccion);
            groupBox_Grupos.Controls.Add(lbl_IdPresupuestado);
            groupBox_Grupos.Controls.Add(cmb_IdPresupuestado);
            groupBox_Grupos.Location = new Point(194, 70);
            groupBox_Grupos.Name = "groupBox_Grupos";
            groupBox_Grupos.Size = new Size(425, 108);
            groupBox_Grupos.TabIndex = 3;
            groupBox_Grupos.TabStop = false;
            groupBox_Grupos.Text = "Grupos";
            // 
            // txt_Presupuestado
            // 
            txt_Presupuestado.Font = new Font("Calibri", 9F);
            txt_Presupuestado.Location = new Point(343, 33);
            txt_Presupuestado.Name = "txt_Presupuestado";
            txt_Presupuestado.ReadOnly = true;
            txt_Presupuestado.Size = new Size(36, 22);
            txt_Presupuestado.TabIndex = 9;
            // 
            // txt_Produccion
            // 
            txt_Produccion.Font = new Font("Calibri", 9F);
            txt_Produccion.Location = new Point(343, 68);
            txt_Produccion.Name = "txt_Produccion";
            txt_Produccion.ReadOnly = true;
            txt_Produccion.Size = new Size(36, 22);
            txt_Produccion.TabIndex = 8;
            // 
            // lbl_IdProduccion
            // 
            lbl_IdProduccion.AutoSize = true;
            lbl_IdProduccion.Location = new Point(29, 71);
            lbl_IdProduccion.Name = "lbl_IdProduccion";
            lbl_IdProduccion.Size = new Size(68, 15);
            lbl_IdProduccion.TabIndex = 4;
            lbl_IdProduccion.Text = "Producción";
            // 
            // cmb_IdProduccion
            // 
            cmb_IdProduccion.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb_IdProduccion.Font = new Font("Calibri", 9F);
            cmb_IdProduccion.FormattingEnabled = true;
            cmb_IdProduccion.Location = new Point(123, 68);
            cmb_IdProduccion.Name = "cmb_IdProduccion";
            cmb_IdProduccion.Size = new Size(202, 22);
            cmb_IdProduccion.TabIndex = 3;
            cmb_IdProduccion.SelectedIndexChanged += cmb_IdProduccion_SelectedIndexChanged;
            // 
            // groupBox_Proveedor
            // 
            groupBox_Proveedor.BackColor = Color.Transparent;
            groupBox_Proveedor.Controls.Add(btn_AddProveedor);
            groupBox_Proveedor.Controls.Add(txt_Proveedor);
            groupBox_Proveedor.Controls.Add(lbl_Proveedor);
            groupBox_Proveedor.Controls.Add(cmb_Proveedor);
            groupBox_Proveedor.Location = new Point(194, 202);
            groupBox_Proveedor.Name = "groupBox_Proveedor";
            groupBox_Proveedor.Size = new Size(426, 73);
            groupBox_Proveedor.TabIndex = 4;
            groupBox_Proveedor.TabStop = false;
            groupBox_Proveedor.Text = "Proveedor";
            // 
            // btn_AddProveedor
            // 
            btn_AddProveedor.BackgroundImage = (Image)resources.GetObject("btn_AddProveedor.BackgroundImage");
            btn_AddProveedor.BackgroundImageLayout = ImageLayout.Stretch;
            btn_AddProveedor.Location = new Point(385, 28);
            btn_AddProveedor.Name = "btn_AddProveedor";
            btn_AddProveedor.Size = new Size(25, 22);
            btn_AddProveedor.TabIndex = 8;
            btn_AddProveedor.UseVisualStyleBackColor = true;
            btn_AddProveedor.Click += btn_AddProveedor_Click;
            // 
            // txt_Proveedor
            // 
            txt_Proveedor.Font = new Font("Calibri", 9F);
            txt_Proveedor.Location = new Point(343, 28);
            txt_Proveedor.Name = "txt_Proveedor";
            txt_Proveedor.ReadOnly = true;
            txt_Proveedor.Size = new Size(36, 22);
            txt_Proveedor.TabIndex = 7;
            // 
            // lbl_Proveedor
            // 
            lbl_Proveedor.AutoSize = true;
            lbl_Proveedor.Location = new Point(46, 28);
            lbl_Proveedor.Name = "lbl_Proveedor";
            lbl_Proveedor.Size = new Size(51, 15);
            lbl_Proveedor.TabIndex = 6;
            lbl_Proveedor.Text = "Nombre";
            // 
            // cmb_Proveedor
            // 
            cmb_Proveedor.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb_Proveedor.Font = new Font("Calibri", 9F);
            cmb_Proveedor.FormattingEnabled = true;
            cmb_Proveedor.Location = new Point(123, 28);
            cmb_Proveedor.Name = "cmb_Proveedor";
            cmb_Proveedor.Size = new Size(202, 22);
            cmb_Proveedor.TabIndex = 5;
            cmb_Proveedor.SelectedIndexChanged += cmb_Proveedor_SelectedIndexChanged;
            // 
            // btn_EjecutarScripts
            // 
            btn_EjecutarScripts.BackColor = Color.White;
            btn_EjecutarScripts.Image = (Image)resources.GetObject("btn_EjecutarScripts.Image");
            btn_EjecutarScripts.ImageAlign = ContentAlignment.MiddleLeft;
            btn_EjecutarScripts.Location = new Point(12, 33);
            btn_EjecutarScripts.Name = "btn_EjecutarScripts";
            btn_EjecutarScripts.Padding = new Padding(5, 0, 5, 0);
            btn_EjecutarScripts.Size = new Size(157, 40);
            btn_EjecutarScripts.TabIndex = 5;
            btn_EjecutarScripts.Text = "Ejecutar Scripts";
            btn_EjecutarScripts.TextAlign = ContentAlignment.MiddleRight;
            btn_EjecutarScripts.UseVisualStyleBackColor = false;
            btn_EjecutarScripts.Click += btn_EjecutarScripts_Click;
            // 
            // btn_EjecutarCarpeta
            // 
            btn_EjecutarCarpeta.BackColor = Color.White;
            btn_EjecutarCarpeta.Image = (Image)resources.GetObject("btn_EjecutarCarpeta.Image");
            btn_EjecutarCarpeta.ImageAlign = ContentAlignment.MiddleLeft;
            btn_EjecutarCarpeta.Location = new Point(12, 80);
            btn_EjecutarCarpeta.Name = "btn_EjecutarCarpeta";
            btn_EjecutarCarpeta.Padding = new Padding(11, 0, 18, 0);
            btn_EjecutarCarpeta.Size = new Size(157, 40);
            btn_EjecutarCarpeta.TabIndex = 6;
            btn_EjecutarCarpeta.Text = "Elegir Scripts";
            btn_EjecutarCarpeta.TextAlign = ContentAlignment.MiddleRight;
            btn_EjecutarCarpeta.UseVisualStyleBackColor = false;
            btn_EjecutarCarpeta.Click += btn_EjecutarCarpeta_Click;
            // 
            // btn_OcultaOpciones
            // 
            btn_OcultaOpciones.BackColor = Color.White;
            btn_OcultaOpciones.Image = (Image)resources.GetObject("btn_OcultaOpciones.Image");
            btn_OcultaOpciones.ImageAlign = ContentAlignment.MiddleLeft;
            btn_OcultaOpciones.Location = new Point(12, 265);
            btn_OcultaOpciones.Name = "btn_OcultaOpciones";
            btn_OcultaOpciones.Padding = new Padding(10, 0, 5, 0);
            btn_OcultaOpciones.Size = new Size(157, 40);
            btn_OcultaOpciones.TabIndex = 7;
            btn_OcultaOpciones.Text = "OcultaOpciones";
            btn_OcultaOpciones.TextAlign = ContentAlignment.MiddleRight;
            btn_OcultaOpciones.UseVisualStyleBackColor = false;
            btn_OcultaOpciones.Click += btn_OcultaOpciones_Click;
            // 
            // btn_ExportarEscandallos
            // 
            btn_ExportarEscandallos.BackColor = Color.White;
            btn_ExportarEscandallos.Image = (Image)resources.GetObject("btn_ExportarEscandallos.Image");
            btn_ExportarEscandallos.ImageAlign = ContentAlignment.MiddleLeft;
            btn_ExportarEscandallos.Location = new Point(12, 219);
            btn_ExportarEscandallos.Name = "btn_ExportarEscandallos";
            btn_ExportarEscandallos.Padding = new Padding(7, 0, 0, 0);
            btn_ExportarEscandallos.Size = new Size(157, 40);
            btn_ExportarEscandallos.TabIndex = 8;
            btn_ExportarEscandallos.Text = "Exportar Escandallos";
            btn_ExportarEscandallos.TextAlign = ContentAlignment.MiddleRight;
            btn_ExportarEscandallos.UseVisualStyleBackColor = false;
            btn_ExportarEscandallos.Click += btn_ExportarEscandallos_Click;
            // 
            // btn_InstalarEscandallos
            // 
            btn_InstalarEscandallos.BackColor = Color.White;
            btn_InstalarEscandallos.Image = (Image)resources.GetObject("btn_InstalarEscandallos.Image");
            btn_InstalarEscandallos.ImageAlign = ContentAlignment.MiddleLeft;
            btn_InstalarEscandallos.Location = new Point(12, 127);
            btn_InstalarEscandallos.Name = "btn_InstalarEscandallos";
            btn_InstalarEscandallos.Padding = new Padding(5, 0, 0, 0);
            btn_InstalarEscandallos.Size = new Size(157, 40);
            btn_InstalarEscandallos.TabIndex = 9;
            btn_InstalarEscandallos.Text = "Instalar Escandallos";
            btn_InstalarEscandallos.TextAlign = ContentAlignment.MiddleRight;
            btn_InstalarEscandallos.UseVisualStyleBackColor = false;
            btn_InstalarEscandallos.Click += btn_InstalarEscandallos_Click;
            // 
            // btn_ShowScripts
            // 
            btn_ShowScripts.BackColor = Color.White;
            btn_ShowScripts.Image = (Image)resources.GetObject("btn_ShowScripts.Image");
            btn_ShowScripts.ImageAlign = ContentAlignment.MiddleLeft;
            btn_ShowScripts.Location = new Point(12, 173);
            btn_ShowScripts.Name = "btn_ShowScripts";
            btn_ShowScripts.Padding = new Padding(10, 0, 12, 0);
            btn_ShowScripts.Size = new Size(157, 40);
            btn_ShowScripts.TabIndex = 10;
            btn_ShowScripts.Text = "Ver Escandallos";
            btn_ShowScripts.TextAlign = ContentAlignment.MiddleRight;
            btn_ShowScripts.UseVisualStyleBackColor = false;
            btn_ShowScripts.Click += btn_ShowScripts_Click;
            // 
            // ActualizadorMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(780, 346);
            Controls.Add(btn_ShowScripts);
            Controls.Add(btn_InstalarEscandallos);
            Controls.Add(btn_ExportarEscandallos);
            Controls.Add(btn_OcultaOpciones);
            Controls.Add(btn_EjecutarCarpeta);
            Controls.Add(btn_EjecutarScripts);
            Controls.Add(groupBox_Proveedor);
            Controls.Add(groupBox_Grupos);
            Controls.Add(statusStrip1);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "ActualizadorMenu";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Actualizador";
            Load += ActualizadorMenu_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            groupBox_Grupos.ResumeLayout(false);
            groupBox_Grupos.PerformLayout();
            groupBox_Proveedor.ResumeLayout(false);
            groupBox_Proveedor.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lbl_Conexion;
        private ComboBox cmb_IdPresupuestado;
        private Label lbl_IdPresupuestado;
        private GroupBox groupBox_Grupos;
        private Label lbl_IdProduccion;
        private ComboBox cmb_IdProduccion;
        private GroupBox groupBox_Proveedor;
        private TextBox txt_Proveedor;
        private Label lbl_Proveedor;
        private ComboBox cmb_Proveedor;
        private Button btn_EjecutarScripts;
        private TextBox txt_Presupuestado;
        private TextBox txt_Produccion;
        private Button btn_EjecutarCarpeta;
        private Button btn_OcultaOpciones;
        private Button btn_ExportarEscandallos;
        private Button btn_InstalarEscandallos;
        private Button btn_ShowScripts;
        private Button btn_AddProveedor;
    }
}