import { useState } from "react";
import { Btn, Combo, Box, Grid, Badge, Alert, SectionHeader } from "./ui";
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, LineChart, Line, CartesianGrid, Legend } from "recharts";

// ── Mock aggregated data ──────────────────────────────────────────────────────
const SALES_BY_MONTH = [
  { month: "Янв", sum: 420000, qty: 85 },
  { month: "Фев", sum: 310000, qty: 62 },
  { month: "Мар", sum: 580000, qty: 116 },
  { month: "Апр", sum: 750000, qty: 150 },
  { month: "Май", sum: 630000, qty: 126 },
  { month: "Июн", sum: 890000, qty: 178 },
];

const PURCHASE_BY_MONTH = [
  { month: "Янв", sum: 960000, qty: 200 },
  { month: "Фев", sum: 0,      qty: 0 },
  { month: "Мар", sum: 780000, qty: 150 },
  { month: "Апр", sum: 0,      qty: 0 },
  { month: "Май", sum: 1170000, qty: 230 },
  { month: "Июн", sum: 480000, qty: 100 },
];

const DEBTORS = [
  { buyer: 'ООО "АгроМаркет"', orders: 2, total: 350000, paid: 265000, debt: 85000 },
  { buyer: 'ООО "ЗерноТорг"',  orders: 1, total: 540000, paid: 310000, debt: 230000 },
  { buyer: 'КФХ Смирнов',      orders: 1, total: 215000, paid: 200000, debt: 15000 },
  { buyer: 'ИП Фёдоров А.В.',  orders: 1, total: 128000, paid: 128000, debt: 0 },
];

const STOCKS_SUMMARY = [
  { good: "Пшеница 3 кл.",    wh1: 85,  wh2: 0,  total: 85,  checked: "09.06.2026" },
  { good: "Ячмень фуражный",  wh1: 30,  wh2: 0,  total: 30,  checked: "09.06.2026" },
  { good: "Кукуруза",         wh1: 0,   wh2: 0,  total: 0,   checked: "07.06.2026" },
  { good: "Подсолнечник",     wh1: 0,   wh2: 44, total: 44,  checked: "08.06.2026" },
];

const PERIOD_OPTS = [
  { value: "2026", label: "2026 год" },
  { value: "q2-2026", label: "II квартал 2026" },
  { value: "q1-2026", label: "I квартал 2026" },
];

type ReportTab = "sales" | "stocks" | "debts";

export function AnalyticsScreen() {
  const [tab, setTab] = useState<ReportTab>("sales");
  const [period, setPeriod] = useState("2026");

  const totalSales     = SALES_BY_MONTH.reduce((s, m) => s + m.sum, 0);
  const totalPurchases = PURCHASE_BY_MONTH.reduce((s, m) => s + m.sum, 0);
  const totalDebt      = DEBTORS.reduce((s, d) => s + d.debt, 0);
  const totalStock     = STOCKS_SUMMARY.reduce((s, g) => s + g.total, 0);

  const TABS: [ReportTab, string][] = [
    ["sales",  "📈 Продажи и закупки"],
    ["stocks", "🏭 Складские запасы"],
    ["debts",  "💳 Дебиторская задолженность"],
  ];

  return (
    <div className="flex flex-col gap-3 h-full text-[13px]">
      {/* ── KPI cards ── */}
      <div className="grid grid-cols-4 gap-3">
        {[
          { label: "Продажи за период",  value: `${(totalSales / 1000000).toFixed(2)} млн ₽`, color: "#2E75B6", icon: "📋" },
          { label: "Закупки за период",  value: `${(totalPurchases / 1000000).toFixed(2)} млн ₽`, color: "#7C3AED", icon: "🛒" },
          { label: "Итого остатки",      value: `${totalStock} т`,                color: "#16A34A", icon: "🏭" },
          { label: "Дебиторка",          value: `${totalDebt.toLocaleString("ru-RU")} ₽`, color: "#DC2626", icon: "💳" },
        ].map((kpi, i) => (
          <div key={i} className="bg-white border border-[#C0C7D0] p-3 flex items-start gap-3">
            <span className="text-[24px]">{kpi.icon}</span>
            <div>
              <div className="text-[11px] text-[#6B7280]">{kpi.label}</div>
              <div className="text-[16px] font-semibold" style={{ color: kpi.color }}>{kpi.value}</div>
            </div>
          </div>
        ))}
      </div>

      {/* ── Toolbar ── */}
      <div className="flex items-center gap-2">
        <Combo label="Период:" value={period} onChange={setPeriod} options={PERIOD_OPTS} width="w-[160px]" labelWidth="w-[60px]" />
        <Btn variant="primary" icon="🖨">Печать отчёта</Btn>
        <Btn icon="📤">Экспорт в Excel</Btn>
      </div>

      {/* ── Tabs ── */}
      <div className="flex border-b border-[#C0C7D0]">
        {TABS.map(([t, l]) => (
          <button key={t} onClick={() => setTab(t)}
            className={`px-5 py-[5px] text-[13px] border-t border-l border-r border-[#C0C7D0] -mb-px ${tab === t ? "bg-white text-[#1E3558] font-semibold" : "bg-[#EEF2F7] text-[#6B7280] hover:bg-[#E5EBF5]"}`}>
            {l}
          </button>
        ))}
      </div>

      {/* ══ TAB: Продажи и закупки ══ */}
      {tab === "sales" && (
        <div className="flex gap-3 flex-1">
          <Box title="Сводка продаж по месяцам (сумма, ₽)" className="flex-1">
            <div className="h-[180px] pt-2">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={SALES_BY_MONTH} margin={{ top: 0, right: 10, left: 0, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#E5E7EB" />
                  <XAxis dataKey="month" tick={{ fontSize: 11 }} />
                  <YAxis tick={{ fontSize: 11 }} tickFormatter={v => `${v/1000}k`} />
                  <Tooltip formatter={(v: number) => [`${v.toLocaleString("ru-RU")} ₽`, "Продажи"]} />
                  <Bar dataKey="sum" fill="#2E75B6" name="Продажи, ₽" />
                </BarChart>
              </ResponsiveContainer>
            </div>
            <Grid cols={[
              { key: "month", header: "Месяц",       width: "70px"  },
              { key: "qty",   header: "Кол-во (т)",  width: "100px" },
              { key: "sum",   header: "Сумма (₽)",   width: "130px" },
            ]} rows={SALES_BY_MONTH.map(m => ({
              month: m.month, qty: m.qty, sum: m.sum.toLocaleString("ru-RU"),
            }))} height="h-32" />
          </Box>

          <Box title="Сравнение: продажи vs закупки (₽)" className="flex-1">
            <div className="h-[180px] pt-2">
              <ResponsiveContainer width="100%" height="100%">
                <LineChart data={SALES_BY_MONTH.map((s, i) => ({ ...s, purchases: PURCHASE_BY_MONTH[i].sum }))}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#E5E7EB" />
                  <XAxis dataKey="month" tick={{ fontSize: 11 }} />
                  <YAxis tick={{ fontSize: 11 }} tickFormatter={v => `${v/1000}k`} />
                  <Tooltip formatter={(v: number) => `${v.toLocaleString("ru-RU")} ₽`} />
                  <Legend wrapperStyle={{ fontSize: 11 }} />
                  <Line type="monotone" dataKey="sum"       stroke="#2E75B6" name="Продажи" strokeWidth={2} dot={false} />
                  <Line type="monotone" dataKey="purchases" stroke="#7C3AED" name="Закупки" strokeWidth={2} dot={false} />
                </LineChart>
              </ResponsiveContainer>
            </div>
            <Grid cols={[
              { key: "month", header: "Месяц",       width: "70px"  },
              { key: "sales", header: "Продажи (₽)", width: "130px" },
              { key: "purch", header: "Закупки (₽)", width: "130px" },
              { key: "profit",header: "Разница (₽)", width: "130px" },
            ]} rows={SALES_BY_MONTH.map((s, i) => {
              const p = PURCHASE_BY_MONTH[i].sum;
              const profit = s.sum - p;
              return {
                month:  s.month,
                sales:  s.sum.toLocaleString("ru-RU"),
                purch:  p.toLocaleString("ru-RU"),
                profit: profit >= 0 ? `+${profit.toLocaleString("ru-RU")}` : profit.toLocaleString("ru-RU"),
              };
            })} height="h-32" />
          </Box>
        </div>
      )}

      {/* ══ TAB: Складские запасы ══ */}
      {tab === "stocks" && (
        <div className="flex gap-3 flex-1">
          <Box title="Текущие складские запасы (по последним ручным данным)" className="flex-1">
            <Alert type="warn">Данные об остатках вносятся вручную после сверки с бумажными журналами. Фактические значения могут отличаться.</Alert>
            <div className="mt-2">
              <Grid cols={[
                { key: "good",    header: "Культура",            width: "160px" },
                { key: "wh1",     header: "Склад №1 (т)",        width: "110px" },
                { key: "wh2",     header: "Склад №2 (т)",        width: "110px" },
                { key: "total",   header: "Итого (т)",           width: "90px"  },
                { key: "checked", header: "Дата проверки",       width: "130px" },
                { key: "state",   header: "Состояние",           width: "110px" },
              ]} rows={STOCKS_SUMMARY.map(g => ({
                ...g,
                state: g.total === 0 ? <Badge text="Нет в наличии" /> : g.total < 30 ? <Badge text="Мало" /> : <Badge text="В наличии" />,
              }))} height="h-40" />
            </div>
            <div className="h-[160px] mt-2">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={STOCKS_SUMMARY} margin={{ top: 0, right: 10, left: 0, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#E5E7EB" />
                  <XAxis dataKey="good" tick={{ fontSize: 10 }} />
                  <YAxis tick={{ fontSize: 11 }} />
                  <Tooltip />
                  <Bar dataKey="wh1"   fill="#2E75B6" name="Склад №1" />
                  <Bar dataKey="wh2"   fill="#16A34A" name="Склад №2" />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </Box>
        </div>
      )}

      {/* ══ TAB: Дебиторская задолженность ══ */}
      {tab === "debts" && (
        <div className="flex gap-3 flex-1">
          <Box title="Контроль дебиторской задолженности покупателей" className="flex-1">
            {totalDebt > 0 && <Alert type="warn">Общая дебиторская задолженность: <b>{totalDebt.toLocaleString("ru-RU")} ₽</b></Alert>}
            <div className="mt-2">
              <Grid cols={[
                { key: "buyer",   header: "Покупатель",         width: "200px" },
                { key: "orders",  header: "Заказов",            width: "70px"  },
                { key: "total",   header: "Сумма заказов (₽)",  width: "140px" },
                { key: "paid",    header: "Оплачено (₽)",       width: "130px" },
                { key: "debt",    header: "Задолженность (₽)",  width: "140px" },
                { key: "state",   header: "Статус",             width: "100px" },
              ]} rows={DEBTORS.map(d => ({
                buyer:  d.buyer,
                orders: String(d.orders),
                total:  d.total.toLocaleString("ru-RU"),
                paid:   d.paid.toLocaleString("ru-RU"),
                debt:   d.debt > 0 ? d.debt.toLocaleString("ru-RU") : "—",
                state:  d.debt > 0 ? <Badge text="Есть долг" /> : <Badge text="Оплачено" />,
              }))} height="h-full" />
            </div>
          </Box>
        </div>
      )}
    </div>
  );
}
