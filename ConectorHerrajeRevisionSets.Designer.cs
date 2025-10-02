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
            txt_FiltroIncluidos = new TextBox();
            lbl_TotalSetsIncluidos = new Label();
            list_SetsUsadosEnConectorH = new ListView();
            tab_NoIncluidos = new TabPage();
            txt_FiltroNoIncluidos = new TextBox();
            lbl_TotalSetsNoIncluidosConector = new Label();
            list_SetsNoUsadosEnConector = new ListView();
            tab_LineasNoXml = new TabPage();
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
            // txt_FiltroIncluidos
            // 
            txt_FiltroIncluidos.Location = new Point(361, 4);
            txt_FiltroIncluidos.Margin = new Padding(3, 2, 3, 2);
            txt_FiltroIncluidos.Name = "txt_FiltroIncluidos";
            txt_FiltroIncluidos.Size = new Size(181, 23);
            txt_FiltroIncluidos.TabIndex = 6;
            txt_FiltroIncluidos.TextChanged += txt_FiltroIncluidos_TextChanged;
            // 
            // lbl_TotalSetsIncluidos
            // 
            lbl_TotalSetsIncluidos.AutoSize = true;
            lbl_TotalSetsIncluidos.Location = new Point(5, 3);
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
            list_SetsUsadosEnConectorH.ItemActivate += list_SetsUsadosEnConectorH_ItemActivate;
            // 
            // tab_NoIncluidos
            // 
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
            // txt_FiltroNoIncluidos
            // 
            txt_FiltroNoIncluidos.Location = new Point(361, 4);
            txt_FiltroNoIncluidos.Margin = new Padding(3, 2, 3, 2);
            txt_FiltroNoIncluidos.Name = "txt_FiltroNoIncluidos";
            txt_FiltroNoIncluidos.Size = new Size(181, 23);
            txt_FiltroNoIncluidos.TabIndex = 5;
            txt_FiltroNoIncluidos.TextChanged += txt_FiltroNoIncluidos_TextChanged;
            // 
            // lbl_TotalSetsNoIncluidosConector
            // 
            lbl_TotalSetsNoIncluidosConector.AutoSize = true;
            lbl_TotalSetsNoIncluidosConector.Location = new Point(6, 3);
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
            list_SetsNoUsadosEnConector.ItemActivate += list_SetsNoUsadosEnConector_ItemActivate;
            // 
            // tab_LineasNoXml
            // 
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
            // txt_FiltroCodigoNoXml
            // 
            txt_FiltroCodigoNoXml.Location = new Point(358, 4);
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
            lbl_TotalCodigosNoXml.Location = new Point(3, 6);
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
            // 
            // ShowInfo
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.None;
            ClientSize = new Size(784, 411);
            Controls.Add(tabControl1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            Name = "ShowInfo";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Revisión de Sets";
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
    }
}