using System.Text.RegularExpressions;

namespace Restoran.Api.Infrastructure;

/// <summary>
/// Veateadete tõlge. Teenused annavad teate eesti keeles; veebileht saadab päises Accept-Language
/// saidi keele (et/ru/en) ja vastus tõlgitakse vene või inglise keelde.
/// {0}, {1}, {2} on muutuvad osad (number, nimi, summa).
/// </summary>
public static class ErrorTranslator
{
    private static readonly (string Et, string Ru, string En)[] Texts =
    {
        ("Broneerida saab kuni 90 päeva ette.", "Бронировать можно не более чем на 90 дней вперёд.", "You can book up to 90 days ahead."),
        ("Broneering on juba makstud.", "Бронь уже оплачена.", "The booking is already paid."),
        ("Broneeringu aeg ei saa olla minevikus.", "Время брони не может быть в прошлом.", "The booking time cannot be in the past."),
        ("Eeltellimust saab lisada ainult aktiivsele broneeringule.", "Предзаказ можно добавить только к активной брони.", "A pre-order can only be added to an active booking."),
        ("Inimeste arv peab olema 1 kuni 20.", "Число гостей должно быть от 1 до 20.", "The number of guests must be 1 to 20."),
        ("Kaardi number on vigane.", "Неверный номер карты.", "Invalid card number."),
        ("Kasutajat ei leitud.", "Пользователь не найден.", "User not found."),
        ("Kellaaeg peab olema kujul HH:mm.", "Время должно быть в формате ЧЧ:мм.", "Time must be in HH:mm format."),
        ("Kliendi nimel saab broneerida ainult personal.", "Бронировать за гостя может только персонал.", "Only staff can book on behalf of a guest."),
        ("Klienti ei leitud.", "Гость не найден.", "Guest not found."),
        ("Külastuse aeg on möödas – makselink ei kehti enam.", "Время визита прошло — ссылка на оплату больше не действует.", "The visit time has passed – the payment link is no longer valid."),
        ("Külastuseni on vähem kui 24 tundi: makstud summat ei tagastata. Tühistamiseks kinnitage, et olete sellega nõus.", "До визита меньше 24 часов: оплаченная сумма не возвращается. Для отмены подтвердите, что Вы согласны.", "Less than 24 hours before the visit the paid amount is not refunded. Please confirm that you agree to cancel."),
        ("Laual on tulevasi broneeringuid. Märgi laud mitteaktiivseks.", "У стола есть будущие брони. Отметьте стол неактивным.", "The table has future bookings. Mark it inactive instead."),
        ("Lauda selles restoranis ei leitud.", "Стол в этом ресторане не найден.", "Table not found in this restaurant."),
        ("Lisateenuseid saab muuta ainult aktiivsel broneeringul.", "Дополнительные услуги можно менять только в активной брони.", "Extras can only be changed on an active booking."),
        ("Makselinki ei leitud või see on aegunud.", "Ссылка на оплату не найдена или устарела.", "Payment link not found or expired."),
        ("Maksta saab ainult aktiivse broneeringu eest.", "Оплатить можно только активную бронь.", "Only an active booking can be paid."),
        ("Muuta saab ainult aktiivset broneeringut.", "Изменить можно только активную бронь.", "Only an active booking can be changed."),
        ("Olete sellele õhtule juba registreerunud.", "Вы уже записаны на этот вечер.", "You are already registered for this evening."),
        ("Palun kirjutage oma nimi.", "Пожалуйста, напишите своё имя.", "Please enter your name."),
        ("Personal ei registreeru õhtule enda nimele.", "Персонал не может записаться на вечер на своё имя.", "Staff cannot register for an evening themselves."),
        ("Personal saab broneerida ainult kliendi nimel (Uus broneering).", "Персонал бронирует только за гостя («Новая бронь»).", "Staff can only book on behalf of a guest (New booking)."),
        ("Pilt peab olema väike (kuni 300 KB) JPEG või PNG.", "Фото должно быть JPEG или PNG до 300 КБ.", "The photo must be a JPEG or PNG up to 300 KB."),
        ("Pole midagi maksta: eeltellimus ja lisateenused puuduvad.", "Оплачивать нечего: нет предзаказа и дополнительных услуг.", "Nothing to pay: no pre-order or extras."),
        ("Pole sisse logitud.", "Вы не вошли в систему.", "You are not signed in."),
        ("Päevade arv peab olema 1 kuni 60.", "Число дней должно быть от 1 до 60.", "The number of days must be 1 to 60."),
        ("Päevade arv peab olema 1 kuni 62.", "Число дней должно быть от 1 до 62.", "The number of days must be 1 to 62."),
        ("Registreerimiseks tuleb nõustuda õhtu reeglitega.", "Для записи нужно согласиться с правилами вечера.", "You must accept the evening's rules to register."),
        ("See broneering on juba kinnitatud või tühistatud.", "Эта бронь уже подтверждена или отменена.", "This booking is already confirmed or cancelled."),
        ("See ei ole sinu broneering.", "Это не Ваша бронь.", "This is not your booking."),
        ("See ei ole sinu teade.", "Это не Ваше уведомление.", "This is not your notification."),
        ("See õhtu on juba toimunud.", "Этот вечер уже прошёл.", "This evening has already taken place."),
        ("Selle e-postiga kasutaja on juba olemas.", "Пользователь с таким e-mail уже существует.", "A user with this e-mail already exists."),
        ("Sellel ajal pole sobiva suurusega vaba lauda. Proovi muud aega.", "На это время нет свободного стола подходящего размера. Попробуйте другое время.", "No free table of a suitable size at this time. Try another time."),
        ("Sulgemisaeg peab olema hilisem kui avamisaeg.", "Время закрытия должно быть позже времени открытия.", "Closing time must be later than opening time."),
        ("Sõnum on tühi.", "Сообщение пустое.", "The message is empty."),
        ("Te pole sellele õhtule registreerunud.", "Вы не записаны на этот вечер.", "You are not registered for this evening."),
        ("Toit on eeltellimustes kasutusel. Märgi see mitte saadaval olevaks.", "Блюдо есть в предзаказах. Отметьте его как недоступное.", "The dish is used in pre-orders. Mark it unavailable instead."),
        ("Tühistada saab ainult aktiivset broneeringut.", "Отменить можно только активную бронь.", "Only an active booking can be cancelled."),
        ("Vale e-post või parool.", "Неверный e-mail или пароль.", "Wrong e-mail or password."),
        ("Valige klient või sisestage uue kliendi andmed.", "Выберите гостя или введите данные нового гостя.", "Choose a guest or enter a new guest's details."),
        ("Vähem kui 24 tundi enne broneeringut ei saa seda enam muuta ega tühistada. Helista restorani.", "Менее чем за 24 часа бронь нельзя изменить или отменить. Позвоните в ресторан.", "Less than 24 hours before the booking it cannot be changed or cancelled. Please call the restaurant."),
        ("Õhtul saab lisada ainult pudeli šampanjat ja lillekimbu.", "На вечер можно добавить только бутылку шампанского и букет цветов.", "Only a bottle of champagne and a bouquet can be added to the evening."),
        ("CVC on 3 numbrit.", "CVC — это 3 цифры.", "CVC is 3 digits."),
        ("Keel peab olema et, ru või en.", "Язык должен быть et, ru или en.", "Language must be et, ru or en."),
        ("Kehtivus peab olema kujul KK/AA.", "Срок действия в формате ММ/ГГ.", "Expiry must be in MM/YY format."),
        ("Telefoninumber on vigane.", "Неверный номер телефона.", "Invalid phone number."),
        ("Registreerimine on lõppenud – registreeruda saab kuni 12 tundi enne õhtu algust.", "Запись закрыта — записаться можно не позднее чем за 12 часов до начала вечера.", "Registration is closed – you can register up to 12 hours before the evening starts."),
        ("Eripakkumise nimi on liiga lühike.", "Название позиции слишком короткое.", "The item name is too short."),
        ("Õhtule on registreerunud külalisi – restorani ei saa muuta.", "На вечер уже записаны гости — ресторан изменить нельзя.", "Guests have registered – the restaurant cannot be changed."),
        ("Õhtule on registreerunud külalisi – tühistage enne nende registreerimised.", "На вечер записаны гости — сначала отмените их записи.", "Guests have registered – cancel their registrations first."),
        ("Ainult personalile.", "Только для персонала.", "Staff only."),
        ("Kliendi nimel saab registreerida ainult personal.", "Записать гостя может только персонал.", "Only staff can register a guest."),
        ("Õhtuid saab hallata ainult admin.", "Управлять вечерами может только админ.", "Only the admin can manage evenings."),
        ("Selle e-postiga on meil juba konto. Saatsime Teie e-postile lingi parooli loomiseks.", "У нас уже есть аккаунт с этим e-mail. Мы отправили Вам на почту ссылку для создания пароля.", "We already have an account with this e-mail. We sent you a link to create a password."),
        ("Link parooli loomiseks ei kehti enam. Küsige uus link restoranist.", "Ссылка для создания пароля больше не действует. Попросите новую в ресторане.", "The password link is no longer valid. Ask the restaurant for a new one."),
        ("Klient on parooli juba ise loonud.", "Гость уже сам создал пароль.", "The guest has already created a password."),
        ("Broneerimisel makstud teenust ei saa eemaldada. Kirjutage vajadusel restoranile broneeringu sõnumites.", "Услугу, оплаченную при бронировании, убрать нельзя. При необходимости напишите ресторану в сообщениях брони.", "An extra paid when booking cannot be removed. If needed, write to the restaurant in the booking messages."),
        ("Laud ei mahu plaanile – lohistage see saali sisse.", "Стол не помещается на плане — перетащите его внутрь зала.", "The table does not fit on the plan – drag it inside the room."),
        ("Laud ei saa olla baari peal. Valige teine koht.", "Стол не может стоять на баре. Выберите другое место.", "A table cannot stand on the bar. Choose another spot."),
        ("Laud ei saa olla laval. Valige teine koht.", "Стол не может стоять на сцене. Выберите другое место.", "A table cannot stand on the stage. Choose another spot."),
        ("Teenusekonto ei saa broneerida.", "Служебный аккаунт не может бронировать.", "The service account cannot make bookings."),
        ("Saabumist saab märkida ainult aktiivsel broneeringul.", "Отметить приход можно только у активной брони.", "Arrival can only be marked on an active booking."),
        ("Saabumist saab märkida alles külastuse päeval.", "Отметить приход можно только в день визита.", "Arrival can only be marked on the day of the visit."),
        ("Midagi läks valesti. Proovi hiljem uuesti.", "Что-то пошло не так. Попробуйте позже.", "Something went wrong. Please try again later."),
        // muutuvate osadega
        ("Jooki \"{0}\" sellel õhtul ei pakuta.", "Напиток «{0}» на этом вечере не подаётся.", "The drink \"{0}\" is not served this evening."),
        ("Kasutajat {0} ei leitud.", "Пользователь {0} не найден.", "User {0} not found."),
        ("Laud numbriga {0} on selles restoranis juba olemas.", "Стол с номером {0} в этом ресторане уже есть.", "Table number {0} already exists in this restaurant."),
        ("Laud {0} on sel ajal juba broneeritud.", "Стол {0} на это время уже забронирован.", "Table {0} is already booked for this time."),
        ("Laud {0} on {1} kohaga, see on {2} inimesele liiga väike.", "Стол {0} (мест: {1}) слишком мал для {2} гостей.", "Table {0} has {1} seats – too small for {2} guests."),
        ("Laud {0} on juba võetud. Valige teine laud.", "Стол {0} уже занят. Выберите другой стол.", "Table {0} is already taken. Choose another table."),
        ("Laud {0} on {1} kohaga.", "Стол {0}: мест — {1}.", "Table {0} has {1} seats."),
        ("Lauda {0} ei leitud.", "Стол {0} не найден.", "Table {0} not found."),
        ("Lauda {0} selles restoranis ei leitud.", "Стол {0} в этом ресторане не найден.", "Table {0} not found in this restaurant."),
        ("Menüüpunkti {0} ei leitud.", "Блюдо {0} не найдено.", "Menu item {0} not found."),
        ("Restorani {0} ei leitud.", "Ресторан {0} не найден.", "Restaurant {0} not found."),
        ("Tasuda tuleb {0} € – sisestage kaardi andmed.", "К оплате {0} € — введите данные карты.", "You need to pay {0} € – enter your card details."),
        ("Teadet {0} ei leitud.", "Уведомление {0} не найдено.", "Notification {0} not found."),
        ("Tundmatu lisateenus \"{0}\".", "Неизвестная дополнительная услуга «{0}».", "Unknown extra \"{0}\"."),
        ("Vabu kohti on alles {0}.", "Свободных мест осталось: {0}.", "Only {0} seats left."),
        ("Õhtu kuupäev peab olema vähemalt {0} päeva pärast.", "Дата вечера — не раньше чем через {0} дней.", "The evening must be at least {0} days from now."),
        ("Õhtu algus peab jääma lahtiolekuaja sisse ({0}–{1}).", "Начало вечера должно быть в часы работы ({0}–{1}).", "The evening must start within opening hours ({0}–{1})."),
        ("Selles saalis on ainult {0} kohta.", "В этом зале только {0} мест.", "This room has only {0} seats."),
        ("Registreerunud on juba {0} külalist – kohti ei saa olla vähem.", "Уже записано {0} гостей — мест не может быть меньше.", "{0} guests have already registered – capacity cannot be lower."),
        ("Laud kattub lauaga {0}. Valige teine koht.", "Стол пересекается со столом {0}. Выберите другое место.", "The table overlaps table {0}. Choose another spot."),
        ("Sel päeval on selles saalis juba {0} tavalist broneeringut. Valige teine päev või saal.", "В этот день в этом зале уже есть обычные брони ({0}). Выберите другой день или зал.", "There are already {0} regular bookings in this room that day. Choose another day or room."),
        ("Õhtut {0} ei leitud.", "Вечер {0} не найден.", "Evening {0} not found."),
        ("Ühel õhtul saab broneerida kuni {0} lauda.", "На один вечер можно забронировать не более {0} столов.", "You can book up to {0} tables per evening."),
        ("\"{0}\" ei ole selle restorani menüüs.", "«{0}» нет в меню этого ресторана.", "\"{0}\" is not on this restaurant's menu."),
        ("\"{0}\" ei ole praegu saadaval.", "«{0}» сейчас недоступно.", "\"{0}\" is not available right now."),
    };

    /// <summary>.NET-i vaikimisi (ingliskeelsed) valideerimisteated – tõlgitakse kõigisse kolme keelde.</summary>
    private static readonly (Regex Pattern, string Et, string Ru, string En)[] Validation =
    {
        (new Regex(@"^The (.+?) field is required\.$"), "Väli „{0}“ on kohustuslik.", "Поле «{0}» обязательно.", "The {0} field is required."),
        (new Regex(@"^The field (.+?) must be between (.+?) and (.+?)\.$"), "Väli „{0}“: väärtus peab olema {1} kuni {2}.", "Поле «{0}»: значение от {1} до {2}.", "{0} must be between {1} and {2}."),
        (new Regex(@"^The field (.+?) must be a string with a minimum length of (.+?) and a maximum length of (.+?)\.$"), "Väli „{0}“: {1} kuni {2} märki.", "Поле «{0}»: от {1} до {2} символов.", "{0} must be {1} to {2} characters."),
        (new Regex(@"^The field (.+?) must be a string with a maximum length of (.+?)\.$"), "Väli „{0}“: kuni {1} märki.", "Поле «{0}»: не более {1} символов.", "{0} can be up to {1} characters."),
        (new Regex(@"^The (.+?) field is not a valid e-mail address\.$"), "Väli „{0}“: vigane e-posti aadress.", "Поле «{0}»: неверный адрес e-mail.", "{0} is not a valid e-mail address."),
        (new Regex(@"^The field (.+?) must match the regular expression .*$"), "Väli „{0}“: vale vorming.", "Поле «{0}»: неверный формат.", "{0} has an invalid format."),
        (new Regex(@"^The JSON value could not be converted.*$|^The input does not contain any JSON tokens.*$|^'.*' is an invalid.*$"), "Andmed on vigased.", "Неверные данные.", "Invalid data."),
    };

    /// <summary>Valideerimisteade (ka .NET-i ingliskeelne) saidi keelde.</summary>
    public static string TranslateValidation(string message, string lang)
    {
        foreach (var (pattern, et, ru, en) in Validation)
        {
            var m = pattern.Match(message);
            if (!m.Success) continue;
            var result = lang == "ru" ? ru : lang == "en" ? en : et;
            for (int i = 1; i < m.Groups.Count; i++)
                result = result.Replace("{" + (i - 1) + "}", i == 1 ? FieldName(m.Groups[i].Value, lang) : m.Groups[i].Value);
            return result;
        }
        return Translate(message, lang);
    }

    /// <summary>Välja nimi (C# omadus) loetavaks: et/ru/en.</summary>
    private static readonly Dictionary<string, (string Et, string Ru, string En)> Fields = new()
    {
        ["Guests"] = ("Külalisi", "Гостей", "Guests"),
        ["Name"] = ("Nimi", "Имя", "Name"),
        ["Email"] = ("E-post", "E-mail", "E-mail"),
        ["Phone"] = ("Telefon", "Телефон", "Phone"),
        ["Password"] = ("Parool", "Пароль", "Password"),
        ["Text"] = ("Tekst", "Текст", "Text"),
        ["Date"] = ("Kuupäev", "Дата", "Date"),
        ["Time"] = ("Kellaaeg", "Время", "Time"),
        ["CardHolder"] = ("Kaardi omanik", "Владелец карты", "Cardholder"),
        ["CardNumber"] = ("Kaardi number", "Номер карты", "Card number"),
        ["Expiry"] = ("Kehtivus", "Срок действия", "Expiry"),
        ["Cvc"] = ("CVC", "CVC", "CVC"),
        ["Price"] = ("Hind", "Цена", "Price"),
        ["Seats"] = ("Kohti", "Мест", "Seats"),
        ["Number"] = ("Number", "Номер", "Number"),
        ["Category"] = ("Kategooria", "Категория", "Category"),
        ["Description"] = ("Kirjeldus", "Описание", "Description"),
        ["SpecialRequest"] = ("Soovid", "Пожелания", "Requests"),
        ["Quantity"] = ("Kogus", "Количество", "Quantity"),
        ["RestaurantId"] = ("Restoran", "Ресторан", "Restaurant"),
    };

    private static string FieldName(string field, string lang)
    {
        var key = field.Contains('.') ? field[(field.LastIndexOf('.') + 1)..] : field;
        if (!Fields.TryGetValue(key, out var f)) return field;
        return lang == "ru" ? f.Ru : lang == "en" ? f.En : f.Et;
    }

    private static readonly (Regex Pattern, string Ru, string En)[] Rules = Texts
        .Select(t => (new Regex("^" + Regex.Escape(t.Et).Replace(@"\{0}", "(.+?)").Replace(@"\{1}", "(.+?)").Replace(@"\{2}", "(.+?)") + "$"), t.Ru, t.En))
        .ToArray();

    private static readonly Dictionary<string, (string Ru, string En)> Titles = new()
    {
        ["Ei leitud"] = ("Не найдено", "Not found"),
        ["Konflikt"] = ("Конфликт", "Conflict"),
        ["Vigane päring"] = ("Неверный запрос", "Bad request"),
        ["Autentimine ebaõnnestus"] = ("Ошибка входа", "Authentication failed"),
        ["Keelatud"] = ("Доступ запрещён", "Forbidden"),
        ["Serveri viga"] = ("Ошибка сервера", "Server error"),
    };

    /// <summary>Keel päisest Accept-Language: "ru", "en" või "et" (vaikimisi).</summary>
    public static string LanguageOf(HttpContext context)
    {
        var header = context.Request.Headers.AcceptLanguage.ToString().ToLowerInvariant();
        return header.StartsWith("ru") ? "ru" : header.StartsWith("en") ? "en" : "et";
    }

    /// <summary>Tõlgib eestikeelse teate; tundmatu teade jääb muutmata.</summary>
    public static string Translate(string message, string lang)
    {
        if (lang == "et" || string.IsNullOrEmpty(message)) return message;
        foreach (var (pattern, ru, en) in Rules)
        {
            var m = pattern.Match(message);
            if (!m.Success) continue;
            var result = lang == "ru" ? ru : en;
            for (int i = 1; i < m.Groups.Count; i++)
                result = result.Replace("{" + (i - 1) + "}", m.Groups[i].Value);
            return result;
        }
        return message;
    }

    public static string TranslateTitle(string title, string lang) =>
        lang == "et" || !Titles.TryGetValue(title, out var t) ? title : lang == "ru" ? t.Ru : t.En;
}
