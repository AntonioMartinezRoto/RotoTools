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
            label1 = new Label();
            cmb_Conectores = new ComboBox();
            btn_SetsNoUtilizados = new Button();
            label2 = new Label();
            label3 = new Label();
            btn_GeneraConector = new Button();
            label4 = new Label();
            btn_CombinarConectores = new Button();
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
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.Transparent;
            statusStrip1.Items.AddRange(new ToolStripItem[] { lbl_Conexion });
            statusStrip1.Location = new Point(0, 357);
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
            btn_Actualizar.Location = new Point(50, 109);
            btn_Actualizar.Name = "btn_Actualizar";
            btn_Actualizar.Size = new Size(40, 40);
            btn_Actualizar.TabIndex = 6;
            btn_Actualizar.UseVisualStyleBackColor = true;
            btn_Actualizar.Click += btn_Actualizar_Click;
            // 
            // label1
            // 
            label1.BackColor = Color.Transparent;
            label1.Location = new Point(96, 122);
            label1.Name = "label1";
            label1.Size = new Size(124, 16);
            label1.TabIndex = 7;
            label1.Text = "Seleccionar Conector";
            // 
            // cmb_Conectores
            // 
            cmb_Conectores.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb_Conectores.FormattingEnabled = true;
            cmb_Conectores.Location = new Point(261, 119);
            cmb_Conectores.Name = "cmb_Conectores";
            cmb_Conectores.Size = new Size(156, 23);
            cmb_Conectores.TabIndex = 8;
            cmb_Conectores.SelectedValueChanged += cmb_Conectores_SelectedValueChanged;
            // 
            // btn_SetsNoUtilizados
            // 
            btn_SetsNoUtilizados.BackgroundImage = (Image)resources.GetObject("btn_SetsNoUtilizados.BackgroundImage");
            btn_SetsNoUtilizados.BackgroundImageLayout = ImageLayout.Stretch;
            btn_SetsNoUtilizados.Location = new Point(50, 286);
            btn_SetsNoUtilizados.Name = "btn_SetsNoUtilizados";
            btn_SetsNoUtilizados.Size = new Size(40, 40);
            btn_SetsNoUtilizados.TabIndex = 9;
            btn_SetsNoUtilizados.UseVisualStyleBackColor = true;
            btn_SetsNoUtilizados.Click += btn_SetsNoUtilizados_Click;
            // 
            // label2
            // 
            label2.BackColor = Color.Transparent;
            label2.Location = new Point(96, 299);
            label2.Name = "label2";
            label2.Size = new Size(124, 16);
            label2.TabIndex = 10;
            label2.Text = "Revisión de Sets";
            // 
            // label3
            // 
            label3.BackColor = Color.Transparent;
            label3.Location = new Point(96, 181);
            label3.Name = "label3";
            label3.Size = new Size(166, 17);
            label3.TabIndex = 12;
            label3.Text = "Generar Conector de Herraje";
            // 
            // btn_GeneraConector
            // 
            btn_GeneraConector.BackgroundImage = (Image)resources.GetObject("btn_GeneraConector.BackgroundImage");
            btn_GeneraConector.BackgroundImageLayout = ImageLayout.Stretch;
            btn_GeneraConector.Location = new Point(50, 168);
            btn_GeneraConector.Name = "btn_GeneraConector";
            btn_GeneraConector.Size = new Size(40, 40);
            btn_GeneraConector.TabIndex = 11;
            btn_GeneraConector.UseVisualStyleBackColor = true;
            btn_GeneraConector.Click += btn_GeneraConector_Click;
            // 
            // label4
            // 
            label4.BackColor = Color.Transparent;
            label4.Location = new Point(96, 239);
            label4.Name = "label4";
            label4.Size = new Size(189, 15);
            label4.TabIndex = 14;
            label4.Text = "Combinar Conectores de Herraje";
            // 
            // btn_CombinarConectores
            // 
            btn_CombinarConectores.BackgroundImage = (Image)resources.GetObject("btn_CombinarConectores.BackgroundImage");
            btn_CombinarConectores.BackgroundImageLayout = ImageLayout.Stretch;
            btn_CombinarConectores.Location = new Point(50, 226);
            btn_CombinarConectores.Name = "btn_CombinarConectores";
            btn_CombinarConectores.Size = new Size(40, 40);
            btn_CombinarConectores.TabIndex = 13;
            btn_CombinarConectores.UseVisualStyleBackColor = true;
            btn_CombinarConectores.Click += btn_CombinarConectores_Click;
            // 
            // ConectorHerrajeMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(683, 379);
            Controls.Add(label4);
            Controls.Add(btn_CombinarConectores);
            Controls.Add(label3);
            Controls.Add(btn_GeneraConector);
            Controls.Add(label2);
            Controls.Add(btn_SetsNoUtilizados);
            Controls.Add(cmb_Conectores);
            Controls.Add(label1);
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
        private Label label1;
        private ComboBox cmb_Conectores;
        private Button btn_SetsNoUtilizados;
        private Label label2;
        private Label label3;
        private Button btn_GeneraConector;
        private Label label4;
        private Button btn_CombinarConectores;
    }
}