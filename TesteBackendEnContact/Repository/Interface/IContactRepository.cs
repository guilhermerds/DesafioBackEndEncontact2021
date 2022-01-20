using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Interface.ContactBook;

namespace TesteBackendEnContact.Repository.Interface
{
    public interface IContactRepository
    {
        Task<List<IContact>> SaveAsync(IFormFile file);
        Task<ContactResponse> GetAsync(int id, int contactBookId, string name, string phone, string email, string address, int companyId, string companyName, int page);
        Task<ContactResponse> GetAllContactFromCompanyAsync(int companyId, int ContactBookId, int page);
    }
}
