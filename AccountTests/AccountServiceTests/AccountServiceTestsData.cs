﻿using System;

namespace AccountTests.AccountServiceTests
{
    // Класс, содержащий все необходимые поля для тестирования сервиса AccountService.
    public abstract class AccountServiceTestsData
    {
        // Объект сервиса для того, чтобы взаимодействовать с методами.
        public AccountGRPC.AccountService service = new();
    }
}
