namespace gtavvehicles {
    public partial class ConnectionCredentials : Form {
        public ConnectionCredentials() { InitializeComponent(); }

        private void btnConnect_Click(object sender, EventArgs e) {
            var username = txtUsername.Text;
            var password = txtPassword.Text;

            // If the username or password is empty then show an error message
            if (username == "") {
                MessageBox.Show("Please enter at least a username.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            frmMain.db.ConnectionString = $"server=vulpecula.flaviopereira.digital;user={username};password={password};database=fivem_opadrinhorp";

            try {
                frmMain.db.Open();
                DialogResult = DialogResult.OK;
                Close();
            } catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}
