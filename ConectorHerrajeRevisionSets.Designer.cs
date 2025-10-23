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
            btn_ExportExcelCodigos = new Button();
            txt_FiltroCodigoNoXml = new TextBox();
            lbl_TotalCodigosNoXml = new Label();
            list_CodigosNoXml = new ListView();
            tabControl1.SuspendLayout();
            tab_Incluidos.SuspendLayout();
            tab_NoIncluidos.SuspendLayout();
            tab_LineasNoXml.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tab_Incluidos);
            tabControl1.Controls.Add(tab_NoIncluidos);
            tabControl1.Controls.Add(tab_LineasNoXml);
            tabControl1.Location = new Point(12, 12);
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
            txt_FiltroCodigoNoXml.Location = new Point(324, 7);
            txt_FiltroCodigoNoXml.Margin = new Padding(3, 2, 3, 2);
            txt_FiltroCodigoNoXml.Name = "txt_FiltroCodigoNoXml";
            txt_FiltroCodigoNoXml.Size = new Size(181, 23);
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
            // ConectorHerrajeRevisionSets
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(784, 411);
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
            ResumeLayout(false);
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
    }
}