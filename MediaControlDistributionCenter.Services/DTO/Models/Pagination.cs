using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services.DTO.Models
{
    public class Pagination
    {
        [JsonProperty("currentPage", NullValueHandling = NullValueHandling.Ignore)]
        public long? CurrentPage { get; set; }

        [JsonProperty("pageSize", NullValueHandling = NullValueHandling.Ignore)]
        public long? PageSize { get; set; }

        [JsonProperty("totalItems", NullValueHandling = NullValueHandling.Ignore)]
        public long? TotalItems { get; set; }

        [JsonProperty("totalPages", NullValueHandling = NullValueHandling.Ignore)]
        public long? TotalPages { get; set; }
    }
}
