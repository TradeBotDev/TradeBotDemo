using AccountGRPC.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountTests
{
    public static class ClearAfterCompletion
    {
        public static void Clear(params string[] files)
        {
            foreach (string file in files)
                if (File.Exists(file))
                    File.Delete(file);

            using (var database = new AccountContext())
            {
                database.Accounts.RemoveRange(database.Accounts);
                database.ExchangeAccesses.RemoveRange(database.ExchangeAccesses);
            }
            State.loggedIn = new();
        }
    }
}
