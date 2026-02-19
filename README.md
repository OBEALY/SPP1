# SPP1: Собственное средство автоматизации тестирования

## О проекте
Этот репозиторий содержит учебную реализацию собственного мини-фреймворка для автотестов на C#/.NET 8.

Проект демонстрирует полный цикл:
1. библиотека тестирования;
2. тестируемый проект;
3. проект с тестами;
4. программа загрузки и выполнения тестов.

## Структура
- `MiniTestFramework/` - библиотека тестирования (атрибуты, assert'ы, исключения, раннер, отчёт).
- `TestedProject/` - тестируемый код (сервисы `MathService` и `TextService`).
- `Tests/` - тестовые классы, включая async-тесты, lifecycle и shared context.
- `Program.cs` - запуск раннера и вывод результата в консоль + файл `TestResults.txt`.

## Что реализовано
### 1) Атрибуты (маркеры)
- Классы: `TestClass`, `TestClassInfo`, `UseSharedContext`.
- Методы: `Test`, `TestCase`, `TestInfo`, `BeforeAll`, `AfterAll`, `BeforeEach`, `AfterEach`.

Поддерживаются:
- маркеры без свойств;
- маркеры со свойствами;
- тестовые методы с параметрами и без параметров.

### 2) Проверки (Assertions)
Реализовано более 10 проверок:
- `True`, `False`
- `Equal`, `NotEqual`
- `Null`, `NotNull`
- `Contains`, `DoesNotContain`
- `Greater`, `Less`
- `SequenceEqual`
- `ThrowsAsync`
- `IsType`

### 3) Обработка ошибок
Собственные исключения:
- `TestFrameworkException`
- `TestDiscoveryException`
- `AssertionFailedException`
- `TestExecutionException`

Раннер различает:
- `Passed`
- `Failed` (провал проверки/assert)
- `Error` (ошибка инфраструктуры/выполнения)

### 4) Контекст тестов
Поддержка lifecycle-методов:
- `BeforeAll` / `AfterAll`
- `BeforeEach` / `AfterEach`

### 5) Асинхронность
Поддержан запуск:
- `Task`
- `ValueTask`

И есть демонстрационные async-тесты (например, проверка `MultiplyAsync`).

### 6) Выполнение тестов
- Автопоиск тестовых классов и методов через reflection.
- Запуск тестов с параметрами (`TestCase`).
- Вывод результатов в консоль.
- Сохранение отчёта в файл `bin/Debug/net8.0/TestResults.txt`.

### 7) Shared Context (дополнительно)
Реализован `ISharedContext` и использование через `UseSharedContext`.
Пример: `Tests/AppSharedContext.cs` и внедрение в `MathServiceTests`.

## Как запустить
```powershell
dotnet run
```

После запуска:
- в консоли печатается сводка;
- создаётся файл `TestResults.txt` с отчётом.
