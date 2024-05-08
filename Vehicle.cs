namespace gtavvehicles {
    internal class Vehicle {
        // The database doesn't care if the values are a string or an int so we'll just use strings

        public string model { get; set; }
        public string key_type { get; set; }
        public string capacity_fuel { get; set; }
        public string capacity_oil { get; set; }
        public string capacity_radiator { get; set; }
        public string capacity_trunk { get; set; }
        public string capacity_glovebox { get; set; }
        public string fuel_type { get; set; }
        public string factory_price { get; set; }

        public Vehicle(string[] data) {
            model             = data[0];
            key_type          = data[1];
            capacity_fuel     = data[2];
            capacity_oil      = data[3];
            capacity_radiator = data[4];
            capacity_trunk    = data[5];
            capacity_glovebox = data[6];
            fuel_type         = data[7];
            factory_price     = data[9];
        }
    }
}
