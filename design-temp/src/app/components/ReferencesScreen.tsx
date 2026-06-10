import { useState } from "react";
import { Btn, Field, Combo, Box, Grid, Badge, Modal, Alert, SectionHeader } from "./ui";

// Справочник контрагентов = Покупатель + Поставщик
const INIT_COUNTERPARTIES = [
  { id: 1, name: 'ООО "АгроМаркет"',         inn: "7701234567",   phone: "+7 495 123-45-67", email: "info@agromarket.ru",    address: "г. Москва, ул. Ленина, 10", type: "Покупатель" },
  { id: 2, name: 'ИП Фёдоров А.В.',           inn: "771234567890", phone: "+7 926 234-56-78", email: "fedorov@mail.ru",       address: "г. Москва, ул. Садовая, 5", type: "Покупатель" },
  { id: 3, name: 'ООО "ЗерноТорг"',           inn: "7709876543",   phone: "+7 499 345-67-89", email: "info@zernotorg.ru",     address: "г. Москва, пр. Мира, 22",   type: "Покупатель" },
  { id: 4, name: 'КФХ Смирнов',               inn: "501234567890", phone: "+7 916 456-78-90", email: "kfh.smirnov@mail.ru",   address: "Московская обл., Серпухов", type: "Покупатель" },
  { id: 5, name: 'ООО "АгроСнаб"',            inn: "5001234567",   phone: "+7 495 500-10-20", email: "agro@snab.ru",          address: "г. Москва, ул. Тверская, 1", type: "Поставщик" },
  { id: 6, name: 'ЗАО "Зерновые ресурсы"',   inn: "7812345678",   phone: "+7 812 600-20-30", email: "grain@res.ru",          address: "г. Санкт-Петербург, пр. Невский, 50", type: "Поставщик" },
  { id: 7, name: 'ИП Гришин В.П.',            inn: "924123456789", phone: "+7 926 700-30-40", email: "grishin@mail.ru",       address: "Краснодарский край",        type: "Поставщик" },
];

// Справочник номенклатуры = Товар
const INIT_GOODS = [
  { id: 1, name: "Пшеница 3 кл.",   sort: "Экстра",    season: "Лето-2025",  unit: "т",  basePrice: 5000 },
  { id: 2, name: "Ячмень фуражный", sort: "1-й класс", season: "Осень-2025", unit: "т",  basePrice: 4000 },
  { id: 3, name: "Кукуруза",        sort: "Гибрид",    season: "Осень-2025", unit: "т",  basePrice: 5500 },
  { id: 4, name: "Подсолнечник",    sort: "Масличный", season: "Лето-2025",  unit: "т",  basePrice: 8000 },
  { id: 5, name: "Рожь",            sort: "Озимая",    season: "Лето-2025",  unit: "т",  basePrice: 3500 },
];

type Tab = "counterparties" | "goods";

export function ReferencesScreen() {
  const [tab, setTab] = useState<Tab>("counterparties");
  const [counterparties, setCounterparties] = useState(INIT_COUNTERPARTIES);
  const [goods, setGoods] = useState(INIT_GOODS);

  const [selCP, setSelCP] = useState<number | undefined>();
  const [selGood, setSelGood] = useState<number | undefined>();
  const [filterType, setFilterType] = useState("all");
  const [search, setSearch] = useState("");
  const [modal, setModal] = useState<"none" | "addCP" | "addGood">("none");

  // Add counterparty form
  const [cpName, setCpName] = useState("");
  const [cpInn, setCpInn] = useState("");
  const [cpPhone, setCpPhone] = useState("");
  const [cpEmail, setCpEmail] = useState("");
  const [cpAddress, setCpAddress] = useState("");
  const [cpType, setCpType] = useState("Покупатель");

  // Add good form
  const [gName, setGName] = useState("");
  const [gSort, setGSort] = useState("");
  const [gSeason, setGSeason] = useState("");
  const [gUnit, setGUnit] = useState("т");
  const [gPrice, setGPrice] = useState("");

  const filteredCP = counterparties.filter(c => {
    const typeOk = filterType === "all" || c.type === filterType;
    const textOk = !search || c.name.toLowerCase().includes(search.toLowerCase()) || c.inn.includes(search);
    return typeOk && textOk;
  });

  function handleAddCP() {
    setCounterparties(prev => [...prev, {
      id: prev.length + 1, name: cpName, inn: cpInn, phone: cpPhone,
      email: cpEmail, address: cpAddress, type: cpType,
    }]);
    setModal("none");
    setCpName(""); setCpInn(""); setCpPhone(""); setCpEmail(""); setCpAddress("");
  }

  function handleAddGood() {
    setGoods(prev => [...prev, {
      id: prev.length + 1, name: gName, sort: gSort, season: gSeason,
      unit: gUnit, basePrice: Number(gPrice) || 0,
    }]);
    setModal("none");
    setGName(""); setGSort(""); setGSeason(""); setGPrice("");
  }

  const selCPData = selCP !== undefined ? filteredCP[selCP] : null;
  const selGoodData = selGood !== undefined ? goods[selGood] : null;

  return (
    <div className="flex flex-col gap-3 h-full text-[13px]">
      {/* ── Tabs ── */}
      <div className="flex border-b border-[#C0C7D0]">
        {([["counterparties","👥 Контрагенты"],["goods","🌾 Номенклатура"]] as [Tab,string][]).map(([t,l]) => (
          <button key={t} onClick={() => setTab(t)}
            className={`px-5 py-[5px] text-[13px] border-t border-l border-r border-[#C0C7D0] -mb-px ${tab===t ? "bg-white text-[#1E3558] font-semibold" : "bg-[#EEF2F7] text-[#6B7280] hover:bg-[#E5EBF5]"}`}>
            {l}
          </button>
        ))}
      </div>

      {/* ══ TAB: Контрагенты ══ */}
      {tab === "counterparties" && (
        <>
          <div className="flex items-center gap-2 flex-wrap">
            <Btn variant="primary" onClick={() => setModal("addCP")} icon="＋">Добавить</Btn>
            <Btn disabled={selCP === undefined} icon="✏">Изменить</Btn>
            <Btn variant="danger" disabled={selCP === undefined} icon="✕">Удалить</Btn>
            <div className="ml-auto flex gap-2">
              <Field placeholder="Поиск по названию, ИНН..." value={search} onChange={setSearch} width="w-[200px]" />
              <Combo value={filterType} onChange={setFilterType} width="w-[140px]" options={[
                { value: "all", label: "Все типы" },
                { value: "Покупатель", label: "Покупатели" },
                { value: "Поставщик", label: "Поставщики" },
              ]} />
            </div>
          </div>

          <Grid
            cols={[
              { key: "name",    header: "Наименование",    width: "220px" },
              { key: "type",    header: "Тип",             width: "90px"  },
              { key: "inn",     header: "ИНН",             width: "120px" },
              { key: "phone",   header: "Телефон",         width: "140px" },
              { key: "email",   header: "E-mail",          width: "160px" },
            ]}
            rows={filteredCP.map(c => ({ ...c, type: <Badge text={c.type} /> }))}
            selected={selCP}
            onSelect={setSelCP}
            height={selCPData ? "h-40" : "flex-1"}
          />

          {selCPData && (
            <Box title="Реквизиты контрагента (Покупатель / Поставщик)">
              <div className="grid grid-cols-2 gap-x-6 gap-y-2 pt-1">
                <Field label="Наименование:" value={selCPData.name}    readOnly />
                <Field label="Тип:"          value={selCPData.type}    readOnly />
                <Field label="ИНН:"          value={selCPData.inn}     readOnly />
                <Field label="Телефон:"      value={selCPData.phone}   readOnly />
                <Field label="E-mail:"       value={selCPData.email}   readOnly />
                <Field label="Адрес:"        value={selCPData.address} readOnly />
              </div>
            </Box>
          )}
        </>
      )}

      {/* ══ TAB: Номенклатура ══ */}
      {tab === "goods" && (
        <>
          <div className="flex items-center gap-2">
            <Btn variant="primary" onClick={() => setModal("addGood")} icon="＋">Добавить</Btn>
            <Btn disabled={selGood === undefined} icon="✏">Изменить</Btn>
            <Btn variant="danger" disabled={selGood === undefined} icon="✕">Удалить</Btn>
          </div>

          <Grid
            cols={[
              { key: "name",      header: "Наименование",   width: "180px" },
              { key: "sort",      header: "Сорт",           width: "110px" },
              { key: "season",    header: "Сезонность",     width: "110px" },
              { key: "unit",      header: "Ед. изм.",       width: "70px"  },
              { key: "basePrice", header: "Баз. цена (₽/т)", width: "120px" },
            ]}
            rows={goods.map(g => ({ ...g, basePrice: g.basePrice.toLocaleString("ru-RU") }))}
            selected={selGood}
            onSelect={setSelGood}
            height={selGoodData ? "h-40" : "flex-1"}
          />

          {selGoodData && (
            <Box title="Карточка товара (Товар)">
              <div className="grid grid-cols-2 gap-x-6 gap-y-2 pt-1">
                <Field label="Наименование:" value={selGoodData.name}  readOnly />
                <Field label="Сорт:"         value={selGoodData.sort}  readOnly />
                <Field label="Сезонность:"   value={selGoodData.season} readOnly />
                <Field label="Ед. изм.:"     value={selGoodData.unit}  readOnly />
                <Field label="Баз. цена:"    value={`${selGoodData.basePrice.toLocaleString("ru-RU")} ₽/т`} readOnly />
              </div>
            </Box>
          )}
        </>
      )}

      {/* ═══ MODAL: Add counterparty ═══ */}
      {modal === "addCP" && (
        <Modal title="Добавить контрагента" onClose={() => setModal("none")} width={440}>
          <div className="p-4 flex flex-col gap-3">
            <Box title="Покупатель / Поставщик">
              <div className="flex flex-col gap-2 pt-1">
                <Combo label="Тип:" value={cpType} onChange={setCpType} options={[
                  { value: "Покупатель", label: "Покупатель" },
                  { value: "Поставщик", label: "Поставщик"  },
                  { value: "Оба",       label: "Оба"        },
                ]} />
                <Field label="Наименование:" value={cpName}    onChange={setCpName}    placeholder="ООО «Название»" />
                <Field label="ИНН:"          value={cpInn}     onChange={setCpInn}     placeholder="1234567890" />
                <Field label="Телефон:"      value={cpPhone}   onChange={setCpPhone}   placeholder="+7 495 ..." />
                <Field label="E-mail:"       value={cpEmail}   onChange={setCpEmail}   placeholder="info@company.ru" />
                <Field label="Адрес:"        value={cpAddress} onChange={setCpAddress} placeholder="г. Москва, ул. ..." />
              </div>
            </Box>
            <div className="flex justify-end gap-2">
              <Btn variant="primary" onClick={handleAddCP} icon="💾">Сохранить</Btn>
              <Btn onClick={() => setModal("none")}>Отмена</Btn>
            </div>
          </div>
        </Modal>
      )}

      {/* ═══ MODAL: Add good ═══ */}
      {modal === "addGood" && (
        <Modal title="Добавить позицию номенклатуры" onClose={() => setModal("none")} width={420}>
          <div className="p-4 flex flex-col gap-3">
            <Box title="Товар (с/х культура)">
              <div className="flex flex-col gap-2 pt-1">
                <Field label="Наименование:" value={gName}   onChange={setGName}   placeholder="Пшеница 3 кл." />
                <Field label="Сорт:"         value={gSort}   onChange={setGSort}   placeholder="Экстра" />
                <Field label="Сезонность:"   value={gSeason} onChange={setGSeason} placeholder="Лето-2026" />
                <Combo label="Ед. изм.:" value={gUnit} onChange={setGUnit} options={[
                  { value: "т", label: "т (тонна)" },
                  { value: "кг", label: "кг" },
                  { value: "ц", label: "ц (центнер)" },
                ]} />
                <Field label="Баз. цена (₽/т):" value={gPrice} onChange={setGPrice} placeholder="5000" />
              </div>
            </Box>
            <div className="flex justify-end gap-2">
              <Btn variant="primary" onClick={handleAddGood} icon="💾">Сохранить</Btn>
              <Btn onClick={() => setModal("none")}>Отмена</Btn>
            </div>
          </div>
        </Modal>
      )}
    </div>
  );
}
