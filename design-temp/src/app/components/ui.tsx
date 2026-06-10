// Shared WinForms-style UI primitives
// Designed to map 1:1 to C# Windows Forms controls

import React, { useState } from "react";

// ─── Btn → Button ────────────────────────────────────────────────────────────
interface BtnProps {
  children: React.ReactNode;
  onClick?: () => void;
  variant?: "primary" | "default" | "danger" | "success";
  disabled?: boolean;
  className?: string;
  type?: "button" | "submit";
  icon?: string;
}
export function Btn({ children, onClick, variant = "default", disabled, className = "", type = "button", icon }: BtnProps) {
  const base = "inline-flex items-center gap-1 px-3 h-[26px] text-[13px] border cursor-pointer select-none whitespace-nowrap transition-colors";
  const styles: Record<string, string> = {
    primary: "bg-[#2E75B6] hover:bg-[#1A5A9A] text-white border-[#1A5A9A]",
    default: "bg-white hover:bg-[#EEF2F7] text-[#1A1A2E] border-[#C0C7D0]",
    danger:  "bg-white hover:bg-[#FEE2E2] text-[#DC2626] border-[#C0C7D0]",
    success: "bg-[#16A34A] hover:bg-[#15803D] text-white border-[#15803D]",
  };
  return (
    <button type={type} onClick={onClick} disabled={disabled}
      className={`${base} ${styles[variant]} ${disabled ? "opacity-40 pointer-events-none" : ""} ${className}`}>
      {icon && <span>{icon}</span>}{children}
    </button>
  );
}

// ─── Field → TextBox ─────────────────────────────────────────────────────────
interface FieldProps {
  label?: string;
  value?: string;
  onChange?: (v: string) => void;
  type?: string;
  placeholder?: string;
  readOnly?: boolean;
  width?: string;
  labelWidth?: string;
}
export function Field({ label, value, onChange, type = "text", placeholder, readOnly, width = "flex-1", labelWidth = "w-[110px]" }: FieldProps) {
  return (
    <div className="flex items-center gap-2">
      {label && <label className={`text-[13px] text-[#374151] shrink-0 ${labelWidth}`}>{label}</label>}
      <input type={type} value={value ?? ""} readOnly={readOnly} onChange={e => onChange?.(e.target.value)} placeholder={placeholder}
        className={`${width} h-[24px] border border-[#C0C7D0] bg-white text-[13px] text-[#1A1A2E] px-2 focus:outline-none focus:border-[#2E75B6] ${readOnly ? "bg-[#F3F4F6] text-[#6B7280]" : ""}`} />
    </div>
  );
}

// ─── Combo → ComboBox ────────────────────────────────────────────────────────
interface ComboProps {
  label?: string;
  value?: string;
  onChange?: (v: string) => void;
  options: { value: string; label: string }[];
  width?: string;
  labelWidth?: string;
}
export function Combo({ label, value, onChange, options, width = "flex-1", labelWidth = "w-[110px]" }: ComboProps) {
  return (
    <div className="flex items-center gap-2">
      {label && <label className={`text-[13px] text-[#374151] shrink-0 ${labelWidth}`}>{label}</label>}
      <select value={value} onChange={e => onChange?.(e.target.value)}
        className={`${width} h-[24px] border border-[#C0C7D0] bg-white text-[13px] text-[#1A1A2E] px-1 focus:outline-none focus:border-[#2E75B6]`}>
        {options.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
      </select>
    </div>
  );
}

// ─── Box → GroupBox ──────────────────────────────────────────────────────────
interface BoxProps { title: string; children: React.ReactNode; className?: string; }
export function Box({ title, children, className = "" }: BoxProps) {
  return (
    <fieldset className={`border border-[#C0C7D0] bg-white ${className}`}>
      <legend className="text-[12px] text-[#6B7280] px-2 ml-2 font-semibold tracking-wide uppercase">{title}</legend>
      <div className="px-3 pb-3 pt-1">{children}</div>
    </fieldset>
  );
}

// ─── Grid → DataGridView ──────────────────────────────────────────────────────
interface Col { key: string; header: string; width?: string; }
interface GridProps {
  cols: Col[];
  rows: Record<string, React.ReactNode>[];
  selected?: number;
  onSelect?: (i: number) => void;
  height?: string;
  onDblClick?: (i: number) => void;
}
export function Grid({ cols, rows, selected, onSelect, height = "h-44", onDblClick }: GridProps) {
  return (
    <div className={`border border-[#C0C7D0] overflow-auto ${height} bg-white`}>
      <table className="w-full border-collapse text-[13px]" style={{ tableLayout: "fixed" }}>
        <thead className="sticky top-0 z-10">
          <tr className="bg-[#EEF2F7] border-b border-[#C0C7D0]">
            {cols.map(c => (
              <th key={c.key} style={{ width: c.width }} className="border-r border-[#C0C7D0] px-2 py-[3px] text-left text-[12px] text-[#374151] font-semibold whitespace-nowrap overflow-hidden text-ellipsis">
                {c.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.length === 0 && (
            <tr><td colSpan={cols.length} className="text-center text-[#9CA3AF] text-[13px] py-4">Нет данных</td></tr>
          )}
          {rows.map((row, i) => (
            <tr key={i} onClick={() => onSelect?.(i)} onDoubleClick={() => onDblClick?.(i)}
              className={`border-b border-[#F0F0F0] cursor-pointer ${selected === i ? "bg-[#2E75B6] text-white" : i % 2 === 0 ? "bg-white hover:bg-[#F0F5FF]" : "bg-[#F8F9FA] hover:bg-[#F0F5FF]"}`}>
              {cols.map(c => (
                <td key={c.key} className="px-2 py-[2px] border-r border-[#F0F0F0] whitespace-nowrap overflow-hidden text-ellipsis">{row[c.key]}</td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

// ─── Badge ───────────────────────────────────────────────────────────────────
const BADGE_COLORS: Record<string, string> = {
  "Новый":             "bg-[#DBEAFE] text-[#1D4ED8]",
  "Подтверждён":       "bg-[#FEF9C3] text-[#854D0E]",
  "Готов к отгрузке":  "bg-[#FEF3C7] text-[#92400E]",
  "Отгружен":          "bg-[#DCFCE7] text-[#15803D]",
  "Принят на склад":   "bg-[#DCFCE7] text-[#15803D]",
  "Оформлен":          "bg-[#DBEAFE] text-[#1D4ED8]",
  "Исполнен":          "bg-[#DCFCE7] text-[#15803D]",
  "Отменён":           "bg-[#FEE2E2] text-[#DC2626]",
  "Покупатель":        "bg-[#DBEAFE] text-[#1D4ED8]",
  "Поставщик":         "bg-[#FEF3C7] text-[#92400E]",
  "Оба":               "bg-[#F3E8FF] text-[#7C3AED]",
};
export function Badge({ text }: { text: string }) {
  const cls = BADGE_COLORS[text] ?? "bg-[#F3F4F6] text-[#374151]";
  return <span className={`inline-block px-2 py-[1px] text-[11px] font-semibold ${cls}`}>{text}</span>;
}

// ─── Modal ────────────────────────────────────────────────────────────────────
interface ModalProps { title: string; children: React.ReactNode; onClose: () => void; width?: number; }
export function Modal({ title, children, onClose, width = 460 }: ModalProps) {
  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-[200]" onClick={onClose}>
      <div style={{ width, maxHeight: "90vh" }} className="flex flex-col bg-[#F5F6F8] border border-[#9CA3AF] shadow-[4px_6px_16px_rgba(0,0,0,0.35)] overflow-hidden" onClick={e => e.stopPropagation()}>
        {/* Title bar */}
        <div className="h-[26px] bg-[#1E3558] flex items-center justify-between px-3 shrink-0">
          <span className="text-white text-[13px] font-semibold truncate">{title}</span>
          <button onClick={onClose} className="text-white opacity-80 hover:opacity-100 text-[16px] leading-none">✕</button>
        </div>
        <div className="overflow-auto">{children}</div>
      </div>
    </div>
  );
}

// ─── Alert ────────────────────────────────────────────────────────────────────
type AlertType = "info" | "warn" | "success" | "error";
export function Alert({ type, children }: { type: AlertType; children: React.ReactNode }) {
  const styles: Record<AlertType, string> = {
    info:    "bg-[#EFF6FF] border-[#BFDBFE] text-[#1D4ED8]",
    warn:    "bg-[#FFFBEB] border-[#FDE68A] text-[#92400E]",
    success: "bg-[#F0FDF4] border-[#BBF7D0] text-[#15803D]",
    error:   "bg-[#FEF2F2] border-[#FECACA] text-[#DC2626]",
  };
  const icons: Record<AlertType, string> = { info: "ℹ", warn: "⚠", success: "✔", error: "✕" };
  return (
    <div className={`border px-3 py-2 text-[13px] flex items-start gap-2 ${styles[type]}`}>
      <span className="shrink-0">{icons[type]}</span><span>{children}</span>
    </div>
  );
}

// ─── Divider ─────────────────────────────────────────────────────────────────
export function Divider() { return <div className="h-px bg-[#E5E7EB] my-2" />; }

// ─── SectionHeader ────────────────────────────────────────────────────────────
export function SectionHeader({ title, children }: { title: string; children?: React.ReactNode }) {
  return (
    <div className="flex items-center gap-2 mb-2">
      <h2 className="text-[14px] font-semibold text-[#1E3558]">{title}</h2>
      <div className="h-px flex-1 bg-[#E5E7EB]" />
      {children}
    </div>
  );
}
