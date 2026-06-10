import { useState } from "react";
import { Btn, Field, Combo, Box, Grid, Badge, Modal, Alert, SectionHeader } from "./ui";

// Остаток_товара ← Товар, Склад
const WAREHOUSES = [{ id: 1, name: "Склад №1 (Центральный)" }, { id: 2, name: "Склад №2 (Северный)" }];
const GOODS = [
  { id: 1, name: "Пшеница 3 кл.",   sort: "Экстра",    unit: "т" },
  { id: 2, name: "Ячмень фуражный", sort: "1-й класс", unit: "т" },
  { id: 3, name: "Кукуруза",        sort: "Гибрид",    unit: "т" },
  { id: 4, name: "Подсолнечник",    sort: "Масличный", unit: "т" },
];

interface Stock { id: number; goodId: number; warehouseId: number; qty: number; checked: string; }
const INIT_STOCKS: Stock[] = [
  { id: 1, goodId: 1, warehouseId: 1, qty: 85,  checked: "09.06.2026 10:15" },
  { id: 2, goodId: 2, warehouseId: 1, qty: 30,  checked: "09.06.2026 10:15" },
  { id: 3, goodId: 3, warehouseId: 2, qty: 0,   checked: "07.06.2026 14:30" },
  { id: 4, goodId: 4, warehouseId: 2, qty: 44,  checked: "08.06.2026 09:00" },
];

// Заказы для отгрузки/приёмки
const ORDERS_FOR_ACTION = [
  { id: "ЗП-00002", date: "08.06.2026", party: 'ИП Фёдоров А.В.', status: "Подтверждён",       type: "Продажа", goodId: 2, qty: 20 },
  { id: "ЗП-00003", date: "07.06.2026", party: 'ООО "ЗерноТорг"', status: "Готов к отгрузке",  type: "Продажа", goodId: 1, qty: 60 },
  { id: "ЗЗ-00001", date: "05.06.2026", party: 'ООО "АгроСнаб"',  status: "Оформлен",           type: "Закупка", goodId: 1, qty: 100 },
];

type TabType = "stocks" | "shipment";
type ShipAction = "ship" | "receive";

export function WarehouseScreen() {
  const [tab, setTab] = useState<TabType>("stocks");
  const [stocks, setStocks] = useState<Stock[]>(INIT_STOCKS);
  const [selStock, setSelStock] = useState<number | undefined>();
  const [editModal, setEditModal] = useState(false);
  const [editQty, setEditQty] = useState("");
  const [editNote, setEditNote] = useState("");

  const [selOrder, setSelOrder] = useState<number | undefined>();
  const [shipAction, setShipAction] = useState<ShipAction | null>(null);
  const [actualQty, setActualQty] = useState("");
  const [shipNote, setShipNote] = useState("");
  const [savedMsg, setSavedMsg] = useState("");

  function goodName(id: number) { return GOODS.find(g => g.id === id)?.name ?? "?"; }
  function warehouseName(id: number) { return WAREHOUSES.find(w => w.id === id)?.name ?? "?"; }

  const stockRows = stocks.map(s => ({
    good:      goodName(s.goodId),
    sort:      GOODS.find(g => g.id === s.goodId)?.sort ?? "?",
    warehouse: warehouseName(s.warehouseId),
    qty:       `${s.qty} т`,
    checked:   s.checked,
    state:     s.qty === 0 ? <Badge text="Нет в наличии" /> : s.qty < 20 ? <Badge text="Мало" /> : <Badge text="В наличии" />,
  }));

  function handleSaveStock() {
    if (selStock === undefined) return;
    setStocks(prev => prev.map((s, i) => i === selStock ? { ...s, qty: Number(editQty) || s.qty, checked: new Date().toLocaleString("ru-RU") } : s));
    setEditModal(false);
  }

  function handleShip() {
    setSavedMsg(`Зафиксировано: ${shipAction === "ship" ? "Отгружено" : "Принято"} — ${actualQty} т по заказу ${ORDERS_FOR_ACTION[selOrder!]?.id}. Остатки обновлены.`);
    setShipAction(null);
    setActualQty("");
  }

  return (
    <div className="flex flex-col gap-3 h-full text-[13px]">
      {/* ── Tabs ── */}
      <div className="flex border-b border-[#C0C7D0]">
        {([["stocks","🏭 Остатки на складах"],["shipment","🚛 Отгрузка / Приёмка"]] as [TabType,string][]).map(([t,l]) => (
          <button key={t} onClick={() => setTab(t)}
            className={`px-5 py-[5px] text-[13px] border-t border-l border-r border-[#C0C7D0] -mb-px ${tab===t ? "bg-white text-[#1E3558] font-semibold" : "bg-[#EEF2F7] text-[#6B7280] hover:bg-[#E5EBF5]"}`}>
            {l}
          </button>
        ))}
      </div>

      {/* ══ TAB: Остатки ══ */}
      {tab === "stocks" && (
        <>
          <div className="flex items-center gap-2">
            <Btn variant="primary" icon="＋" disabled={selStock === undefined} onClick={() => { if (selStock !== undefined) { setEditQty(String(stocks[selStock].qty)); setEditModal(true); } }}>Обновить остаток</Btn>
            <Btn disabled={selStock === undefined} icon="📝">Журнал изменений</Btn>
            <Combo value="0" onChange={() => {}} width="w-[180px]" options={[
              { value: "0", label: "Все склады" },
              ...WAREHOUSES.map(w => ({ value: String(w.id), label: w.name })),
            ]} />
          </div>

          <Alert type="warn">Складской учёт ведётся на бумаге. Данные вводятся вручную после сверки руководителями складов с бумажными журналами.</Alert>

          <Grid
            cols={[
              { key: "good",      header: "Культура",          width: "160px" },
              { key: "sort",      header: "Сорт",              width: "100px" },
              { key: "warehouse", header: "Склад",             width: "180px" },
              { key: "qty",       header: "Остаток",           width: "80px"  },
              { key: "state",     header: "Состояние",         width: "100px" },
              { key: "checked",   header: "Дата проверки",     width: "140px" },
            ]}
            rows={stockRows}
            selected={selStock}
            onSelect={setSelStock}
            height="flex-1"
          />

          {editModal && selStock !== undefined && (
            <Modal title="Обновление остатков на складе — UC-02" onClose={() => setEditModal(false)} width={420}>
              <div className="p-4 flex flex-col gap-3">
                <Alert type="info">После сверки с бумажными журналами внесите актуальное количество. Дата и время фиксируются автоматически.</Alert>
                <Box title="Остаток_товара (запись в БД)">
                  <div className="flex flex-col gap-2 pt-1">
                    <Field label="Культура:"     value={goodName(stocks[selStock].goodId)}      readOnly />
                    <Field label="Склад:"        value={warehouseName(stocks[selStock].warehouseId)} readOnly />
                    <Field label="Кол-во (т):"   value={editQty} onChange={setEditQty} />
                    <Field label="Дата проверки:" value={new Date().toLocaleString("ru-RU")} readOnly />
                    <Field label="Примечание:"   value={editNote} onChange={setEditNote} placeholder="Расхождения, замечания..." />
                  </div>
                </Box>
                <div className="flex justify-end gap-2">
                  <Btn variant="primary" onClick={handleSaveStock} icon="💾">Сохранить</Btn>
                  <Btn onClick={() => setEditModal(false)}>Отмена</Btn>
                </div>
              </div>
            </Modal>
          )}
        </>
      )}

      {/* ══ TAB: Отгрузка / Приёмка ══ */}
      {tab === "shipment" && (
        <>
          <Alert type="info">Выберите заказ и зафиксируйте фактическое движение товара. Статус заказа и остатки будут обновлены — UC-05.</Alert>

          <Grid
            cols={[
              { key: "id",     header: "Номер заказа", width: "110px" },
              { key: "date",   header: "Дата",         width: "90px"  },
              { key: "party",  header: "Контрагент",   width: "200px" },
              { key: "type",   header: "Тип",          width: "80px"  },
              { key: "status", header: "Статус",       width: "130px" },
              { key: "good",   header: "Культура",     width: "140px" },
              { key: "qty",    header: "Кол-во (т)",   width: "90px"  },
            ]}
            rows={ORDERS_FOR_ACTION.map(o => ({
              id: o.id, date: o.date, party: o.party,
              type: o.type,
              status: <Badge text={o.status} />,
              good: goodName(o.goodId), qty: String(o.qty),
            }))}
            selected={selOrder}
            onSelect={i => { setSelOrder(i); setSavedMsg(""); setShipAction(null); }}
            height="h-40"
          />

          {selOrder !== undefined && (
            <div className="flex gap-3 flex-1">
              <Box title="Действие по заказу" className="flex-1">
                <div className="flex flex-col gap-3 pt-2">
                  <div className="flex gap-2">
                    <Btn variant={shipAction === "ship" ? "primary" : "default"} onClick={() => setShipAction("ship")} icon="🚛">Отгрузить (→ Отгружен)</Btn>
                    <Btn variant={shipAction === "receive" ? "primary" : "default"} onClick={() => setShipAction("receive")} icon="📦">Принять на склад</Btn>
                  </div>

                  {shipAction && (
                    <Box title="Фиксация данных (Списание / Остаток_товара)">
                      <div className="flex flex-col gap-2 pt-1">
                        <Field label="Заказано (т):" value={String(ORDERS_FOR_ACTION[selOrder].qty)} readOnly />
                        <Field label="Факт. кол-во (т):" value={actualQty} onChange={setActualQty} placeholder="Введите фактическое количество" />
                        <Field label="Дата/время:" value={new Date().toLocaleString("ru-RU")} readOnly />
                        <Field label="Примечание:" value={shipNote} onChange={setShipNote} placeholder="Расхождения, повреждения..." />
                      </div>
                    </Box>
                  )}

                  {shipAction && (
                    <div className="flex gap-2">
                      <Btn variant="primary" onClick={handleShip} icon="💾">Зафиксировать</Btn>
                      <Btn onClick={() => setShipAction(null)}>Отмена</Btn>
                    </div>
                  )}

                  {savedMsg && <Alert type="success">{savedMsg}</Alert>}
                </div>
              </Box>

              <Box title="Текущие остатки на складах" className="w-[280px]">
                <Grid cols={[
                  { key: "good", header: "Культура",    width: "150px" },
                  { key: "qty",  header: "Остаток (т)", width: "90px"  },
                ]} rows={stocks.map(s => ({ good: goodName(s.goodId), qty: `${s.qty}` }))} height="h-full" />
                <div className="text-[11px] text-[#9CA3AF] mt-1">* обновляется после фиксации</div>
              </Box>
            </div>
          )}
        </>
      )}
    </div>
  );
}
