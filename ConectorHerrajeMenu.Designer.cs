namespace RotoTools
{
    partial class ConectorHerrajeMenu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConectorHerrajeMenu));
            btn_LoadXml = new Button();
            lbl_Xml = new Label();
            statusStrip1 = new StatusStrip();
            lbl_Conexion = new ToolStripStatusLabel();
            btn_Actualizar = new Button();
            btn_SetsNoUtilizados = new Button();
            lbl_Revision = new Label();
            lbl_GenerarConector = new Label();
            btn_GeneraConector = new Button();
            lbl_Combinar = new Label();
            btn_CombinarConectores = new Button();
            chk_ConfigAE = new CheckBox();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // btn_LoadXml
            // 
            btn_LoadXml.BackgroundImage = (Image)resources.GetObject("btn_LoadXml.BackgroundImage");
            btn_LoadXml.BackgroundImageLayout = ImageLayout.Stretch;
            btn_LoadXml.Location = new Point(50, 48);
            btn_LoadXml.Name = "btn_LoadXml";
            btn_LoadXml.Size = new Size(40, 40);
            btn_LoadXml.TabIndex = 3;
            btn_LoadXml.UseVisualStyleBackColor = true;
            btn_LoadXml.Click += btn_LoadXml_Click;
            // 
            // lbl_Xml
            // 
            lbl_Xml.BackColor = Color.Transparent;
            lbl_Xml.Location = new Point(96, 51);
            lbl_Xml.Name = "lbl_Xml";
            lbl_Xml.Size = new Size(519, 37);
            lbl_Xml.TabIndex = 4;
            lbl_Xml.Text = "Seleccionar XML";
            lbl_Xml.TextAlign = ContentAlignment.MiddleLeft;
            lbl_Xml.Click += lbl_Xml_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.Transparent;
            statusStrip1.Items.AddRange(new ToolStripItem[] { lbl_Conexion });
            statusStrip1.Location = new Point(0, 285);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(683, 22);
            statusStrip1.SizingGrip = false;
            statusStrip1.TabIndex = 5;
            statusStrip1.Text = "statusStrip1";
            // 
            // lbl_Conexion
            // 
            lbl_Conexion.Name = "lbl_Conexion";
            lbl_Conexion.Size = new Size(118, 17);
            lbl_Conexion.Text = "toolStripStatusLabel1";
            // 
            // btn_Actualizar
            // 
            btn_Actualizar.BackgroundImage = (Image)resources.GetObject("btn_Actualizar.BackgroundImage");
            btn_Actualizar.BackgroundImageLayout = ImageLayout.Stretch;
            btn_Actualizar.Location = new Point(641, 12);
            btn_Actualizar.Name = "btn_Actualizar";
            btn_Actualizar.Size = new Size(30, 25);
            btn_Actualizar.TabIndex = 6;
            btn_Actualizar.UseVisualStyleBackColor = true;
            btn_Actualizar.Visible = false;
            btn_Actualizar.Click += btn_Actualizar_Click;
            // 
            // btn_SetsNoUtilizados
            // 
            btn_SetsNoUtilizados.BackgroundImage = (Image)resources.GetObject("btn_SetsNoUtilizados.BackgroundImage");
            btn_SetsNoUtilizados.BackgroundImageLayout = ImageLayout.Stretch;
            btn_SetsNoUtilizados.Location = new Point(50, 225);
            btn_SetsNoUtilizados.Name = "btn_SetsNoUtilizados";
            btn_SetsNoUtilizados.Size = new Size(40, 40);
            btn_SetsNoUtilizados.TabIndex = 9;
            btn_SetsNoUtilizados.UseVisualStyleBackColor = true;
            btn_SetsNoUtilizados.Click += btn_SetsNoUtilizados_Click;
            // 
            // lbl_Revision
            // 
            lbl_Revision.BackColor = Color.Transparent;
            lbl_Revision.Location = new Point(96, 238);
            lbl_Revision.Name = "lbl_Revision";
            lbl_Revision.Size = new Size(124, 16);
            lbl_Revision.TabIndex = 10;
            lbl_Revision.Text = "Revisión de Sets";
            // 
            // lbl_GenerarConector
            // 
            lbl_GenerarConector.BackColor = Color.Transparent;
            lbl_GenerarConector.Location = new Point(96, 120);
            lbl_GenerarConector.Name = "lbl_GenerarConector";
            lbl_GenerarConector.Size = new Size(166, 17);
            lbl_GenerarConector.TabIndex = 12;
            lbl_GenerarConector.Text = "Generar Conector de Herraje";
            // 
            // btn_GeneraConector
            // 
            btn_GeneraConector.BackgroundImage = (Image)resources.GetObject("btn_GeneraConector.BackgroundImage");
            btn_GeneraConector.BackgroundImageLayout = ImageLayout.Stretch;
            btn_GeneraConector.Location = new Point(50, 107);
            btn_GeneraConector.Name = "btn_GeneraConector";
            btn_GeneraConector.Size = new Size(40, 40);
            btn_GeneraConector.TabIndex = 11;
            btn_GeneraConector.UseVisualStyleBackColor = true;
            btn_GeneraConector.Click += btn_GeneraConector_Click;
            // 
            // lbl_Combinar
            // 
            lbl_Combinar.BackColor = Color.Transparent;
            lbl_Combinar.Location = new Point(96, 178);
            lbl_Combinar.Name = "lbl_Combinar";
            lbl_Combinar.Size = new Size(189, 15);
            lbl_Combinar.TabIndex = 14;
            lbl_Combinar.Text = "Combinar Conectores de Herraje";
            // 
            // btn_CombinarConectores
            // 
            btn_CombinarConectores.BackgroundImage = (Image)resources.GetObject("btn_CombinarConectores.BackgroundImage");
            btn_CombinarConectores.BackgroundImageLayout = ImageLayout.Stretch;
            btn_CombinarConectores.Location = new Point(50, 165);
            btn_CombinarConectores.Name = "btn_CombinarConectores";
            btn_CombinarConectores.Size = new Size(40, 40);
            btn_CombinarConectores.TabIndex = 13;
            btn_CombinarConectores.UseVisualStyleBackColor = true;
            btn_CombinarConectores.Click += btn_CombinarConectores_Click;
            // 
            // chk_ConfigAE
            // 
            chk_ConfigAE.AutoSize = true;
            chk_ConfigAE.BackColor = Color.Transparent;
            chk_ConfigAE.Location = new Point(264, 119);
            chk_ConfigAE.Name = "chk_ConfigAE";
            chk_ConfigAE.Size = new Size(177, 19);
            chk_ConfigAE.TabIndex = 24;
            chk_ConfigAE.Text = "Balconeras AE con Sets de AI";
            chk_ConfigAE.UseVisualStyleBackColor = false;
            // 
            // ConectorHerrajeMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(683, 307);
            Controls.Add(chk_ConfigAE);
            Controls.Add(lbl_Combinar);
            Controls.Add(btn_CombinarConectores);
            Controls.Add(lbl_GenerarConector);
            Controls.Add(btn_GeneraConector);
            Controls.Add(lbl_Revision);
            Controls.Add(btn_SetsNoUtilizados);
            Controls.Add(btn_Actualizar);
            Controls.Add(statusStrip1);
            Controls.Add(lbl_Xml);
            Controls.Add(btn_LoadXml);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "ConectorHerrajeMenu";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Menú Conector de Herraje";
            Load += ConectorHerrajeMenu_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btn_LoadXml;
        private Label lbl_Xml;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lbl_Conexion;
        private Button btn_Actualizar;
        private Button btn_SetsNoUtilizados;
        private Label lbl_Revision;
        private Label lbl_GenerarConector;
        private Button btn_GeneraConector;
        private Label lbl_Combinar;
        private Button btn_CombinarConectores;
        private CheckBox chk_ConfigAE;
    }
}