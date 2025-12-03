namespace RotoTools
{
    partial class CamInstalarOperaciones
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CamInstalarOperaciones));
            btn_InstalarOperations = new Button();
            chkList_Operations = new CheckedListBox();
            txt_filter = new TextBox();
            lbl_Buscar = new Label();
            chk_SelectAll = new CheckBox();
            SuspendLayout();
            // 
            // btn_InstalarOperations
            // 
            btn_InstalarOperations.BackColor = Color.White;
            btn_InstalarOperations.Image = (Image)resources.GetObject("btn_InstalarOperations.Image");
            btn_InstalarOperations.ImageAlign = ContentAlignment.MiddleLeft;
            btn_InstalarOperations.Location = new Point(413, 293);
            btn_InstalarOperations.Name = "btn_InstalarOperations";
            btn_InstalarOperations.Padding = new Padding(0, 0, 5, 0);
            btn_InstalarOperations.Size = new Size(102, 40);
            btn_InstalarOperations.TabIndex = 19;
            btn_InstalarOperations.Text = "Instalar";
            btn_InstalarOperations.TextAlign = ContentAlignment.MiddleRight;
            btn_InstalarOperations.UseVisualStyleBackColor = false;
            // 
            // chkList_Operations
            // 
            chkList_Operations.FormattingEnabled = true;
            chkList_Operations.Location = new Point(40, 58);
            chkList_Operations.Name = "chkList_Operations";
            chkList_Operations.Size = new Size(475, 220);
            chkList_Operations.TabIndex = 18;
            // 
            // txt_filter
            // 
            txt_filter.Location = new Point(366, 27);
            txt_filter.Name = "txt_filter";
            txt_filter.Size = new Size(149, 23);
            txt_filter.TabIndex = 17;
            // 
            // lbl_Buscar
            // 
            lbl_Buscar.AutoSize = true;
            lbl_Buscar.BackColor = Color.Transparent;
            lbl_Buscar.Location = new Point(314, 32);
            lbl_Buscar.Name = "lbl_Buscar";
            lbl_Buscar.Size = new Size(42, 15);
            lbl_Buscar.TabIndex = 16;
            lbl_Buscar.Text = "Buscar";
            // 
            // chk_SelectAll
            // 
            chk_SelectAll.AutoSize = true;
            chk_SelectAll.BackColor = Color.Transparent;
            chk_SelectAll.Location = new Point(43, 31);
            chk_SelectAll.Margin = new Padding(3, 2, 3, 2);
            chk_SelectAll.Name = "chk_SelectAll";
            chk_SelectAll.Size = new Size(119, 19);
            chk_SelectAll.TabIndex = 15;
            chk_SelectAll.Text = "Seleccionar todos";
            chk_SelectAll.UseVisualStyleBackColor = false;
            // 
            // CamInstalarOperaciones
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(664, 360);
            Controls.Add(btn_InstalarOperations);
            Controls.Add(chkList_Operations);
            Controls.Add(txt_filter);
            Controls.Add(lbl_Buscar);
            Controls.Add(chk_SelectAll);
            DoubleBuffered = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "CamInstalarOperaciones";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Operaciones";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btn_InstalarOperations;
        private CheckedListBox chkList_Operations;
        private TextBox txt_filter;
        private Label lbl_Buscar;
        private CheckBox chk_SelectAll;
    }
}