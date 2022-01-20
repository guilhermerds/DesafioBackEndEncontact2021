using TesteBackendEnContact.Core.Interface.ContactBook;

namespace TesteBackendEnContact.Core.Domain.ContactBook
{
    public class Contact : IContact
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public int ContactBookId { get; set; }
        public int CompanyId { get; set; }

        public Contact(int id, string name, string phone, string email,string address, int contactBookId, int companyId)
        {
            Id = id;
            Name = name;
            Phone = phone;
            Email = email;
            Address = address;
            ContactBookId = contactBookId;
            CompanyId = companyId;
        }
    }
}
