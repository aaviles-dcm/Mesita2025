using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Data.Entities
{
    public class User
    {
        public Guid UserId { get; set; }
        public string DomainUsername { get; set; } // DOMAIN\jsmith
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }

    public enum UserRole { User, Engineer, Administrator }
}
