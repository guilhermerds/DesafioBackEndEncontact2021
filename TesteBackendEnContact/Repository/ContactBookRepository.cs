using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Domain.ContactBook;
using TesteBackendEnContact.Core.Interface.ContactBook;
using TesteBackendEnContact.Database;
using TesteBackendEnContact.Repository.Interface;

namespace TesteBackendEnContact.Repository
{
    public class ContactBookRepository : IContactBookRepository
    {
        private readonly DatabaseConfig databaseConfig;

        public ContactBookRepository(DatabaseConfig databaseConfig)
        {
            this.databaseConfig = databaseConfig;
        }


        public async Task<IContactBook> SaveAsync(IContactBook contactBook)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);
            var dao = new ContactBookDao(contactBook);

            if (dao.Id == 0)
                dao.Id = await connection.InsertAsync(dao);
            else
                await connection.UpdateAsync(dao);

            return dao.Export();
        }

        public async Task<IContactBook> UpdateAsync(IContactBook contactBook)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);
            var dao = new ContactBookDao(contactBook);

            if (dao.Id != 0)
                await connection.UpdateAsync(dao);

            return dao.Export();
        }


        public async Task DeleteAsync(int id)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            await connection.DeleteAsync(new ContactBookDao { Id = id });
        }

        public async Task<IEnumerable<IContactBook>> GetAllAsync()
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var query = "SELECT * FROM ContactBook";
            var result = await connection.QueryAsync<ContactBookDao>(query);

            var returnList = new List<IContactBook>();

            foreach (var AgendaSalva in result.ToList())
            {
                IContactBook Agenda = new ContactBook(AgendaSalva.Id, AgendaSalva.Name.ToString());
                returnList.Add(Agenda);
            }

            return returnList.ToList();
        }
        public async Task<IContactBook> GetAsync(int id)
        {
            var list = await GetAllAsync();

            return list.ToList().Where(item => item.Id == id).FirstOrDefault();
        }
    }

    [Table("ContactBook")]
    public class ContactBookDao : IContactBook
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        public ContactBookDao()
        {
        }

        public ContactBookDao(IContactBook contactBook)
        {
            Id = contactBook.Id;
            Name = contactBook.Name;
        }

        public IContactBook Export() => new ContactBook(Id, Name);
    }
}
