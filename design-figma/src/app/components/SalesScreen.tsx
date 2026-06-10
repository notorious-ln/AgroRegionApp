import { useState } from "react";
import { Btn, Field, Combo, Box, Grid, Badge, Modal, Alert, SectionHeader, Divider } from "./ui";

// Mock data aligned with 3NF: Заказ_на_продажу ← Покупатель, Сотрудник, Остаток_товара, Статус
const BUYERS = [
  { id: 1, name: 'ООО "АгроМаркет"',  inn: "7701234567", phone: "+7 495 123-45-67", debt: 85000 },
  { id: 2, name: 'ИП Фёдоров А.В.',   inn: "771234567890", phone: "+7 926 234-56-78", debt: 0 },
  { id: 3, name: 'ООО "ЗерноТорг"',   inn: "7709876543", phone: "+7 499 345-67-89", debt: 230000 },
  { id: 4, name: 'КФХ Смирнов',        inn: "501234567890", phone: "+7 916 456-78-90", debt: 15000 },
];
const GOODS = [
  { id: 1, name: "Пшеница 3 кл.",     sort: "Экстра",     unit: "т",  price: 5000,  season: "Лето-2025" },
  { id: 2, name: "Ячмень фуражный",   sort: "1-й класс",  unit: "т",  price: 4000,  season: "Осень-2025" },
  { id: 3, name: "Кукуруза",          sort: "Гибрид",     unit: "т",  price: 5500,  season: "Осень-2025" },
  { id: 4, name: "Подсолнечник",      sort: "Масличный",  unit: "т",  price: 8000,  season: "Лето-2025" },
];
const STOCKS = [
  { id: 1, goodId: 1, warehouseId: 1, qty: 85,  checked: "09.06.2026" },
  { id: 2, goodId: 2, warehouseId: 1, qty: 30,  checked: "09.06.2026" },
  { id: 3, goodId: 3, warehouseId: 2, qty: 0,   checked: "07.06.2026" },
  { id: 4, goodId: 4, warehouseId: 2, qty: 44,  checked: "08.06.2026" },
];
const WAREHOUSES = [{ id: 1, name: "Склад №1 (Центральный)" }, { id: 2, name: "Склад №2 (Северный)" }];

type SaleStatus = "Новый" | "Подтверждён" | "Готов к отгрузке" | "Отгружен";
interface Order {
  id: string; date: string; buyerId: number; stockId: number;
  employeeId: number; statusId: number; pricePerKg: number; qty: number;
}

const STATUS_MAP: Record<number, SaleStatus> = { 1: "Новый", 2: "Подтверждён", 3: "Готов к отгрузке", 4: "Отгружен" };

const INIT_ORDERS: Order[] = [
  { id: "ЗП-00001", date: "09.06.2026", buyerId: 1, stockId: 1, employeeId: 1, statusId: 1,  pricePerKg: 5.00, qty: 50000 },
  { id: "ЗП-00002", date: "08.06.2026", buyerId: 2, stockId: 2, employeeId: 1, statusId: 2,  pricePerKg: 4.00, qty: 20000 },
  { id: "ЗП-00003", date: "07.06.2026", buyerId: 3, stockId: 1, employeeId: 1, statusId: 3,  pricePerKg: 5.00, qty: 60000 },
  { id: "ЗП-00004", date: "06.06.2026", buyerId: 4, stockId: 4, employeeId: 1, statusId: 4,  pricePerKg: 8.00, qty: 10000 },
];

interface SalesScreenProps { role: string; }

type ModalType = "none" | "create" | "docs" | "stocks";

export function SalesScreen({ role }: SalesScreenProps) {
  const [orders, setOrders] = useState<Order[]>(INIT_ORDERS);
  const [sel, setSel] = useState<number | undefined>();
  const [modal, setModal] = useState<ModalType>("none");
  const [filterStatus, setFilterStatus] = useState("0");
  const [search, setSearch] = useState("");

  // Create form state
  const [cfBuyerId, setCfBuyerId] = useState("1");
  const [cfStockId, setCfStockId] = useState("1");
  const [cfQty, setCfQty] = useState("");
  const [cfPrice, setCfPrice] = useState("");
  const [cfStockConfirmed, setCfStockConfirmed] = useState(false);
  const [cfStockNote, setCfStockNote] = useState("");

  const canCreateDocs = sel !== undefined && (orders[sel]?.statusId === 2 || orders[sel]?.statusId === 3);
  const canRequestStock = sel !== undefined;

  function goodName(stockId: number) {
    const s = STOCKS.find(s => s.id === stockId);
    return s ? GOODS.find(g => g.id === s.goodId)?.name ?? "?" : "?";
  }
  function buyerName(id: number) { return BUYERS.find(b => b.id === id)?.name ?? "?"; }
  function warehouseName(stockId: number) {
    const s = STOCKS.find(s => s.id === stockId);
    return s ? WAREHOUSES.find(w => w.id === s.warehouseId)?.name ?? "?" : "?";
  }

  const filtered = orders.filter(o => {
    const statusOk = filterStatus === "0" || o.statusId === Number(filterStatus);
    const textOk = !search || buyerName(o.buyerId).toLowerCase().includes(search.toLowerCase()) || o.id.includes(search);
    return statusOk && textOk;
  });

  const gridRows = filtered.map(o => ({
    id: o.id,
    date: o.date,
    buyer: buyerName(o.buyerId),
    good: goodName(o.stockId),
    warehouse: warehouseName(o.stockId),
    qty: `${(o.qty / 1000).toFixed(0)} т`,
    total: `${((o.pricePerKg * o.qty) / 1000).toLocaleString("ru-RU")} ₽`,
    status: <Badge text={STATUS_MAP[o.statusId]} />,
  }));

  function handleCreate() {
    const stock = STOCKS.find(s => s.id === Number(cfStockId));
    const newOrder: Order = {
      id: `ЗП-0000${orders.length + 1}`,
      date: new Date().toLocaleDateString("ru-RU"),
      buyerId: Number(cfBuyerId),
      stockId: Number(cfStockId),
      employeeId: 1,
      statusId: 1,
      pricePerKg: Number(cfPrice) || (stock ? GOODS.find(g => g.id === stock.goodId)?.price ?? 0 : 0) / 1000,
      qty: Number(cfQty) * 1000 || 1000,
    };
    setOrders(prev => [newOrder, ...prev]);
    setModal("none");
    setCfQty(""); setCfPrice(""); setCfStockConfirmed(false); setCfStockNote("");
  }

  const selOrder = sel !== undefined ? filtered[sel] : null;
  const selStock = selOrder ? STOCKS.find(s => s.id === selOrder.stockId) : null;
  const selGood = selStock ? GOODS.find(g => g.id === selStock.goodId) : null;
  const selBuyer = selOrder ? BUYERS.find(b => b.id === selOrder.buyerId) : null;

  return (
    <div className="flex flex-col gap-3 h-full text-[13px]">
      {/* ── Toolbar ── */}
      <div className="flex items-center gap-2 flex-wrap">
        <Btn variant="primary" onClick={() => setModal("create")} icon="＋">Создать заказ</Btn>
        <Btn disabled={sel === undefined} icon="✏">Изменить</Btn>
        <Btn variant="danger" disabled={sel === undefined} icon="✕">Удалить</Btn>
        <Btn disabled={!canCreateDocs} onClick={() => canCreateDocs && setModal("docs")} icon="📄">Документы</Btn>
        <Btn disabled={!canRequestStock} onClick={() => canRequestStock && setModal("stocks")} icon="🔍">Запрос остатков</Btn>
        <div className="ml-auto flex items-center gap-2">
          <Field placeholder="Поиск по покупателю, номеру..." value={search} onChange={setSearch} width="w-[200px]" />
          <Combo value={filterStatus} onChange={setFilterStatus} width="w-[160px]" options={[
            { value: "0", label: "Все статусы" },
            { value: "1", label: "Новый" },
            { value: "2", label: "Подтверждён" },
            { value: "3", label: "Готов к отгрузке" },
            { value: "4", label: "Отгружен" },
          ]} />
        </div>
      </div>

      {/* ── Orders grid ── */}
      <Grid
        cols={[
          { key: "id",        header: "Номер заказа", width: "110px" },
          { key: "date",      header: "Дата",         width: "90px"  },
          { key: "buyer",     header: "Покупатель",   width: "200px" },
          { key: "good",      header: "Культура",     width: "160px" },
          { key: "warehouse", header: "Склад",        width: "160px" },
          { key: "qty",       header: "Кол-во",       width: "70px"  },
          { key: "total",     header: "Сумма",        width: "110px" },
          { key: "status",    header: "Статус",       width: "130px" },
        ]}
        rows={gridRows}
        selected={sel}
        onSelect={setSel}
        height="h-52"
      />

      {/* ── Detail panel ── */}
      {selOrder && (
        <div className="flex gap-3">
          <Box title="Детали заказа" className="flex-1">
            <div className="grid grid-cols-2 gap-x-6 gap-y-2 pt-1">
              <Field label="Номер:" value={selOrder.id} readOnly labelWidth="w-[90px]" />
              <Field label="Дата:" value={selOrder.date} readOnly labelWidth="w-[90px]" />
              <Field label="Покупатель:" value={selBuyer?.name} readOnly labelWidth="w-[90px]" />
              <Field label="ИНН:" value={selBuyer?.inn} readOnly labelWidth="w-[90px]" />
              <Field label="Культура:" value={selGood?.name} readOnly labelWidth="w-[90px]" />
              <Field label="Сорт:" value={selGood?.sort} readOnly labelWidth="w-[90px]" />
              <Field label="Склад:" value={warehouseName(selOrder.stockId)} readOnly labelWidth="w-[90px]" />
              <Field label="Цена (₽/кг):" value={String(selOrder.pricePerKg)} readOnly labelWidth="w-[90px]" />
              <Field label="Кол-во (т):" value={`${selOrder.qty / 1000}`} readOnly labelWidth="w-[90px]" />
              <Field label="Сумма (₽):" value={(selOrder.pricePerKg * selOrder.qty / 1000).toLocaleString("ru-RU")} readOnly labelWidth="w-[90px]" />
            </div>
          </Box>
          <Box title="Дебиторская задолженность покупателя" className="w-[260px]">
            <div className="flex flex-col gap-2 pt-1">
              <Field label="Покупатель:" value={selBuyer?.name} readOnly labelWidth="w-[90px]" />
              <Field label="Телефон:" value={selBuyer?.phone} readOnly labelWidth="w-[90px]" />
              <Field label="Задолженность:" value={`${selBuyer?.debt?.toLocaleString("ru-RU")} ₽`} readOnly labelWidth="w-[90px]" />
              {(selBuyer?.debt ?? 0) > 0
                ? <Alert type="warn">Имеется дебиторская задолженность</Alert>
                : <Alert type="success">Задолженности нет</Alert>}
            </div>
          </Box>
        </div>
      )}

      {/* ═══ MODAL: Create order ═══ */}
      {modal === "create" && (
        <Modal title="Новый заказ на продажу — UC-01" onClose={() => setModal("none")} width={500}>
          <div className="p-4 flex flex-col gap-3">
            <Box title="Реквизиты (Заказ_на_продажу)">
              <div className="flex flex-col gap-2 pt-1">
                <Field label="Номер:" value={`ЗП-0000${orders.length + 1} (авто)`} readOnly />
                <Field label="Дата:" value={new Date().toLocaleDateString("ru-RU")} readOnly />
                <Combo label="Покупатель:" value={cfBuyerId} onChange={setCfBuyerId}
                  options={BUYERS.map(b => ({ value: String(b.id), label: b.name }))} />
              </div>
            </Box>
            <Box title="Номенклатура и остаток (Остаток_товара → Товар, Склад)">
              <div className="flex flex-col gap-2 pt-1">
                <Combo label="Остаток / Склад:" value={cfStockId} onChange={setCfStockId}
                  options={STOCKS.map(s => ({
                    value: String(s.id),
                    label: `${GOODS.find(g => g.id === s.goodId)?.name} — ${WAREHOUSES.find(w => w.id === s.warehouseId)?.name} (${s.qty} т)`,
                  }))} />
                <Field label="Кол-во (т):" value={cfQty} onChange={setCfQty} placeholder="напр. 50" />
                <Field label="Цена (₽/кг):" value={cfPrice} onChange={setCfPrice} placeholder="напр. 5.00" />
              </div>
            </Box>
            <Box title="Проверка остатков на складе — UC-02">
              <div className="flex flex-col gap-2 pt-1">
                <Alert type="info">Складской учёт ведётся на бумаге. Уточните остатки у руководителя склада и внесите данные вручную.</Alert>
                <Field label="Подтверждение:" value={cfStockNote} onChange={setCfStockNote}
                  placeholder="напр.: Пшеница — 120 т на 09.06.2026" />
                <div className="flex items-center gap-2">
                  <input type="checkbox" id="sc" checked={cfStockConfirmed} onChange={e => setCfStockConfirmed(e.target.checked)} />
                  <label htmlFor="sc" className="text-[13px] text-[#374151]">Остатки подтверждены руководителем склада</label>
                </div>
                {!cfStockConfirmed && <Alert type="warn">Система зафиксирует необходимость уточнения у склада</Alert>}
              </div>
            </Box>
            <div className="flex justify-end gap-2 pt-1">
              <Btn variant="primary" onClick={handleCreate} icon="💾">Сохранить</Btn>
              <Btn onClick={() => setModal("none")}>Отмена</Btn>
            </div>
          </div>
        </Modal>
      )}

      {/* ═══ MODAL: Documents ═══ */}
      {modal === "docs" && selOrder && (
        <Modal title={`Документы по заказу ${selOrder.id} — UC-04`} onClose={() => setModal("none")} width={400}>
          <div className="p-4 flex flex-col gap-3">
            <Alert type="info">Статус заказа: <b>{STATUS_MAP[selOrder.statusId]}</b>. Комплект документов готов к формированию.</Alert>
            <Box title="Состав комплекта">
              <div className="flex flex-col gap-2 pt-1">
                {[
                  { label: "Счёт на оплату",         default: true  },
                  { label: "Договор купли-продажи",   default: true  },
                  { label: "Товарная накладная (ТОРГ-12)", default: true },
                  { label: "ТТН (при необходимости)", default: false },
                ].map((d, i) => (
                  <div key={i} className="flex items-center gap-2">
                    <input type="checkbox" defaultChecked={d.default} id={`doc${i}`} />
                    <label htmlFor={`doc${i}`} className="text-[13px] text-[#374151]">{d.label}</label>
                  </div>
                ))}
              </div>
            </Box>
            <Combo label="Формат вывода:" value="pdf" options={[{ value: "pdf", label: "PDF" }, { value: "docx", label: "DOCX" }]} />
            <Field label="Путь сохранения:" value={`C:\\АгроТорг\\Документы\\${selOrder.id}\\`} readOnly />
            <div className="flex justify-end gap-2">
              <Btn variant="primary" icon="📄">Сформировать</Btn>
              <Btn onClick={() => setModal("none")}>Закрыть</Btn>
            </div>
          </div>
        </Modal>
      )}

      {/* ═══ MODAL: Stock check request ═══ */}
      {modal === "stocks" && selOrder && (
        <Modal title="Запрос остатков на складе — UC-02" onClose={() => setModal("none")} width={440}>
          <div className="p-4 flex flex-col gap-3">
            <Alert type="info">Складской учёт ведётся на бумаге. Запрос фиксируется в системе с датой/временем проверки.</Alert>
            <Box title="Запрошенные позиции">
              <Grid cols={[
                { key: "good",  header: "Культура",      width: "180px" },
                { key: "need",  header: "Требуется (т)", width: "110px" },
                { key: "avail", header: "Остаток в БД",  width: "110px" },
              ]} rows={[{
                good: selGood?.name ?? "?",
                need: `${selOrder.qty / 1000} т`,
                avail: selStock ? `${selStock.qty} т (${selStock.checked})` : "—",
              }]} height="h-16" />
            </Box>
            <Box title="Ответ руководителя склада (вносится вручную)">
              <div className="flex flex-col gap-2 pt-1">
                <Field label="Фактически:" placeholder="напр.: Пшеница — 85 т, подтверждено 09.06.2026" />
                <Field label="Время проверки:" value={new Date().toLocaleString("ru-RU")} readOnly />
                <Field label="Руководитель:" value="Ахметов А.М. (Склад №1)" readOnly />
              </div>
            </Box>
            <div className="flex justify-end gap-2">
              <Btn variant="success" onClick={() => setModal("none")} icon="✔">Подтвердить и закрыть</Btn>
              <Btn onClick={() => setModal("none")}>Отмена</Btn>
            </div>
          </div>
        </Modal>
      )}
    </div>
  );
}
