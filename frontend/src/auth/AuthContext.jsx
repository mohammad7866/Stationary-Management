import React, { createContext, useContext, useEffect, useMemo, useState } from "react";
import { jwtDecode } from "jwt-decode";
import { setAuthToken } from "../lib/api"; // ⬅️ NEW: in-memory token setter

// ---- Helpers ---------------------------------------------------------------

function extractRoles(decoded) {
  if (!decoded || typeof decoded !== "object") return [];

  // Common claim keys for roles
  const msClaim = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
  const altClaim = "roles";

  if (Array.isArray(decoded[altClaim])) return decoded[altClaim];
  if (Array.isArray(decoded[msClaim])) return decoded[msClaim];

  // also handle a single 'role' value or array
  if (Array.isArray(decoded.role)) return decoded.role;
  if (typeof decoded.role === "string") return [decoded.role];
  if (typeof decoded[msClaim] === "string") return [decoded[msClaim]];

  return [];
}

function decodeToken(token) {
  try {
    const decoded = jwtDecode(token);
    const roles = extractRoles(decoded);

    const user = {
      id: decoded.sub || decoded.nameid || decoded.userId || null,
      name: decoded.name || decoded.given_name || null,
      email: decoded.email || null,
    };

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

  // Keep localStorage AND in-memory token in sync whenever token changes
  useEffect(() => {
    try {
      if (token) {
        localStorage.setItem("token", token);
        setAuthToken(token);       // ⬅️ keep api.js in-memory token updated
      } else {
        localStorage.removeItem("token");
        setAuthToken("");          // ⬅️ clear in-memory token
      }
    } catch {
      // ignore storage errors
      setAuthToken(token || "");
    }
  }, [token]);

  // Optional: respond to other tabs logging in/out
  useEffect(() => {
    const onStorage = (e) => {
      if (e.key === "token") {
        setToken(e.newValue);
        setAuthToken(e.newValue || ""); // ⬅️ mirror across tabs immediately
      }
    };
    window.addEventListener("storage", onStorage);
    return () => window.removeEventListener("storage", onStorage);
  }, []);

  const { roles, user, isExpired } = useMemo(() => {
    if (!token) return { roles: [], user: {}, isExpired: true };
    return decodeToken(token);
  }, [token]);

  const isAuthed = !!token && !isExpired;

  // ⬅️ IMPORTANT: set the in-memory token synchronously on login/logout
  const login = (newToken) => {
    setAuthToken(newToken || "");
    setToken(newToken || null);
  };
  const logout = () => {
    setAuthToken("");
    setToken(null);
  };

  const value = useMemo(
    () => ({ token, roles, user, isAuthed, login, logout }),
    [token, roles, user, isAuthed]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  return useContext(AuthContext);
}
