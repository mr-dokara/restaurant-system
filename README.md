### Уважаемые тестировщики!
Спасибо, что присоединились к проверке работоспособности нашего проекта. Перед тем, как приступить к тесту, ознакомлю вас с кратким содержанием всего того, что вам понадобится скачать с нашего репозитория для теста:
1. Папка Client (за авторством sA1mon) - проект WPF, являющийся терминалом для официантов;
2. Папка Restaurant-Manager (загруженная MrDokara) - тоже проект WPF, созданный для комфортного менеджмента базы данных;
3. Папка SiteASPAuth по ссылке из файла "Site on ASP.NET MVC.TXT" (так как GitHub отказывается принимать файлы больше 25 Мб, пришлось закидывать его на гоголь.диск).

#### Запуск
С проектами WPF сложности быть не должно - это стандартные проекты, использование обычное. С проектом ASP.NET MVC (который с диска) интереснее - в скачанной папке требуется запустить файл с расширением .sln (то есть открыть проект), далее запустить его зеленой кнопкой вверху с надписью "IIS Express (<ваш браузер по умолчанию>)", после этого согласиться со всем, что будет вам предложено в открывающихся окошках (они запрашивают разрешение на локальное развертывание сайта (localhost) и на создание локальной же базы данных, если не ошибаюсь), в результате у вас откроется заглавная страница сайта в браузере. После первого такого запуска сайт можно будет запускать и без дебага (через Ctrl+F5).
По аккаунтам - в приложении менеджера аккаунт не нужен, на сайте можно спокойно создать свой личный аккаунт ("Account" -> "Register"), для входа в приложение официантов нужно создать свой аккаунт в приложении менеджера.
Кстати, у сайта нет мобильной версии, как бы прискорбно не было это сообщать, держитесь там и не сильно балуйтесь с разрешением (уже нет сил заниматься версткой).


#### Краткое описание
Наш проект - система ресторана, удобно переводящая в электронную сферу то, что обычно создавалось на бумаге. Основные части - сайт, с помощью которого клиенты могут создавать заказы (в своем личном аккаунте, причем корзина сохраняется после выхода из аккаунта); приложение менеджера, который контролирует список блюд, доступных для заказа на сайте и список официантов, работающих в настоящий момент (плюс редактирование вот этого вот всего); и приложение официантов, позволяющее быстро создавать заказы прямо в ресторане. Дополнительные элементы (их тест необязателен, так как мы они лежат в основе нашего проекта, и мы сами занимались их тестированием во время разработки) - база данных MySQL, развернутая в облачной службе Microsoft Azure по студенческой подписке (в ней хранится информация о блюдах, официантах и заказах); и библиотека DatabaseConnectionLib, которая упрощает работу с этой базой данных.

#### Ответы на возможные вопросы
- Как попасть в приложение официантов (там же требуется пароль)?
- Можно создать собственный аккаунт в приложении менеджера и уже с их помощью зайти в приложение официантов.

* Как получить доступ к базе данных MySQL?
* Доступ к ней запрещен, потому что ее создание занимает слишком много времени, и не хотелось бы ее восстанаваливать после нечаянного ```drop database restaurant``` (если что-то подобное произойдет - руки оторву и тестирование закончится).

- Почему нет исходного кода библиотеки DatabaseConnectionLib?
- В ее коде встречаются данные для подключения к БД, которые я сначала не хотел показывать (поэтому эта часть проекта загружалась в мой личный репозиторий), но эти же данные есть в проекте сайта... Короче, голова странно работает у меня, и все равно это библиотека используется лишь нами - разработчиками, пользователи о ней вообще не должны знать.

* Что за странные картинки и прочие непонятные файлы?
* Файл DatabaseConnectionLib.xml - конфигурационный файл, упрощающий работу с нашей библиотекой (описание при наведении на методы и классы - та же документация, проще говоря), картинки - некоторые из них загружались для краткого описания проекта или его частей (например, базы данных), некоторые же - сделаны в неустойчивом психическом состоянии (комментировать это не буду, сами поймете), папка Site no.1 - первый концепт сайта, в настоящий момент устарел (в связи с переходом на другую платформу разработки)
<br>
<br>

> Возможно, этот файл будет исправляться (если будут добавляться файлы или исправляться некоторые ошибки).
##### Разработчики - sA1mon (Семен), MrDokara (Иван), bilskla (Никита)
###### Автор этой ахинеи - bilskla (Никита)
