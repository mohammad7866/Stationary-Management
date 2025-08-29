// frontend/src/auth/AuthContext.jsx
import React, { createContext, useContext, useEffect, useState } from "react";
import { Auth } from "../lib/api";

const AuthCtx = createContext(null);
export const useAuth = () => useContext(AuthCtx);

export default function AuthProvider({ children }) {
  const [token, setToken] = useState(() => localStorage.getItem("jwt") || "");
  const [username, setUsername] = useState("");
  const [roles, setRoles] = useState([]);

  useEffect(() => {
    if (!token) { setUsername(""); setRoles([]); return; }
    Auth.me(token)
      .then(m => {
        setUsername(m?.username || "");
        setRoles(m?.roles || []);
      })
      .catch(() => {
        localStorage.removeItem("jwt");
        setToken(""); setUsername(""); setRoles([]);
      });
  }, [token]);

  const login = async (username, password) => {
    const res = await Auth.login({ username, password });
    localStorage.setItem("jwt", res.token);
    setToken(res.token);
    setUsername(res.username);
    setRoles(res.roles || []);
  };

  const logout = () => {
    localStorage.removeItem("jwt");
    setToken(""); setUsername(""); setRoles([]);
  };

  return (
    <AuthCtx.Provider value={{ token, username, roles, login, logout }}>
      {children}
    </AuthCtx.Provider>
  );
}
