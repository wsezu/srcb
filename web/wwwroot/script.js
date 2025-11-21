let currentLang = 'nl';

async function sendMessage() {
    const input = document.getElementById("user-input").value;
    if (!input) return;

    const chatBox = document.getElementById("chat-box");
    chatBox.innerHTML += `<div class='user'>${input}</div>`;

    const response = await fetch("/chat", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ question: input })
    });

    const data = await response.json();
    chatBox.innerHTML += `<div class='bot'>${data.answer}</div>`;
    document.getElementById("user-input").value = "";
}

function switchLanguage(lang) {
    currentLang = lang;
    document.getElementById("user-input").placeholder = lang === 'nl' ? "Stel je vraag..." : "Ask your question...";
}