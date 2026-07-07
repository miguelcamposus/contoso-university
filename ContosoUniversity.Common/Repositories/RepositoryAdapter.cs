using ContosoUniversity.Common.Interfaces;
using ContosoUniversity.Data.DbContexts;
using ContosoUniversity.Data.Entities;

namespace ContosoUniversity.Common.Repositories
{
    // Adapter that binds IRepository<T> to Repository<T, ApplicationContext>
    public class Repository<T> : ContosoUniversity.Data.Repository<T, ApplicationContext>, IRepository<T> where T : BaseEntity
    {
        public Repository(ApplicationContext context) : base(context)
        {
        }
    }
}
