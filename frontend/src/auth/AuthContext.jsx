// src/auth/AuthContext.jsx
import React, { createContext, useContext, useEffect, useMemo, useState } from "react";
import { jwtDecode } from "jwt-decode"; // v4+ uses named export

// ---- Helpers ---------------------------------------------------------------

function extractRoles(decoded) {
  if (!decoded || typeof decoded !== "object") return [];

  // Common places roles can live in JWTs
  const msClaim = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
  const altClaim = "roles";

  if (Array.isArray(decoded[altClaim])) return decoded[altClaim];
  if (Array.isArray(decoded[msClaim])) return decoded[msClaim];

  if (typeof decoded.role === "string") return [decoded.role];
  if (typeof decoded[msClaim] === "string") return [decoded[msClaim]];

  return [];
}

function decodeToken(token) {
  try {
    const decoded = jwtDecode(token);
    const roles = extractRoles(decoded);

    // Pull a couple of common identity fields if you want them in UI
    const user = {
      id: decoded.sub || decoded.nameid || decoded.userId || null,
      name: decoded.name || decoded.given_name || null,
      email: decoded.email || null,
    };

    // JWT exp is seconds since epoch
    const expMs = decoded.exp ? decoded.exp * 1000 : null;
    const isExpired = expMs ? Date.now() >= expMs : false;

    return { decoded, roles, user, isExpired };
  } catch {
    return { decoded: null, roles: [], user: {}, isExpired: true };
  }
}

// ---- Context ---------------------------------------------------------------

const AuthContext = createContext({
  token: null,
  roles: [],
  user: {},
  isAuthed: false,
  login: (_t) => {},
  logout: () => {},
});

export default function AuthProvider({ children }) {
  const [token, setToken] = useState(() => {
    try {
      return localStorage.getItem("token");
    } catch {
      return null;
    }
  });

  // Keep localStorage in sync when token changes
  useEffect(() => {
    try {
      if (token) localStorage.setItem("token", token);
      else localStorage.removeItem("token");
    } catch {
      // ignore storage errors
    }
  }, [token]);

  // Optional: respond to other tabs logging in/out
  useEffect(() => {
    const onStorage = (e) => {
      if (e.key === "token") setToken(e.newValue);
    };
    window.addEventListener("storage", onStorage);
    return () => window.removeEventListener("storage", onStorage);
  }, []);

  const { roles, user, isExpired } = useMemo(() => {
    if (!token) return { roles: [], user: {}, isExpired: true };
    return decodeToken(token);
  }, [token]);

  const isAuthed = !!token && !isExpired;

  const login = (newToken) => setToken(newToken);
  const logout = () => setToken(null);

  const value = useMemo(
    () => ({ token, roles, user, isAuthed, login, logout }),
    [token, roles, user, isAuthed]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  return useContext(AuthContext);
}
