using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace DocParser.Models
{
    public class Demand
    {
        [JsonProperty]
        private string demandNumber;
        [JsonProperty]
        private string demand;
       

        public Demand(string demand)
        {
            this.demand = demand;
        }

        public String getDemand() {
            return demand;
        }
        public string getdemandNumber()
        {
            return demandNumber;
        }
        public void setdemandNumber(string number)
        {
            this.demandNumber = number;
        }

        public override string ToString()
        {
            return string.Format(
            CultureInfo.InvariantCulture,
            "{{Demand: {0}, /n {1}}}",
            demandNumber,
            demand);
        }
    }
}