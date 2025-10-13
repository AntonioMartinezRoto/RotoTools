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
            groupBox1 = new GroupBox();
            txt_Presupuestado = new TextBox();
            txt_Produccion = new TextBox();
            lbl_IdProduccion = new Label();
            cmb_IdProduccion = new ComboBox();
            groupBox2 = new GroupBox();
            txt_Proveedor = new TextBox();
            lbl_Proveedor = new Label();
            cmb_Proveedor = new ComboBox();
            btn_EjecutarScripts = new Button();
            btn_EjecutarCarpeta = new Button();
            btn_OcultaOpciones = new Button();
            statusStrip1.SuspendLayout();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.Transparent;
            statusStrip1.Items.AddRange(new ToolStripItem[] { lbl_Conexion });
            statusStrip1.Location = new Point(0, 338);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(623, 22);
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
            cmb_IdPresupuestado.FormattingEnabled = true;
            cmb_IdPresupuestado.Location = new Point(133, 28);
            cmb_IdPresupuestado.Name = "cmb_IdPresupuestado";
            cmb_IdPresupuestado.Size = new Size(214, 23);
            cmb_IdPresupuestado.TabIndex = 1;
            cmb_IdPresupuestado.SelectedIndexChanged += cmb_IdPresupuestado_SelectedIndexChanged;
            // 
            // lbl_IdPresupuestado
            // 
            lbl_IdPresupuestado.AutoSize = true;
            lbl_IdPresupuestado.Location = new Point(20, 31);
            lbl_IdPresupuestado.Name = "lbl_IdPresupuestado";
            lbl_IdPresupuestado.Size = new Size(85, 15);
            lbl_IdPresupuestado.TabIndex = 2;
            lbl_IdPresupuestado.Text = "Presupuestado";
            // 
            // groupBox1
            // 
            groupBox1.BackColor = Color.Transparent;
            groupBox1.Controls.Add(txt_Presupuestado);
            groupBox1.Controls.Add(txt_Produccion);
            groupBox1.Controls.Add(lbl_IdProduccion);
            groupBox1.Controls.Add(cmb_IdProduccion);
            groupBox1.Controls.Add(lbl_IdPresupuestado);
            groupBox1.Controls.Add(cmb_IdPresupuestado);
            groupBox1.Location = new Point(32, 37);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(466, 108);
            groupBox1.TabIndex = 3;
            groupBox1.TabStop = false;
            groupBox1.Text = "Grupos";
            // 
            // txt_Presupuestado
            // 
            txt_Presupuestado.Location = new Point(365, 28);
            txt_Presupuestado.Name = "txt_Presupuestado";
            txt_Presupuestado.ReadOnly = true;
            txt_Presupuestado.Size = new Size(67, 23);
            txt_Presupuestado.TabIndex = 9;
            // 
            // txt_Produccion
            // 
            txt_Produccion.Location = new Point(365, 63);
            txt_Produccion.Name = "txt_Produccion";
            txt_Produccion.ReadOnly = true;
            txt_Produccion.Size = new Size(67, 23);
            txt_Produccion.TabIndex = 8;
            // 
            // lbl_IdProduccion
            // 
            lbl_IdProduccion.AutoSize = true;
            lbl_IdProduccion.Location = new Point(20, 66);
            lbl_IdProduccion.Name = "lbl_IdProduccion";
            lbl_IdProduccion.Size = new Size(68, 15);
            lbl_IdProduccion.TabIndex = 4;
            lbl_IdProduccion.Text = "Producción";
            // 
            // cmb_IdProduccion
            // 
            cmb_IdProduccion.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb_IdProduccion.FormattingEnabled = true;
            cmb_IdProduccion.Location = new Point(133, 63);
            cmb_IdProduccion.Name = "cmb_IdProduccion";
            cmb_IdProduccion.Size = new Size(214, 23);
            cmb_IdProduccion.TabIndex = 3;
            cmb_IdProduccion.SelectedIndexChanged += cmb_IdProduccion_SelectedIndexChanged;
            // 
            // groupBox2
            // 
            groupBox2.BackColor = Color.Transparent;
            groupBox2.Controls.Add(txt_Proveedor);
            groupBox2.Controls.Add(lbl_Proveedor);
            groupBox2.Controls.Add(cmb_Proveedor);
            groupBox2.Location = new Point(32, 166);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(466, 73);
            groupBox2.TabIndex = 4;
            groupBox2.TabStop = false;
            groupBox2.Text = "Proveedor";
            // 
            // txt_Proveedor
            // 
            txt_Proveedor.Location = new Point(365, 26);
            txt_Proveedor.Name = "txt_Proveedor";
            txt_Proveedor.ReadOnly = true;
            txt_Proveedor.Size = new Size(67, 23);
            txt_Proveedor.TabIndex = 7;
            // 
            // lbl_Proveedor
            // 
            lbl_Proveedor.AutoSize = true;
            lbl_Proveedor.Location = new Point(20, 29);
            lbl_Proveedor.Name = "lbl_Proveedor";
            lbl_Proveedor.Size = new Size(51, 15);
            lbl_Proveedor.TabIndex = 6;
            lbl_Proveedor.Text = "Nombre";
            // 
            // cmb_Proveedor
            // 
            cmb_Proveedor.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb_Proveedor.FormattingEnabled = true;
            cmb_Proveedor.Location = new Point(133, 26);
            cmb_Proveedor.Name = "cmb_Proveedor";
            cmb_Proveedor.Size = new Size(214, 23);
            cmb_Proveedor.TabIndex = 5;
            cmb_Proveedor.SelectedIndexChanged += cmb_Proveedor_SelectedIndexChanged;
            // 
            // btn_EjecutarScripts
            // 
            btn_EjecutarScripts.BackColor = Color.White;
            btn_EjecutarScripts.Image = (Image)resources.GetObject("btn_EjecutarScripts.Image");
            btn_EjecutarScripts.ImageAlign = ContentAlignment.MiddleLeft;
            btn_EjecutarScripts.Location = new Point(364, 268);
            btn_EjecutarScripts.Name = "btn_EjecutarScripts";
            btn_EjecutarScripts.Size = new Size(134, 40);
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
            btn_EjecutarCarpeta.Location = new Point(199, 268);
            btn_EjecutarCarpeta.Name = "btn_EjecutarCarpeta";
            btn_EjecutarCarpeta.Padding = new Padding(5, 0, 8, 0);
            btn_EjecutarCarpeta.Size = new Size(134, 40);
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
            btn_OcultaOpciones.Location = new Point(32, 268);
            btn_OcultaOpciones.Name = "btn_OcultaOpciones";
            btn_OcultaOpciones.Size = new Size(134, 40);
            btn_OcultaOpciones.TabIndex = 7;
            btn_OcultaOpciones.Text = "OcultaOpciones";
            btn_OcultaOpciones.TextAlign = ContentAlignment.MiddleRight;
            btn_OcultaOpciones.UseVisualStyleBackColor = false;
            btn_OcultaOpciones.Click += btn_OcultaOpciones_Click;
            // 
            // ActualizadorMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(623, 360);
            Controls.Add(btn_OcultaOpciones);
            Controls.Add(btn_EjecutarCarpeta);
            Controls.Add(btn_EjecutarScripts);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(statusStrip1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "ActualizadorMenu";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Actualizador";
            Load += ActualizadorMenu_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lbl_Conexion;
        private ComboBox cmb_IdPresupuestado;
        private Label lbl_IdPresupuestado;
        private GroupBox groupBox1;
        private Label lbl_IdProduccion;
        private ComboBox cmb_IdProduccion;
        private GroupBox groupBox2;
        private TextBox txt_Proveedor;
        private Label lbl_Proveedor;
        private ComboBox cmb_Proveedor;
        private Button btn_EjecutarScripts;
        private TextBox txt_Presupuestado;
        private TextBox txt_Produccion;
        private Button btn_EjecutarCarpeta;
        private Button btn_OcultaOpciones;
    }
}