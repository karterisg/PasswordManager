// ── VaultPass content script ───────────────────────────────────────────────
const VP_Z = "2147483647";
let _dropdown    = null;
let _cachedItems = null;
let _cacheTimer  = null;
let _capturedEmail = "", _capturedPassword = "";

injectStyles();

// ── 1. Autofill dropdown on field focus / click ───────────────────────────

document.addEventListener("pointerdown", async e => {
  const el = e.target;
  if (!isCredentialField(el)) { closeDropdown(); return; }
  const items = await getMatchingItems();
  if (!items.length) return;
  showDropdown(el, items);
}, true);

document.addEventListener("focusin", async e => {
  const el = e.target;
  if (!isCredentialField(el)) return;
  const items = await getMatchingItems();
  if (!items.length) return;
  showDropdown(el, items);
}, true);

document.addEventListener("focusout", () => {
  setTimeout(() => { if (!document.querySelector("#vp-dropdown:hover")) closeDropdown(); }, 250);
}, true);

document.addEventListener("keydown", e => { if (e.key === "Escape") closeDropdown(); }, true);

function isCredentialField(el) {
  if (!el || el.tagName !== "INPUT") return false;
  if (el.type === "password") return true;
  if (el.type === "email")    return true;
  if (el.type === "text" && /user|email|login|username|mail/i.test(el.name + el.id + el.placeholder + (el.autocomplete||""))) return true;
  return false;
}

function showDropdown(anchor, items) {
  closeDropdown();
  const rect = anchor.getBoundingClientRect();
  const d = document.createElement("div");
  d.id = "vp-dropdown";
  d.className = "vp-ui";
  d.style.cssText = `
    position:fixed !important;
    left:${rect.left}px !important;
    top:${rect.bottom + 4}px !important;
    min-width:${Math.max(rect.width, 260)}px !important;
    z-index:${VP_Z} !important;
  `;
  d.innerHTML = `
    <div class="vp-dd-header">
      <img src="${chrome.runtime.getURL("icon16.png")}" class="vp-dd-logo"> VaultPass
    </div>
    ${items.map(item => `
      <div class="vp-dd-row" data-enc="${esc(item.encryptedData)}">
        <span class="vp-dd-icon">${esc(item.icon || "🔑")}</span>
        <div class="vp-dd-info">
          <div class="vp-dd-name">${esc(item.name)}</div>
          ${item.websiteUrl ? `<div class="vp-dd-url">${esc(item.websiteUrl)}</div>` : ""}
        </div>
        <span class="vp-dd-fill">Fill</span>
      </div>`).join("")}
  `;
  d.querySelectorAll(".vp-dd-row").forEach(row => {
    row.addEventListener("mouseenter", () => row.classList.add("hover"));
    row.addEventListener("mouseleave", () => row.classList.remove("hover"));
    row.addEventListener("mousedown", ev => {
      ev.preventDefault(); ev.stopPropagation();
      fillItem(row.dataset.enc);
      closeDropdown();
    });
  });
  document.documentElement.appendChild(d);
  _dropdown = d;
}

function closeDropdown() { _dropdown?.remove(); _dropdown = null; }

async function getMatchingItems() {
  if (_cachedItems !== null) return _cachedItems;
  return new Promise(resolve => {
    try {
      chrome.runtime.sendMessage({ type: "GET_MATCHING", origin: location.origin }, res => {
        if (chrome.runtime.lastError) { resolve([]); return; }
        _cachedItems = res?.items || [];
        clearTimeout(_cacheTimer);
        _cacheTimer = setTimeout(() => { _cachedItems = null; }, 10000);
        resolve(_cachedItems);
      });
    } catch { resolve([]); }
  });
}

function fillItem(encryptedData) {
  chrome.runtime.sendMessage({ type: "DECRYPT", encryptedData }, res => {
    if (!res?.ok) return;
    let username = "", password = "";
    try {
      const obj = JSON.parse(res.plaintext);
      username = obj.username ?? obj.Username ?? obj.email ?? obj.Email ?? "";
      password = obj.password ?? obj.Password ?? Object.values(obj).pop() ?? "";
    } catch { password = res.plaintext; }

    const inputs    = Array.from(document.querySelectorAll("input"));
    const pwdField  = inputs.find(i => i.type === "password");
    const userField = inputs.find(i => i.type === "email" || (i.type === "text" && isUsernameField(i)));
    if (userField && username) setVal(userField, username);
    if (pwdField  && password)  setVal(pwdField, password);
  });
}

// ── 2. Form submit → floating save banner ────────────────────────────────

document.addEventListener("input", e => {
  const el = e.target;
  if (!el || el.tagName !== "INPUT") return;
  if (el.type === "password") _capturedPassword = el.value;
  if (el.type === "email" || (el.type === "text" && isUsernameField(el))) _capturedEmail = el.value;
}, true);

document.addEventListener("submit", async e => {
  const form = e.target;
  if (!form || form.tagName !== "FORM") return;
  const inputs    = Array.from(form.querySelectorAll("input"));
  const pwdField  = inputs.find(i => i.type === "password");
  const userField = inputs.find(i => i.type === "email" || (i.type === "text" && isUsernameField(i)));
  const email     = userField?.value || _capturedEmail;
  const password  = pwdField?.value  || _capturedPassword;
  if (!email || !password) return;

  let hasToken = false;
  try {
    const s = await chrome.storage.local.get("token");
    hasToken = !!s.token;
  } catch {}
  if (!hasToken) return;

  showSaveBanner(email, password, location.origin);
}, true);

// Also detect URL change (SPA logins that don't submit a form)
let _lastUrl = location.href;
new MutationObserver(() => {
  if (location.href !== _lastUrl) {
    _lastUrl = location.href;
    // If we had captured credentials before navigation, show banner
    if (_capturedEmail && _capturedPassword) {
      setTimeout(() => showSaveBanner(_capturedEmail, _capturedPassword, location.origin), 600);
      _capturedEmail = _capturedPassword = "";
    }
  }
}).observe(document.body, { childList: true, subtree: true });

function showSaveBanner(email, password, origin) {
  document.getElementById("vp-save-banner")?.remove();
  const hostname = (() => { try { return new URL(origin).hostname; } catch { return origin; } })();

  const b = document.createElement("div");
  b.id = "vp-save-banner";
  b.className = "vp-ui";
  b.innerHTML = `
    <div class="vp-banner-top">
      <img src="${chrome.runtime.getURL("icon16.png")}" style="width:16px;height:16px;flex-shrink:0">
      <div style="flex:1">
        <div class="vp-banner-title">Save to VaultPass?</div>
        <div class="vp-banner-sub">${esc(hostname)} · ${esc(email)}</div>
      </div>
      <button class="vp-banner-x" id="vp-x">✕</button>
    </div>
    <div class="vp-banner-btns">
      <button class="vp-btn-save" id="vp-save">Save</button>
      <button class="vp-btn-no"   id="vp-no">Not now</button>
    </div>
  `;
  document.documentElement.appendChild(b);

  const dismiss = () => b.remove();
  document.getElementById("vp-x").onclick  = dismiss;
  document.getElementById("vp-no").onclick = dismiss;
  document.getElementById("vp-save").onclick = async () => {
    document.getElementById("vp-save").textContent = "Saving…";
    const res = await new Promise(resolve =>
      chrome.runtime.sendMessage({ type: "SAVE_ITEM", name: hostname, url: origin, username: email, password }, resolve)
    );
    if (res?.ok) {
      b.innerHTML = `<div style="padding:10px 14px;font-size:13px;color:#34c759;font-weight:600;text-align:center">✓ Saved to VaultPass!</div>`;
      _cachedItems = null;
      setTimeout(() => b.remove(), 2000);
    } else { dismiss(); }
  };

  setTimeout(dismiss, 15000);
}

// ── Helpers ────────────────────────────────────────────────────────────────

function isUsernameField(el) {
  return /user|email|login|username|mail/i.test(el.name + el.id + el.placeholder + (el.autocomplete||""));
}
function setVal(el, v) {
  const s = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, "value")?.set;
  s ? s.call(el, v) : (el.value = v);
  el.dispatchEvent(new Event("input",  { bubbles: true }));
  el.dispatchEvent(new Event("change", { bubbles: true }));
}
function esc(s) {
  return String(s??'').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}

// ── FILL_FORM from popup ───────────────────────────────────────────────────
chrome.runtime.onMessage.addListener((msg, _s, sendResponse) => {
  if (msg.type !== "FILL_FORM") return;
  fillItem(msg.encryptedData);
  sendResponse({ ok: true });
  return true;
});

// ── CSS injected into host page ────────────────────────────────────────────
function injectStyles() {
  const s = document.createElement("style");
  s.id = "vp-styles";
  s.textContent = `
    .vp-ui { font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif !important; }

    #vp-dropdown {
      background:#fff !important; border:1px solid #e2e2e7 !important;
      border-radius:10px !important; box-shadow:0 8px 28px rgba(0,0,0,.15) !important;
      overflow:hidden !important;
    }
    .vp-dd-header {
      display:flex; align-items:center; gap:6px;
      padding:7px 12px 6px; font-size:11px; font-weight:600;
      color:#6e6e73; letter-spacing:.4px; text-transform:uppercase;
      border-bottom:1px solid #f0f0f5;
    }
    .vp-dd-logo { width:14px; height:14px; }
    .vp-dd-row {
      display:flex; align-items:center; gap:10px;
      padding:9px 12px; cursor:pointer;
    }
    .vp-dd-row.hover { background:#f5f5f7 !important; }
    .vp-dd-icon  { font-size:18px; flex-shrink:0; }
    .vp-dd-info  { flex:1; min-width:0; }
    .vp-dd-name  { font-size:13px; font-weight:500; white-space:nowrap; overflow:hidden; text-overflow:ellipsis; color:#1d1d1f; }
    .vp-dd-url   { font-size:11px; color:#6e6e73; white-space:nowrap; overflow:hidden; text-overflow:ellipsis; }
    .vp-dd-fill  { font-size:12px; color:#0066cc; font-weight:600; flex-shrink:0; }

    #vp-save-banner {
      position:fixed !important; top:16px !important; right:16px !important;
      z-index:2147483647 !important; width:300px !important;
      background:#fff !important; border:1px solid #e2e2e7 !important;
      border-radius:12px !important; box-shadow:0 8px 30px rgba(0,0,0,.16) !important;
      padding:14px 16px !important; animation:vpIn .2s ease;
    }
    @keyframes vpIn { from{opacity:0;transform:translateY(-8px)} to{opacity:1;transform:translateY(0)} }
    .vp-banner-top  { display:flex; align-items:flex-start; gap:10px; margin-bottom:10px; }
    .vp-banner-title{ font-size:13px; font-weight:600; color:#1d1d1f; }
    .vp-banner-sub  { font-size:12px; color:#6e6e73; margin-top:2px; }
    .vp-banner-x    { background:none; border:none; cursor:pointer; font-size:14px; color:#6e6e73; padding:0; margin-left:auto; line-height:1; flex-shrink:0; }
    .vp-banner-btns { display:flex; gap:8px; }
    .vp-btn-save    { flex:1; padding:8px; background:#0066cc; color:#fff; border:none; border-radius:8px; font-size:13px; font-weight:500; cursor:pointer; }
    .vp-btn-no      { flex:1; padding:8px; background:none; border:1px solid #e2e2e7; border-radius:8px; font-size:13px; cursor:pointer; color:#1d1d1f; }
  `;
  document.documentElement.appendChild(s);
}
