const API = "http://localhost:5243";

chrome.runtime.onMessage.addListener((msg, _sender, sendResponse) => {
  if      (msg.type === "LOGIN")                handleLogin(msg, sendResponse);
  else if (msg.type === "GET_ITEMS")            handleGetItems(sendResponse);
  else if (msg.type === "GET_MATCHING")         handleGetMatching(msg, sendResponse);
  else if (msg.type === "SAVE_ITEM")            handleSaveItem(msg, sendResponse);
  else if (msg.type === "GET_TOKEN")            handleGetToken(sendResponse);
  else if (msg.type === "DECRYPT")              handleDecrypt(msg, sendResponse);
  else if (msg.type === "CREDENTIALS_CAPTURED") handleCredentialsCaptured(msg, sendResponse);
  else if (msg.type === "GET_PENDING_SAVE")     handleGetPendingSave(sendResponse);
  else if (msg.type === "DISMISS_PENDING_SAVE") handleDismissPendingSave(sendResponse);
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

    const [itemsRes, templatesRes] = await Promise.all([
      fetch(`${API}/api/vaultitems?vaultId=${vaultId}`, { headers: { Authorization: `Bearer ${token}` } }),
      fetch(`${API}/api/itemtemplates?vaultId=${vaultId}`, { headers: { Authorization: `Bearer ${token}` } })
    ]);

    const items     = itemsRes.ok     ? await itemsRes.json()     : [];
    const templates = templatesRes.ok ? await templatesRes.json() : [];
    sendResponse({ ok: true, items, templates });
  } catch (e) { sendResponse({ ok: false, error: e.message }); }
}

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
    const vaultId   = parseVaultId(token);
    const encKey    = await deriveKey(passwordHash, vaultId);
    const payload   = JSON.stringify({ username: username || "", password });
    const encrypted = await encryptAesGcm(encKey, payload);
    const res = await fetch(`${API}/api/vaultitems`, {
      method: "POST",
      headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
      body: JSON.stringify({ vaultId, name, type: 1, encryptedData: encrypted, websiteUrl: url })
    });
    sendResponse({ ok: res.ok });
  } catch (e) { sendResponse({ ok: false, error: e.message }); }
}

async function handleDecrypt({ encryptedData }, sendResponse) {
  try {
    const { passwordHash, token } = await chrome.storage.local.get(["passwordHash", "token"]);
    if (!passwordHash || !token) { sendResponse({ ok: false }); return; }
    const vaultId  = parseVaultId(token);
    const encKey   = await deriveKey(passwordHash, vaultId);
    const plain    = await decryptAesGcm(encKey, encryptedData);
    sendResponse({ ok: true, plaintext: plain });
  } catch (e) { sendResponse({ ok: false, error: e.message }); }
}

async function handleGetToken(sendResponse) {
  const { token } = await chrome.storage.local.get("token");
  sendResponse({ token: token || null });
}

async function handleCredentialsCaptured({ email, password, origin, title }, sendResponse) {
  const { token } = await chrome.storage.local.get("token");
  if (!token) { sendResponse({ ok: false }); return; }

  const hostname = (() => { try { return new URL(origin).hostname; } catch { return origin; } })();
  const notifId  = "vp-save-" + Date.now();

  await chrome.storage.local.set({ [`notif_${notifId}`]: { email, password, origin, hostname } });

  chrome.notifications.create(notifId, {
    type:             "basic",
    iconUrl:          "icon128.png",
    title:            "Save to VaultPass?",
    message:          `${hostname}  ·  ${email}`,
    buttons:          [{ title: "Save" }, { title: "Dismiss" }],
    requireInteraction: true
  });

  sendResponse({ ok: true });
}

async function handleGetPendingSave(sendResponse) {
  const { pending_save } = await chrome.storage.local.get("pending_save");
  sendResponse({ pending: pending_save || null });
}

async function handleDismissPendingSave(sendResponse) {
  await chrome.storage.local.remove("pending_save");
  sendResponse({ ok: true });
}

chrome.notifications.onButtonClicked.addListener(async (notifId, btnIndex) => {
  const key   = `notif_${notifId}`;
  const store = await chrome.storage.local.get(key);
  const data  = store[key];
  chrome.notifications.clear(notifId);
  await chrome.storage.local.remove(key);
  if (!data || btnIndex !== 0) return;

  const { token, passwordHash } = await chrome.storage.local.get(["token", "passwordHash"]);
  if (!token) return;
  const vaultId   = parseVaultId(token);
  const encKey    = await deriveKey(passwordHash, vaultId);
  const payload   = JSON.stringify({ username: data.email, password: data.password });
  const encrypted = await encryptAesGcm(encKey, payload);

  await fetch(`${API}/api/vaultitems`, {
    method:  "POST",
    headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
    body:    JSON.stringify({ vaultId, name: data.hostname, type: 1, encryptedData: encrypted, websiteUrl: data.origin })
  });
});

chrome.notifications.onClosed.addListener(async notifId => {
  await chrome.storage.local.remove(`notif_${notifId}`);
});

function parseVaultId(token) {
  const payload = token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/");
  return JSON.parse(atob(payload)).vaultId;
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
  out.set(iv); out.set(new Uint8Array(enc), 12);
  return btoa(String.fromCharCode(...out));
}

async function decryptAesGcm(key, b64) {
  const bytes = Uint8Array.from(atob(b64), c => c.charCodeAt(0));
  const iv  = bytes.slice(0, 12);
  const data = bytes.slice(12);
  const dec = await crypto.subtle.decrypt({ name: "AES-GCM", iv }, key, data);
  return new TextDecoder().decode(dec);
}
