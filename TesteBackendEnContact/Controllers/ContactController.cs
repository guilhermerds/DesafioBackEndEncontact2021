using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Interface.ContactBook;
using TesteBackendEnContact.Repository;
using TesteBackendEnContact.Repository.Interface;

namespace TesteBackendEnContact.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly ILogger<ContactController> _logger;

        public ContactController(ILogger<ContactController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<IContact>> Post([Required] IFormFile files, [FromServices] IContactRepository contactRepository)
        {
            return Ok(await contactRepository.SaveAsync(files));
        }

        [HttpGet]

        public async Task<ContactResponse> Get(int id, int contactBookId, string name, string phone, string email, string address, int companyId, string companyName, int page, [FromServices] IContactRepository contactRepository)
        {
            return await contactRepository.GetAsync(id, contactBookId, name, phone, email, address, companyId, companyName, page);
        }

        [HttpGet("{companyId}")]

        public async Task<ContactResponse> Get([Required]int companyId, int contactBookId, int page, [FromServices] IContactRepository contactRepository)
        {
            return await contactRepository.GetAllContactFromCompanyAsync(companyId, contactBookId, page);
        }
    }
}
