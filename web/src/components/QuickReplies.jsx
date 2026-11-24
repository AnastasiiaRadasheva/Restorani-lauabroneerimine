import { useTr } from '../staff'

/** Personali kiirvastused: klõps lisab valmis teksti vastuse väljale (saab enne saatmist muuta). */
const REPLIES = [
  ['Broneeringu andmed', 'Данные для брони', 'Booking details',
    [
      'Broneerime Teile laua hea meelega. Palun kirjutage:',
      '– ees- ja perekonnanimi;',
      '– e-post ja telefoninumber;',
      '– restoran, kuupäev ja kellaaeg;',
      '– külaliste arv (ja mitu neist on lapsed);',
      '– erisoovid: allergiad, tähtpäev, laud akna all vms.',
      '',
      'Vormistame broneeringu ja loome Teile konto. E-postile saadame makselingi ning lingi parooli loomiseks – pärast parooli loomist näete ja saate broneeringut ise veebilehel muuta.',
    ].join('\n'),
    [
      'С удовольствием забронируем для Вас стол. Пожалуйста, напишите:',
      '– имя и фамилию;',
      '– e-mail и номер телефона;',
      '– ресторан, дату и время;',
      '– число гостей (и сколько из них детей);',
      '– особые пожелания: аллергии, повод, стол у окна и т. п.',
      '',
      'Мы оформим бронь и создадим для Вас аккаунт. На e-mail придут ссылка на оплату и ссылка для создания пароля – после этого Вы сможете сами просматривать и изменять бронь на сайте.',
    ].join('\n'),
    [
      'We will be happy to book a table for you. Please send us:',
      '– your first and last name;',
      '– e-mail and phone number;',
      '– restaurant, date and time;',
      '– number of guests (and how many of them are children);',
      '– special requests: allergies, occasion, window table etc.',
      '',
      'We will make the booking and create an account for you. You will receive a payment link and a link to set your password by e-mail – once the password is set, you can view and change the booking on the website yourself.',
    ].join('\n')],
  ['Makselink', 'Ссылка на оплату', 'Payment link',
    'Saatsime Teie e-postile makselingi. Palun tasuge broneering – kui kõik on korras, saabub kinnitus koos tšekiga.',
    'Мы отправили Вам на почту ссылку на оплату. Пожалуйста, оплатите бронь — если всё хорошо, придёт подтверждение с чеком.',
    'We have e-mailed you a payment link. Please pay for the booking – once it is done you will receive a confirmation with the receipt.'],
  ['Kinnitatud', 'Подтверждено', 'Confirmed',
    'Teie broneering on kinnitatud. Ootame Teid!',
    'Ваша бронь подтверждена. Ждём Вас!',
    'Your booking is confirmed. We look forward to seeing you!'],
  ['Muudetud', 'Изменено', 'Changed',
    'Muutsime Teie broneeringut Teie soovi järgi. Uued andmed on broneeringus ja teates.',
    'Мы изменили Вашу бронь по Вашей просьбе. Новые данные — в брони и в уведомлении.',
    'We have changed your booking as requested. The new details are in the booking and the notification.'],
  ['Allergia', 'Аллергия', 'Allergy',
    'Aitäh! Edastasime Teie soovi kokale – roog valmistatakse vastavalt.',
    'Спасибо! Мы передали Ваше пожелание шеф-повару — блюдо приготовят с учётом этого.',
    'Thank you! We passed your request to the chef – the dish will be prepared accordingly.'],
  ['Laud', 'Стол', 'Table',
    'Laud on Teie päralt kogu õhtu. Kui hilinete, andke palun siin teada.',
    'Стол закреплён за Вами на весь вечер. Если опаздываете, пожалуйста, напишите здесь.',
    'The table is yours for the whole evening. If you are running late, please let us know here.'],
  ['Helistame', 'Перезвоним', 'Call back',
    'Helistame Teile lähima tunni jooksul, et kõik üksikasjad kokku leppida.',
    'Мы перезвоним Вам в течение часа, чтобы обсудить детали.',
    'We will call you within the hour to agree on the details.'],
]

export default function QuickReplies({ name, onPick }) {
  const tr = useTr()
  const hello = name ? tr(`Tere, ${name}! `, `Здравствуйте, ${name}! `, `Hello ${name}! `) : ''
  return (
    <div className="quick-replies">
      <span className="eyebrow">{tr('Kiirvastused', 'Быстрые ответы', 'Quick replies')}</span>
      <div className="chips">
        {REPLIES.map(([et, ru, en, tEt, tRu, tEn]) => (
          <button key={et} type="button" className="chip small" onClick={() => onPick(hello + tr(tEt, tRu, tEn))}>
            {tr(et, ru, en)}
          </button>
        ))}
      </div>
    </div>
  )
}
