using System;
using System.Linq;

namespace AccountGRPC
{
    public static class LoggedAccountsManagement
    {
        // Метод, который проверяет, истекло ли время сессии, и в случае, если оно истекло,
        // удаляет текущий вход и возвращает true. Иначе возвращает false.
        public static bool TimePassed(string sessionId)
        {
            using (var database = new Models.AccountContext())
            {
                // Поиск аккаунта с тем же Id сессии.
                var checkAccount = database.LoggedAccounts.Where(login => login.SessionId == sessionId);
                if (checkAccount.Count() > 0)
                {
                    // Проверка на то, вышло ли время с последнего входа в аккаунт.
                    if (checkAccount.First().LoginDate.AddDays(3) < DateTime.Now)
                    {
                        // Если время вышло, запись удаляется.
                        database.Remove(checkAccount.First());
                        database.SaveChanges();
                        return true;
                    }
                    // Если время не вышло, но при этом пользователь вошел в аккаунт, оно обновляется.
                    checkAccount.First().LoginDate = DateTime.Now;
                    database.SaveChanges();
                    return false;
                }
                // Если аккаунт не был найден среди вошедших, время считается вышедшим, но ничего не происходит.
                else return true;
            }
        }
    }
}
