using System;

namespace AccountTests.ExchangeAccessServiceTests
{
    // Класс, содержащий все необходимые поля для тестирования сервиса ExchangeAccessService.
    public abstract class ExchangeAccessServiceTestsData
    {
        // Объект сервиса для того, чтобы взаимодействовать с методами.
        public AccountGRPC.ExchangeAccessService service = new();
        public Random random = new();
    }
}
