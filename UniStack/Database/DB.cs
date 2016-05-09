using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Entity;

namespace UniStack.Database
{
    public class DB : DbContext
    {
        public DbSet<Post> Posts { get; set; }
        public DbSet<Term> Terms { get; set; }



        protected override void OnConfiguring(DbContextOptionsBuilder opts)
        {
            opts.UseNpgsql("Server=localhost;Port=5432;Username=postgres;Password=icecave17;Database=unistackTest");

            //var cfg = Configs.DatabaseConfig.ToStringDictionary();
            //var cnStr = cfg.Keys.Zip(cfg.Values, (k, v) => $"{k}={v};").Aggregate((c, n) => c + n);

            //opts.UseNpgsql(cnStr);
        }
    }
}
