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

        protected override void OnConfiguring(DbContextOptionsBuilder opts)
        {
            //var cfg = Configs.DatabaseConfig.ToStringDictionary();
            //var cnStr = cfg.Keys.Zip(cfg.Values, (k, v) => $"{k}={v};").Aggregate((c, n) => c + n);

            //opts.UseNpgsql(cnStr);
        }
    }
}
