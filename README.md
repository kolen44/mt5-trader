<img width="1919" height="1150" alt="image" src="https://github.com/user-attachments/assets/98c51775-73a1-4769-a455-6069ad043af1" />
<img width="1183" height="699" alt="image" src="https://github.com/user-attachments/assets/72143dff-ba24-45fd-85a4-7608a8279e81" />

# Tick Lead-Lag Analyzer

Приложение для анализа тиков MT5 — показывает какой символ ведущий, какой отстающий.

## Запуск

### Обычный запуск

Нужен .NET 9.0 SDK и Windows.

```bash
cd result
dotnet restore
dotnet build
dotnet run --project src/TickLeadLagAnalyzer
```

### Docker

```bash
docker build -t tick-analyzer .
docker run -it tick-analyzer
```

Или через docker-compose:
```bash
docker-compose up --build
```

### Тестовый счёт

- Server: 185.97.160.70:443
- Login: 5002166
- Password: !l2kCkPka

## Структура проекта

Clean Architecture, три слоя:

```
src/
├── TickLeadLagAnalyzer/              # WPF интерфейс
│   ├── Views/
│   ├── ViewModels/
│   └── Themes/
│
├── TickLeadLagAnalyzer.Domain/       # Модели и интерфейсы
│
└── TickLeadLagAnalyzer.Infrastructure/   # Сервисы
```

## Как работает

MT5 шлёт тики → складываем в ring buffer → считаем gap-to-base и корреляции → рисуем на графике.

### TickBuffer

Кольцевой буфер на LinkedList. Хранит тики за последние N секунд, старые автоматом удаляются.

### Анализ

**Gap-to-Base** — разница накопленной доходности между символом и базовым:
```
GapToBase = (Price/Price0 - 1) - (BasePrice/BasePrice0 - 1)
```

**Lag-корреляция** — ищем при каком сдвиге по времени корреляция максимальна. Если лаг положительный — символ ведущий, отрицательный — отстающий.

## Зависимости

- CommunityToolkit.Mvvm — MVVM
- LiveChartsCore.SkiaSharpView.WPF — графики
- Serilog — логи
- mtapi.mt5 — подключение к MT5

Логи пишутся в `logs/app-{date}.log`
