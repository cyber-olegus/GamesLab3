# Лабораторная работа №3

C#, .NET 8, WPF. Откройте GamesLab3.sln в Visual Studio 2022.

- TimedClicker.Wpf: задания 1–2, кликер на время. Флажок включает бонусы и перезарядку.
- EnemyClicker.Wpf: задание 3, противники из JSON Лабы 1, перезарядка, улучшения и бонусы.
- GamesLab3.Core: игровые классы; абстрактный CCollectable и единый List<CCollectable> в CController.
- GamesLab3.Checks: консольные проверки без внешних библиотек.

Сборка: `dotnet build GamesLab3.sln -c Release`.
Проверки: `dotnet run --project tests/GamesLab3.Checks/GamesLab3.Checks.csproj -c Release`.
В общей основе 7 проверок; с частью кликера на время — 21, с боевой частью — 20; после объединения — 34.

Для запуска правой кнопкой назначьте нужный WPF-проект запускаемым и нажмите F5.
В боевом проекте выберите Data/enemies.demo.json для быстрой демонстрации или собственный JSON редактора.
PNG из прежней игры можно положить в src/EnemyClicker.Wpf/Assets/Icons с вложенными папками.
Если PNG отсутствует, используется встроенный рисунок.

Вариант 01-base содержит только окна-заготовки. Приложения появляются после добавления частей из feature-веток.
