namespace RotoTools
{
    partial class MostrarInfo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MostrarInfo));
            txt_ContenidoInfo = new TextBox();
            SuspendLayout();
            // 
            // txt_ContenidoInfo
            // 
            txt_ContenidoInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txt_ContenidoInfo.Location = new Point(26, 12);
            txt_ContenidoInfo.Multiline = true;
            txt_ContenidoInfo.Name = "txt_ContenidoInfo";
            txt_ContenidoInfo.ScrollBars = ScrollBars.Both;
            txt_ContenidoInfo.Size = new Size(901, 664);
            txt_ContenidoInfo.TabIndex = 11;
            txt_ContenidoInfo.WordWrap = false;
            // 
            // MostrarInfo
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1158, 689);
            Controls.Add(txt_ContenidoInfo);
            DoubleBuffered = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MostrarInfo";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Info";
            Load += MostrarInfo_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txt_ContenidoInfo;
    }
}