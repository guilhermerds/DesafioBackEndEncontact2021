using System.ComponentModel.DataAnnotations;
using TesteBackendEnContact.Core.Domain.ContactBook;
using TesteBackendEnContact.Core.Interface.ContactBook;

namespace TesteBackendEnContact.Controllers.Models
{
    public class UpdateContactBookRequest
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public IContactBook toContactBook() => new ContactBook(Id, Name);
    }
}
