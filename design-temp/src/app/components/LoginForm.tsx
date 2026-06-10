import { useState } from "react";
import { Btn, Alert } from "./ui";

const USERS = [
  { login: "manager1",     password: "1234", role: "Менеджер по продажам",   name: "Иванов И.И.",   blocked: false },
  { login: "warehouse1",   password: "1234", role: "Руководитель склада",    name: "Петров П.П.",   blocked: false },
  { login: "director",     password: "1234", role: "Генеральный директор",   name: "Сидоров С.С.", blocked: false },
  { login: "comdirector",  password: "1234", role: "Коммерческий директор",  name: "Козлов К.К.",  blocked: false },
  { login: "blocked_user", password: "1234", role: "Менеджер по продажам",   name: "Блок Б.Б.",    blocked: true  },
];

interface LoginFormProps {
  onLogin: (role: string, name: string) => void;
}

export function LoginForm({ onLogin }: LoginFormProps) {
  const [login, setLogin] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<"wrong" | "blocked" | null>(null);

  function handleSubmit() {
    const user = USERS.find(u => u.login === login && u.password === password);
    if (!user) { setError("wrong"); setPassword(""); return; }
    if (user.blocked) { setError("blocked"); return; }
    setError(null);
    onLogin(user.role, user.name);
  }

  return (
    <div className="size-full flex items-center justify-center" style={{ background: "linear-gradient(135deg,#1E3558 0%,#2E75B6 100%)", fontFamily: "Segoe UI, Tahoma, sans-serif" }}>
      {/* App window chrome */}
      <div className="w-[360px] bg-[#F5F6F8] border border-[#9CA3AF] shadow-[6px_8px_24px_rgba(0,0,0,0.45)]">
        {/* Title bar */}
        <div className="h-[28px] bg-[#1E3558] flex items-center justify-between px-3">
          <div className="flex items-center gap-2">
            <span className="text-[14px]">🌾</span>
            <span className="text-white text-[13px] font-semibold">АИС «Агро-Торг» — Вход в систему</span>
          </div>
          <div className="flex gap-1">
            {["─","□","✕"].map((s,i) => (
              <button key={i} className={`w-[16px] h-[16px] text-[10px] flex items-center justify-center border border-[#4A6A90] text-white ${i===2 ? "bg-[#C42B1C] hover:bg-[#E81123]" : "hover:bg-[#2E75B6]"}`}>{s}</button>
            ))}
          </div>
        </div>

        {/* Body */}
        <div className="p-6 flex flex-col gap-4">
          {/* Logo */}
          <div className="text-center pb-2 border-b border-[#E5E7EB]">
            <div className="text-[22px] font-semibold text-[#1E3558] tracking-tight">АИС «Агро-Торг»</div>
            <div className="text-[12px] text-[#6B7280] mt-1">Система управления коммерческой деятельностью</div>
            <div className="text-[11px] text-[#9CA3AF] mt-1">Агропредприятие «ПолеПрод»</div>
          </div>

          {/* Fields */}
          <div className="flex flex-col gap-3">
            <div className="flex items-center gap-3">
              <label className="text-[13px] text-[#374151] w-[60px]">Логин:</label>
              <input type="text" value={login} onChange={e => setLogin(e.target.value)}
                className="flex-1 h-[26px] border border-[#C0C7D0] bg-white text-[13px] px-2 focus:outline-none focus:border-[#2E75B6]" />
            </div>
            <div className="flex items-center gap-3">
              <label className="text-[13px] text-[#374151] w-[60px]">Пароль:</label>
              <input type="password" value={password} onChange={e => setPassword(e.target.value)}
                onKeyDown={e => e.key === "Enter" && handleSubmit()}
                className="flex-1 h-[26px] border border-[#C0C7D0] bg-white text-[13px] px-2 focus:outline-none focus:border-[#2E75B6]" />
            </div>
          </div>

          {/* Errors */}
          {error === "wrong"   && <Alert type="error">Ошибка авторизации. Неверный логин или пароль.</Alert>}
          {error === "blocked" && <Alert type="error">Доступ ограничен. Данный аккаунт заблокирован.</Alert>}

          {/* Buttons */}
          <div className="flex justify-center gap-3 pt-1">
            <Btn variant="primary" onClick={handleSubmit} icon="→">Войти</Btn>
            <Btn onClick={() => { setLogin(""); setPassword(""); setError(null); }}>Очистить</Btn>
          </div>

          {/* Hint */}
          <div className="bg-[#EFF6FF] border border-[#BFDBFE] p-2 text-[11px] text-[#3B82F6] text-center">
            Тест: manager1 · warehouse1 · director · comdirector / пароль 1234
          </div>
          <div className="text-[11px] text-[#9CA3AF] text-center">
            Для получения учётных данных обратитесь к администратору системы
          </div>
        </div>

        {/* Status bar */}
        <div className="h-[22px] bg-[#E5E7EB] border-t border-[#C0C7D0] flex items-center px-3 gap-3 text-[11px] text-[#6B7280]">
          <span className="w-2 h-2 rounded-full bg-[#16A34A] inline-block" />
          <span>Подключено к MS SQL Server</span>
          <span className="ml-auto">{new Date().toLocaleDateString("ru-RU")}</span>
        </div>
      </div>
    </div>
  );
}
