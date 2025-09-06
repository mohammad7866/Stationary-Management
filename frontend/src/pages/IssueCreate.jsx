import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Requests, Items, Issues } from "../lib/api";

export default function IssueCreate() {
  const { requestId } = useParams();
  const nav = useNavigate();

  const [req, setReq] = useState(null);
  const [itemId, setItemId] = useState(null);
  const [qty, setQty] = useState(0);
  const [msg, setMsg] = useState("");
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    (async () => {
      try {
        const r = await Requests.get(requestId);
        setReq(r);
        setQty(r.quantity);
        // Resolve itemId by name (exact match preferred)
        const res = await Items.list({ q: r.itemName });
        const items = Array.isArray(res?.data) ? res.data : (res || []);
        const match = items.find(i => i.name?.toLowerCase() === r.itemName.toLowerCase()) || items[0];
        if (!match) throw new Error(`Item '${r.itemName}' not found`);
        setItemId(match.id);
      } catch (e) { setMsg(e.message); }
    })();
  }, [requestId]);

  async function createIssue() {
    setBusy(true); setMsg("");
    try {
      const payload = {
        requestId: Number(requestId),
        lines: [{ itemId: Number(itemId), quantity: Number(qty) }],
        idempotencyKey: `req-${requestId}-once`
      };
      const issue = await Issues.create(payload);
      setMsg(`Issue #${issue.id} created.`);
      nav(`/issue/details/${issue.id}`);
    } catch (e) { setMsg(e.message); } 
    finally { setBusy(false); }
  }

  if (!req) return <div style={{padding:20}}>Loading… {msg && <span style={{color:"crimson"}}>{msg}</span>}</div>;

  return (
    <div style={{padding:20, maxWidth:600, margin:"0 auto"}}>
      <h2>Create Issue for Request #{req.id}</h2>
      <div style={{border:"1px solid #ddd", borderRadius:8, padding:12, margin:"12px 0"}}>
        <div><b>Item:</b> {req.itemName}</div>
        <div><b>Office:</b> {req.office}</div>
        <div style={{display:"flex", gap:8, alignItems:"center", marginTop:8}}>
          <label>Quantity</label>
          <input type="number" min={1} value={qty} onChange={e=>setQty(e.target.value)} style={{width:100, padding:6}}/>
        </div>
      </div>
      <div style={{display:"flex", gap:8}}>
        <button disabled={busy || !itemId} onClick={createIssue} style={{padding:"8px 12px"}}>{busy ? "Creating…" : "Create Issue"}</button>
        <button onClick={()=>nav(-1)} style={{padding:"8px 12px"}}>Cancel</button>
      </div>
      {msg && <div style={{marginTop:10, color:"#555"}}>{msg}</div>}
    </div>
  );
}
