const $ = id => document.getElementById(id);

async function init() {
  const { token } = await chrome.runtime.sendMessage({ type: "GET_TOKEN" }).catch(() => ({}));
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
  await loadItems();
}

async function loadItems() {
  const list = $("vault-list");
  list.innerHTML = "<p style='color:#6e6e73;font-size:13px'>Loading...</p>";
  const res = await chrome.runtime.sendMessage({ type: "GET_ITEMS" });
  if (!res.ok || !res.items?.length) {
    list.innerHTML = `<div class="empty"><div class="icon">🔑</div><p>No saved passwords yet.</p></div>`;
    return;
  }
  list.innerHTML = res.items.map(item => `
    <div class="vault-item">
      <div class="vault-item-icon">🌐</div>
      <div class="vault-item-info">
        <div class="vault-item-name">${escHtml(item.name)}</div>
        <div class="vault-item-url">${escHtml(item.websiteUrl || "")}</div>
      </div>
      <button class="vault-item-fill" data-id="${item.id}" data-url="${escHtml(item.websiteUrl || "")}" data-enc="${escHtml(item.encryptedData)}">Fill</button>
    </div>
  `).join("");

  list.querySelectorAll(".vault-item-fill").forEach(btn => {
    btn.addEventListener("click", () => fillCredentials(btn.dataset.enc));
  });
}

async function fillCredentials(encryptedData) {
  const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
  chrome.tabs.sendMessage(tab.id, { type: "FILL_FORM", encryptedData });
  window.close();
}

$("btn-login").addEventListener("click", async () => {
  const email    = $("email").value.trim();
  const password = $("password").value;
  const errDiv   = $("login-error");
  errDiv.style.display = "none";
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

function showError(msg) {
  const e = $("login-error");
  e.textContent = msg;
  e.style.display = "block";
}

function escHtml(str) {
  return str.replace(/&/g,"&amp;").replace(/</g,"&lt;").replace(/>/g,"&gt;").replace(/"/g,"&quot;");
}

init();
