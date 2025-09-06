import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Issues, Returns } from "../lib/api";

export default function ReturnCreate() {
  const { issueId } = useParams();
  const nav = useNavigate();
  const [issue, setIssue] = useState(null);
  const [lines, setLines] = useState([]);
  const [msg, setMsg] = useState("");
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    (async () => {
      try {
        const data = await Issues.get(issueId);
        setIssue(data);
        setLines((data.lines || []).map(l => ({ itemId: l.itemId, quantity: l.quantity })));
      } catch (e) { setMsg(e.message); }
    })();
  }, [issueId]);

  function setQty(idx, val) {
    const v = Math.max(0, Number(val || 0));
    setLines(ls => ls.map((l,i) => i===idx ? { ...l, quantity: v } : l));
  }

  async function submit() {
    setBusy(true); setMsg("");
    try {
      const payload = { issueId: Number(issueId), lines: lines.filter(l => l.quantity > 0) };
      const ret = await Returns.create(payload);
      setMsg(`Return #${ret.id} created`);
      nav(`/issue/details/${issueId}`);
    } catch (e) { setMsg(e.message); }
    finally { setBusy(false); }
  }

  if (!issue) return <div style={{padding:20}}>Loading… {msg && <span style={{color:"crimson"}}>{msg}</span>}</div>;

  return (
    <div style={{padding:20, maxWidth:600, margin:"0 auto"}}>
      <h2>Return for Issue #{issue.id}</h2>
      <div style={{border:"1px solid #ddd", borderRadius:8, padding:12, margin:"12px 0"}}>
        {(lines || []).map((l, idx) => (
          <div key={idx} style={{display:"flex", alignItems:"center", gap:8, marginBottom:8}}>
            <div style={{width:120}}>Item {l.itemId}</div>
            <input type="number" min={0} value={l.quantity} onChange={e=>setQty(idx, e.target.value)} style={{width:100, padding:6}}/>
          </div>
        ))}
      </div>
      <div style={{display:"flex", gap:8}}>
        <button disabled={busy} onClick={submit} style={{padding:"8px 12px"}}>{busy ? "Submitting…" : "Create Return"}</button>
        <button onClick={()=>nav(-1)} style={{padding:"8px 12px"}}>Cancel</button>
      </div>
      {msg && <div style={{marginTop:10, color:"#555"}}>{msg}</div>}
    </div>
  );
}
