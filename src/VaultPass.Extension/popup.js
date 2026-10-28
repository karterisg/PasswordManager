const $ = id => document.getElementById(id);

let allItems = [];
let templates = [];
let currentOrigin = null;

async function init() {
  const { token } = await chrome.storage.local.get("token");
  if (token) showVault();
  else showLogin();
}

function showLogin() {
  $("view-login").style.display = "block";
  $("view-vault").style.display = "none";
  $("btn-signout").style.display = "none";
}

async function showVault() {
  $("view-login").style.display = "none";
  $("view-vault").style.display = "block";
  $("btn-signout").style.display = "block";

  const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
  try { currentOrigin = new URL(tab.url).origin; } catch { currentOrigin = null; }

  await checkPendingSave();
  await loadItems();
}

async function checkPendingSave() {
  const res = await chrome.runtime.sendMessage({ type: "GET_PENDING_SAVE" });
  if (!res?.pending) return;

  const p = res.pending;
  const hostname = (() => { try { return new URL(p.origin).hostname; } catch { return p.origin; } })();
  const banner = document.createElement("div");
  banner.id = "save-banner";
  banner.innerHTML = `
    <div style="background:#f0f7ff;border:1px solid #0066cc33;border-radius:10px;padding:12px 14px;margin-bottom:12px">
      <div style="font-size:13px;font-weight:600;margin-bottom:6px">💾 Save login for <span style="color:#0066cc">${escHtml(hostname)}</span>?</div>
      <div style="font-size:12px;color:#6e6e73;margin-bottom:10px">${escHtml(p.email)}</div>
      <div style="display:flex;gap:8px">
        <button id="btn-save-yes" style="flex:1;padding:7px;background:#0066cc;color:#fff;border:none;border-radius:7px;font-size:13px;font-weight:500;cursor:pointer">Save</button>
        <button id="btn-save-no"  style="flex:1;padding:7px;background:none;border:1px solid #e2e2e7;border-radius:7px;font-size:13px;cursor:pointer">Dismiss</button>
      </div>
    </div>`;

  const list = $("vault-list");
  list.parentNode.insertBefore(banner, list);

  document.getElementById("btn-save-yes").addEventListener("click", async () => {
    document.getElementById("btn-save-yes").textContent = "Saving…";
    const res2 = await chrome.runtime.sendMessage({
      type: "SAVE_ITEM",
      name: hostname,
      url: p.origin,
      username: p.email,
      password: p.password
    });
    await chrome.runtime.sendMessage({ type: "DISMISS_PENDING_SAVE" });
    banner.remove();
    if (res2.ok) await loadItems();
  });

  document.getElementById("btn-save-no").addEventListener("click", async () => {
    await chrome.runtime.sendMessage({ type: "DISMISS_PENDING_SAVE" });
    banner.remove();
  });
}

async function loadItems() {
  const list = $("vault-list");
  list.innerHTML = "<p style='color:#6e6e73;font-size:13px;text-align:center;padding:20px 0'>Loading...</p>";

  const res = await chrome.runtime.sendMessage({ type: "GET_ITEMS" });
  if (!res.ok) { list.innerHTML = `<div class="empty"><div class="icon">⚠️</div><p>Could not load items.</p></div>`; return; }

  allItems = res.items || [];
  templates = res.templates || [];

  const matchCount = currentOrigin
    ? allItems.filter(i => { try { return new URL(i.websiteUrl).origin === currentOrigin; } catch { return false; } }).length
    : 0;

  if (matchCount > 0) {
    $("match-badge").textContent = `${matchCount} match${matchCount > 1 ? "es" : ""}`;
    $("match-badge").style.display = "inline-block";
  } else {
    $("match-badge").style.display = "none";
  }

  renderItems(allItems);
}

function renderItems(items) {
  const list = $("vault-list");
  if (!items.length) {
    list.innerHTML = `<div class="empty"><div class="icon">🔑</div><p>No saved items.</p></div>`;
    return;
  }

  // Sort: matched items first
  const sorted = [...items].sort((a, b) => {
    const aMatch = isMatch(a);
    const bMatch = isMatch(b);
    if (aMatch && !bMatch) return -1;
    if (!aMatch && bMatch) return 1;
    return 0;
  });

  list.innerHTML = sorted.map(item => {
    const matched = isMatch(item);
    const icon = getIcon(item);
    const canFill = item.websiteUrl || matched;
    return `
      <div class="vault-item${matched ? " matched" : ""}">
        <div class="vault-item-icon">${icon}</div>
        <div class="vault-item-info">
          <div class="vault-item-name">${escHtml(item.name)}</div>
          ${item.websiteUrl ? `<div class="vault-item-url">${escHtml(item.websiteUrl)}</div>` : ""}
        </div>
        <div class="vault-item-actions">
          <button class="vault-btn" data-action="copy" data-enc="${escHtml(item.encryptedData)}">Copy</button>
          ${canFill ? `<button class="vault-btn fill" data-action="fill" data-enc="${escHtml(item.encryptedData)}">Fill</button>` : ""}
        </div>
      </div>
    `;
  }).join("");

  list.querySelectorAll("[data-action='copy']").forEach(btn => {
    btn.addEventListener("click", () => copyCredential(btn.dataset.enc, btn));
  });
  list.querySelectorAll("[data-action='fill']").forEach(btn => {
    btn.addEventListener("click", () => fillCredentials(btn.dataset.enc));
  });
}

function isMatch(item) {
  if (!currentOrigin || !item.websiteUrl) return false;
  try { return new URL(item.websiteUrl).origin === currentOrigin; } catch { return false; }
}

function getIcon(item) {
  if (item.templateId) {
    const t = templates.find(t => t.id === item.templateId);
    return t?.icon ?? "🔑";
  }
  return "🔑";
}

async function copyCredential(encryptedData, btn) {
  const res = await chrome.runtime.sendMessage({ type: "DECRYPT", encryptedData });
  if (!res.ok) return;
  let val = res.plaintext;
  try {
    const obj = JSON.parse(val);
    val = Object.values(obj)[0] ?? val;
  } catch { }
  await navigator.clipboard.writeText(val);
  btn.textContent = "✓";
  setTimeout(() => btn.textContent = "Copy", 1500);
}

async function fillCredentials(encryptedData) {
  const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
  chrome.tabs.sendMessage(tab.id, { type: "FILL_FORM", encryptedData });
  window.close();
}

$("btn-login").addEventListener("click", async () => {
  const email    = $("email").value.trim();
  const password = $("password").value;
  $("login-error").style.display = "none";
  if (!email || !password) { showError("Please enter email and password."); return; }
  $("btn-login").textContent = "Signing in...";
  const res = await chrome.runtime.sendMessage({ type: "LOGIN", email, password });
  $("btn-login").textContent = "Sign In";
  if (res.ok) showVault();
  else showError("Invalid email or password.");
});

$("btn-signout").addEventListener("click", async () => {
  await chrome.storage.local.clear();
  showLogin();
});

$("search").addEventListener("input", e => {
  const q = e.target.value.toLowerCase();
  const filtered = q ? allItems.filter(i =>
    i.name.toLowerCase().includes(q) || (i.websiteUrl || "").toLowerCase().includes(q)
  ) : allItems;
  renderItems(filtered);
});

function showError(msg) {
  const e = $("login-error");
  e.textContent = msg;
  e.style.display = "block";
}

function escHtml(str) {
  return String(str).replace(/&/g,"&amp;").replace(/</g,"&lt;").replace(/>/g,"&gt;").replace(/"/g,"&quot;");
}

init();
