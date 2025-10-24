namespace RotoTools
{
    partial class ConectorHerrajeRevisionSets
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConectorHerrajeRevisionSets));
            tabControl1 = new TabControl();
            tab_Incluidos = new TabPage();
            btn_ExportExcelIncluidos = new Button();
            txt_FiltroIncluidos = new TextBox();
            lbl_TotalSetsIncluidos = new Label();
            list_SetsUsadosEnConectorH = new ListView();
            tab_NoIncluidos = new TabPage();
            btn_ExportExcelNoIncluidos = new Button();
            txt_FiltroNoIncluidos = new TextBox();
            lbl_TotalSetsNoIncluidosConector = new Label();
            list_SetsNoUsadosEnConector = new ListView();
            tab_LineasNoXml = new TabPage();
            btn_EliminarLineasConector = new Button();
            btn_ExportExcelCodigos = new Button();
            txt_FiltroCodigoNoXml = new TextBox();
            lbl_TotalCodigosNoXml = new Label();
            list_CodigosNoXml = new ListView();
            cmb_HardwareSupplier = new ComboBox();
            label1 = new Label();
            cmb_Conectores = new ComboBox();
            label2 = new Label();
            statusStrip1 = new StatusStrip();
            lbl_Conexion = new ToolStripStatusLabel();
            tabControl1.SuspendLayout();
            tab_Incluidos.SuspendLayout();
            tab_NoIncluidos.SuspendLayout();
            tab_LineasNoXml.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tab_Incluidos);
            tabControl1.Controls.Add(tab_NoIncluidos);
            tabControl1.Controls.Add(tab_LineasNoXml);
            tabControl1.Location = new Point(37, 55);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(558, 389);
            tabControl1.TabIndex = 1;
            // 
            // tab_Incluidos
            // 
            tab_Incluidos.Controls.Add(btn_ExportExcelIncluidos);
            tab_Incluidos.Controls.Add(txt_FiltroIncluidos);
            tab_Incluidos.Controls.Add(lbl_TotalSetsIncluidos);
            tab_Incluidos.Controls.Add(list_SetsUsadosEnConectorH);
            tab_Incluidos.Location = new Point(4, 24);
            tab_Incluidos.Name = "tab_Incluidos";
            tab_Incluidos.Padding = new Padding(3);
            tab_Incluidos.Size = new Size(550, 361);
            tab_Incluidos.TabIndex = 0;
            tab_Incluidos.Text = "Sets incluidos en Conector";
            tab_Incluidos.UseVisualStyleBackColor = true;
            // 
            // btn_ExportExcelIncluidos
            // 
            btn_ExportExcelIncluidos.BackColor = Color.Transparent;
            btn_ExportExcelIncluidos.BackgroundImage = (Image)resources.GetObject("btn_ExportExcelIncluidos.BackgroundImage");
            btn_ExportExcelIncluidos.BackgroundImageLayout = ImageLayout.Stretch;
            btn_ExportExcelIncluidos.Location = new Point(514, 5);
            btn_ExportExcelIncluidos.Margin = new Padding(3, 2, 3, 2);
            btn_ExportExcelIncluidos.Name = "btn_ExportExcelIncluidos";
            btn_ExportExcelIncluidos.Size = new Size(28, 23);
            btn_ExportExcelIncluidos.TabIndex = 25;
            btn_ExportExcelIncluidos.UseVisualStyleBackColor = false;
            btn_ExportExcelIncluidos.Click += btn_ExportExcelIncluidos_Click;
            // 
            // txt_FiltroIncluidos
            // 
            txt_FiltroIncluidos.Location = new Point(327, 5);
            txt_FiltroIncluidos.Margin = new Padding(3, 2, 3, 2);
            txt_FiltroIncluidos.Name = "txt_FiltroIncluidos";
            txt_FiltroIncluidos.Size = new Size(181, 23);
            txt_FiltroIncluidos.TabIndex = 6;
            txt_FiltroIncluidos.TextChanged += txt_FiltroIncluidos_TextChanged;
            // 
            // lbl_TotalSetsIncluidos
            // 
            lbl_TotalSetsIncluidos.AutoSize = true;
            lbl_TotalSetsIncluidos.Location = new Point(6, 9);
            lbl_TotalSetsIncluidos.Name = "lbl_TotalSetsIncluidos";
            lbl_TotalSetsIncluidos.Size = new Size(38, 15);
            lbl_TotalSetsIncluidos.TabIndex = 4;
            lbl_TotalSetsIncluidos.Text = "label1";
            // 
            // list_SetsUsadosEnConectorH
            // 
            list_SetsUsadosEnConectorH.Location = new Point(6, 29);
            list_SetsUsadosEnConectorH.Margin = new Padding(3, 2, 3, 2);
            list_SetsUsadosEnConectorH.Name = "list_SetsUsadosEnConectorH";
            list_SetsUsadosEnConectorH.Size = new Size(536, 327);
            list_SetsUsadosEnConectorH.TabIndex = 1;
            list_SetsUsadosEnConectorH.UseCompatibleStateImageBehavior = false;
            list_SetsUsadosEnConectorH.MouseDoubleClick += list_SetsUsadosEnConectorH_MouseDoubleClick;
            // 
            // tab_NoIncluidos
            // 
            tab_NoIncluidos.Controls.Add(btn_ExportExcelNoIncluidos);
            tab_NoIncluidos.Controls.Add(txt_FiltroNoIncluidos);
            tab_NoIncluidos.Controls.Add(lbl_TotalSetsNoIncluidosConector);
            tab_NoIncluidos.Controls.Add(list_SetsNoUsadosEnConector);
            tab_NoIncluidos.Location = new Point(4, 24);
            tab_NoIncluidos.Name = "tab_NoIncluidos";
            tab_NoIncluidos.Padding = new Padding(3);
            tab_NoIncluidos.Size = new Size(550, 361);
            tab_NoIncluidos.TabIndex = 1;
            tab_NoIncluidos.Text = "Sets NO incluidos en Conector";
            tab_NoIncluidos.UseVisualStyleBackColor = true;
            // 
            // btn_ExportExcelNoIncluidos
            // 
            btn_ExportExcelNoIncluidos.BackColor = Color.Transparent;
            btn_ExportExcelNoIncluidos.BackgroundImage = (Image)resources.GetObject("btn_ExportExcelNoIncluidos.BackgroundImage");
            btn_ExportExcelNoIncluidos.BackgroundImageLayout = ImageLayout.Stretch;
            btn_ExportExcelNoIncluidos.Location = new Point(514, 5);
            btn_ExportExcelNoIncluidos.Margin = new Padding(3, 2, 3, 2);
            btn_ExportExcelNoIncluidos.Name = "btn_ExportExcelNoIncluidos";
            btn_ExportExcelNoIncluidos.Size = new Size(28, 23);
            btn_ExportExcelNoIncluidos.TabIndex = 26;
            btn_ExportExcelNoIncluidos.UseVisualStyleBackColor = false;
            btn_ExportExcelNoIncluidos.Click += btn_ExportExcelNoIncluidos_Click;
            // 
            // txt_FiltroNoIncluidos
            // 
            txt_FiltroNoIncluidos.Location = new Point(327, 5);
            txt_FiltroNoIncluidos.Margin = new Padding(3, 2, 3, 2);
            txt_FiltroNoIncluidos.Name = "txt_FiltroNoIncluidos";
            txt_FiltroNoIncluidos.Size = new Size(181, 23);
            txt_FiltroNoIncluidos.TabIndex = 5;
            txt_FiltroNoIncluidos.TextChanged += txt_FiltroNoIncluidos_TextChanged;
            // 
            // lbl_TotalSetsNoIncluidosConector
            // 
            lbl_TotalSetsNoIncluidosConector.AutoSize = true;
            lbl_TotalSetsNoIncluidosConector.Location = new Point(6, 9);
            lbl_TotalSetsNoIncluidosConector.Name = "lbl_TotalSetsNoIncluidosConector";
            lbl_TotalSetsNoIncluidosConector.Size = new Size(38, 15);
            lbl_TotalSetsNoIncluidosConector.TabIndex = 4;
            lbl_TotalSetsNoIncluidosConector.Text = "label1";
            // 
            // list_SetsNoUsadosEnConector
            // 
            list_SetsNoUsadosEnConector.Location = new Point(6, 29);
            list_SetsNoUsadosEnConector.Margin = new Padding(3, 2, 3, 2);
            list_SetsNoUsadosEnConector.Name = "list_SetsNoUsadosEnConector";
            list_SetsNoUsadosEnConector.Size = new Size(536, 327);
            list_SetsNoUsadosEnConector.TabIndex = 2;
            list_SetsNoUsadosEnConector.UseCompatibleStateImageBehavior = false;
            list_SetsNoUsadosEnConector.MouseDoubleClick += list_SetsNoUsadosEnConector_MouseDoubleClick;
            // 
            // tab_LineasNoXml
            // 
            tab_LineasNoXml.Controls.Add(btn_EliminarLineasConector);
            tab_LineasNoXml.Controls.Add(btn_ExportExcelCodigos);
            tab_LineasNoXml.Controls.Add(txt_FiltroCodigoNoXml);
            tab_LineasNoXml.Controls.Add(lbl_TotalCodigosNoXml);
            tab_LineasNoXml.Controls.Add(list_CodigosNoXml);
            tab_LineasNoXml.Location = new Point(4, 24);
            tab_LineasNoXml.Margin = new Padding(3, 2, 3, 2);
            tab_LineasNoXml.Name = "tab_LineasNoXml";
            tab_LineasNoXml.Size = new Size(550, 361);
            tab_LineasNoXml.TabIndex = 2;
            tab_LineasNoXml.Text = "Códigos que no están en XML";
            tab_LineasNoXml.UseVisualStyleBackColor = true;
            // 
            // btn_EliminarLineasConector
            // 
            btn_EliminarLineasConector.BackColor = Color.Transparent;
            btn_EliminarLineasConector.BackgroundImage = (Image)resources.GetObject("btn_EliminarLineasConector.BackgroundImage");
            btn_EliminarLineasConector.BackgroundImageLayout = ImageLayout.Stretch;
            btn_EliminarLineasConector.Location = new Point(480, 7);
            btn_EliminarLineasConector.Margin = new Padding(3, 2, 3, 2);
            btn_EliminarLineasConector.Name = "btn_EliminarLineasConector";
            btn_EliminarLineasConector.Size = new Size(28, 23);
            btn_EliminarLineasConector.TabIndex = 27;
            btn_EliminarLineasConector.UseVisualStyleBackColor = false;
            btn_EliminarLineasConector.Click += btn_EliminarLineasConector_Click;
            // 
            // btn_ExportExcelCodigos
            // 
            btn_ExportExcelCodigos.BackColor = Color.Transparent;
            btn_ExportExcelCodigos.BackgroundImage = (Image)resources.GetObject("btn_ExportExcelCodigos.BackgroundImage");
            btn_ExportExcelCodigos.BackgroundImageLayout = ImageLayout.Stretch;
            btn_ExportExcelCodigos.Location = new Point(511, 7);
            btn_ExportExcelCodigos.Margin = new Padding(3, 2, 3, 2);
            btn_ExportExcelCodigos.Name = "btn_ExportExcelCodigos";
            btn_ExportExcelCodigos.Size = new Size(28, 23);
            btn_ExportExcelCodigos.TabIndex = 26;
            btn_ExportExcelCodigos.UseVisualStyleBackColor = false;
            btn_ExportExcelCodigos.Click += btn_ExportExcelCodigos_Click;
            // 
            // txt_FiltroCodigoNoXml
            // 
            txt_FiltroCodigoNoXml.Location = new Point(448, 8);
            txt_FiltroCodigoNoXml.Margin = new Padding(3, 2, 3, 2);
            txt_FiltroCodigoNoXml.Name = "txt_FiltroCodigoNoXml";
            txt_FiltroCodigoNoXml.Size = new Size(23, 23);
            txt_FiltroCodigoNoXml.TabIndex = 6;
            txt_FiltroCodigoNoXml.Visible = false;
            txt_FiltroCodigoNoXml.TextChanged += txt_FiltroCodigoNoXml_TextChanged;
            // 
            // lbl_TotalCodigosNoXml
            // 
            lbl_TotalCodigosNoXml.AutoSize = true;
            lbl_TotalCodigosNoXml.Location = new Point(3, 10);
            lbl_TotalCodigosNoXml.Name = "lbl_TotalCodigosNoXml";
            lbl_TotalCodigosNoXml.Size = new Size(38, 15);
            lbl_TotalCodigosNoXml.TabIndex = 5;
            lbl_TotalCodigosNoXml.Text = "label1";
            // 
            // list_CodigosNoXml
            // 
            list_CodigosNoXml.Location = new Point(3, 30);
            list_CodigosNoXml.Margin = new Padding(3, 2, 3, 2);
            list_CodigosNoXml.Name = "list_CodigosNoXml";
            list_CodigosNoXml.Size = new Size(536, 333);
            list_CodigosNoXml.TabIndex = 3;
            list_CodigosNoXml.UseCompatibleStateImageBehavior = false;
            list_CodigosNoXml.MouseDoubleClick += list_CodigosNoXml_MouseDoubleClick;
            // 
            // cmb_HardwareSupplier
            // 
            cmb_HardwareSupplier.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb_HardwareSupplier.FormattingEnabled = true;
            cmb_HardwareSupplier.Location = new Point(430, 19);
            cmb_HardwareSupplier.Name = "cmb_HardwareSupplier";
            cmb_HardwareSupplier.Size = new Size(165, 23);
            cmb_HardwareSupplier.TabIndex = 10;
            cmb_HardwareSupplier.SelectedValueChanged += cmb_HardwareSupplier_SelectedValueChanged;
            // 
            // label1
            // 
            label1.BackColor = Color.Transparent;
            label1.Location = new Point(318, 22);
            label1.Name = "label1";
            label1.Size = new Size(106, 20);
            label1.TabIndex = 9;
            label1.Text = "Hardware Supplier";
            // 
            // cmb_Conectores
            // 
            cmb_Conectores.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb_Conectores.FormattingEnabled = true;
            cmb_Conectores.Location = new Point(99, 19);
            cmb_Conectores.Name = "cmb_Conectores";
            cmb_Conectores.Size = new Size(175, 23);
            cmb_Conectores.TabIndex = 12;
            cmb_Conectores.SelectedValueChanged += cmb_Conectores_SelectedValueChanged;
            // 
            // label2
            // 
            label2.BackColor = Color.Transparent;
            label2.Location = new Point(37, 22);
            label2.Name = "label2";
            label2.Size = new Size(86, 20);
            label2.TabIndex = 11;
            label2.Text = "Conector";
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.Transparent;
            statusStrip1.Items.AddRange(new ToolStripItem[] { lbl_Conexion });
            statusStrip1.Location = new Point(0, 449);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(784, 22);
            statusStrip1.SizingGrip = false;
            statusStrip1.TabIndex = 13;
            statusStrip1.Text = "statusStrip1";
            // 
            // lbl_Conexion
            // 
            lbl_Conexion.Name = "lbl_Conexion";
            lbl_Conexion.Size = new Size(118, 17);
            lbl_Conexion.Text = "toolStripStatusLabel1";
            // 
            // ConectorHerrajeRevisionSets
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(784, 471);
            Controls.Add(statusStrip1);
            Controls.Add(cmb_Conectores);
            Controls.Add(label2);
            Controls.Add(cmb_HardwareSupplier);
            Controls.Add(label1);
            Controls.Add(tabControl1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            Name = "ConectorHerrajeRevisionSets";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Revisión de Sets";
            Load += ConectorHerrajeRevisionSets_Load;
            tabControl1.ResumeLayout(false);
            tab_Incluidos.ResumeLayout(false);
            tab_Incluidos.PerformLayout();
            tab_NoIncluidos.ResumeLayout(false);
            tab_NoIncluidos.PerformLayout();
            tab_LineasNoXml.ResumeLayout(false);
            tab_LineasNoXml.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TabControl tabControl1;
        private TabPage tab_Incluidos;
        private ListView list_SetsUsadosEnConectorH;
        private TabPage tab_NoIncluidos;
        private ListView list_SetsNoUsadosEnConector;
        private Label lbl_TotalSetsIncluidos;
        private Label lbl_TotalSetsNoIncluidosConector;
        private TextBox txt_FiltroIncluidos;
        private TextBox txt_FiltroNoIncluidos;
        private TabPage tab_LineasNoXml;
        private TextBox txt_FiltroCodigoNoXml;
        private Label lbl_TotalCodigosNoXml;
        private ListView list_CodigosNoXml;
        private Button btn_ExportExcelIncluidos;
        private Button btn_ExportExcelNoIncluidos;
        private Button btn_ExportExcelCodigos;
        private Button btn_EliminarLineasConector;
        private ComboBox cmb_HardwareSupplier;
        private Label label1;
        private ComboBox cmb_Conectores;
        private Label label2;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lbl_Conexion;
    }
}