namespace ErrorCheckService
{
    partial class ProjectInstaller
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ErrorCheckServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.ErrorCheckServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // ErrorCheckServiceProcessInstaller
            // 
            this.ErrorCheckServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.ErrorCheckServiceProcessInstaller.Password = null;
            this.ErrorCheckServiceProcessInstaller.Username = null;
            // 
            // ErrorCheckServiceInstaller
            // 
            this.ErrorCheckServiceInstaller.Description = "This Error Check Service will check a given directory and report any errors withi" +
    "n that directory";
            this.ErrorCheckServiceInstaller.DisplayName = "99x Error Check Service";
            this.ErrorCheckServiceInstaller.ServiceName = "ErrorCheck";
            this.ErrorCheckServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.ErrorCheckServiceProcessInstaller,
            this.ErrorCheckServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller ErrorCheckServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller ErrorCheckServiceInstaller;
    }
}