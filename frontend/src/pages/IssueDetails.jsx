import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Issues } from "../lib/api";

export default function IssueDetails() {
  const { id } = useParams();
  const nav = useNavigate();
  const [issue, setIssue] = useState(null);
  const [msg, setMsg] = useState("");

  useEffect(() => {
    (async () => {
      try { setIssue(await Issues.get(id)); }
      catch (e) { setMsg(e.message); }
    })();
  }, [id]);

  if (!issue) return <div style={{padding:20}}>Loading… {msg && <span style={{color:"crimson"}}>{msg}</span>}</div>;

  return (
    <div style={{padding:20, maxWidth:600, margin:"0 auto"}}>
      <h2>Issue #{issue.id}</h2>
      <div style={{border:"1px solid #ddd", borderRadius:8, padding:12, margin:"12px 0"}}>
        <div><b>Request:</b> {issue.requestId}</div>
        <div><b>Issued At:</b> {new Date(issue.issuedAt).toLocaleString()}</div>
      </div>
      <div style={{border:"1px solid #eee", borderRadius:8, padding:12}}>
        <h3 style={{marginTop:0}}>Lines</h3>
        <ul style={{marginLeft:18}}>
          {(issue.lines || []).map(l => (
            <li key={l.id}>Item {l.itemId} — Qty {l.quantity}</li>
          ))}
        </ul>
      </div>
      <div style={{display:"flex", gap:8, marginTop:12}}>
        <button onClick={()=>nav(`/return/${issue.id}`)} style={{padding:"8px 12px"}}>Create Return</button>
        <button onClick={()=>nav(-1)} style={{padding:"8px 12px"}}>Back</button>
      </div>
      {msg && <div style={{marginTop:10, color:"#555"}}>{msg}</div>}
    </div>
  );
}
