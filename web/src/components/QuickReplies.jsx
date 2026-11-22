import { useTr } from '../staff'

/** Personali kiirvastused: klõps lisab valmis teksti vastuse väljale (saab enne saatmist muuta). */
const REPLIES = [
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
