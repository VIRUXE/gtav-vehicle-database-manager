using MySqlConnector;
using System.Data;

namespace gtavvehicles {
    public partial class frmMain : Form {
        public static MySqlConnection db = new();
        private List<Vehicle> vehicles = new();
        private int currentVehicle = 0;
        private Dictionary<string, string> vehicleImages = new();

        public frmMain() {
            InitializeComponent();

            ConnectionCredentials cc = new();

            // If the dialog doesn't come back as OK then close the application
            if (cc.ShowDialog() != DialogResult.OK) return;

            // We need to get the vehicles that either have 0 or NULL in their columns.
            // Columns: key_type, capacity_fuel, capacity_oil, capacity_radiator, capacity_trunk, capacity_glovebox, fuel_type and factory_price
            // Execute a query to get all the vehicles that need to be edited
            MySqlCommand cmd = new("SELECT * FROM vehicles_metadata_custom WHERE key_type = 0 OR key_type IS NULL OR capacity_fuel = 0 OR capacity_fuel IS NULL OR capacity_oil = 0 OR capacity_oil IS NULL OR capacity_radiator = 0 OR capacity_radiator IS NULL OR capacity_trunk = 0 OR capacity_trunk IS NULL OR capacity_glovebox = 0 OR capacity_glovebox IS NULL OR fuel_type = 0 OR fuel_type IS NULL OR factory_price = 0 OR factory_price IS NULL", db);
            MySqlDataReader reader = cmd.ExecuteReader();

            // Add each vehicle to the list
            while (reader.Read()) {
                // Put all the reader values inside an array
                string[] data = new string[reader.FieldCount];
                for (int i = 0; i < reader.FieldCount; i++) data[i] = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i).ToString();

                vehicles.Add(new Vehicle(data));
            }

            // Close the reader
            reader.Close();

            Text = $"GTAV Vehicles - {vehicles.Count} vehicles";

            // Now we get the images from the "vehicles_images"
            // We'll use the model name as the key with "vehicleImages"
            cmd = new("SELECT model, url FROM vehicles_images", db);
            reader = cmd.ExecuteReader();

            // Add each vehicle to the list
            while (reader.Read()) {
                var model = reader.GetString(0);

                if (!vehicleImages.ContainsKey(model)) vehicleImages.Add(model, reader.GetString(1));
            }

            // Close the reader
            reader.Close();

            db.Close();
        }

        private void frmMain_Load(object sender, EventArgs e) {
            // Close the app if there's no connection to the database
            if (vehicles.Count == 0) {
                Application.Exit();
                return; // ? Is this needed?
            }

            // Load the first vehicle in the list
            PopulateForm();
        }

        // Application Close
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e) {
            // Close the database connection if it's open
            if (db.State == ConnectionState.Open) db.Close();
        }

        private void btnPrevious_Click(object sender, EventArgs e) {
            // If the current vehicle is the first one then don't do anything
            if (currentVehicle == 0) return;

            currentVehicle--;
            PopulateForm();
        }
        private void btnNext_Click(object sender, EventArgs e) {
            if (currentVehicle == vehicles.Count - 1) return;

            currentVehicle++;
            PopulateForm();
        }

        private void PopulateForm() {
            Vehicle v = vehicles[currentVehicle];
            string model = v.model;

            Text = $"GTAV Vehicles - {currentVehicle + 1} / {vehicles.Count} - {model}";

            cmbKeyType.Text          = v.key_type;
            txtFuelCapacity.Text     = v.capacity_fuel;
            txtOilCapacity.Text      = v.capacity_oil;
            txtRadiatorCapacity.Text = v.capacity_radiator;
            txtTrunkCapacity.Text    = v.capacity_trunk;
            txtGloveboxCapacity.Text = v.capacity_glovebox;
            cmbFuelType.Text         = v.fuel_type;
            txtFactoryPrice.Text     = v.factory_price;

            // Try to load up the image we go from the database
            try {
                if (vehicleImages.ContainsKey(model)) {
                    // We cancel any loading the could have been happening before
                    pctVehicle.CancelAsync();
                    pctVehicle.LoadAsync($"https://gtabase.com{vehicleImages[model]}");
                } else {
                    // Show that the image is loading
                    pctVehicle.Text = this.Text = $"{this.Text} (Sem imagem.)";
                    pctVehicle.Image = null;
                }
            } catch (Exception) { pctVehicle.Image = null; }

            cmbKeyType.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e) {
            // Open the database connection
            if (db.State != ConnectionState.Open) db.Open();

            Vehicle v = vehicles[currentVehicle];
            // Don't do anything if the form values are the same as the database values
            if (cmbKeyType.Text == v.key_type &&
                txtFuelCapacity.Text == v.capacity_fuel &&
                txtOilCapacity.Text == v.capacity_oil &&
                txtRadiatorCapacity.Text == v.capacity_radiator &&
                txtTrunkCapacity.Text == v.capacity_trunk &&
                txtGloveboxCapacity.Text == v.capacity_glovebox &&
                cmbFuelType.Text == v.fuel_type &&
                txtFactoryPrice.Text == v.factory_price)
                return;

            // Build the query string with the text from the form
            // We need to make sure that if any value is NULL we don't put apostrophes around it
            MySqlCommand cmd = new(
                $"UPDATE vehicles_metadata_custom SET " +
                $"key_type = {(cmbKeyType.Text.ToLower() == "null" ? "NULL" : $"'{cmbKeyType.Text}'")}, " +
                $"capacity_fuel = {(txtFuelCapacity.Text.ToLower() == "null" ? "NULL" : $"'{txtFuelCapacity.Text}'")}, " +
                $"capacity_oil = {(txtOilCapacity.Text.ToLower() == "null" ? "NULL" : $"'{txtOilCapacity.Text}'")}, " +
                $"capacity_radiator = {(txtRadiatorCapacity.Text.ToLower() == "null" ? "NULL" : $"'{txtRadiatorCapacity.Text}'")}, " +
                $"capacity_trunk = {(txtTrunkCapacity.Text.ToLower() == "null" ? "NULL" : $"'{txtTrunkCapacity.Text}'")}, " +
                $"capacity_glovebox = {(txtGloveboxCapacity.Text.ToLower() == "null" ? "NULL" : $"'{txtGloveboxCapacity.Text}'")}, " +
                $"fuel_type = {(cmbFuelType.Text.ToLower() == "null" ? "NULL" : $"'{cmbFuelType.Text}'")}, " +
                $"factory_price = {(txtFactoryPrice.Text.ToLower() == "null" ? "NULL" : $"'{txtFactoryPrice.Text}'")} " +
                $"WHERE model = '{v.model}';"
            , db);

            // Execute the query and show a message box with the result
            try {
                if (cmd.ExecuteNonQuery() == 0) MessageBox.Show("Não foi possível guardar os dados!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Update the vehicle object with the new values
                v.key_type          = cmbKeyType.Text;
                v.capacity_fuel     = txtFuelCapacity.Text;
                v.capacity_oil      = txtOilCapacity.Text;
                v.capacity_radiator = txtRadiatorCapacity.Text;
                v.capacity_trunk    = txtTrunkCapacity.Text;
                v.capacity_glovebox = txtGloveboxCapacity.Text;
                v.fuel_type         = cmbFuelType.Text;
                v.factory_price     = txtFactoryPrice.Text;

                btnNext_Click(sender, e);
            } catch (Exception ex) { MessageBox.Show($"Ocorreu um erro ao guardar os dados:\n\n{ex.Message}\n\nConsulta: {cmd.CommandText}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }

            // Close the database connection
            if (db.State != ConnectionState.Closed) db.Close();
        }

        private void btnReset_Click(object sender, EventArgs e) { PopulateForm(); }
    }
}
