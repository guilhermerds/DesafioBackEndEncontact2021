using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Domain.ContactBook;
using TesteBackendEnContact.Core.Domain.ContactBook.Company;
using TesteBackendEnContact.Core.Interface.ContactBook;
using TesteBackendEnContact.Core.Interface.ContactBook.Company;
using TesteBackendEnContact.Database;
using TesteBackendEnContact.Repository.Interface;

namespace TesteBackendEnContact.Repository
{
    public class ContactRepository : IContactRepository
    {
        private readonly DatabaseConfig databaseConfig;

        public ContactRepository(DatabaseConfig databaseConfig)
        {
            this.databaseConfig = databaseConfig;
        }

        public async Task<List<IContact>> SaveAsync(IFormFile file)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            List<string> fileLines;
            List<IContact> contactList = new List<IContact>();
            using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream))
            {
                string content = await reader.ReadToEndAsync();
                fileLines = content.Split("\r\n").ToList();
            }

            for (int i = 1; i < fileLines.Count; i++)
            {
                string[] fields = fileLines[i].Split(",");

                if (fields.Length >= 5)
                {
                    int ContactBookId = int.Parse(fields[0]);
                    string Name = fields[1];
                    string Phone = fields[2];
                    string Email = fields[3];
                    string Address = fields[4];
                    int CompanyId = 0;

                    string query = "SELECT Name FROM ContactBook WHERE Id = @Id;";
                    var resultContactBook = await connection.QueryAsync<ContactBookDao>(query, new { Id = ContactBookId });

                    if (!resultContactBook.Any())
                    {
                        continue;
                    }

                    if (fields.Length >= 6)
                    {
                        CompanyId = int.Parse(fields[5]);
                        query = "SELECT Name FROM Company WHERE Id = @Id;";
                        var resultCompany = await connection.QueryAsync<CompanyDao>(query, new { Id = CompanyId });

                        if (!resultCompany.Any())
                        {
                            CompanyId = 0;
                        }
                    }

                    try
                    {
                        IContact contact = new Contact(0, Name, Phone, Email, Address, ContactBookId, CompanyId)
                        {
                            Address = Address,
                            Email = Email,
                            Name = Name,
                            Phone = Phone
                        };

                        var dao = new ContactDao(contact);

                        dao.Id = await connection.InsertAsync(dao);
                        contactList.Add(dao);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);

                    }
                }
            }
            return contactList;
        }

        public async Task<ContactResponse> GetAsync(int Id, int ContactBookId, string Name, string Phone, string Email, string Address, int CompanyId, string CompanyName, int page)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            page = (page <= 0) ? 1 : page;

            int Limit = 4;//10
            int OffSet = (page - 1) * Limit;

            List<string> filters = new List<string>();
            var values = new
            {
                Id,
                Name,
                Phone,
                Email,
                Address,
                CompanyId,
                CompanyName,
                ContactBookId,
                Limit,
                OffSet
            };


            if (Id != 0)
                filters.Add("Contact.Id = @Id");

            if (ContactBookId != 0)
                filters.Add("Contact.ContactBookId = @ContactBookId");

            if (CompanyId != 0)
                filters.Add("Contact.CompanyId = @CompanyId");

            if (!string.IsNullOrEmpty(Name))
                filters.Add("Contact.Name LIKE @Name");

            if (!string.IsNullOrEmpty(Phone))
                filters.Add("Contact.Phone LIKE @Phone");

            if (!string.IsNullOrEmpty(Email))
                filters.Add("Contact.Email LIKE @Email");

            if (!string.IsNullOrEmpty(Address))
                filters.Add("Contact.Address LIKE @Address");


            var query = @"SELECT Contact.Id,
                   Contact.ContactBookId,
                   Contact.CompanyId,
                   Contact.Name,
                   Contact.Phone,
                   Contact.Email,
                   Contact.Address
                FROM Contact ";

            var queryCount = "SELECT COUNT(Contact.Id) FROM Contact ";

            if (!string.IsNullOrEmpty(CompanyName))
            {
                filters.Add("Company.Name LIKE @CompanyName");

                query += @"INNER JOIN Company 
                    ON Contact.CompanyId = Company.Id ";

                queryCount += @"INNER JOIN Company 
                    ON Contact.CompanyId = Company.Id ";
            }


            if (filters.Count > 0)
            {
                string whereAttributes = filters.Aggregate((a, b) => a + " AND " + b);
                queryCount += "WHERE " + whereAttributes;
                query += "WHERE " + whereAttributes;
            }

            var totalRecord = await connection.QuerySingleOrDefaultAsync<int>(queryCount, values);

            query += " LIMIT @Limit OFFSET @OffSet";

            var result = await connection.QueryAsync<ContactDao>(query, values);

            double totalPages = (double)totalRecord / Limit;
            var response = new ContactResponse
            {
                Contact = result?.Select(item => item.Export()),
                TotalPages = (int)Math.Ceiling(totalPages)
            };

            return response;
        }
        public async Task<ContactResponse> GetAllContactFromCompanyAsync(int CompanyId, int ContactBookId, int page)
        {
            if (CompanyId == 0)
                return null;

            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            page = (page <= 0) ? 1 : page;

            int Limit = 4;
            int OffSet = (page - 1) * Limit;

            var values = new
            {
                Limit,
                OffSet,
                CompanyId,
                ContactBookId
            };

            string query = @"SELECT Id,
                   ContactBookId,
                   CompanyId,
                   Name,
                   Phone,
                   Email,
                   Address
                FROM Contact
                WHERE CompanyId = @CompanyId ";

            string queryCount = @"SELECT COUNT(Id)
                FROM Contact
                WHERE CompanyId = @CompanyId ";

            if (ContactBookId > 0)
            {
                query += "AND ContactBookId = @ContactBookId";

                queryCount += "AND ContactBookId = @ContactBookId";
            }

            var totalRecord = await connection.QuerySingleOrDefaultAsync<int>(queryCount, values);

            query += " LIMIT @Limit OFFSET @OffSet";

            var result = await connection.QueryAsync<ContactDao>(query, values);

            double totalPages = (double)totalRecord / Limit;
            var response = new ContactResponse
            {
                Contact = result?.Select(item => item.Export()),
                TotalPages = (int)Math.Ceiling(totalPages)
            };

            return response;
        }
    }

    [Table("Contact")]
    public class ContactDao : IContact
    {
        [Key]
        public int Id { get; set; }
        public int ContactBookId { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

        public ContactDao()
        {
        }

        public ContactDao(IContact contact)
        {
            Id = contact.Id;
            Name = contact.Name;
            Phone = contact.Phone;
            Email = contact.Email;
            Address = contact.Address;
            CompanyId = contact.CompanyId;
            ContactBookId = contact.ContactBookId;
        }

        public IContact Export() => new Contact(Id, Name, Phone, Email, Address, ContactBookId, CompanyId);
    }

    public class ContactResponse{
        public int TotalPages { get; set; }
        public IEnumerable<IContact> Contact { get; set; }
    }
}
