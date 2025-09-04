import React, { useState } from "react";
import { useAuth } from "../auth/AuthContext";
import { useNavigate } from "react-router-dom";

export default function LoginPage() {
  const { login } = useAuth();
  const nav = useNavigate();
  const [u, setU] = useState("super@demo.local"); // demo user
  const [p, setP] = useState("P@ssw0rd!");
  const [err, setErr] = useState("");
  const [loading, setLoading] = useState(false);
  const API = import.meta.env.VITE_API_BASE_URL || "";

  async function onSubmit(e) {
    e.preventDefault();
    setErr("");
    if (!API) { setErr("VITE_API_BASE_URL is not set."); return; }
    setLoading(true);
    try {
      const r = await fetch(`${API}/api/Auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username: u, password: p })
      });
      const text = await r.text();
      let data = null;
      try { data = text ? JSON.parse(text) : null; } catch {}
      if (!r.ok) {
        const msg = data?.title || data?.error || data?.message || text || `HTTP ${r.status}`;
        throw new Error(msg);
      }
      const { token, username, roles } = data || {};
      if (!token) throw new Error("No token returned.");
      login(token, username || u, roles || []);
      nav("/");
    } catch (e2) {
      setErr(e2.message || "Login failed");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div style={{maxWidth:360, margin:"60px auto"}}>
      <h1>Login</h1>
      {err && <div style={{color:"crimson", marginBottom:8}}>{err}</div>}
      <form onSubmit={onSubmit}>
        <div style={{margin:"8px 0"}}>
          <label>Username / Email</label>
          <input value={u} onChange={e=>setU(e.target.value)} style={{width:"100%", padding:8}} />
        </div>
        <div style={{margin:"8px 0"}}>
          <label>Password</label>
          <input type="password" value={p} onChange={e=>setP(e.target.value)} style={{width:"100%", padding:8}} />
        </div>
        <button type="submit" disabled={loading} style={{padding:"8px 12px"}}>
          {loading ? "Signing in…" : "Sign in"}
        </button>
      </form>
      <p style={{marginTop:12, color:"#666"}}>
        Demo users: super@demo.local, admin@demo.local, user@demo.local (pwd: P@ssw0rd!)
      </p>
    </div>
  );
}
