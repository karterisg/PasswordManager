chrome.runtime.onMessage.addListener((msg) => {
  if (msg.type === "FILL_FORM") fillForm(msg.encryptedData);
});

// Auto-suggest autofill when page loads and we have a matching saved credential
window.addEventListener("load", async () => {
  const { token } = await chrome.runtime.sendMessage({ type: "GET_TOKEN" }).catch(() => ({}));
  if (!token) return;

  const res = await chrome.runtime.sendMessage({ type: "GET_MATCHING", origin: location.origin });
  if (!res.ok || !res.items?.length) return;

  // Only suggest if there's a password field on the page
  if (!document.querySelector("input[type=password]")) return;

  showAutofillPrompt(res.items);
});

// Save prompt: fires when a password form is submitted
document.addEventListener("submit", async (e) => {
  const form     = e.target;
  const pwField  = findPasswordField(form);
  const userField = findUsernameField(form);
  if (!pwField) return;

  const password = pwField.value;
  const username = userField?.value || "";
  if (!password) return;

  const { token } = await chrome.runtime.sendMessage({ type: "GET_TOKEN" }).catch(() => ({}));
  if (!token) return;

  showSavePrompt(username, password);
}, true);

function findPasswordField(root) {
  return root.querySelector("input[type=password]");
}

function findUsernameField(root) {
  return root.querySelector(
    "input[type=email], input[type=text][name*=user], input[type=text][name*=email], " +
    "input[type=text][id*=user], input[type=text][id*=email], input[type=text]"
  );
}

// --- Autofill suggestion banner ---
function showAutofillPrompt(items) {
  document.getElementById("vaultpass-autofill")?.remove();

  const bar = document.createElement("div");
  bar.id = "vaultpass-autofill";
  bar.style.cssText = `
    position:fixed;top:16px;right:16px;z-index:2147483647;
    background:#fff;border:1px solid #e2e2e7;border-radius:12px;
    box-shadow:0 4px 20px rgba(0,0,0,0.15);padding:14px 16px;
    font-family:-apple-system,BlinkMacSystemFont,"Segoe UI",sans-serif;
    font-size:14px;color:#1d1d1f;width:300px;
  `;

  const listHtml = items.map((item, i) => `
    <button class="vp-fill-btn" data-idx="${i}"
      style="width:100%;text-align:left;padding:8px 10px;background:#f5f5f7;border:1px solid #e2e2e7;
             border-radius:8px;cursor:pointer;font-size:13px;margin-bottom:6px;display:block">
      🔑 ${escHtml(item.name)}
    </button>
  `).join("");

  bar.innerHTML = `
    <div style="display:flex;align-items:center;gap:8px;margin-bottom:10px">
      <span style="font-size:20px">🔐</span>
      <strong>VaultPass</strong>
      <span id="vp-af-close" style="margin-left:auto;cursor:pointer;color:#6e6e73;font-size:18px">✕</span>
    </div>
    <div style="font-size:12px;color:#6e6e73;margin-bottom:10px">Saved credentials for this site</div>
    ${listHtml}
  `;
  document.body.appendChild(bar);

  bar.querySelector("#vp-af-close").onclick = () => bar.remove();
  bar.querySelectorAll(".vp-fill-btn").forEach(btn => {
    btn.addEventListener("click", () => {
      fillForm(items[+btn.dataset.idx].encryptedData);
      bar.remove();
    });
  });

  setTimeout(() => bar.remove(), 20000);
}

// --- Save prompt ---
function showSavePrompt(username, password) {
  document.getElementById("vaultpass-prompt")?.remove();

  // Use only origin+pathname to avoid leaking query params / tokens in the URL
  const safeUrl = location.origin + location.pathname;

  const bar = document.createElement("div");
  bar.id = "vaultpass-prompt";
  bar.style.cssText = `
    position:fixed;top:16px;right:16px;z-index:2147483647;
    background:#fff;border:1px solid #e2e2e7;border-radius:12px;
    box-shadow:0 4px 20px rgba(0,0,0,0.15);padding:14px 16px;
    font-family:-apple-system,BlinkMacSystemFont,"Segoe UI",sans-serif;
    font-size:14px;color:#1d1d1f;width:300px;
  `;
  bar.innerHTML = `
    <div style="display:flex;align-items:center;gap:8px;margin-bottom:10px">
      <span style="font-size:20px">🔐</span>
      <strong>Save password in VaultPass?</strong>
      <span id="vp-close" style="margin-left:auto;cursor:pointer;color:#6e6e73;font-size:18px">✕</span>
    </div>
    <div style="font-size:12px;color:#6e6e73;margin-bottom:12px">${escHtml(username)} · ${escHtml(location.hostname)}</div>
    <div style="display:flex;gap:8px">
      <button id="vp-save" style="flex:1;padding:8px;background:#0066cc;color:#fff;border:none;border-radius:8px;cursor:pointer;font-size:13px;font-weight:500">Save</button>
      <button id="vp-dismiss" style="flex:1;padding:8px;background:#f5f5f7;border:1px solid #e2e2e7;border-radius:8px;cursor:pointer;font-size:13px">Not now</button>
    </div>
  `;
  document.body.appendChild(bar);

  bar.querySelector("#vp-close").onclick   = () => bar.remove();
  bar.querySelector("#vp-dismiss").onclick = () => bar.remove();
  bar.querySelector("#vp-save").onclick    = async () => {
    bar.querySelector("#vp-save").textContent = "Saving...";
    const res = await chrome.runtime.sendMessage({
      type: "SAVE_ITEM",
      name: location.hostname,
      url:  safeUrl,
      username,
      password
    });
    bar.remove();
    if (res?.ok) showToast("Password saved to VaultPass ✓");
  };

  setTimeout(() => bar.remove(), 15000);
}

// --- Autofill: decrypts and fills the login form ---
async function fillForm(encryptedData) {
  const { token, passwordHash } = await chrome.storage.local.get(["token", "passwordHash"]);
  if (!token) return;

  const vaultId = parseVaultId(token);
  const key     = await deriveKey(passwordHash, vaultId);
  const plain   = await decryptAesGcm(key, encryptedData);

  // Support both old format (plain password string) and new format ({username, password})
  let username = "", password = plain;
  try {
    const parsed = JSON.parse(plain);
    if (parsed.password !== undefined) {
      username = parsed.username || "";
      password = parsed.password;
    }
  } catch { /* old entry — plain string password */ }

  const pwField   = document.querySelector("input[type=password]");
  const userField = findUsernameField(document);
  if (pwField)              setNativeValue(pwField, password);
  if (userField && username) setNativeValue(userField, username);
}

// React/Vue-compatible value setter
function setNativeValue(el, value) {
  const setter = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, "value").set;
  setter.call(el, value);
  el.dispatchEvent(new Event("input", { bubbles: true }));
}

function showToast(msg) {
  const t = document.createElement("div");
  t.style.cssText = `position:fixed;bottom:24px;right:24px;z-index:2147483647;background:#1d1d1f;color:#fff;padding:10px 16px;border-radius:10px;font-family:-apple-system,sans-serif;font-size:14px`;
  t.textContent = msg;
  document.body.appendChild(t);
  setTimeout(() => t.remove(), 3000);
}

function parseVaultId(token) {
  const payload = token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/");
  return JSON.parse(atob(payload)).vaultId;
}

async function deriveKey(passwordHash, vaultId) {
  const raw  = new TextEncoder().encode(passwordHash + vaultId);
  const hash = await crypto.subtle.digest("SHA-256", raw);
  return crypto.subtle.importKey("raw", hash, { name: "AES-GCM" }, false, ["encrypt", "decrypt"]);
}

async function decryptAesGcm(key, encryptedBase64) {
  const data  = Uint8Array.from(atob(encryptedBase64), c => c.charCodeAt(0));
  const plain = await crypto.subtle.decrypt({ name: "AES-GCM", iv: data.slice(0, 12) }, key, data.slice(12));
  return new TextDecoder().decode(plain);
}

function escHtml(str) {
  if (!str) return "";
  return str
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#x27;");
}
