using System.Globalization;

namespace Restoran.Application.Services;

/// <summary>
/// Serveri tekstid kolmes keeles (et/ru/en): teated (kelluke), SMS-id, e-kirjad,
/// samuti näidismenüü roogade nimed, kategooriad ja restoranide kirjeldused.
/// Kliendi keel salvestatakse (User.Language), et sõnum jõuaks talle tema keeles.
/// </summary>
public static class L
{
    public static string Norm(string? lang) => lang is "ru" or "en" ? lang : "et";

    public static CultureInfo Culture(string? lang) => Norm(lang) switch
    {
        "ru" => new CultureInfo("ru-RU"),
        "en" => new CultureInfo("en-GB"),
        _ => new CultureInfo("et-EE"),
    };

    /// <summary>Kuupäev ja kellaaeg loetaval kujul kliendi keeles.</summary>
    public static string When(DateTime t, string? lang) => Norm(lang) switch
    {
        "ru" => t.ToString("dddd, d MMMM, HH:mm", Culture("ru")),
        "en" => t.ToString("dddd d MMMM, HH:mm", Culture("en")),
        _ => t.ToString("dddd, d. MMMM 'kell' HH:mm", Culture("et")),
    };

    private static readonly Dictionary<string, (string Et, string Ru, string En)> Texts = new()
    {
        // Broneering
        ["booking.confirmed"] = ("Argent: broneering kinnitatud. {0}, {1}. {2}, laud {3}, {4} in., nimele {5}. Aitäh!",
                                 "Argent: бронь подтверждена. {0}, {1}. {2}, стол {3}, гостей: {4}, на имя {5}. Спасибо!",
                                 "Argent: booking confirmed. {0}, {1}. {2}, table {3}, {4} guests, for {5}. Thank you!"),
        ["booking.subject"] = ("Argent – broneering {0}", "Argent – бронь {0}", "Argent – booking {0}"),
        ["booking.payRequest"] = ("Argent: Teile on broneeritud laud – {0}, {1}, {2} in. Broneering kinnitub pärast makset: {3}",
                                  "Argent: для Вас забронирован стол — {0}, {1}, гостей: {2}. Бронь подтвердится после оплаты: {3}",
                                  "Argent: a table has been booked for you – {0}, {1}, {2} guests. The booking is confirmed after payment: {3}"),
        ["booking.paySubject"] = ("Argent – kinnitage oma broneering", "Argent – подтвердите бронь", "Argent – confirm your booking"),
        ["booking.reminder"] = ("Meeldetuletus: homme kell {0} ootab Teid {1}, laud {2} ({3} in.). Muuta enam ei saa – küsimuste korral helistage restorani.",
                                "Напоминание: завтра в {0} Вас ждёт {1}, стол {2} (гостей: {3}). Изменить бронь уже нельзя — если есть вопросы, позвоните в ресторан.",
                                "Reminder: tomorrow at {0} {1} is expecting you, table {2} ({3} guests). It can no longer be changed – call the restaurant with any questions."),
        ["booking.unpaidCancelled"] = ("Argent: broneering {0} tühistati, sest seda ei makstud õigel ajal.",
                                       "Argent: бронь на {0} отменена, так как она не была оплачена вовремя.",
                                       "Argent: the booking for {0} was cancelled because it was not paid in time."),
        ["dish.unavailable"] = ("Vabandame: „{0}“ ei ole {1} saadaval. Pakume samaväärse asenduse ja peakoka kingituse – kirjutage meile broneeringu sõnumites, kui soovite midagi muud.",
                                "Извините: «{0}» {1} недоступно. Предложим равноценную замену и подарок от шеф-повара — напишите нам в сообщениях брони, если хотите что-то другое.",
                                "Sorry: “{0}” is not available on {1}. We will offer an equivalent replacement and a gift from the chef – write to us in the booking messages if you would like something else."),
        // Personal muutis kliendi broneeringut (klient näeb teadet kellukeses)
        ["avail.free"] = ("Vaba: {0} lauda {1} külalisele. Pakume lauda nr {2}.",
                          "Свободно столов на {1} гост.: {0}. Предлагаем стол № {2}.",
                          "Free: {0} tables for {1} guests. We suggest table no. {2}."),
        ["avail.none"] = ("Sel ajal pole {0} külalisele vaba lauda. Valige teine aeg või päev.",
                          "На это время нет свободного стола на {0} гост. Выберите другое время или день.",
                          "No free table for {0} guests at this time. Please choose another time or day."),
        ["staffChange.time"] = ("Argent muutis Teie broneeringut: nüüd {0}, {1} külalist. Vaadake üle „Minu broneeringutes“.",
                                "Argent изменил Вашу бронь: теперь {0}, гостей: {1}. Посмотрите в «Мои брони».",
                                "Argent changed your booking: now {0}, {1} guests. See “My bookings”."),
        ["staffChange.preorder"] = ("Argent muutis Teie eeltellimust ({0}). Vaadake üle „Minu broneeringutes“.",
                                    "Argent изменил Ваш предзаказ ({0}). Посмотрите в «Мои брони».",
                                    "Argent changed your pre-order ({0}). See “My bookings”."),
        ["staffChange.extras"] = ("Argent muutis Teie lisateenuseid ({0}). Vaadake üle „Minu broneeringutes“.",
                                  "Argent изменил дополнительные услуги ({0}). Посмотрите в «Мои брони».",
                                  "Argent changed your extras ({0}). See “My bookings”."),
        ["staffChange.details"] = ("Argent uuendas Teie soove ja allergiaid ({0}). Vaadake üle „Minu broneeringutes“.",
                                   "Argent обновил Ваши пожелания и аллергии ({0}). Посмотрите в «Мои брони».",
                                   "Argent updated your wishes and allergies ({0}). See “My bookings”."),
        // Sõnumid
        ["msg.toGuest"] = ("Argent kirjutas Teile broneeringu kohta: {0}", "Argent написал Вам по брони: {0}", "Argent wrote to you about your booking: {0}"),
        ["msg.toStaff"] = ("Uus sõnum: {0} (broneering {1}): {2}", "Новое сообщение: {0} (бронь {1}): {2}", "New message: {0} (booking {1}): {2}"),
        ["support.reply"] = ("Argent vastas Teie küsimusele: {0}", "Argent ответил на Ваш вопрос: {0}", "Argent answered your question: {0}"),
        ["staff.paid"] = ("Makstud ja kinnitatud: {0}, {1}", "Оплачено и подтверждено: {0}, {1}", "Paid and confirmed: {0}, {1}"),
        // Gastronoomiline õhtu
        ["event.confirmed"] = ("Argent: olete registreeritud – {0}, {1}, {2}{3}, {4} in. Makstud {5} €. Aitäh!",
                               "Argent: Вы записаны — {0}, {1}, {2}{3}, гостей: {4}. Оплачено {5} €. Спасибо!",
                               "Argent: you are registered – {0}, {1}, {2}{3}, {4} guests. Paid {5} €. Thank you!"),
        ["event.payRequest"] = ("Argent: Teid on registreeritud – {0}, {1}, {2} in. Koht kinnitub pärast makset ({3} €): {4}",
                                "Argent: Вас записали — {0}, {1}, гостей: {2}. Место подтвердится после оплаты ({3} €): {4}",
                                "Argent: you have been registered – {0}, {1}, {2} guests. Your seat is confirmed after payment ({3} €): {4}"),
        ["event.paySubject"] = ("Argent – kinnitage oma koht õhtul", "Argent – подтвердите место на вечере", "Argent – confirm your seat at the evening"),
        ["event.tableSuffix"] = (", laud {0}", ", стол {0}", ", table {0}"),
        // Konto
        ["account.created"] = ("Argent: Teie konto on loodud.", "Argent: Ваш аккаунт создан.", "Argent: your account has been created."),
        ["account.passwordText"] = (" Looge parool, et näha oma broneeringuid: {0}", " Создайте пароль, чтобы видеть свои брони: {0}", " Create a password to see your bookings: {0}"),
        ["account.passwordSubject"] = ("Argent – looge oma parool", "Argent – создайте пароль", "Argent – create your password"),
        ["account.passwordHtml"] = ("Teie konto lõi restorani administraator. Looge parool, et näha ja muuta oma broneeringuid:",
                                    "Ваш аккаунт создал администратор ресторана. Создайте пароль, чтобы видеть и менять свои брони:",
                                    "The restaurant host created your account. Create a password to see and change your bookings:"),
        ["account.passwordButton"] = ("LOO PAROOL", "СОЗДАТЬ ПАРОЛЬ", "CREATE PASSWORD"),
        ["hello"] = ("Tere, {0}!", "Здравствуйте, {0}!", "Hello {0}!"),
        ["broadcast.subject"] = ("Argent – teade Teie broneeringu kohta ({0})", "Argent – сообщение о Вашей брони ({0})", "Argent – a message about your booking ({0})"),
        ["broadcast.sms"] = ("Argent ({0}, {1}): {2}", "Argent ({0}, {1}): {2}", "Argent ({0}, {1}): {2}"),
        // E-kirja tšekk
        ["mail.tagline"] = ("Õhtusöök kui rituaal", "Ужин как ритуал", "Dinner as a ritual"),
        ["mail.waiting"] = ("Ootame Teid, {0}!", "Ждём Вас, {0}!", "We look forward to seeing you, {0}!"),
        ["mail.bookedFor"] = ("Teile on broneeritud laud, {0}!", "Для Вас забронирован стол, {0}!", "A table has been booked for you, {0}!"),
        ["mail.where"] = ("Kus", "Где", "Where"),
        ["mail.when"] = ("Millal", "Когда", "When"),
        ["mail.table"] = ("Laud", "Стол", "Table"),
        ["mail.guests"] = ("Külalisi", "Гостей", "Guests"),
        ["mail.for"] = ("Nimele", "На имя", "Name"),
        ["mail.extra"] = ("lisa", "доп.", "extra"),
        ["mail.seat"] = ("laua valik", "выбор стола", "table choice"),
        ["mail.total"] = ("KOKKU", "ИТОГО", "TOTAL"),
        ["mail.footer"] = ("Laud on Teie päralt kogu õhtu. Broneeringut saab muuta kuni 24 tundi enne külastust.",
                           "Стол за Вами на весь вечер. Изменить бронь можно не позднее чем за 24 часа до визита.",
                           "The table is yours for the whole evening. The booking can be changed up to 24 hours before the visit."),
        ["mail.questions"] = ("Küsimused", "Вопросы", "Questions"),
        ["mail.payButton"] = ("KINNITA JA MAKSA", "ПОДТВЕРДИТЬ И ОПЛАТИТЬ", "CONFIRM AND PAY"),
        ["mail.payNote"] = ("Broneering ei ole kinnitatud enne makset. Maksmata broneering tühistatakse 24 tunni pärast.",
                            "Бронь не подтверждена до оплаты. Неоплаченная бронь отменяется через 24 часа.",
                            "The booking is not confirmed until paid. Unpaid bookings are cancelled after 24 hours."),
        ["mail.eventFee"] = ("Osalustasu × {0}", "Участие × {0}", "Participation × {0}"),
        ["mail.eventSee"] = ("Ootame Teid!", "Ждём Вас!", "We look forward to seeing you!"),
        ["mail.eventPayNote"] = ("Koht ei ole kinnitatud enne makset. Maksmata registreerimine tühistatakse 24 tundi enne õhtut.",
                                 "Место не подтверждено до оплаты. Неоплаченная запись отменяется за 24 часа до вечера.",
                                 "The seat is not confirmed until paid. Unpaid registrations are cancelled 24 hours before the evening."),
    };

    public static string T(string? lang, string key, params object?[] args)
    {
        var t = Texts[key];
        var s = Norm(lang) switch { "ru" => t.Ru, "en" => t.En, _ => t.Et };
        return args.Length == 0 ? s : string.Format(Culture(lang), s, args);
    }

    // ---------- Näidismenüü ja restoranid ----------

    private static readonly Dictionary<string, (string Ru, string En)> Words = new(StringComparer.OrdinalIgnoreCase)
    {
        // Kategooriad
        ["Külmad road"] = ("Холодные закуски", "Cold dishes"),
        ["Pearoad"] = ("Основные блюда", "Main courses"),
        ["Mereannid"] = ("Морепродукты", "Seafood"),
        ["Magustoidud"] = ("Десерты", "Desserts"),
        ["Setid"] = ("Сеты", "Sets"),
        ["Veinid"] = ("Вина", "Wines"),
        // Road
        ["Austrid Fine de Claire"] = ("Устрицы Fine de Claire", "Fine de Claire oysters"),
        ["šalott, veiniäädikas, rukkileib · 6 tk"] = ("шалот, винный уксус, ржаной хлеб · 6 шт.", "shallot, wine vinegar, rye bread · 6 pcs"),
        ["Marmorveise tartar"] = ("Тартар из мраморной говядины", "Marbled beef tartare"),
        ["söel küpsetatud munakollane, kapparid, trühvel"] = ("желток на углях, каперсы, трюфель", "ember-baked egg yolk, capers, truffle"),
        ["Hiidlest, beurre blanc, kalamari"] = ("Палтус, beurre blanc, икра", "Halibut, beurre blanc, caviar"),
        ["spargel, sidruniõli"] = ("спаржа, лимонное масло", "asparagus, lemon oil"),
        ["Part, kirss, suitsupeet"] = ("Утка, вишня, копчёная свёкла", "Duck, cherry, smoked beetroot"),
        ["laagerdatud rind, portveinikaste"] = ("выдержанная грудка, соус из портвейна", "aged breast, port wine sauce"),
        ["Šokolaad ja meresool"] = ("Шоколад и морская соль", "Chocolate and sea salt"),
        ["tume šokolaad 70%, oliiviõli"] = ("тёмный шоколад 70%, оливковое масло", "70% dark chocolate, olive oil"),
        ["Söel küpsetatud carabinero"] = ("Карабинеро на углях", "Ember-grilled carabinero"),
        ["vetikavõi, laim"] = ("масло с водорослями, лайм", "seaweed butter, lime"),
        ["Kammkarbid ja lillkapsas"] = ("Гребешки и цветная капуста", "Scallops and cauliflower"),
        ["sarapuupähkel, pruunistatud või"] = ("фундук, топлёное масло", "hazelnut, brown butter"),
        ["Kamtšatka krabi"] = ("Камчатский краб", "Kamchatka crab"),
        ["roheline õun, nuikapsas, till"] = ("зелёное яблоко, кольраби, укроп", "green apple, kohlrabi, dill"),
        ["Tuurakalamari, 30 g"] = ("Осетровая икра, 30 г", "Sturgeon caviar, 30 g"),
        ["tatrablinid, hapukoor"] = ("гречневые блины, сметана", "buckwheat blini, sour cream"),
        ["Set „Argent“ – 7 käiku"] = ("Сет «Argent» — 7 подач", "“Argent” set – 7 courses"),
        ["maja peamine õhtusöök, muutub hooajaga"] = ("главный ужин дома, меняется по сезону", "the house signature dinner, changes with the season"),
        ["Set „Meri“ – 5 käiku"] = ("Сет «Море» — 5 подач", "“Sea” set – 5 courses"),
        ["austrid, krabi, kammkarbid, hiidlest, magustoit"] = ("устрицы, краб, гребешки, палтус, десерт", "oysters, crab, scallops, halibut, dessert"),
        ["Set „Teras“ – 6 käiku"] = ("Сет «Сталь» — 6 подач", "“Steel” set – 6 courses"),
        ["liha ja ulukid lahtisel tulel"] = ("мясо и дичь на открытом огне", "meat and game over an open fire"),
        ["Veinisaade"] = ("Винное сопровождение", "Wine pairing"),
        ["sommeljee klaas igale käigule"] = ("бокал от сомелье к каждой подаче", "a sommelier's glass for every course"),
        ["Veise sisefilee ja mürklid"] = ("Говяжья вырезка и сморчки", "Beef tenderloin and morels"),
        ["Périgueux' kaste, kartulimuss"] = ("соус Перигё, картофельный мусс", "Périgueux sauce, potato mousse"),
        ["Pirnisuflee"] = ("Грушевое суфле", "Pear soufflé"),
        ["Supid"] = ("Супы", "Soups"),
        ["Peedicarpaccio, kitsejuust"] = ("Карпаччо из свёклы, козий сыр", "Beetroot carpaccio, goat cheese"),
        ["röstitud kreeka pähkel, mesi, tüümian"] = ("жареный грецкий орех, мёд, тимьян", "roasted walnut, honey, thyme"),
        ["Suitsulõhe, rukkikrõps"] = ("Копчёный лосось, ржаные чипсы", "Smoked salmon, rye crisps"),
        ["kapparikreem, punane sibul, till"] = ("крем с каперсами, красный лук, укроп", "caper cream, red onion, dill"),
        ["Burrata ja tomatid"] = ("Буррата и томаты", "Burrata and tomatoes"),
        ["basiilikuõli, balsamico"] = ("базиликовое масло, бальзамик", "basil oil, balsamic"),
        ["Seenesupp"] = ("Грибной суп", "Mushroom soup"),
        ["metsaseened, koor, tatrakrõps"] = ("лесные грибы, сливки, гречневые чипсы", "forest mushrooms, cream, buckwheat crisp"),
        ["Bouillabaisse"] = ("Буйабес", "Bouillabaisse"),
        ["kala ja mereannid, safran, rouille"] = ("рыба и морепродукты, шафран, руй", "fish and seafood, saffron, rouille"),
        ["Kõrvitsasupp"] = ("Тыквенный суп", "Pumpkin soup"),
        ["röstitud kõrvitsaseemned, kookos"] = ("жареные тыквенные семечки, кокос", "roasted pumpkin seeds, coconut"),
        ["Lambakarree, rosmariin"] = ("Каре ягнёнка, розмарин", "Rack of lamb, rosemary"),
        ["röstitud juurviljad, punase veini kaste"] = ("печёные корнеплоды, соус из красного вина", "roasted root vegetables, red wine sauce"),
        ["Tursk ja pruunistatud või"] = ("Треска и коричневое масло", "Cod and brown butter"),
        ["herned, kartulipüree, kaaviar"] = ("горошек, картофельное пюре, икра", "peas, mashed potato, caviar"),
        ["Seenerisotto"] = ("Ризотто с грибами", "Mushroom risotto"),
        ["puravikud, parmesan, trühliõli"] = ("белые грибы, пармезан, трюфельное масло", "porcini, parmesan, truffle oil"),
        ["Köögiviljad söel"] = ("Овощи на углях", "Ember-roasted vegetables"),
        ["hummus, granaatõun, ürdid"] = ("хумус, гранат, травы", "hummus, pomegranate, herbs"),
        ["Crème brûlée"] = ("Крем-брюле", "Crème brûlée"),
        ["Madagaskari vanill"] = ("мадагаскарская ваниль", "Madagascar vanilla"),
        ["Juustuvalik"] = ("Сырная тарелка", "Cheese selection"),
        ["kolm Prantsuse juustu, viigimarjamoos, kreeka pähkel"] = ("три французских сыра, инжирный джем, грецкий орех", "three French cheeses, fig jam, walnut"),
        ["Marjasorbett"] = ("Ягодный сорбет", "Berry sorbet"),
        ["metsamarjad, piparmünt"] = ("лесные ягоды, мята", "forest berries, mint"),
        ["Chablis, klaas"] = ("Шабли, бокал", "Chablis, glass"),
        ["Burgundia valge, Prantsusmaa"] = ("белое бургундское, Франция", "white Burgundy, France"),
        ["Pinot Noir, klaas"] = ("Пино нуар, бокал", "Pinot Noir, glass"),
        ["punane, Burgundia"] = ("красное, Бургундия", "red, Burgundy"),
        ["Sauternes, klaas"] = ("Сотерн, бокал", "Sauternes, glass"),
        ["magus magustoiduvein"] = ("сладкое десертное вино", "sweet dessert wine"),
        ["Alkoholivaba kaaslane"] = ("Безалкогольное сопровождение", "Non-alcoholic pairing"),
        ["kodused limonaadid ja teed igale käigule"] = ("домашние лимонады и чаи к каждой подаче", "house lemonades and teas for every course"),
        ["vaniljekreem, karamell"] = ("ванильный крем, карамель", "vanilla cream, caramel"),
        ["Šampanja, klaas"] = ("Шампанское, бокал", "Champagne, glass"),
        ["brut, Prantsusmaa"] = ("брют, Франция", "brut, France"),
        // Lisateenused
        ["Lillekimp lauale"] = ("Букет к столу", "Bouquet for the table"),
        ["Pudel šampanjat"] = ("Бутылка шампанского", "Bottle of champagne"),
        ["Tellimustort, 1 kg"] = ("Торт на заказ, 1 кг", "Custom cake, 1 kg"),
        ["Viiuldaja, 20 minutit"] = ("Скрипач, 20 минут", "Violinist, 20 minutes"),
        ["Fotograaf, 1 tund"] = ("Фотограф, 1 час", "Photographer, 1 hour"),
        ["Lastetool"] = ("Детский стул", "High chair"),
        // Restoranide kirjeldused
        ["Kõrgköögi restoran vanalinna südames. Degustatsioonimenüü ja lahtine tuli."] =
            ("Ресторан высокой кухни в сердце Старого города. Дегустационное меню и открытый огонь.", "Fine dining in the heart of the Old Town. Tasting menu and an open fire."),
        ["Mereäärne saal ja suur terrass. Mereannid ja šampanja."] =
            ("Зал у моря и большая терраса. Морепродукты и шампанское.", "A seaside room and a large terrace. Seafood and champagne."),
        ["Vaikne saal Kadrioru pargi kõrval. Hooajalised setid ja veinid."] =
            ("Тихий зал рядом с парком Кадриорг. Сезонные сеты и вина.", "A quiet room next to Kadriorg Park. Seasonal sets and wines."),
    };

    /// <summary>Näidisandmete tekst (roog, kirjeldus, kategooria, restorani kirjeldus) kliendi keeles.
    /// Admini lisatud tekst jääb muutmata.</summary>
    public static string Word(string? text, string? lang)
    {
        if (string.IsNullOrEmpty(text) || Norm(lang) == "et") return text ?? "";
        return Words.TryGetValue(text, out var w) ? (Norm(lang) == "ru" ? w.Ru : w.En) : text;
    }
}
