// frontend/src/pages/Login.jsx
import React, { useState } from "react";
import { useAuth } from "../auth/AuthContext";
import { useNavigate } from "react-router-dom";

export default function LoginPage() {
  const { login } = useAuth();
  const nav = useNavigate();
  const [u, setU] = useState("super@demo.local"); // demo defaults
  const [p, setP] = useState("P@ssw0rd!");
  const [err, setErr] = useState("");

  async function onSubmit(e) {
    e.preventDefault();
    setErr("");
    try {
      await login(u, p);
      nav("/");
    } catch (e) { setErr(e.message || "Login failed"); }
  }

  return (
    <div style={{maxWidth:360, margin:"60px auto"}}>
      <h1>Login</h1>
      {err && <div style={{color:"crimson"}}>{err}</div>}
      <form onSubmit={onSubmit}>
        <div style={{margin:"8px 0"}}>
          <label>Username / Email</label>
          <input value={u} onChange={e=>setU(e.target.value)} style={{width:"100%", padding:8}} />
        </div>
        <div style={{margin:"8px 0"}}>
          <label>Password</label>
          <input type="password" value={p} onChange={e=>setP(e.target.value)} style={{width:"100%", padding:8}} />
        </div>
        <button style={{padding:"8px 12px"}}>Sign in</button>
      </form>
      <p style={{marginTop:12, color:"#666"}}>
        Demo users: super@demo.local, admin@demo.local, user@demo.local (password: P@ssw0rd!)
      </p>
    </div>
  );
}
