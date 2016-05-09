using Microsoft.Data.Entity;

namespace UniStack.Database
{
    public class DB : DbContext
    {
        public DbSet<Post> Posts { get; set; }
        public DbSet<Term> Terms { get; set; }



        protected override void OnConfiguring(DbContextOptionsBuilder opts)
        {

            //var cfg = Configs.DatabaseConfig.ToStringDictionary();
            //var cnStr = cfg.Keys.Zip(cfg.Values, (k, v) => $"{k}={v};").Aggregate((c, n) => c + n);

            //opts.UseNpgsql(cnStr);
        }
    }
}
