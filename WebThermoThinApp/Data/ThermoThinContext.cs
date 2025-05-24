using Microsoft.EntityFrameworkCore;

namespace WebThermoThinApp.Data
{
    public class ThermoThinContext :DbContext
    {
        public DbSet<Variant> Variants { get; set; }

        public ThermoThinContext(DbContextOptions<ThermoThinContext> options) : base(options)
        {
        }
    }
}
