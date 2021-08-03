using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountGRPC
{
    public static class LoggedAccountsManagement
    {
        public static bool TimePassed(string sessionId)
        {
            using (var database = new Models.AccountContext())
            {
                var checkAccount = database.LoggedAccounts.Where(login => login.SessionId == sessionId);
                if (checkAccount.Count() > 0)
                {
                    if (checkAccount.First().LoginDate.AddSeconds(7) < DateTime.Now)
                    {
                        database.Remove(checkAccount.First());
                        database.SaveChanges();
                        return true;
                    }
                    return false;
                }
                else return true;
            }
        }
    }
}
