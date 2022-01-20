using System.Collections.Generic;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Interface.ContactBook.Company;

namespace TesteBackendEnContact.Repository.Interface
{
    public interface ICompanyRepository
    {
        Task<ICompany> SaveAsync(ICompany company);
        Task<ICompany> UpdateAsync(ICompany company);
        Task DeleteAsync(int id);
        Task<IEnumerable<ICompany>> GetAllAsync();
        Task<ICompany> GetAsync(int id);
    }
}
