﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CITR.UZBookingProvider.Domain
{
    public class UZPlaces
    {
        [JsonProperty(PropertyName = "places")]
        public Dictionary<string, int[]> AvaliablePlaceNumbers;
    }
}
