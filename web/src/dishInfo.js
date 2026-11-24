/**
 * Roogade koostis kolmes keeles. Võti on roa foto failinimi (/img/menu/austrid.jpg -> austrid),
 * sest sama roog on kõigis kolmes restoranis ja nimi tõlgitakse serveris.
 * Allergeenid tulevad andmebaasist (MenuItem.Tags).
 */
const DISHES = {
  austrid: {
    et: ['austrid Fine de Claire (6 tk)', 'šalottsibul', 'punase veini äädikas', 'sidrun', 'rukkileib', 'või'],
    ru: ['устрицы Fine de Claire (6 шт.)', 'лук-шалот', 'красный винный уксус', 'лимон', 'ржаной хлеб', 'сливочное масло'],
    en: ['Fine de Claire oysters (6 pcs)', 'shallot', 'red wine vinegar', 'lemon', 'rye bread', 'butter'],
  },
  tartar: {
    et: ['marmorveise sisefilee', 'söel küpsetatud munakollane', 'kapparid', 'šalottsibul', 'must trühvel', 'Dijoni sinep', 'oliiviõli'],
    ru: ['мраморная говяжья вырезка', 'желток, запечённый на углях', 'каперсы', 'лук-шалот', 'чёрный трюфель', 'дижонская горчица', 'оливковое масло'],
    en: ['marbled beef tenderloin', 'charcoal-baked egg yolk', 'capers', 'shallot', 'black truffle', 'Dijon mustard', 'olive oil'],
  },
  hiidlest: {
    et: ['hiidlest', 'beurre blanc (või, valge vein, šalott)', 'kalamari', 'roheline spargel', 'sidruniõli'],
    ru: ['палтус', 'соус бёр блан (масло, белое вино, шалот)', 'икра', 'зелёная спаржа', 'лимонное масло'],
    en: ['halibut', 'beurre blanc (butter, white wine, shallot)', 'caviar', 'green asparagus', 'lemon oil'],
  },
  part: {
    et: ['laagerdatud pardirind', 'kirsid', 'suitsupeet', 'portveinikaste', 'tüümian'],
    ru: ['выдержанная утиная грудка', 'вишня', 'копчёная свёкла', 'соус из портвейна', 'тимьян'],
    en: ['dry-aged duck breast', 'cherries', 'smoked beetroot', 'port wine sauce', 'thyme'],
  },
  sokolaad: {
    et: ['tume šokolaad 70%', 'koor', 'oliiviõli', 'meresool', 'kakaonibsid'],
    ru: ['тёмный шоколад 70%', 'сливки', 'оливковое масло', 'морская соль', 'какао-крупка'],
    en: ['70% dark chocolate', 'cream', 'olive oil', 'sea salt', 'cocoa nibs'],
  },
  carabinero: {
    et: ['carabinero krevetid', 'vetikavõi', 'laim', 'küüslauk', 'tšilli'],
    ru: ['креветки карабинеро', 'масло с водорослями', 'лайм', 'чеснок', 'чили'],
    en: ['carabinero prawns', 'seaweed butter', 'lime', 'garlic', 'chilli'],
  },
  kammkarbid: {
    et: ['kammkarbid', 'lillkapsa püree', 'röstitud sarapuupähklid', 'pruunistatud või', 'kappar'],
    ru: ['гребешки', 'пюре из цветной капусты', 'жареный фундук', 'тёмное сливочное масло', 'каперсы'],
    en: ['scallops', 'cauliflower purée', 'roasted hazelnuts', 'brown butter', 'capers'],
  },
  krabi: {
    et: ['Kamtšatka krabi', 'roheline õun', 'nuikapsas', 'till', 'sidrunimahl'],
    ru: ['камчатский краб', 'зелёное яблоко', 'кольраби', 'укроп', 'лимонный сок'],
    en: ['king crab', 'green apple', 'kohlrabi', 'dill', 'lemon juice'],
  },
  kalamari: {
    et: ['tuurakalamari (30 g)', 'tatrablinid', 'hapukoor', 'punane sibul', 'murulauk'],
    ru: ['осетровая икра (30 г)', 'гречневые блины', 'сметана', 'красный лук', 'шнитт-лук'],
    en: ['sturgeon caviar (30 g)', 'buckwheat blini', 'sour cream', 'red onion', 'chives'],
  },
  'set-argent': {
    et: ['7 käiku peakoka valikul', 'austrid', 'hiidlest', 'marmorveis', 'juustud', 'magustoit', 'leib ja või'],
    ru: ['7 подач по выбору шефа', 'устрицы', 'палтус', 'мраморная говядина', 'сыры', 'десерт', 'хлеб и масло'],
    en: ["7 courses chosen by the chef", 'oysters', 'halibut', 'marbled beef', 'cheeses', 'dessert', 'bread and butter'],
  },
  'set-meri': {
    et: ['austrid', 'Kamtšatka krabi', 'kammkarbid', 'hiidlest', 'magustoit'],
    ru: ['устрицы', 'камчатский краб', 'гребешки', 'палтус', 'десерт'],
    en: ['oysters', 'king crab', 'scallops', 'halibut', 'dessert'],
  },
  'set-teras': {
    et: ['6 käiku lahtisel tulel', 'veis', 'part', 'hirveliha', 'juurviljad', 'juuretisleib'],
    ru: ['6 подач на открытом огне', 'говядина', 'утка', 'оленина', 'корнеплоды', 'хлеб на закваске'],
    en: ['6 courses over open fire', 'beef', 'duck', 'venison', 'root vegetables', 'sourdough bread'],
  },
  veinisaade: {
    et: ['sommeljee valitud klaas igale käigule', 'vahuvein', 'valge vein', 'punane vein', 'magustoiduvein'],
    ru: ['бокал к каждой подаче по выбору сомелье', 'игристое', 'белое вино', 'красное вино', 'десертное вино'],
    en: ["a glass for every course, chosen by the sommelier", 'sparkling wine', 'white wine', 'red wine', 'dessert wine'],
  },
  sisefilee: {
    et: ['veise sisefilee', 'mürklid', "Périgueux' kaste (trühvel, madeira)", 'kartulipüree', 'või'],
    ru: ['говяжья вырезка', 'сморчки', 'соус Перигё (трюфель, мадера)', 'картофельное пюре', 'сливочное масло'],
    en: ['beef tenderloin', 'morels', 'Périgueux sauce (truffle, Madeira)', 'potato purée', 'butter'],
  },
  suflee: {
    et: ['pirn', 'munad', 'nisujahu', 'suhkur', 'vaniljekreem', 'karamell'],
    ru: ['груша', 'яйца', 'пшеничная мука', 'сахар', 'ванильный крем', 'карамель'],
    en: ['pear', 'eggs', 'wheat flour', 'sugar', 'vanilla custard', 'caramel'],
  },
  sampanja: {
    et: ['šampanja brut, Prantsusmaa (125 ml)'],
    ru: ['шампанское брют, Франция (125 мл)'],
    en: ['Champagne brut, France (125 ml)'],
  },
}

/**
 * Gastronoomiliste õhtute joogid ("ainult sel õhtul"): allergeenid ja kirjeldus kolmes keeles.
 * Võti on joogi kood (EventCatalog). Vein ja šampanja sisaldavad sulfiteid.
 */
const DRINKS = {
  whisky: { tags: ['vegan'], et: 'suitsune Islay, Speyside ja Jaapani single malt', ru: 'дымный Islay, Speyside и японский односолодовый', en: 'smoky Islay, Speyside and Japanese single malt' },
  oldfashioned: { tags: ['vegan'], et: 'burbon, apelsin, tammesuits', ru: 'бурбон, апельсин, дубовый дым', en: 'bourbon, orange, oak smoke' },
  barolo: { tags: ['sulphites', 'vegan'], et: 'Piemonte, Nebbiolo', ru: 'Пьемонт, неббиоло', en: 'Piedmont, Nebbiolo' },
  chablis: { tags: ['sulphites'], et: 'Burgundia, mineraalne', ru: 'Бургундия, минеральное', en: 'Burgundy, mineral' },
  blanc: { tags: ['sulphites'], et: '100% Chardonnay', ru: '100% шардоне', en: '100% Chardonnay' },
  burgundy: { tags: ['sulphites'], et: 'iga käigu juurde oma vein', ru: 'к каждой подаче своё вино', en: 'a different wine with each course' },
  manhattan: { tags: ['vegan'], et: 'rukkiviski, amaro, kirss', ru: 'ржаной виски, амаро, вишня', en: 'rye whiskey, amaro, cherry' },
  espresso: { tags: ['vegan'], et: 'viin, espresso, kohvilikköör', ru: 'водка, эспрессо, кофейный ликёр', en: 'vodka, espresso, coffee liqueur' },
  rose: { tags: ['sulphites'], et: 'brut rosé, Prantsusmaa', ru: 'брют розе, Франция', en: 'brut rosé, France' },
  barbaresco: { tags: ['sulphites'], et: 'Piemonte, Nebbiolo', ru: 'Пьемонт, неббиоло', en: 'Piedmont, Nebbiolo' },
  trufflemartini: { tags: ['vegan'], et: 'džinn, kuiv vermut, trühvliõli', ru: 'джин, сухой вермут, трюфельное масло', en: 'gin, dry vermouth, truffle oil' },
  muscadet: { tags: ['sulphites'], et: 'Loire, mineraalne', ru: 'Луара, минеральное', en: 'Loire, mineral' },
  brutnature: { tags: ['sulphites'], et: 'kuiv ja värske', ru: 'сухое и свежее', en: 'dry and fresh' },
  natural: { tags: ['sulphites'], et: 'väikeveinimajast, sommeljee valik', ru: 'от небольшой винодельни, выбор сомелье', en: "from a small house, the sommelier's choice" },
  pinot: { tags: ['sulphites'], et: 'Burgundia', ru: 'Бургундия', en: 'Burgundy' },
  rhubarb: { tags: ['sulphites', 'vegan'], et: 'rabarber, vahuvein, piparmünt', ru: 'ревень, игристое вино, мята', en: 'rhubarb, sparkling wine, mint' },
  riesling: { tags: ['sulphites'], et: 'Mosel, kerge', ru: 'Мозель, лёгкое', en: 'Mosel, light' },
}

/** Õhtu joogi allergeenid (null, kui jook on tundmatu – siis palutakse küsida teenindajalt). */
export function drinkTags(code) {
  return DRINKS[code]?.tags ?? null
}

/** Õhtu joogi kirjeldus valitud keeles (või serverist tulnud tekst). */
export function drinkDescription(item, lang) {
  return DRINKS[item.code]?.[lang] ?? item.description
}

/** Roa koostis valitud keeles või null, kui seda pole (nt admini lisatud uus roog). */
export function ingredientsOf(item, lang) {
  const key = item?.imageUrl?.split('/').pop()?.replace(/\.[a-z]+$/i, '')
  const d = key && DISHES[key]
  return d ? d[lang] ?? d.et : null
}
