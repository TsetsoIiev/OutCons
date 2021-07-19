using Microsoft.EntityFrameworkCore;

namespace OutCons.Web.Extensions
{
    public static class DbContextExtensions
    {
        public static void RemoveEntities<T>(this DbSet<T> dbSet) where T : class
        {
            dbSet.RemoveRange(dbSet);
        }
    }
}
