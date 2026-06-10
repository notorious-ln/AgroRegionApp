import { useState } from "react";
import { LoginForm } from "./components/LoginForm";
import { MainWindow } from "./components/MainWindow";

export default function App() {
  const [user, setUser] = useState<{ role: string; name: string } | null>(null);

  return (
    <div className="size-full" style={{ fontFamily: "Segoe UI, Tahoma, sans-serif" }}>
      {!user
        ? <LoginForm onLogin={(role, name) => setUser({ role, name })} />
        : <MainWindow role={user.role} userName={user.name} onLogout={() => setUser(null)} />
      }
    </div>
  );
}
