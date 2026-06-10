import { useState } from "react";
import { Btn, Field, Combo, Box, Grid, Badge, Modal, Alert } from "./ui";

const SUPPLIERS = [
  { id: 1, name: 'ООО "АгроСнаб"',          phone: "+7 495 500-10-20", email: "agro@snab.ru"   },
  { id: 2, name: 'ЗАО "Зерновые ресурсы"', phone: "+7 812 600-20-30", email: "grain@res.ru"   },
  { id: 3, name: 'ИП Гришин В.П.',           phone: "+7 926 700-30-40", email: "grishin@mail.ru" },
];
const GOODS = [
  { id: 1, name: "Пшеница 3 кл.",   unit: "т" },
  { id: 2, name: "Ячмень фуражный", unit: "т" },
  { id: 3, name: "Кукуруза",        unit: "т" },
  { id: 4, name: "Подсолнечник",    unit: "т" },
];

interface PurchaseOrder {
  id: string; date: string; supplierId: number; statusId: number; items: { goodId: number; qty: number; pricePerT: number }[];
}
const STATUS_MAP: Record<number, string> = { 1: "Оформлен", 2: "Исполнен", 3: "Отменён" };

const INIT_ORDERS: PurchaseOrder[] = [
  { id: "ЗЗ-00001", date: "05.06.2026", supplierId: 1, statusId: 1, items: [{ goodId: 1, qty: 100, pricePerT: 4800 }, { goodId: 2, qty: 50, pricePerT: 4000 }] },
  { id: "ЗЗ-00002", date: "01.06.2026", supplierId: 2, statusId: 2, items: [{ goodId: 3, qty: 80,  pricePerT: 5200 }] },
  { id: "ЗЗ-00003", date: "25.05.2026", supplierId: 3, statusId: 2, items: [{ goodId: 2, qty: 40,  pricePerT: 3900 }] },
];
const HISTORY = [
  { date: "01.06.2026", supplierId: 1, goodId: 1, qty: 200, pricePerT: 4800 },
  { date: "15.05.2026", supplierId: 3, goodId: 2, qty: 80,  pricePerT: 3900 },
  { date: "10.05.2026", supplierId: 2, goodId: 3, qty: 150, pricePerT: 5200 },
  { date: "01.05.2026", supplierId: 1, goodId: 1, qty: 250, pricePerT: 4700 },
  { date: "20.04.2026", supplierId: 2, goodId: 4, qty: 60,  pricePerT: 7800 },
];

function supplierName(id: number) { return SUPPLIERS.find(s => s.id === id)?.name ?? "?"; }
function goodName(id: number) { return GOODS.find(g => g.id === id)?.name ?? "?"; }
function orderTotal(o: PurchaseOrder) { return o.items.reduce((s, i) => s + i.qty * i.pricePerT, 0); }

export function PurchasesScreen() {
  const [orders, setOrders] = useState<PurchaseOrder[]>(INIT_ORDERS);
  const [sel, setSel] = useState<number | undefined>();
  const [showCreate, setShowCreate] = useState(false);

  const [cfSupplierId, setCfSupplierId] = useState("1");
  const [cfComment, setCfComment] = useState("");

  const gridRows = orders.map(o => ({
    id:       o.id,
    date:     o.date,
    supplier: supplierName(o.supplierId),
    items:    `${o.items.length} поз.`,
    total:    `${orderTotal(o).toLocaleString("ru-RU")} ₽`,
    status:   <Badge text={STATUS_MAP[o.statusId]} />,
  }));

  function handleCreate() {
    setOrders(prev => [{
      id: `ЗЗ-0000${prev.length + 1}`,
      date: new Date().toLocaleDateString("ru-RU"),
      supplierId: Number(cfSupplierId),
      statusId: 1,
      items: [{ goodId: 1, qty: 50, pricePerT: 4800 }],
    }, ...prev]);
    setShowCreate(false);
  }

  const selOrder = sel !== undefined ? orders[sel] : null;
  const selSupplier = selOrder ? SUPPLIERS.find(s => s.id === selOrder.supplierId) : null;

  return (
    <div className="flex flex-col gap-3 h-full text-[13px]">
      <div className="flex items-center gap-2">
        <Btn variant="primary" onClick={() => setShowCreate(true)} icon="＋">Создать заказ</Btn>
        <Btn disabled={sel === undefined} icon="✏">Изменить</Btn>
        <Btn variant="danger" disabled={sel === undefined} icon="✕">Удалить</Btn>
        <Btn disabled={sel === undefined} icon="📄">Документы</Btn>
      </div>

      <Grid
        cols={[
          { key: "id",       header: "Номер",       width: "100px" },
          { key: "date",     header: "Дата",        width: "90px"  },
          { key: "supplier", header: "Поставщик",   width: "220px" },
          { key: "items",    header: "Позиции",     width: "70px"  },
          { key: "total",    header: "Сумма",       width: "120px" },
          { key: "status",   header: "Статус",      width: "100px" },
        ]}
        rows={gridRows}
        selected={sel}
        onSelect={setSel}
        height="h-44"
      />

      {/* Detail + History */}
      <div className="flex gap-3 flex-1">
        {selOrder && (
          <Box title={`Состав заказа ${selOrder.id}`} className="w-[340px]">
            <div className="flex flex-col gap-2 pt-1">
              <Field label="Поставщик:" value={selSupplier?.name} readOnly />
              <Field label="Телефон:"   value={selSupplier?.phone} readOnly />
              <Field label="E-mail:"    value={selSupplier?.email} readOnly />
              <Field label="Статус:"    value={STATUS_MAP[selOrder.statusId]} readOnly />
              <Grid cols={[
                { key: "good",  header: "Культура",  width: "140px" },
                { key: "qty",   header: "Кол-во (т)", width: "80px" },
                { key: "price", header: "Цена ₽/т",  width: "80px" },
                { key: "sum",   header: "Сумма ₽",   width: "90px" },
              ]} rows={selOrder.items.map(i => ({
                good: goodName(i.goodId),
                qty:  String(i.qty),
                price: i.pricePerT.toLocaleString("ru-RU"),
                sum:  (i.qty * i.pricePerT).toLocaleString("ru-RU"),
              }))} height="h-28" />
            </div>
          </Box>
        )}

        <Box title="История закупок (ретроспектива цен)" className="flex-1">
          <Grid cols={[
            { key: "date",     header: "Дата",       width: "90px"  },
            { key: "supplier", header: "Поставщик",  width: "170px" },
            { key: "good",     header: "Культура",   width: "140px" },
            { key: "qty",      header: "Кол-во (т)", width: "90px"  },
            { key: "price",    header: "Цена ₽/т",  width: "90px"  },
          ]} rows={HISTORY.map(h => ({
            date:     h.date,
            supplier: supplierName(h.supplierId),
            good:     goodName(h.goodId),
            qty:      String(h.qty),
            price:    h.pricePerT.toLocaleString("ru-RU"),
          }))} height="h-full" />
        </Box>
      </div>

      {/* ═══ MODAL: Create purchase order ═══ */}
      {showCreate && (
        <Modal title="Новый заказ на закупку — UC-03" onClose={() => setShowCreate(false)} width={480}>
          <div className="p-4 flex flex-col gap-3">
            <Alert type="info">Система показывает историю закупок и текущие остатки для принятия решения об объёме.</Alert>
            <Box title="Заказ_на_закупку">
              <div className="flex flex-col gap-2 pt-1">
                <Field label="Номер:" value={`ЗЗ-0000${orders.length + 1} (авто)`} readOnly />
                <Field label="Дата:" value={new Date().toLocaleDateString("ru-RU")} readOnly />
                <Combo label="Поставщик:" value={cfSupplierId} onChange={setCfSupplierId}
                  options={SUPPLIERS.map(s => ({ value: String(s.id), label: s.name }))} />
                <Field label="Комментарий:" value={cfComment} onChange={setCfComment} placeholder="Причина закупки / основание..." />
              </div>
            </Box>
            <Box title="Перечень товаров">
              <Grid cols={[
                { key: "good",  header: "Культура",  width: "160px" },
                { key: "qty",   header: "Объём (т)", width: "90px"  },
                { key: "price", header: "Цена ₽/т",  width: "90px"  },
              ]} rows={[
                { good: "Пшеница 3 кл.", qty: "50", price: "4 800" },
                { good: "Ячмень фуражный", qty: "30", price: "4 000" },
              ]} height="h-16" />
              <div className="flex gap-2 mt-2">
                <Btn icon="＋">Строку</Btn>
                <Btn variant="danger" icon="−">Строку</Btn>
              </div>
            </Box>
            <div className="flex justify-end gap-2">
              <Btn variant="primary" onClick={handleCreate} icon="💾">Сохранить</Btn>
              <Btn onClick={() => setShowCreate(false)}>Отмена</Btn>
            </div>
          </div>
        </Modal>
      )}
    </div>
  );
}
