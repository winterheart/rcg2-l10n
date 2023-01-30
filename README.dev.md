# Разработка мода

## Необходимое  окружение

* Unity 2019.4.39f (возможно обновление в рамках 2019.4.*)
  * Пакет TextMeshPro 2.2.0-preview.3
* Visual Studio 2019 с поддержкой разработки проектов .NET 4.7.2
* Python 3.10
  * Модули docopts и polib
* Редактор переводов в формате Gettext

## Процесс сборки

Сборка осуществляется по следующему процессу:

1. Переводчик/текстовый редактор: Перевод файлов
2. Разработчик/rcg2_translate.py: Обновление translation.csv
3. Разработчик/Unity: Обновление ассета с обновленными ресурсами
4. Разработчик/Visual Studio: Пересборка проекта с включением ассета

## Необходимые внешние ресурсы

Для сборки библиотеки мода необходимы компоненты из установленной игры,
которые необходимо сохранить в каталог external

* `0Harmony.dll` (из `MelonLoader`)
* `Assembly-CSharp.dll` (из `RCG2_Data/Managed`)
* `MelonLoader.dll` (из `MelonLoader`)
* `Unity.TextMeshPro.dll` (из `RCG2_Data/Managed`)
* `UnityEngine.AssetBudleModule.dll` (из `RCG2_Data/Managed`)
* `UnityEngine.CoreModule.dll` (из `RCG2_Data/Managed`)
