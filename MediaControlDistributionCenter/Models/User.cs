namespace MediaControlDistributionCenter.Models
{
    public class User : IModel
    {

        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public string AliasName { get; set; }
        public string GroupName { get; set; }

    }
}
