import { useState } from "react";
import { SalesScreen } from "./SalesScreen";
import { WarehouseScreen } from "./WarehouseScreen";
import { PurchasesScreen } from "./PurchasesScreen";
import { ReferencesScreen } from "./ReferencesScreen";
import { AnalyticsScreen } from "./AnalyticsScreen";

type Screen = "sales" | "warehouse" | "purchases" | "references" | "analytics";

interface NavItem { id: Screen; label: string; icon: string; roles: string[]; }

const NAV: NavItem[] = [
  { id: "sales",      label: "Продажи",            icon: "📋", roles: ["Менеджер по продажам","Коммерческий директор","Генеральный директор"] },
  { id: "warehouse",  label: "Складской учёт",     icon: "🏭", roles: ["Руководитель склада","Коммерческий директор","Генеральный директор"] },
  { id: "purchases",  label: "Закупки",             icon: "🛒", roles: ["Генеральный директор","Коммерческий директор"] },
  { id: "references", label: "Справочники",         icon: "📖", roles: ["Менеджер по продажам","Руководитель склада","Коммерческий директор","Генеральный директор"] },
  { id: "analytics",  label: "Аналитика",           icon: "📊", roles: ["Коммерческий директор","Генеральный директор"] },
];

interface MainWindowProps { role: string; userName: string; onLogout: () => void; }

export function MainWindow({ role, userName, onLogout }: MainWindowProps) {
  const available = NAV.filter(n => n.roles.includes(role));
  const [screen, setScreen] = useState<Screen>(available[0]?.id ?? "sales");
  const [showLogoutConfirm, setShowLogoutConfirm] = useState(false);
  const current = NAV.find(n => n.id === screen);

  return (
    <div className="size-full flex flex-col" style={{ fontFamily: "Segoe UI, Tahoma, sans-serif", background: "#F0F2F5" }}>
      {/* ── Title bar ── */}
      <div className="h-[28px] bg-[#1E3558] flex items-center justify-between px-3 shrink-0">
        <div className="flex items-center gap-2">
          <span className="text-[16px]">🌾</span>
          <span className="text-white text-[13px] font-semibold">АИС «Агро-Торг»</span>
          <span className="text-[#7FA8CC] text-[13px]">—</span>
          <span className="text-[#7FA8CC] text-[13px]">{current?.label}</span>
        </div>
        <div className="flex gap-1">
          {["─","□","✕"].map((s,i) => (
            <button key={i} className={`w-[16px] h-[16px] text-[10px] flex items-center justify-center border border-[#4A6A90] text-white ${i===2 ? "bg-[#C42B1C] hover:bg-[#E81123]" : "hover:bg-[#2E75B6]"}`}>{s}</button>
          ))}
        </div>
      </div>

      {/* ── Main layout ── */}
      <div className="flex flex-1 overflow-hidden">
        {/* ── Sidebar ── */}
        <div className="w-[180px] bg-[#1E3558] flex flex-col shrink-0">
          {/* User card */}
          <div className="px-3 py-3 border-b border-[#2E5280]">
            <div className="w-10 h-10 bg-[#2E75B6] flex items-center justify-center text-white text-[16px] font-bold mb-2">
              {userName.charAt(0)}
            </div>
            <div className="text-white text-[12px] font-semibold leading-tight">{userName}</div>
            <div className="text-[#7FA8CC] text-[11px] leading-tight mt-1">{role}</div>
          </div>

          {/* Nav items */}
          <nav className="flex-1 pt-2">
            {available.map(item => (
              <button key={item.id} onClick={() => setScreen(item.id)}
                className={`w-full flex items-center gap-3 px-4 py-[7px] text-[13px] text-left transition-colors ${screen === item.id ? "bg-[#2E75B6] text-white" : "text-[#B8CDE0] hover:bg-[#253F66] hover:text-white"}`}>
                <span className="text-[16px] shrink-0">{item.icon}</span>
                <span>{item.label}</span>
              </button>
            ))}
          </nav>

          {/* Logout */}
          <div className="pb-3 border-t border-[#2E5280] pt-2">
            <button onClick={() => setShowLogoutConfirm(true)}
              className="w-full flex items-center gap-3 px-4 py-[7px] text-[13px] text-[#B8CDE0] hover:bg-[#C42B1C] hover:text-white transition-colors">
              <span className="text-[16px]">🚪</span>
              <span>Выйти из профиля</span>
            </button>
          </div>
        </div>

        {/* ── Content ── */}
        <div className="flex-1 overflow-hidden flex flex-col">
          {/* Content header */}
          <div className="h-[36px] bg-white border-b border-[#D1D5DB] flex items-center px-4 gap-3 shrink-0">
            <span className="text-[16px]">{current?.icon}</span>
            <span className="text-[14px] font-semibold text-[#1E3558]">{current?.label}</span>
            <div className="ml-auto text-[12px] text-[#9CA3AF]">
              {new Date().toLocaleString("ru-RU")}
            </div>
          </div>

          {/* Screen content */}
          <div className="flex-1 overflow-auto p-3">
            {screen === "sales"      && <SalesScreen role={role} />}
            {screen === "warehouse"  && <WarehouseScreen />}
            {screen === "purchases"  && <PurchasesScreen />}
            {screen === "references" && <ReferencesScreen />}
            {screen === "analytics"  && <AnalyticsScreen />}
          </div>
        </div>
      </div>

      {/* ── Status bar ── */}
      <div className="h-[22px] bg-[#E5E7EB] border-t border-[#C0C7D0] flex items-center px-3 gap-4 text-[11px] text-[#6B7280] shrink-0">
        <span className="flex items-center gap-1"><span className="w-2 h-2 rounded-full bg-[#16A34A] inline-block" />Подключено к MS SQL Server</span>
        <span className="border-l border-[#C0C7D0] pl-4">Пользователь: <b className="text-[#1E3558]">{userName}</b></span>
        <span className="border-l border-[#C0C7D0] pl-4">Роль: {role}</span>
        <span className="ml-auto">АИС «Агро-Торг» v1.0</span>
      </div>

      {/* ── Logout confirm dialog ── */}
      {showLogoutConfirm && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-[300]" onClick={() => setShowLogoutConfirm(false)}>
          <div className="w-[340px] bg-[#F5F6F8] border border-[#9CA3AF] shadow-[4px_6px_16px_rgba(0,0,0,0.35)]" onClick={e => e.stopPropagation()}>
            <div className="h-[26px] bg-[#1E3558] flex items-center px-3">
              <span className="text-white text-[13px] font-semibold">Выход из системы</span>
            </div>
            <div className="p-4">
              <p className="text-[13px] text-[#374151] mb-4">Вы действительно хотите завершить рабочую сессию?<br />Все открытые формы будут закрыты.</p>
              <div className="flex justify-end gap-2">
                <button onClick={onLogout} className="px-4 h-[26px] bg-[#2E75B6] hover:bg-[#1A5A9A] text-white text-[13px] border border-[#1A5A9A]">Да, выйти</button>
                <button onClick={() => setShowLogoutConfirm(false)} className="px-4 h-[26px] bg-white hover:bg-[#F3F4F6] text-[#374151] text-[13px] border border-[#C0C7D0]">Отмена</button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
