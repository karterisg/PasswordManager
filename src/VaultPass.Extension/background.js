const API = "http://localhost:5243";

chrome.runtime.onMessage.addListener((msg, _sender, sendResponse) => {
  if      (msg.type === "LOGIN")           handleLogin(msg, sendResponse);
  else if (msg.type === "GET_ITEMS")       handleGetItems(sendResponse);
  else if (msg.type === "GET_MATCHING")    handleGetMatching(msg, sendResponse);
  else if (msg.type === "SAVE_ITEM")       handleSaveItem(msg, sendResponse);
  else if (msg.type === "GET_TOKEN")       handleGetToken(sendResponse);
  return true;
});

async function handleLogin({ email, password }, sendResponse) {
  try {
    const hash = await sha256(password);
    const res  = await fetch(`${API}/api/auth/login`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, masterPasswordHash: hash })
    });
    if (!res.ok) { sendResponse({ ok: false }); return; }
    const token = (await res.text()).replace(/"/g, "");
    await chrome.storage.local.set({ token, passwordHash: hash });
    sendResponse({ ok: true });
  } catch (e) { sendResponse({ ok: false, error: e.message }); }
}

async function handleGetItems(sendResponse) {
  try {
    const { token } = await chrome.storage.local.get("token");
    if (!token) { sendResponse({ ok: false }); return; }
    const vaultId = parseVaultId(token);
    const res = await fetch(`${API}/api/vaultitems?vaultId=${vaultId}`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    const items = await res.json();
    sendResponse({ ok: true, items });
  } catch (e) { sendResponse({ ok: false, error: e.message }); }
}

// Returns items whose websiteUrl origin matches the given origin
async function handleGetMatching({ origin }, sendResponse) {
  try {
    const { token } = await chrome.storage.local.get("token");
    if (!token) { sendResponse({ ok: false, items: [] }); return; }
    const vaultId = parseVaultId(token);
    const res = await fetch(`${API}/api/vaultitems?vaultId=${vaultId}`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    const items = await res.json();
    const matching = items.filter(item => {
      try { return new URL(item.websiteUrl).origin === origin; } catch { return false; }
    });
    sendResponse({ ok: true, items: matching });
  } catch (e) { sendResponse({ ok: false, items: [] }); }
}

async function handleSaveItem({ name, url, username, password }, sendResponse) {
  try {
    const { token, passwordHash } = await chrome.storage.local.get(["token", "passwordHash"]);
    if (!token) { sendResponse({ ok: false }); return; }
    const vaultId  = parseVaultId(token);
    const encKey   = await deriveKey(passwordHash, vaultId);
    // Encrypt both username and password together as JSON
    const payload  = JSON.stringify({ username: username || "", password });
    const encrypted = await encryptAesGcm(encKey, payload);
    const res = await fetch(`${API}/api/vaultitems`, {
      method: "POST",
      headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
      body: JSON.stringify({ vaultId, name, type: 1, encryptedData: encrypted, websiteUrl: url })
    });
    sendResponse({ ok: res.ok });
  } catch (e) { sendResponse({ ok: false, error: e.message }); }
}

async function handleGetToken(sendResponse) {
  const { token } = await chrome.storage.local.get("token");
  sendResponse({ token: token || null });
}

function parseVaultId(token) {
  const payload = token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/");
  const json    = JSON.parse(atob(payload));
  return json.vaultId;
}

async function sha256(str) {
  const buf = await crypto.subtle.digest("SHA-256", new TextEncoder().encode(str));
  return Array.from(new Uint8Array(buf)).map(b => b.toString(16).padStart(2, "0")).join("");
}

async function deriveKey(passwordHash, vaultId) {
  const raw  = new TextEncoder().encode(passwordHash + vaultId);
  const hash = await crypto.subtle.digest("SHA-256", raw);
  return crypto.subtle.importKey("raw", hash, { name: "AES-GCM" }, false, ["encrypt", "decrypt"]);
}

async function encryptAesGcm(key, plaintext) {
  const iv  = crypto.getRandomValues(new Uint8Array(12));
  const enc = await crypto.subtle.encrypt({ name: "AES-GCM", iv }, key, new TextEncoder().encode(plaintext));
  const out = new Uint8Array(12 + enc.byteLength);
  out.set(iv);
  out.set(new Uint8Array(enc), 12);
  return btoa(String.fromCharCode(...out));
}
