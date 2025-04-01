using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services.DTO.Models
{
    public abstract class BaseDto
    {
        /// <summary>
        /// 主键
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }
    }
}
