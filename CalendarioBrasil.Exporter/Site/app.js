const lista = document.getElementById("calendarios");

async function carregarCalendarios() {
    try {
        const resposta = await fetch("/manifest.json", { cache: "no-store" });

        if (!resposta.ok) {
            throw new Error("Não foi possível consultar o manifesto.");
        }

        const manifesto = await resposta.json();

        if (!Array.isArray(manifesto.calendarios)) {
            throw new Error("Manifesto inválido.");
        }

        lista.replaceChildren();

        for (const calendario of manifesto.calendarios) {
            if (!calendario || !Number.isInteger(calendario.ano) || calendario.ano < 2024 || calendario.ano > 2100 || !/^[A-Z]{2}$/.test(calendario.uf) || !Number.isInteger(calendario.total)) {
                continue;
            }

            const item = document.createElement("li");
            const link = document.createElement("a");

            link.href = `/${calendario.ano}/${calendario.uf}.json`;
            link.textContent = `${calendario.ano} / ${calendario.uf} - ${calendario.cobertura} (${calendario.total} eventos)`;

            item.appendChild(link);
            lista.appendChild(item);
        }

        if (lista.children.length === 0) {
            lista.textContent = "Nenhum calendário disponível.";
        }
    }
    catch {
        lista.textContent = "Não foi possível carregar a lista de calendários.";
    }
}

carregarCalendarios();