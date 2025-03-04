using System.ComponentModel.DataAnnotations;

namespace MediaControlDistributionCenter.Models
{
    public class IModel
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString().ToLower();
    }
}
