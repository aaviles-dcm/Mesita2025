using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDesk.Data.Entities
{
    public class UserPassword
    {
        [Key]
        public Guid UserId { get; set; }
        
        [ForeignKey("UserId")]
        public User User { get; set; }
        
        public string Password { get; set; }
    }
}
